namespace ServiceStack.Owin.Tests.HelloService
{
    using Microsoft.Owin.Hosting;
    using NUnit.Framework;
    using ServiceClient.Web;
    using global::Owin;

    public class HelloHostTests
    {
        [Test]
        public void Can_host_hello_service()
        {
            using (WebApplication.Start<Startup>(port: 8080))
            {
                using (var client = new XmlServiceClient("http://localhost:8080"))
                {
                    var response = client.Send<HelloResponse>(new Hello {Name = "Damian"});
                    Assert.AreEqual("Hello, Damian", response.Result);
                }
            }
        }

        public class Startup
        {
            public void Configuration(IAppBuilder appBuilder)
            {
                var helloOwinHost = new HelloAppHost();
                helloOwinHost.Init();
                appBuilder.UseServiceStack(helloOwinHost);
            }
        }
    }
}