namespace ServiceStack.Owin.Tests.HelloService
{
    using Funq;
    using WebHost.Endpoints;

    public class HelloAppHost : AppHostBase
    {
        public HelloAppHost() : base("Hello Web Services", typeof (HelloService).Assembly) {}

        public override void Configure(Container container)
        {
            Routes
                .Add<Hello>("/hello")
                .Add<Hello>("/hello/{Name}");
        }
    }
}