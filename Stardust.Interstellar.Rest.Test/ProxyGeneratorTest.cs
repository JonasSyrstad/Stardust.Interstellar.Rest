using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stardust.Interstellar.Rest.Annotations.Messaging;
using Stardust.Interstellar.Rest.Client;
using Stardust.Interstellar.Rest.Common;
using Stardust.Interstellar.Rest.Legacy;
using Stardust.Interstellar.Rest.Service;
using Stardust.Nucleus;
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
            Resolver.LoadModuleConfiguration<TestBlueprint>();
            ExtensionsFactory.SetXmlSerializer(typeof(CustomXmlSerializer));
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
        public void TestMessageExtensions()
        {
            var msg = new StringWrapper { Value = "Test" };
            var service = ProxyFactory.CreateInstance<ITestExtendableApi>("http://localhost/Stardust.Interstellar.Test/");
            service.SetGlobalProperty("test", "test");
            service.Test(msg);
        }

        [Fact]
        public async Task UseXmlTest()

        {
            var service = ProxyFactory.CreateInstance<ITestApi>("http://localhost/Stardust.Interstellar.Test/");
            var res = await service.Apply2("101", "Stardust", "Hello");
            output.WriteLine(res.Value);
            await service.PutAsync("test", DateTime.Today);
            output.WriteLine("Put was successfull");
        }

        [Fact]
        public async void ErrorHandlerTest()
        {
            var service = ProxyFactory.CreateInstance<ITestApi>("http://localhost/Stardust.Interstellar.Test/");
            try
            {
                await service.FailingAction("1", DateTime.Now.ToString());
            }
            catch (RestWrapperException ex)
            {
                var error = ex.Message;
                output.WriteLine(error);
                Assert.True(error == "Bad Gateway");
            }

        }

        [Fact]
        public async Task GetOptions()
        {

            var service = ProxyFactory.CreateInstance<ITestApi>("http://localhost/Stardust.Interstellar.Test/");
            var options = await service.GetOptions();
            Assert.Equal(4, options.Count);
        }

        [Fact]
        public async Task GetHead()
        {

            var service = ProxyFactory.CreateInstance<ITestApi>("http://localhost/Stardust.Interstellar.Test/");
            await service.GetHead();

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
            testProxy.SetGlobalProperty("test", "test");
            var putRes = testProxy.TestImplementationPut("hello", new StringWrapper { Value = "hell" });
            output.WriteLine(putRes.Value);

        }

        [Fact]
        public async Task WcfWrapperTest2()
        {
            var testProxy = ProxyFactory.CreateInstance<IWcfWrapper>("http://localhost/Stardust.Interstellar.Test",
                extras =>
                {
                    foreach (var extra in extras)
                    {
                        output.WriteLine($"{extra.Key}:{extra.Value}");
                    }
                });

            var getRes = testProxy.TestImplementationGet("test");
            output.WriteLine(getRes.Value);
            testProxy.SetGlobalProperty("test", "test");
            var envents = new Dictionary<string, IEnumerable<object>>
                              {
                                  {
                                      "collection1",
                                      new List<object>
                                          {
                                              new { TimeStamp = DateTime.UtcNow, Name = "UnitTest2" },
                                              new { TimeStamp = DateTime.UtcNow, Name = "UnitTest3" },
                                              new { TimeStamp = DateTime.UtcNow, Name = "UnitTest4" }
                                          }
                                  },
                                  {
                                      "collection2",
                                      new List<object>
                                          {
                                              new { TimeStamp2 = DateTime.UtcNow, Name2 = "UnitTest2" },
                                              new { TimeStamp2 = DateTime.UtcNow, Name2 = "UnitTest3" },
                                              new { TimeStamp2 = DateTime.UtcNow, Name2 = "UnitTest4" }
                                          }
                                  }
                              };
            var putRes = testProxy.TestImplementationPut2("hello", envents);
            output.WriteLine(putRes.Value);

        }
    }
}