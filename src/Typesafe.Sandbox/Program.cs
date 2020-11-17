using System;
using Typesafe.With;

namespace Typesafe.Sandbox
{
    class Person
    {
        public string Name { get; }
        public int Age { get; }

        public Person(string name, int age)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Age = age;
        }

        public override string ToString() => $"Name={Name}; Age={Age};";
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            var person = new Person("Søren", 10);
            Console.WriteLine(person);
            Console.WriteLine(person.GetHashCode());

            var lasse = person
                .With(p => p.Name, "Lasse");
            Console.WriteLine(lasse);
            Console.WriteLine(lasse.GetHashCode());

            var youngerSoren = person.With(p => p.Age, 5);
            Console.WriteLine(youngerSoren);
            Console.WriteLine(youngerSoren.GetHashCode());
        }
    }
}