using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using Typesafe.Snapshots.Cloner;
using Typesafe.Snapshots.Registry;
using Xunit;
// ReSharper disable ClassNeverInstantiated.Global

namespace Typesafe.Snapshots.Tests
{
    public class Tests
    {
        public class PrimitiveTypeTests
        {
            [Theory]
            [ClassData(typeof(TestData.PrimitiveTypes))]
            public void Does_not_throw_on_primitive_types<T>(T value)
            {
                // Act
                Func<T> act = () => value.GetSnapshot();

                // Assert
                act.Should().NotThrow();
                var snapshot = act();
                snapshot.Should().Be(value, because: "the snapshot has the same value as the original");
            }
        }

        public class StringTests
        {
            [Theory, AutoData]
            public void Does_not_throw_on_string_type(string s)
            {
                // Arrange + Act
                Action act = () => s.GetSnapshot();

                // Assert
                act.Should().NotThrow();
            }

            [Theory, AutoData]
            public void Can_create_snapshot_of_string(string s)
            {
                // Arrange + Act
                var snapshot = s.GetSnapshot();

                // Assert
                snapshot.Should().NotBeSameAs(s, because: "it is a distinct copy");
            }
        }

        public class ComplexTypeTests
        {
            public class Person
            {
                // ReSharper disable UnusedAutoPropertyAccessor.Global
                public string Name { get; set; }
                public List<int> Ages { get; set; }
                public StructType ValueType { get; set; }
                // ReSharper enable UnusedAutoPropertyAccessor.Global
            }
            
            public class StructType
            {
                public int Age { get; set; }
            }

            [Theory, AutoData]
            public void Can_create_snapshot_of_complex_type(Person person)
            {
                // Arrange + Act
                var snapshot = person.GetSnapshot();

                // Assert
                snapshot.Should().NotBeSameAs(person, because: "it is a distinct copy");
            }
            
            [Theory, AutoData]
            public void Snapshot_does_not_change_when_original_changes(Person person, string newName)
            {
                // Arrange + Act
                var snapshot = person.GetSnapshot();
                person.Name = newName;

                // Assert
                snapshot.Should().NotBeSameAs(person, because: "it is a distinct copy");
                snapshot.Name.Should().NotBeSameAs(person.Name);
            }
            
            [Theory, AutoData]
            public void Snapshot_does_not_change_when_original_changes2(Person person, int newAge)
            {
                // Arrange + Act
                var snapshot = person.GetSnapshot();
                int countBeforeAdd = person.Ages.Count;
                person.Ages.Add(newAge);

                // Assert
                snapshot.Should().NotBeSameAs(person, because: "it is a distinct copy");
                snapshot.Ages.Should().NotBeSameAs(person.Ages);
                snapshot.Ages.Should().HaveCount(countBeforeAdd, because: "the list is a snapshot");
            }
            
            [Theory, AutoData]
            public void Snapshot_does_not_change_when_original_changes3(Person person, int newAge)
            {
                // Arrange + Act
                var snapshot = person.GetSnapshot();
                var x = person.ValueType;
                x.Age = newAge;

                // Assert
                snapshot.Should().NotBeSameAs(person, because: "it is a distinct copy");
                snapshot.ValueType.Should().NotBeSameAs(person.ValueType, because: "the snapshot is distinct from the original");
                snapshot.ValueType.Age.Should().NotBe(person.ValueType.Age, because: "the snapshot is distinct from the original");
            }
            
            [Theory]
            [ClassData(typeof(TestData.ComplexTypes))]
            public void Does_not_throw_on_complex_type<T>(T value)
            {
                // Act
                Func<T> act = () => value.GetSnapshot();

                // Assert
                act.Should().NotThrow();
            }
            
            [Theory]
            [ClassData(typeof(TestData.CollectionTypesWithAssertions))]
            public void Can_create_snapshot_of_collection_types<T>(T value, Action<T, T> assertion)
            {
                // Act
                var snapshot = value.GetSnapshot();

                // Assert
                ReferenceEquals(snapshot, value).Should().BeFalse(because: "the snapshot is distinct from the original");
                snapshot.Should().NotBeSameAs(value, because: "the snapshot is distinct from the original");
                assertion(value, snapshot);
                // snapshot.As<T>().Should().(value, because: "the snapshot has the same value as the original");
            }
        }

        public class InterfaceTests
        {
            public interface IName
            {
                string Name { get; set; }
            }

            public class NameImpl : IName
            {
                public string Name { get; set; }
            }
            
            public class TypeWithInterfaceProperties
            {
                public IName Name { get; set; }
            }

