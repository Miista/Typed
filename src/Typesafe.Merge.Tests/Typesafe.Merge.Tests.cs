using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace Typesafe.Merge.Tests
{
    internal class TypeWithConstructor
    {
        public string Id { get; }
        public string Name { get; }
        public int? Age { get; }

        public TypeWithConstructor(string id, string name, int? age) => (Id, Name, Age) = (id, name, age);
    }
    
    internal class TypeWithPropertySetters
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int? Age { get; set; }
    }
    
    internal class TypeWithConstructorAndPropertySetters
    {
        public string Id { get; }
        public string Name { get; set; }
        public int? Age { get; set; }

        public TypeWithConstructorAndPropertySetters(string id, string name) => (Id, Name) = (id, name);
    }

    public class Tests
    {
        public class General
        {
            internal class Container<T>
            {
                public T Value { get; set; }
            }
            
            [Theory]
            [MemberData(nameof(With_works_with_any_type_Data))]
            public void With_works_with_any_type<T>(T sourceValue, T destinationValue, T expectedValue)
            {
                // Arrange
                var source = new Container<T> {Value = sourceValue};
                var destination = new Container<T> {Value = destinationValue};

                // Act
                var result = source.Merge(destination);

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
            public void Calling_Merge_creates_a_new_instance()
            {
                // Arrange
                var source = new TypeWithPropertySetters();
                var destination = new TypeWithPropertySetters();

                // Act
                var result = source.Merge(destination);

                // Assert
                result.GetHashCode().Should().NotBe(source.GetHashCode());
            }
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void Can_merge_type_with_property_setters(
            string sourceId, string sourceName, int? sourceAge,
            string withId, string withName, int? withAge,
            string expectedId, string expectedName, int? expectedAge)
        {
            // Arrange
            var source = new TypeWithPropertySetters
            {
                Id = sourceId,
                Name = sourceName,
                Age = sourceAge
            };
            var destination = new TypeWithPropertySetters
            {
                Id = withId,
                Name = withName,
                Age = withAge
            };
            
            // Act
            var result = source.Merge(destination);

            // Assert
            result.Should().BeOfType<TypeWithPropertySetters>();
            result.Age.Should().Be(expectedAge);
            result.Id.Should().Be(expectedId);
            result.Name.Should().Be(expectedName);
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void Can_merge_type_with_constructor(
            string sourceId, string sourceName, int? sourceAge,
            string withId, string withName, int? withAge,
            string expectedId, string expectedName, int? expectedAge)
        {
            // Arrange
            var source = new TypeWithConstructor(sourceId, sourceName, sourceAge);
            var destination = new TypeWithConstructor(withId, withName, withAge);

            // Act
            var result = source.Merge(destination);

            // Assert
            result.Should().BeOfType<TypeWithConstructor>();
            result.Age.Should().Be(expectedAge);
            result.Id.Should().Be(expectedId);
            result.Name.Should().Be(expectedName);
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void Can_merge_type_with_property_setter_and_constructor(
            string sourceId, string sourceName, int? sourceAge,
            string withId, string withName, int? withAge,
            string expectedId, string expectedName, int? expectedAge)
        {
            // Arrange
            var source = new TypeWithConstructorAndPropertySetters(sourceId, sourceName) {Age = sourceAge};
            var destination = new TypeWithConstructorAndPropertySetters(withId, withName) {Age = withAge};

            // Act
            var result = source.Merge(destination);

            // Assert
            result.Should().BeOfType<TypeWithConstructorAndPropertySetters>();
            result.Age.Should().Be(expectedAge);
            result.Id.Should().Be(expectedId);
            result.Name.Should().Be(expectedName);
            
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void Can_merge_from_property_to_property(
            string sourceId, string sourceName, int? sourceAge,
            string destinationId, string destinationName, int? destinationAge,
            string expectedId, string expectedName, int? expectedAge)
        {
            // Arrange
            var source = new TypeWithPropertySetters
            {
                Id = sourceId,
                Age = sourceAge,
                Name = sourceName
            };
            var destination = new TypeWithPropertySetters
            {
                Id = destinationId,
                Age = destinationAge,
                Name = destinationName
            };

            // Act
            var result = source.Merge(destination);

            // Assert
            result.Should().BeOfType<TypeWithPropertySetters>();
            result.Age.Should().Be(expectedAge);
            result.Id.Should().Be(expectedId);
            result.Name.Should().Be(expectedName);
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void Can_merge_from_constructor_to_constructor(
            string sourceId, string sourceName, int? sourceAge,
            string destinationId, string destinationName, int? destinationAge,
            string expectedId, string expectedName, int? expectedAge)
        {
            // Arrange
            var source = new TypeWithConstructor(sourceId, sourceName, sourceAge);
            var destination = new TypeWithConstructor(destinationId, destinationName, destinationAge);

            // Act
            var result = source.Merge(destination);

            // Assert
            result.Should().BeOfType<TypeWithConstructor>();
            result.Age.Should().Be(expectedAge);
            result.Id.Should().Be(expectedId);
            result.Name.Should().Be(expectedName);
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void Can_merge_from_property_to_constructor(
            string sourceId, string sourceName, int? sourceAge,
            string destinationId, string destinationName, int? destinationAge,
            string expectedId, string expectedName, int? expectedAge)
        {
            // Arrange
            var source = new TypeWithPropertySetters
            {
                Id = sourceId,
                Age = sourceAge,
                Name = sourceName
            };
            var destination = new TypeWithConstructor(destinationId, destinationName, destinationAge);

            // Act
            var result = source.Merge<TypeWithConstructor, TypeWithPropertySetters, TypeWithConstructor>(destination);

            // Assert
            result.Should().BeOfType<TypeWithConstructor>();
            result.Age.Should().Be(expectedAge);
            result.Id.Should().Be(expectedId);
            result.Name.Should().Be(expectedName);
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void Can_merge_from_constructor_to_property(
            string sourceId, string sourceName, int? sourceAge,
            string destinationId, string destinationName, int? destinationAge,
            string expectedId, string expectedName, int? expectedAge)
        {
            // Arrange
            var source = new TypeWithConstructor(sourceId, sourceName, sourceAge);
            var destination = new TypeWithPropertySetters
            {
                Id = destinationId,
                Age = destinationAge,
                Name = destinationName
            };

            // Act
            var result = source.Merge<TypeWithPropertySetters, TypeWithConstructor, TypeWithPropertySetters>(destination);

            // Assert
            result.Should().BeOfType<TypeWithPropertySetters>();
            result.Age.Should().Be(expectedAge);
            result.Id.Should().Be(expectedId);
            result.Name.Should().Be(expectedName);
        }

        public class PartialProperties
        {
            internal class SourceWithPropertiesAndMissingProperties
            {
                public string Id { get; set; }
                public string Name { get; set; }
                public int? Age { get; set; }
                public DateTime Birthdate { get; set; }
            }
    
            internal class DestinationWithPropertiesAndMissingProperties
            {
                public string Id { get; set; }
            }
    
            internal class SourceWithConstructorAndMissingProperties
            {
                public string Id { get; }
                public string Name { get; }
                public int? Age { get; }
                public DateTime Birthdate { get; }

                public SourceWithConstructorAndMissingProperties(string id, string name, int? age, DateTime birthdate)
                {
                    Id = id;
                    Name = name;
                    Age = age;
                    Birthdate = birthdate;
                }
            }
    
            internal class DestinationWithConstructorAndMissingProperties
            {
                public string Id { get; }

                public DestinationWithConstructorAndMissingProperties(string id)
                {
                    Id = id;
                }
            }
            
            [Fact]
            public void Can_merge_using_constructor_when_right_side_is_missing_properties()
            {
                // Arrange
                var fixture = new Fixture();
                var left = fixture.Create<SourceWithPropertiesAndMissingProperties>();
                var right = fixture.Create<DestinationWithPropertiesAndMissingProperties>();
            
                // Act
                var result = left.Merge<SourceWithPropertiesAndMissingProperties, SourceWithPropertiesAndMissingProperties, DestinationWithPropertiesAndMissingProperties>(right);
            
                // Assert
                result.Id.Should().Be(right.Id);
                result.Age.Should().Be(left.Age, because: "the right side does not have this property");
                result.Name.Should().Be(left.Name, because: "the right side does not have this property");
                result.Birthdate.Should().Be(left.Birthdate, because: "the right side does not have this property");
            }
            
            [Fact]
            public void Uses_properties_from_the_left_side_if_missing_on_the_right_side()
            {
                // Arrange
                var fixture = new Fixture();
                var left = fixture.Create<SourceWithPropertiesAndMissingProperties>();
                var right = fixture.Create<DestinationWithPropertiesAndMissingProperties>();
            
                // Act
                var result = left.Merge<SourceWithPropertiesAndMissingProperties, SourceWithPropertiesAndMissingProperties, DestinationWithPropertiesAndMissingProperties>(right);
            
                // Assert
                result.Id.Should().Be(right.Id);
                result.Age.Should().Be(left.Age, because: "the right side does not have this property");
                result.Name.Should().Be(left.Name, because: "the right side does not have this property");
                result.Birthdate.Should().Be(left.Birthdate, because: "the right side does not have this property");
            }
        
            [Fact]
            public void Can_merge_using_properties_when_right_side_is_missing_properties()
            {
                // Arrange
                var fixture = new Fixture();
                var left = fixture.Create<SourceWithConstructorAndMissingProperties>();
                var right = fixture.Create<DestinationWithConstructorAndMissingProperties>();
            
                // Act
                var result = left.Merge<SourceWithConstructorAndMissingProperties, SourceWithConstructorAndMissingProperties, DestinationWithConstructorAndMissingProperties>(right);
            
                // Assert
                result.Id.Should().Be(right.Id);
                result.Age.Should().Be(left.Age);
                result.Name.Should().Be(left.Name);
                result.Birthdate.Should().Be(left.Birthdate);
            }
        }

        public class UnsafeMerge
        {
            internal class LeftSide
            {
                public string Name { get; set; }
            }

            internal class RightSide
            {
                public int Age { get; set; }
            }

            internal class Destination
            {
                public string Name { get; set; }
                public int Age { get; set; }
            }
            
            [Fact]
            public void Can_merge_to_a_third_destination_type()
            {
                // Arrange
                var left = new LeftSide {Name = "Test"};
                var right = new RightSide {Age = 10};
                
                // Act
                var result = left.Merge<Destination, LeftSide, RightSide>(right);

                // Assert
                result.Should().NotBeNull();
                result.Name.Should().Be(left.Name);
                result.Age.Should().Be(right.Age);
            }
        }
        
        public static IEnumerable<object[]> TestData
        {
            get
            {
                yield return new object[]{null, null, null, null, null, null, null, null, null};
                yield return new object[]{null, null, 1,    null, null, null, null, null, 1};
                yield return new object[]{null, "1",  null, null, null, null, null, "1",  null};
                yield return new object[]{null, "1",  1,    null, null, null, null, "1",  1};
                yield return new object[]{"1",  null, null, null, null, null, "1",  null, null};
                yield return new object[]{"1",  null, 1,    null, null, null, "1",  null, 1};
                yield return new object[]{"1",  "1",  null, null, null, null, "1",  "1",  null};
                yield return new object[]{"1",  "1",  1,    null, null, null, "1",  "1",  1};
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
                yield return new object[]{"1",  "1",  1,    null, null, null, "1",  "1",  1};
                yield return new object[]{"1",  "1",  1,    null, null, 2,    "1",  "1",  2};
                yield return new object[]{"1",  "1",  1,    null, "2",  null, "1",  "2",  1};
                yield return new object[]{"1",  "1",  1,    null, "2",  2,    "1",  "2",  2};
                yield return new object[]{"1",  "1",  1,    "2",  null, null, "2",  "1",  1};
                yield return new object[]{"1",  "1",  1,    "2",  null, 2,    "2",  "1",  2};
                yield return new object[]{"1",  "1",  1,    "2",  "2",  null, "2",  "2",  1};
                yield return new object[]{"1",  "1",  1,    "2",  "2",  2,    "2",  "2",  2};
                yield return new object[]{null, null, null, "2",  "2",  2,    "2",  "2",  2};
                yield return new object[]{null, null, 1,    "2",  "2",  2,    "2",  "2",  2};
                yield return new object[]{null, "1", null,  "2",  "2",  2,    "2",  "2",  2};
                yield return new object[]{null, "1", 1,     "2",  "2",  2,    "2",  "2",  2};
                yield return new object[]{"1",  null, null, "2",  "2",  2,    "2",  "2",  2};
                yield return new object[]{"1",  null, 1,    "2",  "2",  2,    "2",  "2",  2};
                yield return new object[]{"1",  "1",  null, "2",  "2",  2,    "2",  "2",  2};
            }
        }

        public class ValueResolvement
        {
            internal class OneSideWithConstructor
            {
                public string OneTargetId { get; }

                public OneSideWithConstructor(string oneTargetId)
                {
                    OneTargetId = oneTargetId;
                }
            }

            internal class AnotherSideWithConstructor
            {
                public string AnotherTargetId { get; }
        
                public AnotherSideWithConstructor(string anotherTargetId)
                {
                    AnotherTargetId = anotherTargetId;
                }
            }

            internal class TargetOfLeftAndRightWithConstructor
            {
                public string OneTargetId { get; }
                public string AnotherTargetId { get; }

                public TargetOfLeftAndRightWithConstructor(string oneTargetId, string anotherTargetId)
                {
                    OneTargetId = oneTargetId;
                    AnotherTargetId = anotherTargetId;
                }
            }
    
            internal class OneSideWithProperties
            {
                public string OneTargetId { get; set; }
            }

            internal class AnotherSideWithProperties
            {
                public string AnotherTargetId { get; set; }
            }

            internal class TargetOfLeftAndRightWithProperties
            {
                public string OneTargetId { get; set; }
                public string AnotherTargetId { get; set; }
            }
            
            [Theory]
            [InlineData(null, "RightId", "RightId")]
            [InlineData("LeftId", null, null)]
            [InlineData("LeftId", "RightId", "RightId")]
            public void Takes_value_from_right_side_if_left_side_is_unmatched_using_constructor(
                string leftId,
                string rightId,
                string targetId)
            {
                // Arrange
                var left = new AnotherSideWithConstructor(leftId);
                var right = new OneSideWithConstructor(rightId);

                // Act
                var result =
                    ObjectExtensions
                        .Merge<TargetOfLeftAndRightWithConstructor, AnotherSideWithConstructor, OneSideWithConstructor>(
                            left,
                            right);

                // Assert
                result.OneTargetId.Should().Be(targetId);
            }

            [Theory]
            [InlineData(null, "RightId", "RightId")]
            [InlineData("LeftId", null, null)]
            [InlineData("LeftId", "RightId", "RightId")]
            public void Takes_value_from_right_side_if_left_side_is_unmatched_using_properties(
                string leftId,
                string rightId,
                string targetId)
            {
                // Arrange
                var left = new AnotherSideWithProperties {AnotherTargetId = leftId};
                var right = new OneSideWithProperties {OneTargetId = rightId};

                // Act
                var result =
                    ObjectExtensions
                        .Merge<TargetOfLeftAndRightWithProperties, AnotherSideWithProperties, OneSideWithProperties>(
                            left,
                            right);

                // Assert
                result.OneTargetId.Should().Be(targetId);
            }

            [Theory]
            [InlineData("LeftId", null, "LeftId")]
            [InlineData(null, "RightId", null)]
            [InlineData("LeftId", "RightId", "LeftId")]
            public void Takes_value_from_left_side_if_right_side_is_unmatched_using_constructor(
                string leftId,
                string rightId,
                string targetId)
            {
                // Arrange
                var left = new AnotherSideWithConstructor(leftId);
                var right = new OneSideWithConstructor(rightId);

                // Act
                var result =
                    ObjectExtensions
                        .Merge<TargetOfLeftAndRightWithConstructor, AnotherSideWithConstructor, OneSideWithConstructor>(
                            left,
                            right);

                // Assert
                result.AnotherTargetId.Should().Be(targetId);
            }

            [Theory]
            [InlineData("LeftId", null, "LeftId")]
            [InlineData(null, "RightId", null)]
            [InlineData("LeftId", "RightId", "LeftId")]
            public void Takes_value_from_left_side_if_right_side_is_unmatched_using_properties(
                string leftId,
                string rightId,
                string targetId)
            {
                // Arrange
                var left = new AnotherSideWithProperties {AnotherTargetId = leftId};
                var right = new OneSideWithProperties {OneTargetId = rightId};

                // Act
                var result =
                    ObjectExtensions
                        .Merge<TargetOfLeftAndRightWithProperties, AnotherSideWithProperties, OneSideWithProperties>(
                            left,
                            right);

                // Assert
                result.AnotherTargetId.Should().Be(targetId);
            }
        }
    }
}