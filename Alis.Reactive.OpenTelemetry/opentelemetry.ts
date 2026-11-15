import { OTLPTraceExporter } from "@opentelemetry/exporter-trace-otlp-http";
import { BatchSpanProcessor } from "@opentelemetry/sdk-trace";
import { TraceSink } from "./Tracing/trace-sink";

interface ExporterOptions {
  readonly tracesUrl: string;
  readonly serviceName: string;
}

const sinkName = "trace";

function optionsOf(script: HTMLOrSVGScriptElement | null): ExporterOptions {
  if (!(script instanceof HTMLScriptElement)) throw new Error("The OpenTelemetry exporter runs only as the classic script its layout helper writes.");
  const tracesUrl = script.dataset["tracesUrl"];
  const serviceName = script.dataset["serviceName"];
  if (tracesUrl === undefined || serviceName === undefined) throw new Error("The OpenTelemetry exporter's script names no traces URL or no service name: link it with Html.OpenTelemetryExporter.");
  return { tracesUrl, serviceName };
}

function provide(name: string, instance: unknown): void {
  const queue: unknown = Reflect.get(window, "__alisPlugins");
  if (Array.isArray(queue)) {
    queue.push({ name, instance });
    return;
  }
  Reflect.set(window, "__alisPlugins", [{ name, instance }]);
}

const options = optionsOf(document.currentScript);

provide(sinkName, new TraceSink(new BatchSpanProcessor({ exporter: new OTLPTraceExporter({ url: options.tracesUrl }) }), options.serviceName));
