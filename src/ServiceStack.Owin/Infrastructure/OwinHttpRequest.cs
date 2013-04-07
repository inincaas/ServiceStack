using System.IO;

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
    using ServiceStack.Text;
    using ServiceStack.WebHost.Endpoints.Extensions;

    public class OwinHttpRequest : IHttpRequest
	{
		private readonly IDictionary<string, object> _environment;
		private readonly Container _container;
		private string _responseContentType;
        private MemoryStream _bufferedStream;
        private readonly Stream _inputStream;

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
			_inputStream = _environment.Get<Stream>(OwinConstants.RequestBodyKey);
            IsLocal = _environment.Get<bool>(OwinConstants.IsLocalKey);
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
            XForwardedFor = _environment.GetRequestHeader(OwinConstants.XForwardedFor);
            XRealIp = _environment.GetRequestHeader(OwinConstants.XRealIp);
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
        public Stream InputStream { get { return _bufferedStream ?? _inputStream; } }
        public bool IsLocal { get; private set; }
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

        public bool UseBufferedStream
        {
            get { return _bufferedStream != null; }
            set
            {
                _bufferedStream = value
                    ? _bufferedStream ?? new MemoryStream(_inputStream.ReadFully())
                        : null;
            }
        }

		public string UserAgent
		{
			get { throw new NotImplementedException(); }
		}

		public string UserHostAddress { get; private set; }

        public string XForwardedFor { get; private set; }

        public string XRealIp { get; private set; }

		public string GetRawBody()
		{
            if (_bufferedStream != null)
            {
                return _bufferedStream.ToArray().FromUtf8Bytes();
            }

            using (var reader = new StreamReader(InputStream))
			{
				return reader.ReadToEnd();
			}
		}

		public T TryResolve<T>()
		{
			return _container.TryResolve<T>();
		}
	}
}