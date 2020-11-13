﻿using System;
using TypeWither;

namespace TypeMerger.Sandbox
{
    class Test
    {
        public string Name { get; }
        public int Age { get; }

        public Test(string name, int age)
        {
            Name = name;
            Age = age;
        }

        public override string ToString() => $"Name={Name};Age={Age}";
    }
    
    class Program
    {
        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }

            public override string ToString() => $"Name: {Name}; Age: {Age}";
        }
        
        private static void Main(string[] args)
        {
            var person = new Person();
            Person me = person
                .With(p => p.Name, "Søren Guldmund")
                .With(p => p.Age, 30);
            Console.WriteLine(me); // Writes Name: Søren Guldmund; Age: 30

            Person lasse = me
                .With(p => p.Name, "Lasse Chris Aaroe")
                .With(p => p.Age, -1);
            Console.WriteLine(lasse); // Writes Name: Lasse Chris Aaroe; Age: -1
            Console.WriteLine(me); // Writes Name: Søren Guldmund; Age: 30

            // var instance = new Test("", 0);
            // var updatedInstance = instance
            //     .With(e => e.Name, null);
            //
            // Print(updatedInstance);
            //
            // var newInstance = new Test("First Name", 10);
            // var mergedInstance = TypeMerger.Merge<Test, Test, Test>(instance, newInstance);
            // Print(mergedInstance);
        }

        private static void Print(Test test) => Console.WriteLine(test);
    }
}