using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using X;

namespace AssemblyCreator
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var b = new B();
            var c1 = new C(b);
            Console.WriteLine(c1);

            // var typeOfT = typeof(Program);
            // var assName = new AssemblyName(typeOfT.Assembly.FullName);
            // var assBuilder = AssemblyBuilder.DefineDynamicAssembly(assName, AssemblyBuilderAccess.RunAndSave);
            // // var assBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assName, AssemblyBuilderAccess.RunAndSave);
            // var moduleBuilder = assBuilder.DefineDynamicModule(typeOfT.Module.Name); //, "test.dll");
            // var typeBuilder = moduleBuilder.DefineType(typeOfT.Name + "Proxy", TypeAttributes.BeforeFieldInit);
            // typeBuilder.SetParent(typeof(string));
            //
            // var ctorBuilder = typeBuilder.DefineConstructor(
            //     MethodAttributes.Public,
            //     CallingConventions.Standard,
            //     new Type[] {typeof(string)});
            // var ilGenerator = ctorBuilder.GetILGenerator();
            // ilGenerator.EmitWriteLine("Creating Proxy instance");
            // ilGenerator.Emit(OpCodes.Ldarg_0);
            // ilGenerator.Emit(OpCodes.Ldc_I4_0);
            // ilGenerator.Emit(OpCodes.Newarr, typeof(char));
            // var baseCtor = typeof(string).GetConstructors().Where(c => c.GetParameters().FirstOrDefault()?.ParameterType == typeof(char[])).FirstOrDefault();
            // ilGenerator.Emit(OpCodes.Call, baseCtor);
            // ilGenerator.Emit(OpCodes.Nop);
            // ilGenerator.Emit(OpCodes.Nop);
            // ilGenerator.Emit(OpCodes.Ret);
            //
            // var type = typeBuilder.CreateType();
            // assBuilder.Save("C:\test.dll");
        }
    }
}