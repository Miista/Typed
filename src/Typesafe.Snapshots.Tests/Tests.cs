using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using Xunit;

namespace Typesafe.Snapshots.Tests
{
    public class Tests
    {
        public class PrimitiveTypeTests
        {
            [Theory]
            [ClassData(typeof(PrimitiveTypes))]
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
                public string Name { get; set; }
                public List<int> Ages { get; set; }
                public StructType ValueType { get; set; }
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
            [ClassData(typeof(ComplexTypes))]
            public void Does_not_throw_on_complex_type<T>(T value)
            {
                // Act
                Func<T> act = () => value.GetSnapshot();

                // Assert
                act.Should().NotThrow();
            }
            
            [Theory]
            [ClassData(typeof(ComplexTypesWithAssertions))]
            public void Can_create_snapshot_of_complex_type_2<T>(T value, Action<T, T> assertion)
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

        private class PrimitiveTypes : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {default(int)};
                yield return new object[] {default(short)};
                yield return new object[] {default(ushort)};
                yield return new object[] {default(uint)};
                yield return new object[] {default(ulong)};
                yield return new object[] {default(long)};
                yield return new object[] {default(double)};
                yield return new object[] {default(float)};
                yield return new object[] {default(bool)};
                yield return new object[] {default(char)};
                yield return new object[] {default(byte)};
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class ComplexTypes : ComplexTypesWithAssertions
        {
            public override IEnumerator<object[]> GetEnumerator()
            {
                using (var enumerator = base.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        yield return new object[] { enumerator.Current[0] };
                    }                    
                }
            }
        }
        
        private class ComplexTypesWithAssertions : IEnumerable<object[]>
        {
            public virtual IEnumerator<object[]> GetEnumerator()
            {
                object[] TestCase<T>(T value, Action<T, T> assertion)
                {
                    return new object[] { value, assertion };
                }
                
                var fixture = new Fixture();

                yield return TestCase(
                    fixture.CreateMany<int>().ToList(),
                    (original, snapshot) => snapshot.Should().BeEquivalentTo(original)
                );
                yield return TestCase(
                    fixture.Create<Guid>(),
                    (original, snapshot) => snapshot.Should().Be(original)
                );
                // yield return new object[] { fixture.Create<Guid>() };
                // yield return new object[] {default(short)};
                // yield return new object[] {default(ushort)};
                // yield return new object[] {default(uint)};
                // yield return new object[] {default(ulong)};
                // yield return new object[] {default(long)};
                // yield return new object[] {default(double)};
                // yield return new object[] {default(float)};
                // yield return new object[] {default(bool)};
                // yield return new object[] {'a'};
                // yield return new object[] {default(byte)};
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}