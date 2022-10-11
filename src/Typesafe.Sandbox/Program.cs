using System;
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
    
    class Program
    {
        static void Main(string[] args)
        {
            {
                
                var harry = new Student("Harry Potter", House.Gryffindor);
                var malfoy = harry
                    .With(p => p.Name, name => name.Substring(2))
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
            Console.WriteLine(noCtor1);
        }
    }
}