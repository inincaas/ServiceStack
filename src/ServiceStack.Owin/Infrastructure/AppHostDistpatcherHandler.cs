namespace ServiceStack.Owin.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using ServiceStack.WebHost.Endpoints;
    using ServiceStack.WebHost.Endpoints.Support;

    public class AppHostDistpatcherHandler
    {
        private readonly AppHostBase _appHostBase;

        public AppHostDistpatcherHandler(Func<IDictionary<string, object>, Task> next, AppHostBase appHostBase)
        {
            _appHostBase = appHostBase;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            return Task.Factory.StartNew(() =>
                                         {
                                             var cancellationToken = environment.Get<CancellationToken>(OwinConstants.CallCancelledKey);
                                             if (cancellationToken.IsCancellationRequested)
                                             {
                                                 return;
                                             }
                                             string operationName = GetOperationName(environment);
                                             var httpReq = new OwinHttpRequest(operationName, environment, _appHostBase.Container);
                                             var httpRes = new OwinHttpResponse(environment);
                                             IHttpHandler handler = ServiceStackHttpHandlerFactory.GetHandler(httpReq);
                                             var serviceStackHandler = handler as IServiceStackHttpHandler;
                                             if (serviceStackHandler != null)
                                             {
                                                 var restHandler = serviceStackHandler as RestHandler;
                                                 if (restHandler != null)
                                                 {
                                                     httpReq.OperationName = operationName = restHandler.RestPath.RequestType.Name;
                                                 }
                                                 serviceStackHandler.ProcessRequest(httpReq, httpRes, operationName);
                                                 httpRes.Close();
                                                 return;
                                             }

                                             throw new NotImplementedException("Cannot execute handler: " + handler +
                                                                               " at PathInfo: " + httpReq.PathInfo);
                                         });
        }

        private static string GetOperationName(IDictionary<string, object> environment)
        {
            Uri uri = environment.GetRequestUri();
            return uri.Segments[uri.Segments.Length - 1];
        }
    }
}