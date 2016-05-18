using System;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Stardust.Interstellar.Rest.Legacy;
using Xunit;
using Xunit.Abstractions;

namespace Stardust.Interstellar.Rest.Test
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
    public sealed class CallingMachineNameAttribute : HeaderInspectorAttributeBase
    {
        public override IHeaderHandler[] GetHandlers()
        {
            return new IHeaderHandler[] { new CallingMachineNameHandler() };
        }
    }

    public class CallingMachineNameHandler : IHeaderHandler
    {
        public void SetHeader(HttpWebRequest req)
        {
            req.Headers.Add("x-callingMachine", Environment.MachineName);
        }

        public void GetHeader(HttpWebResponse response)
        {

        }

        public void SetServiceHeaders(WebHeaderCollection headers)
        {

        }

        public void GetServiceHeader(HttpRequestHeaders headers)
        {

        }

        public void SetServiceHeaders(HttpResponseHeaders headers)
        {
            
        }
    }

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
            try
            {
                var res = await service.ApplyAsync("101", "Stardust", "Hello", "World");
                output.WriteLine(res.Value);
            }
            catch (Exception ex)
            {
                throw;
            }
            try
            {
                await service.PutAsync("test", DateTime.Today);
                output.WriteLine("Put was successfull");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Fact]
        public async Task GeneratorPerfTest()
        {
            for (int i = 0; i < 1000; i++)
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
            var testType = ServiceWrapper.ServiceFactory.CreateServiceImplementation<ITestApi>();
            ServiceWrapper.ServiceFactory.FinalizeRegistration();
            Assert.NotNull(testType);
            Assert.True(typeof(ServiceWrapper.ServiceWrapperBase<ITestApi>).IsAssignableFrom(testType));
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