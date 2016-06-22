using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Common;

namespace Stardust.Interstellar.Rest.Service
{
    class ServiceBuilder
    {
        private AssemblyBuilder myAssemblyBuilder;

        private ModuleBuilder myModuleBuilder;

        public ServiceBuilder()
        {
            var myCurrentDomain = AppDomain.CurrentDomain;
            var myAssemblyName = new AssemblyName();
            myAssemblyName.Name = Guid.NewGuid().ToString().Replace("-", "") + "_ServiceWrapper";
            myAssemblyBuilder = myCurrentDomain.DefineDynamicAssembly(myAssemblyName, AssemblyBuilderAccess.RunAndSave);
            myModuleBuilder = myAssemblyBuilder.DefineDynamicModule("TempModule", "service.dll");
        }

        public Type CreateServiceImplementation<T>()
        {
            return CreateServiceImplementation(typeof(T));
        }

        public Type CreateServiceImplementation(Type interfaceType)
        {
            try
            {
                ExtensionsFactory.GetService<ILogger>()?.Message("Generating webapi controller for {0}",interfaceType.FullName);
                var type = CreateServiceType(interfaceType);
                ctor(type, interfaceType);
                foreach (var methodInfo in interfaceType.GetMethods().Length == 0 ? interfaceType.GetInterfaces().First().GetMethods() : interfaceType.GetMethods())
                {
                    if (typeof(Task).IsAssignableFrom(methodInfo.ReturnType))
                    {
                        if (methodInfo.ReturnType.GetGenericArguments().Length == 0)
                        {
                            BuildAsyncVoidMethod(type, methodInfo);
                        }
                        else BuildAsyncMethod(type, methodInfo);
                    }
                    else
                    {
                        if (methodInfo.ReturnType == typeof(void)) BuildVoidMethod(type, methodInfo);
                        else BuildMethod(type, methodInfo);
                    }
                }
                return type.CreateType();
            }
            catch (Exception ex)
            {
                ExtensionsFactory.GetService<ILogger>()?.Error(ex);
                ExtensionsFactory.GetService<ILogger>()?.Message("Skipping type: {0}",interfaceType.FullName);
                return null;
            }
        }

        public MethodBuilder InternalMethodBuilder(TypeBuilder type, MethodInfo implementationMethod, Func<MethodInfo, MethodBuilder, Type[], List<ParameterWrapper>, MethodBuilder> bodyBuilder)
        {
            List<ParameterWrapper> methodParams;
            Type[] pTypes;
            var method = DefineMethod(type, implementationMethod, out methodParams, out pTypes);
            return bodyBuilder(implementationMethod, method, pTypes, methodParams);
        }

        public MethodBuilder BuildMethod(TypeBuilder type, MethodInfo implementationMethod)
        {
            return InternalMethodBuilder(type, implementationMethod, BodyImplementer);
        }

