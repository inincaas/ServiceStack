namespace ServiceStack.Owin.Tests
{
    using System;
    using Microsoft.Owin.Hosting;
    using Microsoft.Owin.Hosting.Services;
    using NUnit.Framework;
    using ServiceClient.Web;
    using WebHost.Endpoints;
    using global::Owin;

    public abstract class ServiceClientOwinTestBase : IDisposable
    {
        private const string BaseUrl = "http://127.0.0.1:8083/";
        private const string ListenUrl = "http://*:8083/";
        private AppHostBase _appHost;
        private IDisposable _server;

        public void Dispose()
        {
            _server.Dispose();
            if (_appHost == null)
            {
                return;
            }
            _appHost.Dispose();
            _appHost = null;
        }

        public abstract AppHostBase CreateAppHost();

        [TestFixtureSetUp]
        public virtual void TestFixtureSetUp()
        {
            _appHost = CreateAppHost();
            _appHost.Init();
            IServiceProvider serviceProvider = DefaultServices.Create(p => p.AddInstance<AppHostBase>(_appHost));
            _server = WebApplication.Start<Startup>(url: ListenUrl, services: serviceProvider);
        }

        public class Startup
        {
            private readonly AppHostBase _appHost;

            public Startup(AppHostBase appHost)
            {
                _appHost = appHost;
            }

            public void Configuration(IAppBuilder appBuilder)
            {
                appBuilder.UseServiceStack(_appHost);
            }
        }

        [TestFixtureTearDown]
        public void OnTestFixtureTearDown()
        {
            Dispose();
        }

        public void Send<TRes>(object request, Action<TRes> validate)
        {
            using (var xmlClient = new XmlServiceClient(BaseUrl))
            {
                using (var jsonClient = new JsonServiceClient(BaseUrl))
                {
                    using (var jsvClient = new JsvServiceClient(BaseUrl))
                    {
                        var xmlResponse = xmlClient.Send<TRes>(request);
                        validate(xmlResponse);

                        var jsonResponse = jsonClient.Send<TRes>(request);
                        validate(jsonResponse);

                        var jsvResponse = jsvClient.Send<TRes>(request);
                        validate(jsvResponse);
                    }
                }
            }
        }
    }
}