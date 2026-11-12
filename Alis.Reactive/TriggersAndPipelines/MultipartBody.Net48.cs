#if NET48
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web;
using Alis.Reactive.Declarations;
using Alis.Reactive.RequestsAndResponses;

namespace Alis.Reactive.TriggersAndPipelines;

public sealed partial class MultipartBody<TRequest>
{
    /// <summary>Sends a file as a part of the body, under a member's name.</summary>
    /// <param name="member">The file member of the request, as a member path.</param>
    /// <param name="file">The file sent.</param>
    /// <returns>The body's members.</returns>
    public MultipartBody<TRequest> File(Expression<Func<TRequest, HttpPostedFileBase?>> member, BrowserFile file) => throw new NotImplementedException();

    /// <summary>Sends several files as parts of the body, each under a member's name.</summary>
    /// <param name="member">The member of the request that lists its files, as a member path.</param>
    /// <param name="files">The files sent, one part each.</param>
    /// <returns>The body's members.</returns>
    public MultipartBody<TRequest> File(Expression<Func<TRequest, List<HttpPostedFileBase>>> member, LiveObjects<BrowserFile> files) => throw new NotImplementedException();
}
#endif