        private static MethodBuilder BodyImplementer(MethodInfo implementationMethod, MethodBuilder method, Type[] pTypes, List<ParameterWrapper> methodParams)
        {
            MethodInfo gatherParams = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType)
                .GetMethod("GatherParameters", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(String), typeof(Object[]) }, null);
            var baseType = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType);
            var implementation = baseType.GetRuntimeFields().Single(f => f.Name == "implementation");
            MethodInfo getValue = typeof(ParameterWrapper).GetMethod("get_value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);
            MethodInfo createResponse = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType).GetMethod("CreateResponse", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (implementationMethod.ReturnType == typeof(void)) createResponse = createResponse.MakeGenericMethod(typeof(object));
            else createResponse = createResponse.MakeGenericMethod(implementationMethod.ReturnType);
            MethodInfo method9 = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType)
                .GetMethod("CreateErrorResponse", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(Exception) }, null);



            // Setting return type

            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder parameters = gen.DeclareLocal(typeof(Object[]));
            LocalBuilder serviceParameters = gen.DeclareLocal(typeof(ParameterWrapper[]));
            LocalBuilder result = gen.DeclareLocal(typeof(String));
            LocalBuilder message = gen.DeclareLocal(typeof(HttpResponseMessage));
            LocalBuilder ex = gen.DeclareLocal(typeof(Exception));
            // Preparing labels
            Label label97 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.BeginExceptionBlock();
            gen.Emit(OpCodes.Nop);
            var ps = pTypes;
            EmitHelpers.EmitInt32(gen, ps.Length);
            gen.Emit(OpCodes.Newarr, typeof(object));
            for (int j = 0; j < ps.Length; j++)
            {
                gen.Emit(OpCodes.Dup);
                EmitHelpers.EmitInt32(gen, j);
                EmitHelpers.EmitLdarg(gen, j + 1);
                var paramType = ps[j];
                if (paramType.IsValueType)
                {
                    gen.Emit(OpCodes.Box, paramType);
                }
                gen.Emit(OpCodes.Stelem_Ref);
            }
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, implementationMethod.Name);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Call, gatherParams);
            gen.Emit(OpCodes.Stloc_1);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, implementation);
            int iii = 0;
            foreach (var parameterWrapper in methodParams)
            {
                gen.Emit(OpCodes.Ldloc_1);
                EmitHelpers.EmitInt32(gen, iii);
                gen.Emit(OpCodes.Ldelem_Ref);
                gen.Emit(OpCodes.Callvirt, getValue);
                if (parameterWrapper.Type.IsValueType) gen.Emit(OpCodes.Unbox_Any, parameterWrapper.Type);
                else gen.Emit(OpCodes.Castclass, parameterWrapper.Type);
                iii++;
            }
            gen.Emit(OpCodes.Callvirt, implementationMethod);
            gen.Emit(OpCodes.Stloc_2);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldc_I4, 200);
            if (implementationMethod.ReturnType != typeof(void)) gen.Emit(OpCodes.Ldloc_2);
            gen.Emit(OpCodes.Call, createResponse);
            gen.Emit(OpCodes.Stloc_3);
            gen.Emit(OpCodes.Leave_S, label97);
            gen.BeginCatchBlock(typeof(Exception));
            gen.Emit(OpCodes.Stloc_S, 4);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldloc_S, 4);
            gen.Emit(OpCodes.Call, method9);
            gen.Emit(OpCodes.Stloc_3);
            gen.Emit(OpCodes.Leave_S, label97);
            gen.EndExceptionBlock();
            gen.MarkLabel(label97);
            gen.Emit(OpCodes.Ldloc_3);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;
        }

        private static MethodBuilder DefineMethod(TypeBuilder type, MethodInfo implementationMethod, out List<ParameterWrapper> methodParams, out Type[] pTypes)
        {
            // Declaring method builder
            // Method attributes
            const MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
            var method = type.DefineMethod(implementationMethod.Name, methodAttributes);
            // Preparing Reflection instances


            var route = typeof(RouteAttribute).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(String) }, null);
            var httpGet = httpMethodAttribute(implementationMethod);
            var uriAttrib = typeof(FromUriAttribute).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);
            var bodyAttrib = typeof(FromBodyAttribute).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);
            if (typeof(Task).IsAssignableFrom(implementationMethod.ReturnType))
                method.SetReturnType(typeof(Task<HttpResponseMessage>));
            else method.SetReturnType(typeof(HttpResponseMessage));
            // Adding parameters
            methodParams = GetMethodParams(implementationMethod);
            pTypes = methodParams.Where(p => p.In != InclutionTypes.Header).Select(p => p.Type).ToArray();
            method.SetParameters(pTypes.ToArray());
            // Parameter id
            int pid = 1;
            foreach (var parameterWrapper in methodParams.Where(p => p.In != InclutionTypes.Header))
            {
                try
                {
                    var p = method.DefineParameter(pid, ParameterAttributes.None, parameterWrapper.Name);
                    if (parameterWrapper.In == InclutionTypes.Path) p.SetCustomAttribute(new CustomAttributeBuilder(uriAttrib, new Type[] { }));
                    else if (parameterWrapper.In == InclutionTypes.Body) p.SetCustomAttribute(new CustomAttributeBuilder(bodyAttrib, new Type[] { }));
                    pid++;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            // Adding custom attributes to method
            // [RouteAttribute]
            var template = implementationMethod.GetCustomAttribute<RouteAttribute>()?.Template;
            if (template == null) template = ExtensionsFactory.GetServiceTemplate(implementationMethod);
            method.SetCustomAttribute(new CustomAttributeBuilder(route, new[] { template }));
            // [HttpGetAttribute]
            method.SetCustomAttribute(new CustomAttributeBuilder(httpGet, new Type[] { }));
            ConstructorInfo ctor5 = typeof(ResponseTypeAttribute).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(Type) }, null);
            method.SetCustomAttribute(new CustomAttributeBuilder(ctor5, new object[] { GetReturnType(implementationMethod) }));
            return method;
        }

        private static Type GetReturnType(MethodInfo implementationMethod)
        {
            if (typeof(Task).IsAssignableFrom(implementationMethod.ReturnType))
            {
                if (implementationMethod.ReturnType.IsGenericType) return implementationMethod.ReturnType.GenericTypeArguments.FirstOrDefault();
                else return typeof(void);
            }

            return implementationMethod.ReturnType;
        }

        private static List<ParameterWrapper> GetMethodParams(MethodInfo implementationMethod)
        {
            var resolver = ExtensionsFactory.GetService<IServiceParameterResolver>();
            var resolvedParams = resolver?.ResolveParameters(implementationMethod);
            if (resolvedParams != null && resolvedParams.Count() == implementationMethod.GetParameters().Length) return resolvedParams.ToList();
            var methodParams = new List<ParameterWrapper>();
            foreach (var parameterInfo in implementationMethod.GetParameters())
            {
                var @in = parameterInfo.GetCustomAttribute<InAttribute>(true);
                if (@in == null)
                {
                    var fromBody = parameterInfo.GetCustomAttribute<FromBodyAttribute>(true);
                    if (fromBody != null) @in = new InAttribute(InclutionTypes.Body);
                    if (@in == null)
                    {
                        var fromUri = parameterInfo.GetCustomAttribute<FromUriAttribute>(true);
                        if (fromUri != null) @in = new InAttribute(InclutionTypes.Path);
                    }
                }
                methodParams.Add(new ParameterWrapper { Name = parameterInfo.Name, Type = parameterInfo.ParameterType, In = @in?.InclutionType ?? InclutionTypes.Body });
            }
            return methodParams;
        }

        public MethodBuilder BuildVoidMethod(TypeBuilder type, MethodInfo implementationMethod)
        {

            return InternalMethodBuilder(type, implementationMethod, VoidMethodBuilder);
        }

        private MethodBuilder VoidMethodBuilder(MethodInfo implementationMethod, MethodBuilder method, Type[] pTypes, List<ParameterWrapper> methodParams)
        {
            MethodInfo gatherParams = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType).GetMethod(
                "GatherParameters",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                              typeof(String),
                              typeof(Object[])
                          },
                null
                );
            var baseType = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType);
            var implementation = baseType.GetRuntimeFields().Single(f => f.Name == "implementation");
            MethodInfo getValue = typeof(ParameterWrapper).GetMethod(
                "get_value",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                          },
                null
                );
            MethodInfo createResponse = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType).GetMethod("CreateResponse", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (implementationMethod.ReturnType == typeof(void))
                createResponse = createResponse.MakeGenericMethod(typeof(object));
            else
                createResponse = createResponse.MakeGenericMethod(implementationMethod.ReturnType);
            MethodInfo method9 = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType).GetMethod(
                "CreateErrorResponse",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                              typeof(Exception)
                          },
                null
                );
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder parameters = gen.DeclareLocal(typeof(Object[]));
            LocalBuilder serviceParameters = gen.DeclareLocal(typeof(ParameterWrapper[]));
            LocalBuilder message = gen.DeclareLocal(typeof(HttpResponseMessage));
            LocalBuilder ex = gen.DeclareLocal(typeof(Exception));
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.BeginExceptionBlock();
            gen.Emit(OpCodes.Nop);
            var ps = pTypes;
            EmitHelpers.EmitInt32(gen, ps.Length);
            gen.Emit(OpCodes.Newarr, typeof(object));
            for (int j = 0; j < ps.Length; j++)
            {
                gen.Emit(OpCodes.Dup);
                EmitHelpers.EmitInt32(gen, j);
                EmitHelpers.EmitLdarg(gen, j + 1);
                var paramType = ps[j];
                if (paramType.IsValueType)
                {
                    gen.Emit(OpCodes.Box, paramType);
                }
                gen.Emit(OpCodes.Stelem_Ref);

            }
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, implementationMethod.Name);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Call, gatherParams);
            gen.Emit(OpCodes.Stloc_1);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, implementation);
            int iii = 0;
            foreach (var parameterWrapper in methodParams)
            {
                gen.Emit(OpCodes.Ldloc_1);
                EmitHelpers.EmitInt32(gen, iii);//gen.Emit(OpCodes.Ldc_I4_0);
                gen.Emit(OpCodes.Ldelem_Ref);
                gen.Emit(OpCodes.Callvirt, getValue);
                if (parameterWrapper.Type.IsValueType)
                {
                    gen.Emit(OpCodes.Unbox_Any, parameterWrapper.Type);

                }
                else
                    gen.Emit(OpCodes.Castclass, parameterWrapper.Type);
                iii++;
            }
            gen.Emit(OpCodes.Callvirt, implementationMethod);
            if (implementationMethod.ReturnType != typeof(void))
                gen.Emit(OpCodes.Stloc_2);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldc_I4, 200);
            if (implementationMethod.ReturnType != typeof(void))
                gen.Emit(OpCodes.Ldloc_2);
            else { gen.Emit(OpCodes.Ldnull); }

            gen.Emit(OpCodes.Call, createResponse);
            gen.Emit(OpCodes.Stloc_2);
            gen.BeginCatchBlock(typeof(Exception));
            gen.Emit(OpCodes.Stloc_3);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldloc_3);
            gen.Emit(OpCodes.Call, method9);
            gen.Emit(OpCodes.Stloc_2);
            gen.EndExceptionBlock();
            gen.Emit(OpCodes.Ldloc_2);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;
        }

        private static ConstructorInfo httpMethodAttribute(MethodInfo implementationMethod)
        {
            var httpMethod = ExtensionsFactory.GetService<IWebMethodConverter>()?.GetHttpMethods(implementationMethod);
            if (httpMethod != null && httpMethod.Any())
            {
                switch (httpMethod.First().Method.ToUpper())
                {
                    case "Get":
                        return typeof(HttpGetAttribute).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);
                    case "POST":
                        return typeof(HttpPostAttribute).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);
                    case "PUT":
                        return typeof(HttpPutAttribute).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);
                    case "DELETE":
                        return typeof(HttpDeleteAttribute).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);
                }
            }
            var attribs = implementationMethod.GetCustomAttributes().ToList();


            if (attribs.Any(p => p is HttpGetAttribute))
                return typeof(HttpGetAttribute).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);
            if (attribs.Any(p => p is HttpPostAttribute))
                return typeof(HttpPostAttribute).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);
            if (attribs.Any(p => p is HttpPutAttribute))
                return typeof(HttpPutAttribute).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);
            if (attribs.Any(p => p is HttpDeleteAttribute))
                return typeof(HttpDeleteAttribute).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);
            return typeof(HttpGetAttribute).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);

        }

        public MethodBuilder BuildAsyncMethod(TypeBuilder type, MethodInfo implementationMethod)
        {
            return InternalMethodBuilder(type, implementationMethod, (a, b, c, d) => BuildAsyncMethodBody(a, b, c, d, type));
        }
        private static int typeCounter=0;
        private Type CreateDelegate(MethodInfo targetMethod, TypeBuilder parent,List<ParameterWrapper> methodParams)
        {
            typeCounter++;
            var typeBuilder = myModuleBuilder.DefineType( string.Format("TempModule.Controllers.{0}{1}{2}Delegate{3}", targetMethod.DeclaringType.Name, targetMethod.Name, targetMethod.GetParameters().Length,typeCounter),
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass,
                typeof(object));
            //var typeBuilder = myModuleBuilder.DefineType(string.Format("{0}{1}{2}Delegate", targetMethod.DeclaringType.Name, targetMethod.Name, targetMethod.GetParameters().Length), TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass, typeof(object));
            var imp = typeBuilder.DefineField("implementation", parent, FieldAttributes.Public);
            var baseController= typeof(ServiceWrapperBase<>).MakeGenericType(targetMethod.DeclaringType).GetField("implementation", BindingFlags.Public | BindingFlags.NonPublic|BindingFlags.Instance);

            var param = typeBuilder.DefineField("parameters", typeof(ParameterWrapper[]), FieldAttributes.Public);
            CreateDelegateCtor(typeBuilder);

            // Declaring method builder
            // Method attributes
            System.Reflection.MethodAttributes methodAttributes =
                  System.Reflection.MethodAttributes.Assembly
                | System.Reflection.MethodAttributes.HideBySig;
            MethodBuilder method = typeBuilder.DefineMethod(targetMethod.Name, methodAttributes);
            // Preparing Reflection instances
            //FieldInfo field1 = typeof(<> c__DisplayClass2_0).GetField("<>4__this", BindingFlags.Public | BindingFlags.NonPublic);
            //FieldInfo field2 = typeof(Stardust.Interstellar.Rest.Service.ServiceWrapperBase<>).MakeGenericType(typeof(ITestApi)).GetField("implementation", BindingFlags.Public | BindingFlags.NonPublic);
            //FieldInfo field3 = typeof(<> c__DisplayClass2_0).GetField("serviceParameters", BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo method4 = typeof(ParameterWrapper).GetMethod(
                "get_value",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                    },
                null
                );
            MethodInfo method5 = targetMethod;
            // Setting return type
            method.SetReturnType(targetMethod.ReturnType);
            // Adding parameters
            ILGenerator gen = method.GetILGenerator();
            
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, imp);
            gen.Emit(OpCodes.Ldfld, baseController);
            var iii = 0;
            foreach (var item in methodParams)
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, param);
                EmitHelpers.EmitInt32(gen,iii);
                gen.Emit(OpCodes.Ldelem_Ref);
                gen.Emit(OpCodes.Callvirt, method4);
                if (item.Type.IsValueType)
                    gen.Emit(OpCodes.Unbox_Any, item.Type);
                else
                    gen.Emit(OpCodes.Castclass, item.Type);
                iii++;
            }
            gen.Emit(OpCodes.Callvirt, method5);
            gen.Emit(OpCodes.Ret);
            // finished


            var t = typeBuilder.CreateType();
            return t;
        }

        private static void CreateDelegateCtor(TypeBuilder typeBuilder)
        {
            var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig, CallingConventions.Standard | CallingConventions.HasThis, new Type[] { });
            var baseCtor = typeof(object).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);
            var gen = ctor.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, baseCtor);
            gen.Emit(OpCodes.Ret);
        }

        private Type CreateVoidDelegate(MethodInfo targetMethod, TypeBuilder parent, List<ParameterWrapper> methodParams)
        {
            typeCounter++;
            var typeBuilder = myModuleBuilder.DefineType(string.Format("TempModule.Controllers.{0}{1}{2}VoidDelegate{3}", targetMethod.DeclaringType.Name, targetMethod.Name, targetMethod.GetParameters().Length,typeCounter),
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass,
                typeof(object));
            var imp = typeBuilder.DefineField("implementation", parent, FieldAttributes.Public);
            var baseController = typeof(ServiceWrapperBase<>).MakeGenericType(targetMethod.DeclaringType).GetField("implementation", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var param = typeBuilder.DefineField("parameters", typeof(ParameterWrapper[]), FieldAttributes.Public);
            CreateDelegateCtor(typeBuilder);

            // Declaring method builder
            // Method attributes
            var methodAttributes =
                  MethodAttributes.Assembly
                | MethodAttributes.HideBySig;
            MethodBuilder method = typeBuilder.DefineMethod(targetMethod.Name, methodAttributes);
            // Preparing Reflection instances
            MethodInfo method4 = typeof(ParameterWrapper).GetMethod(
                "get_value",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                    },
                null
                );
            MethodInfo method5 = targetMethod;
            // Setting return type
            method.SetReturnType( typeof(Task));
            // Adding parameters
            ILGenerator gen = method.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, imp);
            gen.Emit(OpCodes.Ldfld, baseController);
            var iii = 0;
            foreach (var item in methodParams)
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, param);
                EmitHelpers.EmitInt32(gen, iii);
                gen.Emit(OpCodes.Ldelem_Ref);
                gen.Emit(OpCodes.Callvirt, method4);
                if (item.Type.IsValueType)
                    gen.Emit(OpCodes.Unbox_Any, item.Type);
                else
                    gen.Emit(OpCodes.Castclass, item.Type);
                iii++;
            }
            gen.Emit(OpCodes.Callvirt, method5);
            gen.Emit(OpCodes.Ret);
            // finished


            var t = typeBuilder.CreateType();
            return t;
        }

        private static void CreateVoidDelegateCtor(TypeBuilder typeBuilder)
        {
            var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig, CallingConventions.Standard | CallingConventions.HasThis, new Type[] { });
            var baseCtor = typeof(object).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);
            var gen = ctor.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, baseCtor);
            gen.Emit(OpCodes.Ret);
        }

        private MethodBuilder BuildAsyncMethodBody(MethodInfo implementationMethod, MethodBuilder method, Type[] pTypes, List<ParameterWrapper> methodParams, TypeBuilder parent)
        {
            MethodInfo gatherParams = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType).GetMethod(
                    "GatherParameters",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new Type[]{
                                  typeof(string),
                                  typeof(object[])
                              },
                    null
                    );
            var delegateType = CreateDelegate(implementationMethod, parent,methodParams);
            var delegateCtor = delegateType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] {  }, null);
            var baseType = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType);
            var implementation = baseType.GetRuntimeFields().Single(f => f.Name == "implementation");
            MethodInfo getValue = typeof(ParameterWrapper).GetMethod(
                "get_value",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                          },
                null
                );
            ConstructorInfo ctor9 = typeof(System.Func<>).MakeGenericType(typeof(Task<>).MakeGenericType(implementationMethod.ReturnType.GenericTypeArguments)).GetConstructor(
       BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
       null,
       new Type[]{
            typeof(Object),
            typeof(IntPtr)
           },
       null
       );

            MethodInfo createResponse = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType).GetMethod("CreateResponseAsync", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            //createResponse = createResponse.MakeGenericMethod(implementationMethod.ReturnType.GetGenericArguments());
            MethodInfo method9 = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType).GetMethod(
                "CreateErrorResponse",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                                  typeof(Exception)
                          },
                null
                );
      
            var delegateMethod = delegateType.GetMethod(implementationMethod.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);//.MakeGenericMethod(implementationMethod.ReturnType.GenericTypeArguments);
            var method10 = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType)
                .GetMethod("ExecuteMethodAsync", BindingFlags.Instance | BindingFlags.NonPublic);
           method10= method10.MakeGenericMethod(implementationMethod.ReturnType.GenericTypeArguments);
            FieldInfo field7 = delegateType.GetField("parameters", BindingFlags.Public | BindingFlags.NonPublic|BindingFlags.Instance);
            FieldInfo field5 = delegateType.GetField("implementation", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            MethodInfo fromResult = typeof(Task).GetMethod("FromResult").MakeGenericMethod(typeof(HttpResponseMessage));
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder del = gen.DeclareLocal(delegateType);
            LocalBuilder serviceParameters = gen.DeclareLocal(typeof(object[]));
            LocalBuilder result = gen.DeclareLocal(typeof(Task<>).MakeGenericType(typeof(HttpResponseMessage)));
            LocalBuilder ex = gen.DeclareLocal(typeof(Exception));
            // Preparing labels
            Label label97 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.BeginExceptionBlock();
            gen.Emit(OpCodes.Newobj, delegateCtor);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Stfld,field5 );
            var ps = pTypes;
            EmitHelpers.EmitInt32(gen, ps.Length);
            gen.Emit(OpCodes.Newarr, typeof(object));
            for (int j = 0; j < ps.Length; j++)
            {
                gen.Emit(OpCodes.Dup);
                EmitHelpers.EmitInt32(gen, j);
                EmitHelpers.EmitLdarg(gen, j + 1);
                var paramType = ps[j];
                if (paramType.IsValueType)
                {
                    gen.Emit(OpCodes.Box, paramType);
                }
                gen.Emit(OpCodes.Stelem_Ref);

            }
            gen.Emit(OpCodes.Stloc_1);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, implementationMethod.Name);
            gen.Emit(OpCodes.Ldloc_1);
            gen.Emit(OpCodes.Call, gatherParams);

            gen.Emit(OpCodes.Stfld, field7);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ldftn, delegateMethod);
            gen.Emit(OpCodes.Newobj, ctor9);
            gen.Emit(OpCodes.Call, method10);
            gen.Emit(OpCodes.Stloc_2);
            gen.Emit(OpCodes.Leave_S, label97);
            gen.Emit(OpCodes.Leave_S, label97);
            gen.BeginCatchBlock(typeof(Exception));
            gen.Emit(OpCodes.Stloc_S, 3);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldloc_S, 3);
            gen.Emit(OpCodes.Call, method9);
            gen.Emit(OpCodes.Call, fromResult);
            gen.Emit(OpCodes.Stloc_2);
            gen.Emit(OpCodes.Leave_S, label97);
            gen.EndExceptionBlock();
            gen.MarkLabel(label97);
            gen.Emit(OpCodes.Ldloc_2);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;
        }

        public MethodBuilder BuildAsyncVoidMethod(TypeBuilder type, MethodInfo implementationMethod)
        {
            return InternalMethodBuilder(type, implementationMethod, (a, b, c, d) => BuildAsyncVoidMethodBody(a, b, c, d, type));
        }

        private MethodBuilder BuildAsyncVoidMethodBody(MethodInfo implementationMethod, MethodBuilder method, Type[] pTypes, List<ParameterWrapper> methodParams, TypeBuilder parent)
        {
            MethodInfo gatherParams = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType).GetMethod(
                     "GatherParameters",
                     BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                     null,
                     new Type[]{
                                  typeof(string),
                                  typeof(object[])
                               },
                     null
                     );
            var delegateType = CreateVoidDelegate(implementationMethod, parent, methodParams);
            var delegateCtor = delegateType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);
            var baseType = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType);
            var implementation = baseType.GetRuntimeFields().Single(f => f.Name == "implementation");
            MethodInfo getValue = typeof(ParameterWrapper).GetMethod(
                "get_value",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                          },
                null
                );
            ConstructorInfo ctor9 = typeof(System.Func<>).MakeGenericType(typeof(Task)).GetConstructor(
       BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
       null,
       new Type[]{
            typeof(Object),
            typeof(IntPtr)
           },
       null
       );

            MethodInfo createResponse = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType).GetMethod("CreateResponseAsync", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            //createResponse = createResponse.MakeGenericMethod(implementationMethod.ReturnType.GetGenericArguments());
            MethodInfo method9 = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType).GetMethod(
                "CreateErrorResponse",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                                  typeof(Exception)
                          },
                null
                );

            var delegateMethod = delegateType.GetMethod(implementationMethod.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);//.MakeGenericMethod(implementationMethod.ReturnType.GenericTypeArguments);
            var method10 = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType)
                .GetMethod("ExecuteMethodVoidAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo field7 = delegateType.GetField("parameters", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo field5 = delegateType.GetField("implementation", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            MethodInfo fromResult = typeof(Task).GetMethod("FromResult").MakeGenericMethod(typeof(HttpResponseMessage));
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder del = gen.DeclareLocal(delegateType);
            LocalBuilder serviceParameters = gen.DeclareLocal(typeof(object[]));
            LocalBuilder result = gen.DeclareLocal(typeof(Task<>).MakeGenericType(typeof(HttpResponseMessage)));
            LocalBuilder ex = gen.DeclareLocal(typeof(Exception));
            // Preparing labels
            Label label97 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.BeginExceptionBlock();
            gen.Emit(OpCodes.Newobj, delegateCtor);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Stfld, field5);
            var ps = pTypes;
            EmitHelpers.EmitInt32(gen, ps.Length);
            gen.Emit(OpCodes.Newarr, typeof(object));
            for (int j = 0; j < ps.Length; j++)
            {
                gen.Emit(OpCodes.Dup);
                EmitHelpers.EmitInt32(gen, j);
                EmitHelpers.EmitLdarg(gen, j + 1);
                var paramType = ps[j];
                if (paramType.IsValueType)
                {
                    gen.Emit(OpCodes.Box, paramType);
                }
                gen.Emit(OpCodes.Stelem_Ref);

            }
            gen.Emit(OpCodes.Stloc_1);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, implementationMethod.Name);
            gen.Emit(OpCodes.Ldloc_1);
            gen.Emit(OpCodes.Call, gatherParams);

            gen.Emit(OpCodes.Stfld, field7);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ldftn, delegateMethod);
            gen.Emit(OpCodes.Newobj, ctor9);
            gen.Emit(OpCodes.Call, method10);
            gen.Emit(OpCodes.Stloc_2);
            gen.Emit(OpCodes.Leave_S, label97);
            gen.Emit(OpCodes.Leave_S, label97);
            gen.BeginCatchBlock(typeof(Exception));
            gen.Emit(OpCodes.Stloc_S, 3);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldloc_S, 3);
            gen.Emit(OpCodes.Call, method9);
            gen.Emit(OpCodes.Call, fromResult);
            gen.Emit(OpCodes.Stloc_2);
            gen.Emit(OpCodes.Leave_S, label97);
            gen.EndExceptionBlock();
            gen.MarkLabel(label97);
            gen.Emit(OpCodes.Ldloc_2);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;
        }

        public ConstructorBuilder ctor(TypeBuilder type, Type interfaceType)
        {
            const MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig;

            var method = type.DefineConstructor(methodAttributes, CallingConventions.Standard | CallingConventions.HasThis, new[] { interfaceType });
            var implementation = method.DefineParameter(1, ParameterAttributes.None, "implementation");
            var ctor1 = typeof(ServiceWrapperBase<>).MakeGenericType(interfaceType).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { interfaceType }, null);

            var gen = method.GetILGenerator();
            // Writing body
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Call, ctor1);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ret);

            // finished
            return method;
        }

        private TypeBuilder CreateServiceType(Type interfaceType)
        {
            var routePrefix = interfaceType.GetCustomAttribute<IRoutePrefixAttribute>()
                ?? interfaceType.GetInterfaces().FirstOrDefault()?.GetCustomAttribute<IRoutePrefixAttribute>();
            var type = myModuleBuilder.DefineType("TempModule.Controllers." + interfaceType.Name.Remove(0, 1) + "Controller", TypeAttributes.Public | TypeAttributes.Class, typeof(ServiceWrapperBase<>).MakeGenericType(interfaceType));

            if (routePrefix != null)
            {
                var prefix = routePrefix.Prefix;
                if (routePrefix.IncludeTypeName) prefix = prefix + "/" + (interfaceType.GetGenericArguments().Any() ? interfaceType.GetGenericArguments().FirstOrDefault()?.Name.ToLower() : interfaceType.GetInterfaces().FirstOrDefault()?.GetGenericArguments().First().Name.ToLower());
                var routePrefixCtor = typeof(RoutePrefixAttribute).GetConstructor(
                           BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                           null,
                           new Type[]{
                        typeof(string)
                               },
                           null
                           );

                type.SetCustomAttribute(new CustomAttributeBuilder(routePrefixCtor, new object[] { prefix }));
            }
            return type;
        }

        public void Save()
        {
            if (ConfigurationManager.AppSettings["stardust.saveGeneratedAssemblies"] == "true")
                myAssemblyBuilder.Save("service.dll");
        }

        public Assembly GetCustomAssembly()
        {
            return myModuleBuilder.Assembly;
        }
    }
}