namespace ServiceStack.Owin.Tests.HelloService
{
    using ServiceHost;

    public class HelloService : IService<Hello>
    {
        public object Execute(Hello request)
        {
            return new HelloResponse {Result = "Hello, " + request.Name};
        }
    }
}