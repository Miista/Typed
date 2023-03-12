using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Typesafe.Sandbox2
{
    public class B
    {
        private int _m;
        public int M
        {
            get { return _m; }
            set { _m = value; }
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            var typeOfT = typeof(B);
            var assName = new AssemblyName(typeOfT.Assembly.FullName);
            var assBuilder = AssemblyBuilder.DefineDynamicAssembly(assName, AssemblyBuilderAccess.RunAndCollect);
            // var assBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assName, AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = assBuilder.DefineDynamicModule(typeOfT.Module.Name); //, "test.dll");
            var typeBuilder = moduleBuilder.DefineType(typeOfT.Name + "Proxy", TypeAttributes.BeforeFieldInit);
            typeBuilder.SetParent(typeOfT);

            var fieldBuilder = typeBuilder.DefineField("_value", typeOfT, FieldAttributes.Private | FieldAttributes.InitOnly);
            var first = typeof(DebuggerBrowsableAttribute).GetConstructors().First();
            var attributeBuilder = new CustomAttributeBuilder(first, new object[]{DebuggerBrowsableState.Never});
            fieldBuilder.SetCustomAttribute(attributeBuilder);
            
            var ctorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new Type[] {typeOfT});
            var ilGenerator = ctorBuilder.GetILGenerator();
            ilGenerator.EmitWriteLine("Creating Proxy instance");
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Stfld, fieldBuilder);
            ilGenerator.Emit(OpCodes.Ret);

            var implicitConversionMethodBuilder = typeBuilder.DefineMethod(
                "op_Implicit",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Static,
                CallingConventions.Standard,
                typeOfT,
                Type.EmptyTypes
            );
            var generator = implicitConversionMethodBuilder.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, fieldBuilder);
            generator.Emit(OpCodes.Ret);
            
            var getMMethod = typeBuilder.DefineMethod(
                "get_M",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                CallingConventions.HasThis,
                typeof(int),
                Type.EmptyTypes
            );
            var constructorInfo = typeof(CompilerGeneratedAttribute).GetConstructors().First();
            var customAttributeBuilder = new CustomAttributeBuilder(constructorInfo, new object[0]);
            getMMethod.SetCustomAttribute(customAttributeBuilder);
            var getMethodGenerator = getMMethod.GetILGenerator();
            getMethodGenerator.EmitWriteLine("In proxy getter");
            getMethodGenerator.Emit(OpCodes.Ldarg_0);
            getMethodGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
            var existingGetMMethod = typeOfT.GetMethod("get_M", BindingFlags.Instance | BindingFlags.Public);
            getMethodGenerator.Emit(OpCodes.Callvirt, existingGetMMethod);
            getMethodGenerator.Emit(OpCodes.Ret);

            var setMMethod = typeBuilder.DefineMethod(
                "set_M",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                CallingConventions.ExplicitThis | CallingConventions.HasThis,
                typeof(int),
                new Type[] { typeof(int) }
            );
            getMMethod.SetCustomAttribute(customAttributeBuilder);

            var setMethodGenerator = setMMethod.GetILGenerator();
            setMethodGenerator.EmitWriteLine("In proxy setter");
            setMethodGenerator.Emit(OpCodes.Ldarg_0);
            setMethodGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
            setMethodGenerator.Emit(OpCodes.Ldarg_1);
            var existingSetMMethod = typeOfT.GetMethod("set_M", BindingFlags.Instance | BindingFlags.Public);
            setMethodGenerator.Emit(OpCodes.Callvirt, existingSetMMethod);
            setMethodGenerator.Emit(OpCodes.Ret);
            
            var propertyBuilder = typeBuilder.DefineProperty(
                "M",
                PropertyAttributes.None,
                CallingConventions.HasThis | CallingConventions.ExplicitThis,
                typeof(int),
                Type.EmptyTypes
            );
            propertyBuilder.SetGetMethod(getMMethod);
            propertyBuilder.SetSetMethod(setMMethod);

            var proxyType = typeBuilder.CreateType();
            var existing = new B();
            var instance = Activator.CreateInstance(proxyType, existing);
            
            var proxy = instance as B;
            var y = proxy.M;
            proxy.M = 12;
            Console.WriteLine(existing.M);
            Console.WriteLine(proxy.M);
            existing.M = 10;
            Console.WriteLine(existing.M);
            Console.WriteLine(proxy.M);
        }
    }
}