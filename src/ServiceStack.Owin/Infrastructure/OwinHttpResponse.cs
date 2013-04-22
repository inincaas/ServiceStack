using System.Net;

namespace ServiceStack.Owin.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using ServiceStack.ServiceHost;

    public class OwinHttpResponse : IHttpResponse
	{
		private readonly IDictionary<string, object> _environment;
		private readonly StreamWriter _textWriter;
		private readonly Stream _outputStream;
		private bool _isClosed;

		public OwinHttpResponse(IDictionary<string, object> environment)
		{
			if(environment == null) throw new ArgumentNullException("environment");
			_environment = environment;
			_outputStream = _environment.Get<Stream>(OwinConstants.ResponseBodyKey);
			_textWriter = new StreamWriter(_outputStream);
		}

		#region IHttpResponse Members

		public object OriginalResponse { get; private set; }
		public int StatusCode
		{
            get { return _environment.Get<int>(OwinConstants.ResponseStatusCodeKey); }
			set { _environment.Set(OwinConstants.ResponseStatusCodeKey, value); }
		}

        public string StatusDescription { get; set; }

		public string ContentType
		{
			get { return _environment.GetResponseHeader(OwinConstants.ContentTypeHeader); }
			set { _environment.SetResponseHeader(OwinConstants.ContentTypeHeader, value); }
		}

        public ICookies Cookies { get { throw new NotImplementedException(); } }

		public void AddHeader(string name, string value)
		{
			_environment.SetResponseHeader(name, value);
		}

		public void Redirect(string url)
		{
			AddHeader(OwinConstants.LocationHeader, url);
		    StatusCode = (int)HttpStatusCode.Redirect;
		    StatusDescription = "Found";
		}

		public Stream OutputStream
		{
			get { return _outputStream; }
		}

		public void Write(string text)
		{
			_textWriter.Write(text);
			_textWriter.Flush();
		}

		public void Close()
		{
			_isClosed = true;
			/* 	Do not close the stream. From Owin 0.15, Section 3.5 Response Body:
				The application SHOULD NOT close or dispose the given stream as middleware may append additional data.
				It is the responsibility of the stream owner (e.g. the server or middleware) to close the stream once the application delegate’s Task completes.
			 */
		}

		public void End()
		{
			Flush();
			_isClosed = true;
			// Do nothing 
		}

		public void Flush()
		{
			_outputStream.Flush();
		}

		public bool IsClosed
		{
			get { return _isClosed; }
		}

        public void SetContentLength(long contentLength)
        {
            AddHeader(OwinConstants.ContentLengthHeader, contentLength.ToString());
        }

        #endregion
	}
}