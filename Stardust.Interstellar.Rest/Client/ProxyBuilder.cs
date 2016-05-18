using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Stardust.Interstellar.Rest.Common;
using Stardust.Interstellar.Rest.Extensions;

namespace Stardust.Interstellar.Rest.Client
{
    internal class ProxyBuilder<T>
    {
        private AssemblyBuilder myAssemblyBuilder;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public Type Build()
        {
            var myCurrentDomain = AppDomain.CurrentDomain;
            var myAssemblyName = new AssemblyName();
            myAssemblyName.Name = Guid.NewGuid().ToString().Replace("-", "") + "_RestWrapper";
            myAssemblyBuilder = myCurrentDomain.DefineDynamicAssembly(myAssemblyName, AssemblyBuilderAccess.RunAndSave);
            var myModuleBuilder = myAssemblyBuilder.DefineDynamicModule("TempModule", "dyn.dll");
            var type = ReflectionTypeBuilder(myModuleBuilder, typeof(T).Name + "_dynimp");
            ctor(type);
            foreach (var methodInfo in typeof(T).GetMethods())
            {
                if (typeof(Task).IsAssignableFrom(methodInfo.ReturnType))
                    BuildMethodAsync(type, methodInfo);
                else if (methodInfo.ReturnType != typeof(void))
                    BuildMethod(type, methodInfo);
                else
                    BuildVoidMethod(type, methodInfo);
            }

            var result = type.CreateType();
            myAssemblyBuilder.Save("dyn.dll");
            return result;
        }

