using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Typesafe.Kernel;
using Typesafe.Merge;
using Typesafe.With;

namespace Typesafe.Sandbox
{
    class Person
    {
        public string Name { get; }
        public int Age { get; }
        public string LastName { get; set; }

        public Person(string name, int age)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Age = age;
        }

        public override string ToString() => $"Name={Name}; Age={Age}; LastName={LastName}; HashCode={GetHashCode()};";
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

    class TestClass
    {
        public string Name { get; }
        public House House { get; }
        public int? Age { get; set; }
    
        public TestClass(string name, House house) => (Name, House) = (name, house);
    }
    
    class Program
    {
        class ValueResolver : IValueResolver<TestClass>
        {
            public object Resolve(string parameterName)
            {
                switch (parameterName)
                {
                    case "name": return "Harry";
                    case "house": return House.Gryffindor;
                    case "Age": return 1;
                    default: throw new Exception();
                }
            }
        }
        
        class MergeValueResolver : IValueResolver<TestClass>
        {
            private readonly IReadOnlyDictionary<string, object> _values;

            public MergeValueResolver(IReadOnlyDictionary<string, object> values)
            {
                _values = values ?? throw new ArgumentNullException(nameof(values));
            }

            public object Resolve(string parameterName)
            {
                if (_values.TryGetValue(parameterName, out var value))
                {
                    return value;
                }

                throw new Exception();
            }
        }

        static void Main(string[] args)
        {
            {
                var merger = new TestClass("Harry", House.Gryffindor) {Age = 15};
                var mergee = new TestClass("Malfoy", House.Slytherin);

                var dictionary = new Dictionary<string, object>
                {
                    {"name", mergee.Name},
                    {"house", mergee.House},
                    {"Age", mergee.Age ?? merger.Age}
                };
                var valueResolver = new MergeValueResolver(dictionary);
                var instanceBuilder = new InstanceBuilder<TestClass>(valueResolver);
                var student = instanceBuilder.Create();
                Console.WriteLine($"Is not null: {!(student is null)}");
                Console.WriteLine($"Name is correct: {student.Name == "Malfoy"}");
                Console.WriteLine($"House is correct: {student.House == House.Slytherin}");
                Console.WriteLine($"Age is correct: {student.Age == 15}");
            }
            // {
            //     
            //     var harry = new Student("Harry Potter", House.Gryffindor);
            //     var malfoy = harry
            //         .With(p => p.Name, "Malfoy")
            //         .With(p => p.House, House.Slytherin);
            //     Console.WriteLine(malfoy.House);
            // }
            //
            // {
            //     var person1 = new Person("Søren", 10) {LastName = "Ost"};
            //     var person2 = new Person("Lasse", 11);
            //     var person3 = new NoCtor();
            //
            //     var mergedPerson = person1.Merge(person2);
            //     Console.WriteLine(mergedPerson);
            //
            //     var mergedPerson2 = person1.Merge<UnrelatedType, Person, Person>(person2);
            //     Console.WriteLine(mergedPerson2);
            //     
            //     var unsafeMerged = person1.Merge<UnrelatedType, Person, NoCtor>(person3);
            //     Console.WriteLine(unsafeMerged);
            // }
            //
            // var person = new Person("Søren", 10);
            // Console.WriteLine(person);
            //
            // var lasse = person
            //     .With(p => p.Name, "Lasse");
            // Console.WriteLine(lasse);
            //
            // var youngerSoren = person.With(p => p.Age, 5);
            // Console.WriteLine(youngerSoren);
            //
            // var withLastName = person
            //     .With(p => p.Name, "Test")
            //     .With(p => p.LastName, "Guldmund")
            //     .With(p => p.Age, 5);
            // Console.WriteLine(withLastName);
            //
            // var sorenAgain = person
            //     .With(p => p.Name, "Søren");
            // Console.WriteLine(sorenAgain);
            //
            // var noCtor = new NoCtor {Name = "Søren"};
            // Console.WriteLine(noCtor);
            //
            // var noCtor1 = noCtor.With(p => p.Name, "Test");
            // Console.WriteLine(noCtor1);
        }
    }
}