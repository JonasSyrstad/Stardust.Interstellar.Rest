using System;
using System.Threading.Tasks;
using Stardust.Interstellar.Rest.Client;
using Stardust.Interstellar.Rest.Legacy;
using Stardust.Interstellar.Rest.Service;
using Xunit;
using Xunit.Abstractions;

namespace Stardust.Interstellar.Rest.Test
{
    public class ProxyGeneratorTest
    {
        private readonly ITestOutputHelper output;

        public ProxyGeneratorTest(ITestOutputHelper output)
        {
            this.output = output;
            WcfServiceProvider.RegisterWcfAdapters();
        }
        [Fact]
        public async Task GeneratorTest()

        {
            var service = ProxyFactory.CreateInstance<ITestApi>("http://localhost/Stardust.Interstellar.Test/");
            var res = await service.ApplyAsync("101", "Stardust", "Hello", "World");
            output.WriteLine(res.Value);
            await service.PutAsync("test", DateTime.Today);
            output.WriteLine("Put was successfull");
        }

        [Fact]
        public async Task GeneratorPerfTest()
        {
            for (var i = 0; i < 1000; i++)
            {
                var service = ProxyFactory.CreateInstance<ITestApi>("http://localhost/Stardust.Interstellar.Test/",
                    extras =>
                        {
                            foreach (var extra in extras)
                            {
                                output.WriteLine($"{extra.Key}:{extra.Value}");
                            }
                        });
                try
                {
                    var res = await service.ApplyAsync(i.ToString(), "Stardust", "Hello", "World");
                    output.WriteLine(res.Value);
                }
                catch (Exception ex)
                {
                    throw;
                } 
            }
           
        }

        [Fact]
        public void ImplementationBuilderTest()
        {
            var testType = ServiceFactory.CreateServiceImplementation<ITestApi>();
            ServiceFactory.FinalizeRegistration();
            Assert.NotNull(testType);
            Assert.True(typeof(ServiceWrapperBase<ITestApi>).IsAssignableFrom(testType));
        }

        [Fact]
        public async Task WcfWrapperTest()
        {
            var testProxy = ProxyFactory.CreateInstance<IWcfWrapper>("http://localhost/Stardust.Interstellar.Test/",
                extras =>
                    {
                        foreach (var extra in extras)
                        {
                            output.WriteLine($"{extra.Key}:{extra.Value}");
                        }
                    });
            var getRes = testProxy.TestImplementationGet("test");
            output.WriteLine(getRes.Value);
            var putRes = testProxy.TestImplementationPut("hello", new StringWrapper { Value = "hell" });
            output.WriteLine(putRes.Value);

        }
    }
}