namespace ServiceStack.Owin.Tests
{
    using Funq;
    using WebHost.Endpoints;
    using WebHost.Endpoints.Tests.Support.Host;
    using WebHost.Endpoints.Tests.Support.Services;

    public class TestAppHost : AppHostBase
    {
        public TestAppHost()
            : base("Example Service", typeof (TestService).Assembly)
        {
            Instance = null;
        }

        public override void Configure(Container container)
        {
            container.Register<IFoo>(c => new Foo());
        }
    }
}