            [Fact]
            public void Can_create_snapshot_of_class_with_property_having_an_interface_type()
            {
                // Arrange
                var input = new TypeWithInterfaceProperties { Name = new NameImpl { Name = "Name" } };

                // Act
                var snapshot = input.GetSnapshot();

                // Assert
                snapshot.Should().NotBeNull();
                snapshot.Should().NotBeSameAs(input, because: "it is a snapshot");
            }
            
            [Fact]
            public void Can_create_snapshot_of_class_with_interface()
            {
                // Arrange
                IName input = new NameImpl { Name = "Name" };

                // Act
                var snapshot = input.GetSnapshot();

                // Assert
                snapshot.Should().NotBeNull();
                snapshot.Should().NotBeSameAs(input, because: "it is a snapshot");
            }
        }

        public class General
        {
            internal class TypeWithPrivateConstructor
            {
                private TypeWithPrivateConstructor() { }

                public static TypeWithPrivateConstructor Create() => new TypeWithPrivateConstructor();
            }
            
            [Fact]
            public void Throws_on_type_with_private_constructor()
            {
                // Arrange
                var typeWithPrivateConstructor = TypeWithPrivateConstructor.Create();
                
                // Act
                Action act = () => typeWithPrivateConstructor.GetSnapshot();

                // Assert
                act.Should().Throw<Exception>(because: "the type has a private constructor");
            }
        }

        public class Cloning
        {
            // ReSharper disable once ClassNeverInstantiated.Global
            internal class TypeImplementingICloneableByThrowing : ICloneable
            {
                public object Clone() => throw new NotImplementedException();
            }

            [Theory, AutoData]
            internal void Calls_Clone_if_type_implements_ICloneable(TypeImplementingICloneableByThrowing instance)
            {
                // Act
                Action act = () => instance.GetSnapshot();

                // Assert
                act.Should().Throw<NotImplementedException>(because: "the type has implemented the Clone method that way");
            }
            
            // ReSharper disable once ClassNeverInstantiated.Global
            internal class TypeImplementingICloneable : ICloneable
            {
                public object Clone() => (TypeImplementingICloneable) this.MemberwiseClone();
            }

            [Theory, AutoData]
            internal void Supports_type_implementing_ICloneable(TypeImplementingICloneable instance)
            {
                // Act
                var snapshot = instance.GetSnapshot();

                // Assert
                snapshot.Should().NotBeNull();
                snapshot.Should().NotBeSameAs(instance);
            }

            // ReSharper disable once ClassNeverInstantiated.Global
            internal class TypeWithCopyConstructorByThrowing
            {
                // ReSharper disable once UnusedMember.Global
                public TypeWithCopyConstructorByThrowing() { }

                // ReSharper disable once UnusedParameter.Local
                // ReSharper disable once UnusedMember.Global
                public TypeWithCopyConstructorByThrowing(TypeWithCopyConstructorByThrowing otherInstance)
                {
                    throw new NotImplementedException();
                }
            }
            
            [Theory, AutoData]
            internal void Calls_copy_constructor_if_type_has_one(TypeWithCopyConstructorByThrowing instance)
            {
                // Act
                Action act = () => instance.GetSnapshot();

                // Assert
                act.Should().Throw<TargetInvocationException>(because: "the type has implemented the copy constructor that way");
            }
            
            internal class TypeWithCopyConstructor
            {
                public string Text { get; }

                // ReSharper disable once UnusedMember.Global
                public TypeWithCopyConstructor(string text)
                {
                    Text = text;
                }

                // ReSharper disable once UnusedMember.Global
                public TypeWithCopyConstructor(TypeWithCopyConstructor otherInstance)
                {
                    Text = string.Copy(otherInstance.Text);
                }
            }
            
            [Theory, AutoData]
            internal void Supports_type_with_copy_constructor(TypeWithCopyConstructor instance)
            {
                // Act
                var snapshot = instance.GetSnapshot();

                // Assert
                snapshot.Should().NotBeNull();
                snapshot.Should().NotBeSameAs(instance);
                snapshot.Text.Should().NotBeSameAs(instance.Text);
            }
        }
        
        public class TypeRegistrationInAssemblies
        {
            private class SpecialType { }

            private class TypeClonerThrowingException : ITypeCloner<SpecialType>
            {
                public SpecialType Clone(SpecialType instance) => throw new NotImplementedException();
            }

            // ReSharper disable once UnusedType.Global
            public class TypeClonerThrowingExceptionRegistrar : ITypeClonerRegistrar
            {
                public void RegisterTypeCloners(ITypeRegistryBuilder typeRegistryBuilder)
                {
                    typeRegistryBuilder.RegisterCloner(typeof(SpecialType), new TypeClonerThrowingException());
                }
            }