        private MethodBuilder BuildMethod(TypeBuilder type, MethodInfo serviceAction)
        {
            const MethodAttributes methodAttributes = MethodAttributes.Public
                                                      | MethodAttributes.Virtual
                                                      | MethodAttributes.Final
                                                      | MethodAttributes.HideBySig
                                                      | MethodAttributes.NewSlot;
            var method = type.DefineMethod(serviceAction.Name, methodAttributes);
            // Preparing Reflection instances
            var method1 = typeof(RestWrapper).GetMethod(
                "GetParameters",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[]{
                         typeof(string),
                         typeof(object[])
                     },
                null
                );
            MethodInfo method2 = typeof(RestWrapper).GetMethod(
                "Invoke",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[]{
                         typeof(string),
                         typeof(ParameterWrapper[])
                     },
                null
                ).MakeGenericMethod(serviceAction.ReturnType);


            // Setting return type

            method.SetReturnType(serviceAction.ReturnType);
            // Adding parameters
            method.SetParameters(serviceAction.GetParameters().Select(p => p.ParameterType).ToArray());
            var i = 1;
            foreach (var parameterInfo in serviceAction.GetParameters())
            {
                var param = method.DefineParameter(i, ParameterAttributes.None, parameterInfo.Name);
                i++;
            }
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder par = gen.DeclareLocal(typeof(Object[]));
            LocalBuilder parameters = gen.DeclareLocal(typeof(ParameterWrapper[]));
            LocalBuilder result = gen.DeclareLocal(serviceAction.ReturnType);
            LocalBuilder str = gen.DeclareLocal(serviceAction.ReturnType);
            // Preparing labels
            Label label55 = gen.DefineLabel();
            // Writing body
            var ps = serviceAction.GetParameters();
            EmitHelpers.EmitInt32(gen, ps.Length);
            gen.Emit(OpCodes.Newarr, typeof(object));
            for (int j = 0; j < ps.Length; j++)
            {
                gen.Emit(OpCodes.Dup);
                EmitHelpers.EmitInt32(gen, j);
                EmitHelpers.EmitLdarg(gen, j + 1);

                var paramType = ps[j].ParameterType;
                if (paramType.IsValueType)
                {
                    gen.Emit(OpCodes.Box, paramType);
                }
                gen.Emit(OpCodes.Stelem_Ref);
            }
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, serviceAction.Name);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Call, method1);
            gen.Emit(OpCodes.Stloc_1);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, serviceAction.Name);
            gen.Emit(OpCodes.Ldloc_1);
            gen.Emit(OpCodes.Call, method2);
            gen.Emit(OpCodes.Stloc_2);
            gen.Emit(OpCodes.Ldloc_2);
            gen.Emit(OpCodes.Stloc_3);
            gen.Emit(OpCodes.Br_S, label55);
            gen.MarkLabel(label55);
            gen.Emit(OpCodes.Ldloc_3);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;

        }

        private MethodBuilder BuildVoidMethod(TypeBuilder type, MethodInfo serviceAction)
        {
            const MethodAttributes methodAttributes = MethodAttributes.Public
                                                      | MethodAttributes.Virtual
                                                      | MethodAttributes.Final
                                                      | MethodAttributes.HideBySig
                                                      | MethodAttributes.NewSlot;
            var method = type.DefineMethod(serviceAction.Name, methodAttributes);
            // Preparing Reflection instances
            var method1 = typeof(RestWrapper).GetMethod(
                "GetParameters",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[]{
                         typeof(string),
                         typeof(object[])
                     },
                null
                );
            MethodInfo method2 = typeof(RestWrapper).GetMethod(
                "InvokeVoid",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[]{
                         typeof(string),
                         typeof(ParameterWrapper[])
                     },
                null
                );

            // Adding parameters
            method.SetParameters(serviceAction.GetParameters().Select(p => p.ParameterType).ToArray());
            var i = 1;
            foreach (var parameterInfo in serviceAction.GetParameters())
            {
                var param = method.DefineParameter(i, ParameterAttributes.None, parameterInfo.Name);
                i++;
            }
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder par = gen.DeclareLocal(typeof(Object[]));
            LocalBuilder parameters = gen.DeclareLocal(typeof(ParameterWrapper[]));
            // Preparing labels
            Label label55 = gen.DefineLabel();
            // Writing body
            var ps = serviceAction.GetParameters();
            EmitHelpers.EmitInt32(gen, ps.Length);
            gen.Emit(OpCodes.Newarr, typeof(object));
            for (int j = 0; j < ps.Length; j++)
            {
                gen.Emit(OpCodes.Dup);
                EmitHelpers.EmitInt32(gen, j);
                EmitHelpers.EmitLdarg(gen, j + 1);
                var paramType = ps[j].ParameterType;
                if (paramType.IsValueType)
                {
                    gen.Emit(OpCodes.Box, paramType);
                }
                gen.Emit(OpCodes.Stelem_Ref);
            }
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, serviceAction.Name);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Call, method1);
            gen.Emit(OpCodes.Stloc_1);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, serviceAction.Name);
            gen.Emit(OpCodes.Ldloc_1);
            gen.Emit(OpCodes.Call, method2);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;

        }

        private MethodBuilder BuildMethodAsync(TypeBuilder type, MethodInfo serviceAction)
        {
            const MethodAttributes methodAttributes = MethodAttributes.Public
                                                      | MethodAttributes.Virtual
                                                      | MethodAttributes.Final
                                                      | MethodAttributes.HideBySig
                                                      | MethodAttributes.NewSlot;
            var method = type.DefineMethod(serviceAction.Name, methodAttributes);
            // Preparing Reflection instances
            var method1 = typeof(RestWrapper).GetMethod(
                "GetParameters",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[]{
                         typeof(string),
                         typeof(object[])
                     },
                null
                );

            var method2 = typeof(RestWrapper).GetMethod(serviceAction.ReturnType.GetGenericArguments().Length == 0 ? "InvokeVoidAsync" : "InvokeAsync", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(ParameterWrapper[]) }, null);
            if (serviceAction.ReturnType.GenericTypeArguments.Any())
                method2 = method2.MakeGenericMethod(serviceAction.ReturnType.GenericTypeArguments);


            // Setting return type

            method.SetReturnType(serviceAction.ReturnType);
            // Adding parameters
            method.SetParameters(serviceAction.GetParameters().Select(p => p.ParameterType).ToArray());
            var i = 1;
            foreach (var parameterInfo in serviceAction.GetParameters())
            {
                var param = method.DefineParameter(i, ParameterAttributes.None, parameterInfo.Name);
                i++;
            }
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder par = gen.DeclareLocal(typeof(Object[]));
            LocalBuilder parameters = gen.DeclareLocal(typeof(ParameterWrapper[]));
            LocalBuilder result = gen.DeclareLocal(serviceAction.ReturnType);
            LocalBuilder str = gen.DeclareLocal(serviceAction.ReturnType);
            // Preparing labels
            Label label55 = gen.DefineLabel();
            // Writing body
            var ps = serviceAction.GetParameters();
            EmitHelpers.EmitInt32(gen, ps.Length);
            gen.Emit(OpCodes.Newarr, typeof(object));
            for (int j = 0; j < ps.Length; j++)
            {
                gen.Emit(OpCodes.Dup);
                EmitHelpers.EmitInt32(gen, j);
                EmitHelpers.EmitLdarg(gen, j + 1);
                var paramType = ps[j].ParameterType;
                if (paramType.IsValueType)
                {
                    gen.Emit(OpCodes.Box, paramType);
                }
                gen.Emit(OpCodes.Stelem_Ref);

            }
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, serviceAction.Name);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Call, method1);
            gen.Emit(OpCodes.Stloc_1);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, serviceAction.Name);
            gen.Emit(OpCodes.Ldloc_1);
            gen.Emit(OpCodes.Call, method2);
            gen.Emit(OpCodes.Stloc_2);
            gen.Emit(OpCodes.Ldloc_2);
            gen.Emit(OpCodes.Stloc_3);
            gen.Emit(OpCodes.Br_S, label55);
            gen.MarkLabel(label55);
            gen.Emit(OpCodes.Ldloc_3);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;
        }

        

        public ConstructorBuilder ctor(TypeBuilder type)
        {
            const MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig;

            var method = type.DefineConstructor(methodAttributes, CallingConventions.Standard | CallingConventions.HasThis, new[] { typeof(IAuthenticationHandler), typeof(IHeaderHandlerFactory), typeof(TypeWrapper) });
            var authenticationHandler = method.DefineParameter(1, ParameterAttributes.None, "authenticationHandler");
            var headerHandlers = method.DefineParameter(2, ParameterAttributes.None, "headerHandlers");
            var interfaceType = method.DefineParameter(3, ParameterAttributes.None, "interfaceType");
            var ctor1 = typeof(RestWrapper).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(IAuthenticationHandler), typeof(IHeaderHandlerFactory), typeof(TypeWrapper) }, null);

            var gen = method.GetILGenerator();
            // Writing body
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Ldarg_2);
            gen.Emit(OpCodes.Ldarg_3);
            gen.Emit(OpCodes.Call, ctor1);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ret);

            // finished
            return method;
        }



        private TypeBuilder ReflectionTypeBuilder(ModuleBuilder module, string typeName)
        {
            var type = module.DefineType("TempModule." + typeName,
                TypeAttributes.Public | TypeAttributes.Class,
                typeof(RestWrapper),
                new[] { typeof(T) }
                );
            return type;
        }
    }
}