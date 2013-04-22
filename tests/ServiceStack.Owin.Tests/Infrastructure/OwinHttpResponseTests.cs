namespace ServiceStack.Owin.Tests.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using NUnit.Framework;
    using Owin.Infrastructure;

    [TestFixture]
    public class OwinHttpResponseTests
    {
        [SetUp]
        public void SetUp()
        {
            _owinEnvironment = new Dictionary<string, object>
                               {
                                   {OwinConstants.ResponseBodyKey, new MemoryStream()},
                                   {OwinConstants.ResponseHeadersKey, new Dictionary<string, string[]>()}
                               };
            _sut = new OwinHttpResponse(_owinEnvironment);
        }

        private OwinHttpResponse _sut;
        private Dictionary<string, object> _owinEnvironment;

        [Test]
        public void Can_set_content_type_header()
        {
            _sut.ContentType = "text/xml";
            Assert.AreEqual("text/xml", _owinEnvironment.GetResponseHeader(OwinConstants.ContentTypeHeader));
            Assert.AreEqual("text/xml", _sut.ContentType);
        }

        [Test]
        public void Can_set_status_code()
        {
            _sut.StatusCode = 200;
            Assert.AreEqual(200, _owinEnvironment.Get<int>(OwinConstants.ResponseStatusCodeKey));
        }

        [Test]
        public void Can_set_content_length()
        {
            _sut.SetContentLength(5000);
            Assert.AreEqual("5000", _owinEnvironment.GetResponseHeader(OwinConstants.ContentLengthHeader));
        }

        [Test]
        public void Can_write_to_response_stream()
        {
            string text = "Response!";
            _sut.Write(text);

            var stream = _owinEnvironment.Get<Stream>(OwinConstants.ResponseBodyKey);
            stream.Position = 0;
            using (var tr = new StreamReader(stream))
            {
                string writtenText = tr.ReadToEnd();
                Assert.AreEqual(text, writtenText);
            }
        }

        [Test]
        public void When_Close_Then_IsClosed_is_true()
        {
            Assert.False(_sut.IsClosed);
            _sut.Close();
            Assert.True(_sut.IsClosed);
        }

        [Test]
        public void When_Close_Then_stream_should_remain_open()
        {
            _sut.Close();
            Assert.True(_owinEnvironment.Get<Stream>(OwinConstants.ResponseBodyKey).CanWrite);
        }

        [Test]
        public void When_End_Then_IsClosed_is_true()
        {
            Assert.False(_sut.IsClosed);
            _sut.End();
            Assert.True(_sut.IsClosed);
        }

        [Test]
        public void When_create_with_null_environment_Then_should_throw()
        {
            Assert.Throws<ArgumentNullException>(() => new OwinHttpResponse(null));
        }

        [Test]
        public void When_Redirect_Then_Redirected()
        {
            _sut.Redirect("http://www.example.com");
            Assert.AreEqual(302, _owinEnvironment.Get<int>(OwinConstants.ResponseStatusCodeKey));
            Assert.AreEqual("http://www.example.com", _owinEnvironment.GetResponseHeader(OwinConstants.LocationHeader));
        }
    }
}