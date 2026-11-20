$ErrorActionPreference = 'Stop'
$records = [IO.Directory]::CreateDirectory($args[0]).FullName
$hostName = $args[1]
$test = [Diagnostics.ProcessStartInfo]::new('dotnet')
$test.UseShellExecute = $false
foreach ($word in $args[2..($args.Count - 1)]) { $test.ArgumentList.Add($word) }
$test.Environment['ALIS_SERVER_RECORDS'] = $records
$test.Environment.Remove('ALIS_IIS_HOST') | Out-Null
$test.Environment.Remove('ALIS_IIS_ADDRESS') | Out-Null

function Wait-ForMvc($server, $url, $marker) {
    $client = [Net.Http.HttpClient]::new()
    $client.Timeout = [Threading.Timeout]::InfiniteTimeSpan
    $deadline = [Threading.CancellationTokenSource]::new([TimeSpan]::FromMinutes(2))
    $lastError = 'no connection'
    try {
        while (!$deadline.IsCancellationRequested) {
            if ($server.HasExited) { throw "IIS Express exited $($server.ExitCode) before $url was ready." }
            try {
                $response = $client.GetAsync($url, $deadline.Token).GetAwaiter().GetResult()
            } catch [OperationCanceledException] {
                throw "MVC readiness timed out at $url ($lastError)."
            } catch [Net.Http.HttpRequestException] {
                $lastError = $_.Exception.GetBaseException()
                if ($lastError -isnot [Net.Sockets.SocketException] -or $lastError.SocketErrorCode -ne 'ConnectionRefused') { throw }
                Start-Sleep -Milliseconds 100
                continue
            }
            try {
                $body = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
                if (!$response.IsSuccessStatusCode -or !$body.Contains($marker)) {
                    throw "MVC readiness failed at $url (HTTP $([int]$response.StatusCode), expected $marker): $body"
                }
                if ($server.HasExited) { throw "IIS Express exited $($server.ExitCode) while checking $url." }
                Write-Host "[playwright:runner] $hostName ready at $url (HTTP $([int]$response.StatusCode), $marker)"
                return
            } finally { $response.Dispose() }
        }
        throw "MVC readiness timed out at $url ($lastError)."
    } finally {
        $client.Dispose()
        $deadline.Dispose()
    }
}

function Stop-OwnedProcess($process) {
    if ($process.HasExited) { return }
    $process.Kill($true)
    if (!$process.WaitForExit(10000)) { throw "PID $($process.Id) is still alive after termination." }
}

$iis = $null
$exitCode = 1
$left = 0
$freshLeft = 0
try {
    if ($hostName -in 'net48', 'consumerNet48') {
        if ($hostName -eq 'net48') {
            $project = 'tests/Alis.Reactive.Conformance.App.Net48'
            $site = 'C:\AlisReactive.Twin'
            $ready = '/world/ready'
            $marker = '<h1>Ready</h1>'
        } else {
            $project = 'tests/Alis.Reactive.Consumer.Net48'
            $site = (Resolve-Path $project).Path
            $ready = '/'
            $marker = 'id="consumer-status"'
        }
        $name = Split-Path $project -Leaf
        foreach ($file in 'Web.config', 'Global.asax', "bin/$name.dll") {
            if (!(Test-Path -LiteralPath "$site/$file" -PathType Leaf)) { throw "The $hostName site lacks $site/$file." }
        }
        $assets = Get-Content -Raw -LiteralPath "$project/obj/project.assets.json" | ConvertFrom-Json -AsHashtable
        $package = "AlisReactive/$env:PACKAGE_VERSION"
        if (!$assets.libraries.ContainsKey($package)) { throw "$project was not restored on $package." }
        $locations = @($env:ProgramFiles, ${env:ProgramFiles(x86)}) | Where-Object { $_ } |
            ForEach-Object { Join-Path $_ 'IIS Express/iisexpress.exe' }
        $executable = $locations | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1
        if (!$executable) { throw "IIS Express was not found at $($locations -join ' or ')." }
        $listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
        $listener.Start()
        try { $port = $listener.LocalEndpoint.Port } finally { $listener.Stop() }
        $address = "http://localhost:$port"
        $iis = Start-Process $executable -ArgumentList "`"/path:$site`"", "/port:$port", '/clr:v4.0', '/systray:false' -PassThru -NoNewWindow `
            -RedirectStandardOutput "$records/iis.log" -RedirectStandardError "$records/iis.err.log"
        Write-Host "[playwright:runner] $hostName IIS Express PID $($iis.Id) serves $site at $address with CLR v4.0"
        Wait-ForMvc $iis "$address$ready" $marker
        if ($hostName -eq 'net48') {
            Wait-ForMvc $iis "$address/care/admission/contact-move-in" 'name="NextOfKin.Name"'
        }
        $test.Environment['ALIS_IIS_HOST'] = $hostName
        $test.Environment['ALIS_IIS_ADDRESS'] = $address
    }
    $dotnet = [Diagnostics.Process]::Start($test)
    try {
        $dotnet.WaitForExit()
        $exitCode = $dotnet.ExitCode
        if ($exitCode) { [Console]::Error.WriteLine("[playwright:runner] $hostName dotnet test exited $exitCode.") }
    } finally {
        try { Stop-OwnedProcess $dotnet } finally { $dotnet.Dispose() }
    }
    if ($iis -and $iis.HasExited) { throw "IIS Express PID $($iis.Id) exited $($iis.ExitCode) during the $hostName run." }
} catch {
    [Console]::Error.WriteLine("[playwright:runner] $hostName failed: $_")
    $exitCode = 1
} finally {
    if ($iis) {
        try {
            Stop-OwnedProcess $iis
            Write-Host "[playwright:runner] $hostName IIS Express PID $($iis.Id) exited"
        } catch {
            if ($hostName -eq 'consumerNet48') { $freshLeft++ } else { $left++ }
            [Console]::Error.WriteLine("[playwright:runner] IIS Express cleanup failed: $_")
            $exitCode = 1
        } finally { $iis.Dispose() }
    }
    foreach ($file in Get-ChildItem -LiteralPath $records -Filter '*.server') {
        $serverId, $serverHost = $file.BaseName.Split('.')
        try {
            try { $server = [Diagnostics.Process]::GetProcessById([int]$serverId) }
            catch [ArgumentException] {
                Write-Host "[playwright:runner] $serverHost PID $serverId exited"
                continue
            }
            try {
                try { [void]$server.SafeHandle } catch {
                    if (!$server.HasExited) { throw }
                    Write-Host "[playwright:runner] $serverHost PID $serverId exited"
                    continue
                }
                $started = [long](Get-Content -LiteralPath $file.FullName -Raw)
                if ($server.HasExited -or $server.StartTime.ToUniversalTime().ToFileTimeUtc() -ne $started -or $server.ProcessName -ne 'dotnet') {
                    Write-Host "[playwright:runner] $serverHost recorded at PID $serverId exited"
                    continue
                }
                if ($serverHost -eq 'consumerNet10') { $freshLeft++ } else { $left++ }
                [Console]::Error.WriteLine("[playwright:runner] $serverHost left PID $serverId alive after the test process exited")
                Stop-OwnedProcess $server
            } finally { $server.Dispose() }
        } catch {
            [Console]::Error.WriteLine("[playwright:runner] Could not verify or clean up $($file.Name): $_")
            $exitCode = 1
        }
    }
    [IO.File]::WriteAllText("$records/servers-left", "$left $freshLeft`n")
}
exit $exitCode
