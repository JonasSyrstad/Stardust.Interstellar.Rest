using System;
using System.Linq;
using System.Threading.Tasks;
using Stardust.Interstellar.Rest.Client;
using Stardust.Interstellar.Rest.Legacy;
using Xunit;
using Xunit.Abstractions;

namespace Stardust.Interstellar.Rest.Test
{
    public class GraphServiceProxyTest
    {
        private ITestOutputHelper output;

        public GraphServiceProxyTest(ITestOutputHelper output)
        {
            this.output = output;
            WcfServiceProvider.RegisterWcfAdapters();
        }

        [Fact]
        public async Task GetAllEmployees()
        {
            var graphService = new GraphTestApi("http://localhost/Stardust.Interstellar.Test/");
            var emp = graphService.Employees.ToList();
            Assert.Equal(3, emp.Count);
        }

        [Fact]
        public async Task GetMe()
        {
            var graphService = new GraphTestApi("http://localhost/Stardust.Interstellar.Test/");
            var me = await graphService.Employees.GetAsync("jonassyrstad@outlook.com");
            Assert.NotNull(me);
            var me2 = graphService.Employees["jonassyrstad@outlook.com"];
            Assert.Equal(me.Email, me2.Email);
            Assert.Equal(me.Name, me2.Name);
            Assert.Equal(me.ManagerId, me2.ManagerId);
            Assert.NotNull(await graphService.Me.GetAsync());
        }

        [Fact]
        public async Task GetMe2()
        {
            var graphService = new GraphTestApi("http://localhost/Stardust.Interstellar.Test/");

            Assert.NotNull(graphService.Me.Value);
        }


        [Fact]
        public async Task GetMyManager()
        {
            var graphService = new GraphTestApi("http://localhost/Stardust.Interstellar.Test/");
            var me = await graphService.Employees.GetAsync("jonassyrstad@outlook.com");
            Assert.NotNull(me);
            var myManager = me.Manager;
            Assert.NotNull(myManager);
        }

        [Fact]
        public async Task GetMyColleagues()
        {
            var graphService = new GraphTestApi("http://localhost/Stardust.Interstellar.Test/");
            var me =  graphService.Me;
            Assert.NotNull(me);
            var myColleagues = (await me.GetAsync()).Colleagues.ToList();
            Assert.Equal(2, myColleagues.Count);
        }

        [Fact]
        public async Task AddDeleteEmployeeManager()
        {
            var graphService = new GraphTestApi("http://localhost/Stardust.Interstellar.Test/");
            await graphService.Employees.AddAsync(new Employee { Email = "testInsert", Name = "testInsert" });
            var newUser = graphService.Employees["testInsert"];
            await graphService.Employees.DeleteAsync("testInsert");
            try
            {
                var user = graphService.Employees["testInsert"];
            }
            catch (AggregateException agex)
            {
                Assert.IsType<RestWrapperException>(agex.InnerException);
            }
            catch (Exception ex)
            {
                Assert.IsType<RestWrapperException>(ex);
            }

        }
    }
}