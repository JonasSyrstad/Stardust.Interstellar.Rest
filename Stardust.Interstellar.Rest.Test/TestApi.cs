using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Stardust.Interstellar.Rest.Service;

namespace Stardust.Interstellar.Rest.Test
{
    [RoutePrefix("api")]
    public class TestApi : ServiceWrapperBase<ITestApi>
    {
        public TestApi(ITestApi implementation)
            : base(implementation)
        {
            //var project = VisualStudioHelper.CurrentProject;
            //// get all class items from the code model

            //// iterate all classes
            //foreach (VSLangProj.Reference reference in vsProject.References)
            //{
            //    // get all interfaces implemented by this class
            //    var assembly = Assembly.LoadFile(reference.Path) as System.Reflection.Assembly;
            //    try
            //    {
            //        foreach (Type t in assembly.GetTypes())
            //        {
            //            if (t.IsInterface)
            //            {
            //                var marker =
            //                    t.GetCustomAttributes(true)
            //                        .Where(
            //                            tt =>
            //                                tt.GetType().FullName ==
            //                                "Stardust.Interstellar.Rest.Annotations.IRoutePrefixAttribute")
            //                        .SingleOrDefault();
            //                if (marker != null)
            //                {
            //                    try
            //                    {
            //                        //public class t.Name.Remove(0,1)
            //                        foreach (System.Reflection.MethodInfo item in t.GetMethods())
            //                        {
            //                            //	#>
            //                            //	public class <#= t.Name.Remove(0,1) #> : <#= t.Name #>
            //                            //	{
            //                            //}

            //                            //	<#
            //                        }
            //                    }
            //                    catch (Exception ex)
            //                    {
            //                    }
            //                }
            //            }
            //        }
            //    }

            //    catch (Exception ex)
            //    {

            //    }


            //}
        }

        [Route("test1/{id}", Name = "Apply1", Order = 1)]
        [HttpGet]
        [ResponseType(typeof(string))]
        public HttpResponseMessage Apply1([FromUri] string id, [FromUri] string name)
        {
            try
            {
                var parameters = new object[] { id, name };
                var serviceParameters = GatherParameters("Apply1", parameters);
                var result = base.implementation.Apply1((string)serviceParameters[0].value, (string)serviceParameters[1].value);
                return base.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(ex);
            }
        }

        [Route("test1/{id}")]
        [HttpGet]
        public Task<HttpResponseMessage> Apply3([FromUri] string id, [FromUri] string name, [FromUri] string item3, [FromUri] string item4)
        {

            try
            {
                var parameters = new object[] { id, name, item3, item4 };
                var serviceParameters = GatherParameters("Apply3", parameters);
                return base.ExecuteMethodAsync(
                    delegate { return base.implementation.ApplyAsync((string)serviceParameters[0].value, (string)serviceParameters[1].value, (string)serviceParameters[2].value, (string)serviceParameters[3].value); });
                //    var result = base.implementation.ApplyAsync((string)serviceParameters[0].value, (string)serviceParameters[1].value, (string)serviceParameters[2].value, (string)serviceParameters[3].value);
                //    return base.CreateResponseAsync(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Task.FromResult(CreateErrorResponse(ex));
            }
        }


        [Route("test/{id}")]
        [HttpPut]
        public Task<HttpResponseMessage> PutAsync([FromUri] string id, [FromBody] DateTime timestamp)
        {

            try
            {
                var parameters = new object[] { id, timestamp };
                var serviceParameters = GatherParameters("Put", parameters);
                return base.ExecuteMethodVoidAsync(delegate { return base.implementation.PutAsync((string)serviceParameters[0].value, serviceParameters[1].value.ToString()); });
                //    var result = base.implementation.ApplyAsync((string)serviceParameters[0].value, (string)serviceParameters[1].value, (string)serviceParameters[2].value, (string)serviceParameters[3].value);
                //    return base.CreateResponseAsync(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Task.FromResult(CreateErrorResponse(ex));
            }
            try
            {
                var parameters = new object[] { id, timestamp };
                var serviceParameters = GatherParameters("PutAsync", parameters);
                var result = base.implementation.PutAsync((string)serviceParameters[0].value, serviceParameters[1].value.ToString());
                return base.CreateResponseVoidAsync(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Task.FromResult(CreateErrorResponse(ex));
            }
        }

        [Route("test/{id}")]
        [HttpPut]
        public HttpResponseMessage Put([FromUri] string id, [FromBody] DateTime timestamp)
        {
            try
            {
                var parameters = new object[] { id, timestamp };
                var serviceParameters = GatherParameters("Put", parameters);
                implementation.Put((string)serviceParameters[0].value, (DateTime)serviceParameters[1].value);
                return CreateResponse<object>(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(ex);
            }
        }
    }
}