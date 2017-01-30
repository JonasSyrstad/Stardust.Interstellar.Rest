using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stardust.Interstellar.Rest.Annotations.Messaging;
using Stardust.Interstellar.Rest.Client;
using Stardust.Interstellar.Rest.Client.CircuitBreaker;
using Stardust.Interstellar.Rest.Common;
using Stardust.Interstellar.Rest.Legacy;
using Stardust.Interstellar.Rest.Service;
using Stardust.Nucleus;
using Stardust.Particles;
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
            Logging.SetLogger(Logger.Initialize(output));
            ExtensionsFactory.SetXmlSerializer(typeof(CustomXmlSerializer));
        }

        [Fact]
        public async Task GeneratorTest()

        {
            var service = ProxyFactory.CreateInstance<ITestApi>("http://localhost/Stardust.Interstellar.Test/");

            var res = await service.ApplyAsync("101", "Stardust", "Hello", "World");
            output.WriteLine(res.Value);

            res = await service.Apply2("101?20", "Stardust", "Hello");
            output?.WriteLine(res.Value);

            await service.PutAsync("test", DateTime.Today);
            output?.WriteLine("Put was successfull");
        }

        [Fact]
        public async Task ProxyWithCustomSerializerSettirngTest()

        {
            var actual = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                NullValueHandling = NullValueHandling.Include,
                Formatting = Formatting.Indented
            }.AddClientSerializer<StringWrapper>();
            var retreived = JsonSerializerExtensions.GetClientSerializationSettings(typeof(StringWrapper));
            Assert.Equal(actual, retreived);
        }

        [Fact]
        public void TestMessageExtensions()
        {
            var msg = new StringWrapper {Value = "Test"};
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
            var retried = false;
            for (var i = 0; i < 1000; i++)
            {
                var service = ProxyFactory.CreateInstance<ITestApi>("http://localhost/Stardust.Interstellar.Test/",
                    extras =>
                    {
                        foreach (var extra in extras)
                        {
                            output.WriteLine($"{extra.Key}:{extra.Value}");
                        }
                        if (extras.ContainsKey("retryCount")) retried = true;
                    });
                try
                {
                    var res = await service.ApplyAsync(i.ToString(), "Stardust", "Hello", "World");
                    output.WriteLine(res.Value);
                }
                catch (Exception ex)
                {
                    Assert.IsType<SuspendedDependencyException>(ex.InnerException);
                }
            }
            //Assert.True(retried);

            await Task.Delay(TimeSpan.FromMinutes(1));
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
            var putRes = testProxy.TestImplementationPut("hello", new StringWrapper {Value = "hell"});
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
                        new {TimeStamp = DateTime.UtcNow, Name = "UnitTest2"},
                        new {TimeStamp = DateTime.UtcNow, Name = "UnitTest3"},
                        new {TimeStamp = DateTime.UtcNow, Name = "UnitTest4"}
                    }
                },
                {
                    "collection2",
                    new List<object>
                    {
                        new {TimeStamp2 = DateTime.UtcNow, Name2 = "UnitTest2"},
                        new {TimeStamp2 = DateTime.UtcNow, Name2 = "UnitTest3"},
                        new {TimeStamp2 = DateTime.UtcNow, Name2 = "UnitTest4"}
                    }
                }
            };
            var putRes = testProxy.TestImplementationPut2("hello", envents);
            output.WriteLine(putRes.Value);
        }

        [Fact]
        public void CircuitBreakerTest()
        {
            CircuitBreakerContainer.Register<DummyExternaDependency>(10, 1);
            var dummy = new DummyExternaDependency();
            var result = dummy.ExecuteWithCircuitBreaker("", s => s.DoWork("hi"));
            output.WriteLine(result);
        }

        [Fact]
        public void CircuitBreakerFailTest()
        {
            CircuitBreakerContainer.Register<DummyExternaDependencyImpl>(10, 1);
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    var dummy = new DummyExternaDependencyImpl();
                    var result = dummy.ExecuteWithCircuitBreaker("", s => s.DoWorkFail("hi"));
                    output.WriteLine(result);
                }
                catch (Exception ex)
                {
                    output.WriteLine(ex.Message);
                    if (i > 20) Assert.IsType<SuspendedDependencyException>(ex);
                }
            }
        }
    }

    public class DummyExternaDependency
    {
        public string DoWork(string name)
        {
            return name;
        }

        public string DoWorkFail(string hi)
        {
            throw new NotImplementedException("NotImplemented");
        }
    }

    class DummyExternaDependencyImpl : DummyExternaDependency
    {
    }

    public class Logger : ILogging
    {
        private static ITestOutputHelper _output;


        public static Type Initialize(ITestOutputHelper output)
        {
            _output = output;
            return typeof(Logger);
        }

        public void Exception(Exception exceptionToLog, string additionalDebugInformation = null)
        {
            _output.WriteLine(exceptionToLog.Message);
        }

        public void HeartBeat()
        {
        }

        public void DebugMessage(string message, EventLogEntryType entryType = EventLogEntryType.Information,
            string additionalDebugInformation = null)
        {
            _output.WriteLine(message);
        }

        public void SetCommonProperties(string logName)
        {
        }
    }
}