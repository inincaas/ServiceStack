namespace ServiceStack.Owin.Tests.Infrastructure
{
    using System.Collections.Generic;
    using ServiceStack.Owin.Infrasctructure;
    using NUnit.Framework;

    [TestFixture]
    public class OwinEnvironmentExtensionsTests
    {
        private readonly IDictionary<string, object> _owinEnvironment;

        public OwinEnvironmentExtensionsTests()
        {
            _owinEnvironment = new Dictionary<string, object>
                               {
                                   {OwinConstants.RequestSchemeKey, "http"},
                                   {
                                       OwinConstants.RequestHeadersKey, new Dictionary<string, string[]>
                                                                        {
                                                                            {
                                                                                OwinConstants.HostHeader,
                                                                                new[] {"localhost"}
                                                                            }
                                                                        }
                                   },
                                   {OwinConstants.RequestPathBaseKey, "/pathbase"},
                                   {OwinConstants.RequestPathKey, "/path"},
                                   {OwinConstants.RequestQueryStringKey, "key=value"},
                               };
        }

        [Test]
        public void Can_get_header()
        {
            Assert.AreEqual("localhost", _owinEnvironment.GetRequestHeader(OwinConstants.HostHeader));
        }

        [Test]
        public void Can_get_request_scheme()
        {
            Assert.AreEqual("http", _owinEnvironment.Get<string>(OwinConstants.RequestSchemeKey));
        }

        [Test]
        public void Can_get_request_uri()
        {
            Assert.AreEqual("http://localhost/pathbase/path?key=value", _owinEnvironment.GetRequestUri().ToString());
        }
    }
}