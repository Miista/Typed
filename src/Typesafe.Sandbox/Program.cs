using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Typesafe.Merge;
using Typesafe.Sandbox;
using Typesafe.With;
using Typesafe.Snapshots;

namespace Typesafe.Sandbox
{
    public class Nothing
    {
        public string Text { get; set; }
    }
    
    class Person
    {
        public string Name { get; }
        public int Age { get; }
        public string LastName { get; set; }
        public List<Person> Persons { get; set; }
        public ValueType ValueType { get; set; }

        public Person(string name, int age)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Age = age;
        }

        public override string ToString()
        {
            var subPersons = string.Join("", Persons?.Select(p => $"{Environment.NewLine} - {p}") ?? new List<string>());
            var x = $"Name={Name}; Age={Age}; LastName={LastName}; HashCode={GetHashCode()};";
            return $"{x}{subPersons}";
        }
    }

    public interface ILol
    {
    }

    [DebuggerDisplay("Not snapshot: {Value}")]
    class DebuggerWrapper<T>
    {
        public readonly T Value;

        private DebuggerWrapper(T instance)
        {
            Value = instance;
        }

        public static implicit operator DebuggerWrapper<T>(T instance) => new DebuggerWrapper<T>(instance);

        public static implicit operator T(DebuggerWrapper<T> wrapper) => wrapper.Value;
    }
    
    struct ValueType
    {
        public int Age { get; set; }
    }

    class NoCtor
    {
        public string Name { get; set; }

        public override string ToString() => $"Name={Name};";
    }

    class UnrelatedType
    {
    }
    
    class Student
    {
        public string Name { get; }
        public House House { get; }
    
        public Student(string name, House house) => (Name, House) = (name, house);
    }

    enum House
    {
        Gryffindor,
        Slytherin
    }
    
    public class LoggingDecorator<T> : DispatchProxy
    {
        private T _decorated;

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            try
            {
                LogBefore(targetMethod, args);

                var result = targetMethod.Invoke(_decorated, args);

                LogAfter(targetMethod, args, result);
                return result;
            }
            catch (Exception ex) when (ex is TargetInvocationException)
            {
                LogException(ex.InnerException ?? ex, targetMethod);
                throw ex.InnerException ?? ex;
            }
        }

        public static T Create(T decorated)
        {
            object proxy = Create<T, LoggingDecorator<T>>();
            ((LoggingDecorator<T>)proxy).SetParameters(decorated);

            return (T)proxy;
        }

        private void SetParameters(T decorated)
        {
            if (decorated == null)
            {
                throw new ArgumentNullException(nameof(decorated));
            }
            _decorated = decorated;
        }

        private void LogException(Exception exception, MethodInfo methodInfo = null)
        {
            Console.WriteLine($"Class {_decorated.GetType().FullName}, Method {methodInfo.Name} threw exception:\n{exception}");
        }

        private void LogAfter(MethodInfo methodInfo, object[] args, object result)
        {
            Console.WriteLine($"Class {_decorated.GetType().FullName}, Method {methodInfo.Name} executed, Output: {result}");
        }

        private void LogBefore(MethodInfo methodInfo, object[] args)
        {
            Console.WriteLine($"Class {_decorated.GetType().FullName}, Method {methodInfo.Name} is executing");
        }
    }

    public static class DynamicProxyGenerator
    {
        public static T GetInstanceFor<T>()
        {
            var typeOfT = typeof(T);
            var methodInfos = typeOfT.GetMethods();
            var assName = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
            var assBuilder = AssemblyBuilder.DefineDynamicAssembly(assName, AssemblyBuilderAccess.RunAndCollect);
            // var assBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assName, AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = assBuilder.DefineDynamicModule(Assembly.GetExecutingAssembly().ManifestModule.Name); //, "test.dll");
            var typeBuilder = moduleBuilder.DefineType(typeOfT.Name + "Proxy", TypeAttributes.NotPublic);

            // typeBuilder.AddInterfaceImplementation(typeOfT);
            typeBuilder.SetParent(typeOfT);
            var ctorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new Type[] { });
            var ilGenerator = ctorBuilder.GetILGenerator();
            ilGenerator.EmitWriteLine("Creating Proxy instance");
            ilGenerator.Emit(OpCodes.Ret);
            foreach (var methodInfo in methodInfos)
            {
                var methodBuilder = typeBuilder.DefineMethod(
                    methodInfo.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual,
                    methodInfo.ReturnType,
                    methodInfo.GetParameters().Select(p => p.GetType()).ToArray()
                );
                var methodILGen = methodBuilder.GetILGenerator();
                if (methodInfo.ReturnType == typeof(void))
                {
                    methodILGen.Emit(OpCodes.Ret);
                }
                else
                {
                    if (methodInfo.ReturnType.IsValueType || methodInfo.ReturnType.IsEnum)
                    {
                        MethodInfo getMethod = typeof(Activator).GetMethod("CreateInstance", new Type[]
                            { typeof(Type) });
                        LocalBuilder lb = methodILGen.DeclareLocal(methodInfo.ReturnType);
                        methodILGen.Emit(OpCodes.Ldtoken, lb.LocalType);
                        methodILGen.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
                        methodILGen.Emit(OpCodes.Callvirt, getMethod);
                        methodILGen.Emit(OpCodes.Unbox_Any, lb.LocalType);
                    }
                    else
                    {
                        methodILGen.Emit(OpCodes.Ldnull);
                    }

                    methodILGen.Emit(OpCodes.Ret);
                }

                // typeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
            }

            var constructorInfo = typeof(DebuggerDisplayAttribute).GetConstructors().First();
            var customAttributeBuilder = new CustomAttributeBuilder(constructorInfo, new object[]{"Not snapshotted"});
            typeBuilder.SetCustomAttribute(customAttributeBuilder);
            Type constructedType = typeBuilder.CreateType();
            var instance = Activator.CreateInstance(constructedType);
            return (T)instance;
        }
    }  

    
    class Program
    {
        static void Main(string[] args)
        {
            // Debug wrapper
            {
                var ron = new Person("Ron", 3){LastName = "Weasley",ValueType = new ValueType{Age = 3}};
                DebuggerWrapper<Person> debugPerson = ron;

                Console.WriteLine(debugPerson);

                var instanceFor2 = DynamicProxyGenerator.GetInstanceFor<Person>();
                var instanceFor = DynamicProxyGenerator.GetInstanceFor<ILol>();
                var person = LoggingDecorator<Person>.Create(ron);
                Console.WriteLine(person);
            }
            
            // Snapshots
            {
                var ron = new Person("Ron", 3){LastName = "Weasley",ValueType = new ValueType{Age = 3}};
                var draco = new Person("Draco", 2) {LastName = "Malfoy",ValueType = new ValueType{Age = 2}};
                var harry = new Person("Harry", 1) { LastName = "Potter", Persons = new List<Person>(),ValueType = new ValueType{Age = 1}};
                harry.Persons.Add(draco);
                var harrySnapshot = harry.GetSnapshot();
                harry.LastName = "Malfoy";
                harry.Persons.Add(ron);
                ron.LastName = "Not his name";
                draco.LastName = "woops";

                Console.WriteLine(harry);
                Console.WriteLine("-----");
                Console.WriteLine(harrySnapshot);
            }
            /*{
                var harry = new Student("Harry Potter", House.Gryffindor);
                var malfoy = harry
                    .With(p => p.Name, "Malfoy")
                    .With(p => p.House, house => house == House.Slytherin ? House.Gryffindor : house);

                Console.WriteLine(malfoy.Name); // Prints "Malfoy"
                Console.WriteLine(malfoy.House); // Prints "Gryffindor"
            }
            
            {
                
                var harry = new Student("Harry Potter", House.Gryffindor);
                var malfoy = harry
                    .With(p => p.Name, name => name.Length == 1 ? name : "Snape")
                    .With(p => p.House, House.Slytherin);
                Console.WriteLine(malfoy.Name);
            }
            
            {
                var person1 = new Person("Søren", 10) {LastName = "Ost"};
                var person2 = new Person("Lasse", 11);
                var person3 = new NoCtor();
            
                var mergedPerson = person1.Merge(person2);
                Console.WriteLine(mergedPerson);
            
                var mergedPerson2 = person1.Merge<UnrelatedType, Person, Person>(person2);
                Console.WriteLine(mergedPerson2);
                
                var unsafeMerged = person1.Merge<UnrelatedType, Person, NoCtor>(person3);
                Console.WriteLine(unsafeMerged);
            }
            
            var person = new Person("Søren", 10);
            Console.WriteLine(person);
            
            var lasse = person
                .With(p => p.Name, "Lasse");
            Console.WriteLine(lasse);
            
            var youngerSoren = person.With(p => p.Age, 5);
            Console.WriteLine(youngerSoren);
            
            var withLastName = person
                .With(p => p.Name, "Test")
                .With(p => p.LastName, "Guldmund")
                .With(p => p.Age, 5);
            Console.WriteLine(withLastName);
            
            var sorenAgain = person
                .With(p => p.Name, "Søren");
            Console.WriteLine(sorenAgain);
            
            var noCtor = new NoCtor {Name = "Søren"};
            Console.WriteLine(noCtor);
            
            var noCtor1 = noCtor.With(p => p.Name, "Test");
            Console.WriteLine(noCtor1);*/
        }
    }
}