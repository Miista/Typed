using System;
using Typesafe.With;
using Typesafe.Merge;

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

        public override string ToString() => $"Name={Name}; Age={Age}; LastName={LastName};";
    }

    class NoCtor
    {
        public string Name { get; set; }

        public override string ToString() => $"Name={Name};";
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            {
                var person1 = new Person("Søren", 10) {LastName = "Ost"};
                var person2 = new Person("Lasse", 11);
                var person3 = new NoCtor();
            
                var mergedPerson = Merge.ObjectExtensions.Merge<NoCtor, Person, Person>(person1, person2);
                Console.WriteLine(mergedPerson);
                Console.WriteLine(mergedPerson.GetHashCode());
            }
            
            // var person = new Person("Søren", 10);
            // Console.WriteLine(person);
            // Console.WriteLine(person.GetHashCode());
            //
            // var lasse = person
            //     .With(p => p.Name, "Lasse");
            // Console.WriteLine(lasse);
            // Console.WriteLine(lasse.GetHashCode());
            //
            // var youngerSoren = person.With(p => p.Age, 5);
            // Console.WriteLine(youngerSoren);
            // Console.WriteLine(youngerSoren.GetHashCode());
            //
            // var withLastName = person
            //     .With(p => p.Name, "Test")
            //     .With(p => p.LastName, "Guldmund")
            //     .With(p => p.Age, 5);
            // Console.WriteLine(withLastName);
            // Console.WriteLine(withLastName.GetHashCode());
            //
            // var sorenAgain = person
            //     .With(p => p.Name, "Søren");
            // Console.WriteLine(sorenAgain);
            // Console.WriteLine(sorenAgain.GetHashCode());
            //
            // var noCtor = new NoCtor {Name = "Søren"};
            // Console.WriteLine(noCtor);
            // Console.WriteLine(noCtor.GetHashCode());
            //
            // var noCtor1 = noCtor.With(p => p.Name, "Test");
            // Console.WriteLine(noCtor1);
            // Console.WriteLine(noCtor1.GetHashCode());
        }
    }
}