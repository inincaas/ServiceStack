namespace ServiceStack.Owin.Tests
{
    using NUnit.Framework;
    using WebHost.Endpoints;
    using WebHost.Endpoints.Tests.Support.Operations;

    [TestFixture]
    public class ServiceClientOwinTests
        : ServiceClientOwinTestBase
    {
        /// <summary>
        ///     These tests require admin privillages
        /// </summary>
        /// <returns></returns>
        public override AppHostBase CreateAppHost()
        {
            return new TestAppHost();
        }

        [Test]
        public void Can_GetCustomers()
        {
            var request = new GetCustomer {CustomerId = 5};

            Send<GetCustomerResponse>(request, 
                                      response => Assert.That(response.Customer.Id, Is.EqualTo(request.CustomerId)));
        }
    }
}