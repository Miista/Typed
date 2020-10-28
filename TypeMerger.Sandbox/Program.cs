using System;

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
        private static void Main(string[] args)
        {
            var instance = new Test("", 0);
            var updatedInstance = instance
                .With(e => e.Name, null);

            Print(updatedInstance);
        }

        private static void Print(Test test) => Console.WriteLine(test);
    }
}