            [Fact]
            public void Calls_type_cloner_registrars_in_other_assemblies()
            {
                // Arrange
                var specialType = new SpecialType();

                // Act
                Action act = () => specialType.GetSnapshot();

                // Assert
                act.Should().Throw<NotImplementedException>();
            }

        }

        // ReSharper disable once MemberCanBePrivate.Global
        public class TestData
        {
            internal class PrimitiveTypes : IEnumerable<object[]>
            {
                public IEnumerator<object[]> GetEnumerator()
                {
                    yield return new object[] { default(bool) };
                    yield return new object[] { default(byte) };
                    yield return new object[] { default(char) };
                    yield return new object[] { default(double) };
                    yield return new object[] { default(float) };
                    yield return new object[] { default(int) };
                    yield return new object[] { default(long) };
                    yield return new object[] { default(sbyte) };
                    yield return new object[] { default(short) };
                    yield return new object[] { default(uint) };
                    yield return new object[] { default(ulong) };
                    yield return new object[] { default(ushort) };
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }
            }

            internal class ComplexTypes : CollectionTypesWithAssertions, IEnumerable<object[]>
            {
                public override IEnumerator<object[]> GetEnumerator()
                {
                    var fixture = new Fixture();

                    yield return TestCase(fixture.Create<Guid>());

                    using (var enumerator = base.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            // Return only the type
                            // ReSharper disable once PossibleNullReferenceException
                            yield return new[] { enumerator.Current[0] };
                        }
                    }
                    
                    object[] TestCase<T>(T value)
                    {
                        return new object[] { value };
                    }
                }

                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }

            internal class CollectionTypesWithAssertions : IEnumerable<object[]>
            {
                public virtual IEnumerator<object[]> GetEnumerator()
                {
                    var fixture = new Fixture();

                    yield return TestCase(
                        fixture.CreateMany<int>().ToArray(),
                        (original, snapshot) =>
                        {
                            snapshot.Should().BeEquivalentTo(original);
                            snapshot.Should().ContainInOrder(original);
                        }
                    );
                    
                    yield return TestCase(
                        new List<int>(fixture.CreateMany<int>()),
                        (original, snapshot) =>
                        {
                            snapshot.Should().BeEquivalentTo(original);
                            snapshot.Should().ContainInOrder(original);
                        }
                    );
                    
                    yield return TestCase(
                        new SortedList<int, int>(fixture.Create<Dictionary<int, int>>(), Comparer<int>.Default),
                        (original, snapshot) =>
                        {
                            snapshot.Should().BeEquivalentTo(original);
                            snapshot.Should().ContainKeys(original.Keys);
                            snapshot.Should().ContainValues(original.Values);
                            snapshot.Should().HaveSameCount(original);
                        }
                    );
                    
                    yield return TestCase(
                        new LinkedList<int>(fixture.CreateMany<int>()),
                        (original, snapshot) =>
                        {
                            snapshot.Should().BeEquivalentTo(original);
                            snapshot.Should().ContainInOrder(original);
                        }
                    );

                    yield return TestCase(
                        fixture.Create<Dictionary<string, string>>(),
                        (original, snapshot) =>
                        {
                            snapshot.Should().BeEquivalentTo(original);
                            snapshot.Should().Contain(original);
                        }
                    );
                    
                    yield return TestCase(
                        fixture.Create<SortedDictionary<string, string>>(),
                        (original, snapshot) =>
                        {
                            snapshot.Should().BeEquivalentTo(original);
                            snapshot.Should().Contain(original);
                        }
                    );

                    yield return TestCase(
                        new Queue<int>(fixture.CreateMany<int>()),
                        (original, snapshot) =>
                        {
                            snapshot.Should().BeEquivalentTo(original);
                            snapshot.Should().ContainInOrder(original);
                        }
                    );
                    
                    yield return TestCase(
                        new HashSet<int>(fixture.CreateMany<int>()),
                        (original, snapshot) =>
                        {
                            snapshot.Should().BeEquivalentTo(original);
                            snapshot.Should().ContainInOrder(original);
                        }
                    );
                    
                    yield return TestCase(
                        new SortedSet<int>(fixture.CreateMany<int>()),
                        (original, snapshot) =>
                        {
                            snapshot.Should().BeEquivalentTo(original);
                            snapshot.Should().ContainInOrder(original);
                        }
                    );

                    yield return TestCase(
                        new Stack<int>(fixture.CreateMany<int>()),
                        (original, snapshot) =>
                        {
                            snapshot.Should().BeEquivalentTo(original);
                            snapshot.Should().ContainInOrder(original);
                        });

                    object[] TestCase<T>(T value, Action<T, T> assertion)
                    {
                        return new object[] { value, assertion };
                    }
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }
            }
        }
    }
}