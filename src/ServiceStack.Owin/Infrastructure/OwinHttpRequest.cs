namespace ServiceStack.Owin.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Net;
    using System.Web;
    using Funq;
    using ServiceStack.ServiceHost;
    using ServiceStack.WebHost.Endpoints.Extensions;

    public class OwinHttpRequest : IHttpRequest
	{
		private readonly IDictionary<string, object> _environment;
		private readonly Container _container;
		private string _responseContentType;

		public OwinHttpRequest(string operationName, IDictionary<string, object> environment, Container container)
		{
			if (operationName == null) throw new ArgumentNullException("operationName");
			if (environment == null) throw new ArgumentNullException("environment");
			if (container == null) throw new ArgumentNullException("environment");
			OperationName = operationName;
			_environment = environment;
			_container = container;

			AbsoluteUri = (string) _environment[OwinConstants.RequestSchemeKey] +
			              "://" +
			              _environment.GetRequestHeaders()["Host"] +
			              (string) _environment[OwinConstants.RequestPathBaseKey] +
			              (string) _environment[OwinConstants.RequestPathKey];
			AcceptTypes = _environment.GetOwinRequestHeaders()[OwinConstants.AcceptHeader].SelectMany(h => h.Split(';')).ToArray();
			ApplicationFilePath = Environment.CurrentDirectory;
			ContentType = _environment.GetRequestHeader(OwinConstants.ContentTypeHeader);
			ContentLength = long.Parse(_environment.GetRequestHeader(OwinConstants.ContentLengthHeader) ?? "0");
			Headers = _environment.GetRequestHeaders();
			HttpMethod = _environment.Get<string>(OwinConstants.RequestMethodKey);
			Items = new Dictionary<string, object>();
			InputStream = _environment.Get<System.IO.Stream>(OwinConstants.RequestBodyKey);
			IsSecureConnection = _environment.Get<string>(OwinConstants.RequestSchemeKey).ToUpperInvariant() == "HTTPS";
			PathInfo = _environment.Get<string>(OwinConstants.RequestPathKey);
			Files = new IFile[0]; //TODO Should I be doing something here?
			QueryString = HttpUtility.ParseQueryString(_environment.Get<string>(OwinConstants.RequestQueryStringKey));
			RawUrl = _environment.Get<string>(OwinConstants.RequestPathBaseKey) +
			         _environment.Get<string>(OwinConstants.RequestPathKey);
			if (!string.IsNullOrWhiteSpace(_environment.Get<string>(OwinConstants.RequestQueryStringKey)))
			{
				RawUrl += "?" + _environment.Get<string>(OwinConstants.RequestQueryStringKey);
			}
			UserHostAddress = _environment.Get<string>(OwinConstants.ServerRemoteIpAddressKey) + ":" +
			                  _environment.Get<string>(OwinConstants.ServerRemotePortKey);
		}

		public string AbsoluteUri { get; private set; }

		public string[] AcceptTypes { get; private set; }

		public string ApplicationFilePath { get; private set; }
		public long ContentLength { get; private set; }

		public string ContentType { get; private set; }

		public IDictionary<string, Cookie> Cookies
		{
			get { throw new NotImplementedException(); }
		}

		public IFile[] Files { get; private set; }

		public NameValueCollection FormData
		{
			get { throw new NotImplementedException(); }
		}

		public NameValueCollection Headers { get; private set; }
		public string HttpMethod { get; private set; }
		public System.IO.Stream InputStream { get; private set; }
		public bool IsSecureConnection { get; private set; }

		public Dictionary<string, object> Items { get; private set; }

		public string OperationName { get; set; }

		public object OriginalRequest
		{
			get { throw new NotImplementedException(); }
		}

		public string PathInfo { get; private set; }

		public NameValueCollection QueryString { get; private set; }

		public string RawUrl { get; private set; }

		public string RemoteIp
		{
			get { throw new NotImplementedException(); }
		}

		public string ResponseContentType
		{
			get { return _responseContentType ?? (_responseContentType = this.GetResponseContentType()); }
			set { _responseContentType = value; }
		}

		public string UserAgent
		{
			get { throw new NotImplementedException(); }
		}

		public string UserHostAddress { get; private set; }

		public string GetRawBody()
		{
			throw new NotImplementedException();
		}

		public T TryResolve<T>()
		{
			return _container.TryResolve<T>();
		}
	}
}