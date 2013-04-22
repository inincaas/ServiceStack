using System.IO;

namespace ServiceStack.Owin.Tests.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using Funq;
    using NUnit.Framework;
    using Owin.Infrastructure;
    using ServiceStack.WebHost.Endpoints.Tests.Support.Host;

    [TestFixture]
    public class OwinHttpRequestFormTests
    {
        [SetUp]
        public void SetUp()
        {
            var body = "q=search_term&setting1=value1";
            var bodyStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(body));

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
                                                                                   OwinConstants.ContentTypeHeader,
                                                                                   new[] {"application/x-www-form-urlencoded"}
                                                                               },
                                                                               {
                                                                                   OwinConstants.ContentLengthHeader,
                                                                                   new[] {bodyStream.Length.ToString()}
                                                                               },
                                                                               {
                                                                                   OwinConstants.AcceptHeader,
                                                                                   new[] {"text/html"}
                                                                               },
                                                                           }
                                      },
                                      {OwinConstants.RequestPathBaseKey, "/pathbase"},
                                      {OwinConstants.RequestPathKey, "/path"},
                                      {OwinConstants.RequestQueryStringKey, "key=value"},
                                      {OwinConstants.RequestMethodKey, "POST"},
                                      {OwinConstants.ServerRemoteIpAddressKey, "127.0.0.1"},
                                      {OwinConstants.ServerRemotePortKey, "12345"},
                                      {OwinConstants.RequestBodyKey, bodyStream},
                                  };
            var container = new Container();
            _sut = new OwinHttpRequest("op", owinEnvironment, container);
        }

        private OwinHttpRequest _sut;

        [Test]
        public void Can_get_form_data()
        {
            Assert.IsNotNull(_sut.FormData);
        }

        [Test]
        public void Can_get_form_values()
        {
            Assert.AreEqual("value1", _sut.FormData["setting1"]);
        }
    }
}