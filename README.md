# Typesafe.With

Providing with-functionality to any type on the .NET platform.

## What and Why?

Typesafe.With is a strongly typed way to change properties on immutable types that lets you:

* Avoid writing with methods by hand ever again
* Update properties on any type

## Features

* Robust and well tested
* Self contained with no dependencies
* Easily installed through NuGet
* Supports **.NET Core** (.NET Standard 2+)
* Focused but full-featured API

## Supports

Typesafe.With supports types using:
* Constructors
* Property setters
* A mix of constructors and property setters

## Installation

```
PM> Install-Package Typesafe.With
```

## Concepts

Typesafe.With allows you to update properties on immutable types in a type safe way that is type checked by the compiler.

Typesafe.With provides a single method called `With`.
Any call to `With` produces a new instance of the immutable type with the updated property.

The call to `With` is strongly typed which means that the compiler verifies that the assignment is valid.

## Usage
### Using the library
To use Typesafe.With simply import the following namespace:
```csharp
using Typesafe.With;
```

### Update a property
```csharp
public class Person
{
    public string Name { get; }
    
    public Person(string name) => (Name) = (name);
}

var harry = new Person("Harry Potter");
var hermione = harry.With(p => p.Name, "Hermione Granger");

Console.WriteLine(hermione.Name); // Prints "Hermione Granger"
```

### Updating multiple properties
```csharp
public enum House { Gryffindor, Slytherin }

public class Person
{
    public string Name { get; }
    public House House { get; }
    
    public Person(string name, House house) => (Name, House) = (name, house);
}

var harry = new Person("Harry Potter", House.Gryffindor);
var malfoy = harry
	.With(p => p.Name, "Malfoy")
	.With(p => p.House, House.Slytherin);

Console.WriteLine(malfoy.Name); // Prints "Malfoy"
Console.WriteLine(malfoy.House); // Prints "Slytherin"
```

### Update a property depending on its current state
```csharp
public enum House { Gryffindor, Slytherin }

public class Person
{
    public string Name { get; }
    public House House { get; }
    
    public Person(string name, House house) => (Name, House) = (name, house);
}

var harry = new Person("Harry Potter", House.Gryffindor);
var malfoy = harry
	.With(p => p.House, house => house == House.Slytherin ? House.Gryffindor : house);

Console.WriteLine(malfoy.House); // Prints "Gryffindor"
```

### Sequencing
If you need to update many objects at the same time, you can create a sequence of mutations to be applied.

```csharp
public class Person
{
    public string Name { get; }
    
    public Person(string name) => (Name) = (name);
}

var sequence = WithSequence.New<Person>()
                    .With(p => p.Name, "Harry")
                    .ToSequence();

var persons = new List<Person>
{
    new Person("Malfoy", 0),
    new Person("Hermione", 0)
};
var updatedPersons = persons.Select(sequence.ApplyTo);

foreach (var person in updatedPersons)
{
    Console.WriteLine(person.Name); // Prints "Harry"
}
```

### Lazy Evaluation
In cases where object construction is expensive, you can make the calls to `With` be lazily evaluated.

```csharp
using Lazy;

public class Person
{
    public string Name { get; }
    
    public Person(string name) => (Name) = (name);
}

var harry = new Person("Harry Potter");
var hermione = harry.With(p => p.Name, "Hermione Granger");

Console.WriteLine(hermione.Name); // Prints "Hermione Granger"
```

## How it works
The resulting type `T` is constructed via the following steps:

1. Find appropriate<sup>+</sup> constructor _C_ for type `T`
2. Resolve the set of arguments _A<sub>C</sub>_ for constructor _C_
2. Invoke _C_, exchanging the arguments _A<sub>C</sub>_ to be swapped
2. Call property setters, exchanging the arguments _A<sub>C</sub>_ to be swapped

<sup>+</sup> The constructor taking the most arguments. If there is no constructor, the default constructor is called.

In step 2, if the constructor _C_ takes the argument to be swapped, the 

## Convention
Typesafe.With relies on the convention that the property is named the same in the type constructor.
