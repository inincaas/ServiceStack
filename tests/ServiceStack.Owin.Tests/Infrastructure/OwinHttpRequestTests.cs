namespace ServiceStack.Owin.Tests.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using Funq;
    using NUnit.Framework;
    using Owin.Infrastructure;
    using ServiceStack.WebHost.Endpoints.Tests.Support.Host;

    [TestFixture]
    public class OwinHttpRequestTests
    {
        [SetUp]
        public void SetUp()
        {
            var owinEnvironment = new Dictionary<string, object>
                                  {
                                      {OwinConstants.RequestSchemeKey, "https"},
                                      {
                                          OwinConstants.RequestHeadersKey, new Dictionary<string, string[]>
                                                                           {
                                                                               {
                                                                                   OwinConstants.HostHeader,
                                                                                   new[] {"localhost"}
                                                                               },
                                                                               {
                                                                                   OwinConstants.CookieHeader,
                                                                                   new[] { "cookie1=value1; cookie2=value2; scoped.cookie=poor-mans-scope"}
                                                                               },
                                                                               {
                                                                                   OwinConstants.ContentTypeHeader,
                                                                                   new[] {"application/xml"}
                                                                               },
                                                                               {
                                                                                   OwinConstants.ContentLengthHeader,
                                                                                   new[] {"1024"}
                                                                               },
                                                                               {
                                                                                   OwinConstants.AcceptHeader,
                                                                                   new[] {"text/html"}
                                                                               },
                                                                               {
                                                                                   OwinConstants.UserAgent,
                                                                                   new[] { "UnitTest/1.0 (en-us) Unknown/0.0"}
                                                                               },
                                                                               {
                                                                                   OwinConstants.XForwardedFor,
                                                                                   new[] {"example.com"}
                                                                               },
                                                                               {
                                                                                   OwinConstants.XRealIp, 
                                                                                   new[] {"127.0.0.1"}
                                                                               },
                                                                           }
                                      },
                                      {OwinConstants.RequestPathBaseKey, "/pathbase"},
                                      {OwinConstants.RequestPathKey, "/path"},
                                      {OwinConstants.RequestQueryStringKey, "key=value"},
                                      {OwinConstants.RequestMethodKey, "POST"},
                                      {OwinConstants.ServerRemoteIpAddressKey, "127.0.0.1"},
                                      {OwinConstants.ServerRemotePortKey, "12345"},
                                  };
            var container = new Container();
            container.Register<IFoo>(c => new Foo());
            _sut = new OwinHttpRequest("op", owinEnvironment, container);
        }

        private OwinHttpRequest _sut;

        [Test]
        public void Can_get_ApplicationFilePath()
        {
            Assert.AreEqual(Environment.CurrentDirectory, _sut.ApplicationFilePath);
        }

        [Test]
        public void Can_get_Headers()
        {
            Assert.NotNull(_sut.Headers);
        }

        [Test]
        public void Can_get_PathInfo()
        {
            Assert.AreEqual("/path", _sut.PathInfo);
        }

        [Test]
        public void Can_get_UserHostAddress()
        {
            Assert.AreEqual("127.0.0.1:12345", _sut.UserHostAddress);
        }

        [Test]
        public void Can_get_content_length()
        {
            Assert.AreEqual(1024, _sut.ContentLength);
        }

        [Test]
        public void Can_get_content_type()
        {
            Assert.AreEqual("application/xml", _sut.ContentType);
        }

        [Test]
        public void Can_get_cookie_count()
        {
            Assert.AreEqual(3, _sut.Cookies.Count);
        }

        [Test]
        public void Can_get_cookie()
        {
            var cookie = _sut.Cookies["cookie2"];
            Assert.IsNotNull(cookie);
            Assert.AreEqual("value2", cookie.Value);
        }

        [Test]
        public void Can_get_is_secure_connection()
        {
            Assert.True(_sut.IsSecureConnection);
        }

        [Test]
        public void Can_get_operation_name()
        {
            Assert.NotNull(_sut.OperationName);
        }

        [Test]
        public void Can_get_raw_url()
        {
            Assert.AreEqual("/pathbase/path?key=value", _sut.RawUrl);
        }

        [Test]
        public void Can_get_remote_ip()
        {
            Assert.AreEqual("127.0.0.1", _sut.RemoteIp);
        }

        [Test]
        public void Can_get_request_method()
        {
            Assert.AreEqual("POST", _sut.HttpMethod);
        }

        [Test]
        public void Can_get_user_agent()
        {
            Assert.AreEqual("UnitTest/1.0 (en-us) Unknown/0.0", _sut.UserAgent);
        }

        [Test]
        public void Can_get_x_forwarded_for()
        {
            Assert.AreEqual("example.com", _sut.XForwardedFor);
        }

        [Test]
        public void Can_get_x_real_ip()
        {
            Assert.AreEqual("127.0.0.1", _sut.XRealIp);
        }

        [Test]
        public void Can_resolve_service()
        {
            Assert.NotNull(_sut.TryResolve<IFoo>());
        }

        [Test]
        public void When_create_with_null_container_Then_should_throw()
        {
            Assert.Throws<ArgumentNullException>(() => new OwinHttpRequest("op", new Dictionary<string, object>(), null));
        }

        [Test]
        public void When_create_with_null_environment_Then_should_throw()
        {
            Assert.Throws<ArgumentNullException>(() => new OwinHttpRequest("op", null, new Container()));
        }

        [Test]
        public void When_create_with_null_operation_name_Then_should_throw()
        {
            Assert.Throws<ArgumentNullException>(
                () => new OwinHttpRequest(null, new Dictionary<string, object>(), new Container()));
        }
    }
}