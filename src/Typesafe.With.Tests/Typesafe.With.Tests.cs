using System;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Typesafe.With.Tests
{
    internal class SourceWithConstructor {
        public string Id { get; }
        public string Name { get; }
        public int? Age { get; }

        public SourceWithConstructor(string id, string name, int? age) => (Id, Name, Age) = (id, name, age);
    }

    internal class SourceWithSetters {
        public string Id { get; set; }
        public string Name { get; set; }
        public int? Age { get; set; }
    }

    internal class SourceWithMixedConstructorAndSetters {
        public string Id { get; }
        public string Name { get; }
        public int? Age { get; set; }

        public SourceWithMixedConstructorAndSetters(string id, string name) => (Id, Name) = (id, name);
    }

    internal class Container<T>
    {
        public T Value { get; set; }
    }

    public class Tests
    {
        public class PropertyCopy
        {
            private const string TextValue = "Text";
            
            private class TypeWithNoSetter
            {
                public string Text { get; }
                public int Age { get; }

                public TypeWithNoSetter(int age)
                {
                    Age = age;
                    Text = TextValue;
                }
            }

            private class TypeWithPrivateSetter
            {
                public string Text { get; private set; }
                public int Age { get; }

                public TypeWithPrivateSetter(int age)
                {
                    Age = age;
                    Text = TextValue;
                }
            }

            private class TypeWithInternalSetter
            {
                public string Text { get; internal set; }
                public int Age { get; }

                public TypeWithInternalSetter(int age)
                {
                    Age = age;
                    Text = TextValue;
                }
            }
            
            private class TypeWithExpressionBodiedProperty
            {
                public string Text => Age.ToString();

                public int Age { get; }

                public TypeWithExpressionBodiedProperty(int age)
                {
                    Age = age;
                }
            }
            
            [Theory]
            [MemberData(nameof(TestData))]
            public void Can_handle_properties_with_no_setter(int originalValue, int newValue)
            {
                // Arrange
                var source = new TypeWithNoSetter(originalValue);
                
                // Act
                var result = source.With(a => a.Age, newValue);
                
                // Assert
                result.Should().NotBeNull();
                result.Age.Should().Be(newValue);
                result.Text.Should().Be(TextValue);
            }
            
            [Theory]
            [MemberData(nameof(TestData))]
            public void Can_handle_properties_with_private_setter(int originalValue, int newValue)
            {
                // Arrange
                var source = new TypeWithPrivateSetter(originalValue);
                
                // Act
                var result = source.With(a => a.Age, newValue);
                
                // Assert
                result.Should().NotBeNull();
                result.Age.Should().Be(newValue);
                result.Text.Should().Be(TextValue);
            }
            
            [Theory]
            [MemberData(nameof(TestData))]
            public void Can_handle_properties_with_internal_setter(int originalValue, int newValue)
            {
                // Arrange
                var source = new TypeWithInternalSetter(originalValue);
                
                // Act
                var result = source.With(a => a.Age, newValue);
                
                // Assert
                result.Should().NotBeNull();
                result.Age.Should().Be(newValue);
                result.Text.Should().Be(TextValue);
            }
            
            [Theory]
            [MemberData(nameof(TestData))]
            public void Can_handle_expression_bodied_property(int originalValue, int newValue)
            {
                // Arrange
                var source = new TypeWithExpressionBodiedProperty(originalValue);
                
                // Act
                var result = source.With(a => a.Age, newValue);
                
                // Assert
                result.Should().NotBeNull();
                result.Age.Should().Be(newValue);
                result.Text.Should().Be(newValue.ToString());
            }

            public static IEnumerable<object[]> TestData
            {
                get
                {
                    yield return new object [] {int.MinValue, 0};
                    yield return new object [] {-1, 1};
                    yield return new object [] {0, int.MinValue};
                    yield return new object [] {0, int.MaxValue};
                    yield return new object [] {int.MaxValue, -1};
                }
            }

        }

        public class General
        {
            private class TypeWithoutWritableProperty
            {
                public string Text { get; }
            }
            
            [Fact]
            public void With_fails_if_property_is_not_writable()
            {
                // Arrange
                var source = new TypeWithoutWritableProperty();
                
                // Act
                Func<TypeWithoutWritableProperty> act = () => source.With(s => s.Text, "Text");
                
                // Assert
                act.Should()
                    .Throw<InvalidOperationException>(
                        because: $"the property '{nameof(TypeWithoutWritableProperty.Text)}' is not writable");
            }
        }
        
        [Theory]
        [MemberData(nameof(With_works_with_any_type_Data))]
        public void With_works_with_any_type<T>(T sourceValue, T withValue, T expectedValue)
        {
            // Arrange
            var source = new Container<T> {Value = sourceValue};
            
            // Act
            var result = source.With(_ => _.Value, withValue);
            
            // Assert
            result.Should().BeOfType<Container<T>>();
            result.Value.Should().Be(expectedValue);
        }

        // ReSharper disable once InconsistentNaming
        public static IEnumerable<object[]> With_works_with_any_type_Data
        {
            get
            {
                yield return new object[] {0, 1, 1};
                yield return new object[] {(short) 0, (short) 1, (short) 1};
                yield return new object[] {(ushort) 0, (ushort) 1, (ushort) 1};
                yield return new object[] {0u, 1u, 1u};
                yield return new object[] {0ul, 1ul, 1ul};
                yield return new object[] {0L, 1L, 1L};
//                yield return new object[] {0M, 1M, 1M};
                yield return new object[] {0d, 1d, 1d};
                yield return new object[] {0f, 1f, 1f};
                yield return new object[] {false, true, true};
//                yield return new object[] {"Hello", "World", "World"};
                yield return new object[] {'a', 'b', 'b'};
                yield return new object[] {(byte) 0, (byte) 1, (byte) 1};
            }
        }
        
        [Fact]
        public void Calling_With_creates_a_new_instance()
        {
            // Arrange
            var source = new SourceWithSetters();
                
            // Act
            var result = source.With(s => s.Id, "T");

            // Assert
            result.GetHashCode().Should().NotBe(source.GetHashCode());
        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void Can_call_With_on_type_with_only_property_setters(
            string sourceId, string sourceName, int? sourceAge,
            string withId, string withName, int? withAge,
            string expectedId, string expectedName, int? expectedAge)
        {
            // Arrange
            var source = new SourceWithSetters
            {
                Id = sourceId,
                Name = sourceName,
                Age = sourceAge
            };
            
            // Act
            var result = source
                .With(_ => _.Id, withId)
                .With(_ => _.Name, withName)
                .With(_ => _.Age, withAge);

            // Assert
            result.Should().BeOfType<SourceWithSetters>();
            result.Age.Should().Be(expectedAge);
            result.Id.Should().Be(expectedId);
            result.Name.Should().Be(expectedName);
        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void Can_call_With_on_type_with_only_constructor(
            string sourceId, string sourceName, int? sourceAge,
            string withId, string withName, int? withAge,
            string expectedId, string expectedName, int? expectedAge)
        {
            // Arrange
            var source = new SourceWithConstructor(sourceId, sourceName, sourceAge);

            // Act
            var result = source
                .With(_ => _.Id, withId)
                .With(_ => _.Name, withName)
                .With(_ => _.Age, withAge);

            // Assert
            result.Should().BeOfType<SourceWithConstructor>();
            result.Age.Should().Be(expectedAge);
            result.Id.Should().Be(expectedId);
            result.Name.Should().Be(expectedName);
        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void Can_call_With_on_type_with_both_property_setter_and_constructor(
            string sourceId, string sourceName, int? sourceAge,
            string withId, string withName, int? withAge,
            string expectedId, string expectedName, int? expectedAge)
        {
            // Arrange
            var source = new SourceWithMixedConstructorAndSetters(sourceId, sourceName) {Age = sourceAge};

            // Act
            var result = source
                .With(_ => _.Id, withId)
                .With(_ => _.Name, withName)
                .With(_ => _.Age, withAge);

            // Assert
            result.Should().BeOfType<SourceWithMixedConstructorAndSetters>();
            result.Age.Should().Be(expectedAge);
            result.Id.Should().Be(expectedId);
            result.Name.Should().Be(expectedName);
            
        }
        
        public class TestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[]{null, null, null, null, null, null, null, null, null};
                yield return new object[]{null, null, 1,    null, null, null, null, null, null};
                yield return new object[]{null, "1",  null, null, null, null, null, null, null};
                yield return new object[]{null, "1",  1,    null, null, null, null, null, null};
                yield return new object[]{"1",  null, null, null, null, null, null, null, null};
                yield return new object[]{"1",  null, 1,    null, null, null, null, null, null};
                yield return new object[]{"1",  "1",  null, null, null, null, null, null, null};
                yield return new object[]{"1",  "1",  1,    null, null, null, null, null, null};
                yield return new object[]{null, null, null, null, null, 2,    null, null, 2};
                yield return new object[]{null, null, null, null, "2",  null, null, "2",  null};
                yield return new object[]{null, null, null, null, "2",  2,    null, "2",  2};
                yield return new object[]{null, null, null, "2",  null, null, "2",  null, null};
                yield return new object[]{null, null, null, "2",  null, 2,    "2",  null, 2};
                yield return new object[]{null, null, null, "2",  "2",  null, "2",  "2",  null};
                yield return new object[]{null, null, null, "2",  "2",  2,    "2",  "2",  2};
                yield return new object[]{null, null, 1,    null, null, 2,    null, null, 2};
                yield return new object[]{null, "1",  null, null, "2",  null, null, "2",  null};
                yield return new object[]{null, "1",  1,    null, "2",  2,    null, "2",  2};
                yield return new object[]{"1",  null, null, "2",  null, null, "2",  null, null};
                yield return new object[]{"1",  null, 1,    "2",  null, 2,    "2",  null, 2};
                yield return new object[]{"1",  "1",  1,    "2",  "2",  2,    "2",  "2",  2};
                yield return new object[]{"1",  "1",  1,    null, null, null, null, null, null};
                yield return new object[]{"1",  "1",  1,    null, null, 2,    null, null, 2};
                yield return new object[]{"1",  "1",  1,    null, "2",  null, null, "2",  null};
                yield return new object[]{"1",  "1",  1,    null, "2",  2,    null, "2",  2};
                yield return new object[]{"1",  "1",  1,    "2",  null, null, "2",  null, null};
                yield return new object[]{"1",  "1",  1,    "2",  null, 2,    "2",  null, 2};
                yield return new object[]{"1",  "1",  1,    "2",  "2",  null, "2",  "2",  null};
                yield return new object[]{"1",  "1",  1,    "2",  "2",  2,    "2",  "2",  2};
                yield return new object[]{null, null, null, "2",  "2",  2,    "2",  "2",  2};
                yield return new object[]{null, null, 1,    "2",  "2",  2,    "2",  "2",  2};
                yield return new object[]{null, "1", null,  "2",  "2",  2,    "2",  "2",  2};
                yield return new object[]{null, "1", 1,     "2",  "2",  2,    "2",  "2",  2};
                yield return new object[]{"1",  null, null, "2",  "2",  2,    "2",  "2",  2};
                yield return new object[]{"1",  null, 1,    "2",  "2",  2,    "2",  "2",  2};
                yield return new object[]{"1",  "1",  null, "2",  "2",  2,    "2",  "2",  2};
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}