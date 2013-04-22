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
        private static readonly char[] CommaSemicolon = new[] {',', ';'};
        private static readonly char[] AmpersandSemicolon = new[] {'&', ';'};
        private static readonly char[] EqualsSeparator = new[] {'='};
        private static readonly char[] Whitespace = new[] {' '};

		private readonly IDictionary<string, object> _environment;
		private readonly Container _container;
        private NameValueCollection _form;
        private IDictionary<string, Cookie> _cookies; 
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
			QueryString = HttpUtility.ParseQueryString(_environment.Get<string>(OwinConstants.RequestQueryStringKey));
			RawUrl = _environment.Get<string>(OwinConstants.RequestPathBaseKey) +
			         _environment.Get<string>(OwinConstants.RequestPathKey);
			if (!string.IsNullOrWhiteSpace(_environment.Get<string>(OwinConstants.RequestQueryStringKey)))
			{
				RawUrl += "?" + _environment.Get<string>(OwinConstants.RequestQueryStringKey);
			}
		    RemoteIp = _environment.Get<string>(OwinConstants.ServerRemoteIpAddressKey);
		    UserAgent = _environment.GetRequestHeader(OwinConstants.UserAgent);
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
            get
            {
                if (_cookies != null) return _cookies;
                
                _cookies = new Dictionary<string, Cookie>(StringComparer.OrdinalIgnoreCase);
                var text = _environment.GetRequestHeader(OwinConstants.CookieHeader);
                
                if (String.IsNullOrEmpty(text)) return _cookies;

                foreach (var kv in ParseValues(text, CommaSemicolon))
                {
                    if (!_cookies.ContainsKey(kv.Key))
                    {
                        _cookies.Add(kv.Key, new Cookie(kv.Key, kv.Value));
                    }
                }

                return _cookies;
            }
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
			get { return _environment; }
		}

		public string PathInfo { get; private set; }

		public NameValueCollection QueryString { get; private set; }

		public string RawUrl { get; private set; }

        public string RemoteIp { get; private set; }

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

		public string UserAgent { get; private set; }

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

        private string MediaType
        {
            get
            {
                var contentType = ContentType;
                if (contentType == null)
                {
                    return null;
                }

                var delimiterPos = contentType.IndexOfAny(CommaSemicolon);
                return delimiterPos < 0 ? contentType : contentType.Substring(0, delimiterPos);
            }
        }

        public IFile[] Files
        {
            get
            {
                ReadFormData();
                throw new NotImplementedException();
            }
        }

        public NameValueCollection FormData
        {
            get
            {
                ReadFormData();
                return _form;
            }
        }

        private void ReadFormData()
        {
            if (_form != null) return;
            _form = new NameValueCollection();

            var mediaType = MediaType;

            if (String.Equals(mediaType, "application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
            {
                ProcessWwwForm();
            }
            else if (String.Equals(mediaType, "multipart/form-data", StringComparison.OrdinalIgnoreCase))
            {
                ProcessMultiPartForm();
            }
        }

        private void ProcessWwwForm()
        {
            var body = InputStream;

            if (body.CanSeek)
            {
                body.Seek(0, SeekOrigin.Begin);
            }

            var text = new StreamReader(body).ReadToEnd();

            foreach (var kv in ParseValues(text, AmpersandSemicolon))
            {
                _form.Add(kv.Key, kv.Value);
            }
        }

        private void ProcessMultiPartForm()
        {
            // TODO: Need to implement this.
            throw new NotImplementedException();
        }

        private static IEnumerable<KeyValuePair<string, string>> ParseValues(string value, char[] delimiters)
        {
            value = value ?? String.Empty;

            var items = value.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in items)
            {
                string[] pair = item.Split(EqualsSeparator, 2, StringSplitOptions.None);

                string pairKey = UrlDecoder.UrlDecode(pair[0]).TrimStart(Whitespace);
                string pairValue = pair.Length < 2 ? String.Empty : UrlDecoder.UrlDecode(pair[1]);

                yield return new KeyValuePair<string, string>(pairKey, pairValue);
            }
        }
	}
}