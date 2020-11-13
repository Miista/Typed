using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace Typed.Merge.Tests
{
    internal class SourceWithConstructor {
        public string Id { get; }
        public string Name { get; }
        public int? Age { get; }

        public SourceWithConstructor(string id, string name, int? age) => (Id, Name, Age) = (id, name, age);
    }

    internal class DestinationWithConstructor {
        public string Id { get; }
        public string Name { get; }
        public int? Age { get; }
        
        public DestinationWithConstructor(string id, string name, int? age) => (Id, Name, Age) = (id, name, age);
    }
    
    internal class SourceWithSetters {
        public string Id { get; set; }
        public string Name { get; set; }
        public int? Age { get; set; }
    }

    internal class DestinationWithSetters {
        public string Id { get; set; }
        public string Name { get; set; }
        public int? Age { get; set; }
    }

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
    
    public class TypeMergerTests
    {
        [Theory]
        [MemberData(nameof(TestData))]
        public void Can_merge_types_using_constructors(
            string sourceId, string sourceName, int? sourceAge,
            string destinationId, string destinationName, int? destinationAge,
            string expectedId, string expectedName, int? expectedAge)
        {
            // Arrange
            var source = new SourceWithConstructor(sourceId, sourceName, sourceAge);
            var destination = new DestinationWithConstructor(destinationId, destinationName, destinationAge);

            // Act
            var result = TypeMerger.Merge<SourceWithConstructor, SourceWithConstructor, DestinationWithConstructor>(source, destination);

            // Assert
            result.Age.Should().Be(expectedAge);
            result.Id.Should().Be(expectedId);
            result.Name.Should().Be(expectedName);
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void Can_merge_types_using_properties(
            string sourceId, string sourceName, int? sourceAge,
            string destinationId, string destinationName, int? destinationAge,
            string expectedId, string expectedName, int? expectedAge)
        {
            // Arrange
            var source = new SourceWithSetters
            {
                Id = sourceId,
                Age = sourceAge,
                Name = sourceName
            };
            var destination = new DestinationWithSetters
            {
                Id = destinationId,
                Age = destinationAge,
                Name = destinationName
            };

            // Act
            var result = TypeMerger.Merge<SourceWithSetters, SourceWithSetters, DestinationWithSetters>(source, destination);

            // Assert
            result.Age.Should().Be(expectedAge);
            result.Id.Should().Be(expectedId);
            result.Name.Should().Be(expectedName);
        }
        
        [Theory]
        [MemberData(nameof(TestData))]
        public void Can_merge_from_property_based_to_constructor_based(
            string sourceId, string sourceName, int? sourceAge,
            string destinationId, string destinationName, int? destinationAge,
            string expectedId, string expectedName, int? expectedAge)
        {
            // Arrange
            var source = new SourceWithConstructor(sourceId, sourceName, sourceAge);
            var destination = new DestinationWithSetters
            {
                Id = destinationId,
                Age = destinationAge,
                Name = destinationName
            };

            // Act
            var result = TypeMerger.Merge<SourceWithConstructor, SourceWithConstructor, DestinationWithSetters>(source, destination);

            // Assert
            result.Age.Should().Be(expectedAge);
            result.Id.Should().Be(expectedId);
            result.Name.Should().Be(expectedName);
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void Can_merge_from_property_based_to_property_based(
            string sourceId, string sourceName, int? sourceAge,
            string destinationId, string destinationName, int? destinationAge,
            string expectedId, string expectedName, int? expectedAge)
        {
            // Arrange
            var source = new SourceWithConstructor(sourceId, sourceName, sourceAge);
            var destination = new SourceWithConstructor(destinationId, destinationName, destinationAge);

            // Act
            var result = TypeMerger.Merge<SourceWithConstructor, SourceWithConstructor, SourceWithConstructor>(source, destination);

            // Assert
            result.Age.Should().Be(expectedAge);
            result.Id.Should().Be(expectedId);
            result.Name.Should().Be(expectedName);
        }
        
        [Theory]
        [MemberData(nameof(TestData))]
        public void Can_merge_from_constructor_based_to_property_based(
            string sourceId, string sourceName, int? sourceAge,
            string destinationId, string destinationName, int? destinationAge,
            string expectedId, string expectedName, int? expectedAge)
        {
            // Arrange
            var source = new SourceWithSetters
            {
                Id = sourceId,
                Age = sourceAge,
                Name = sourceName
            };
            var destination = new DestinationWithConstructor(destinationId, destinationName, destinationAge);

            // Act
            var result = TypeMerger.Merge<SourceWithSetters, SourceWithSetters, DestinationWithConstructor>(source, destination);

            // Assert
            result.Age.Should().Be(expectedAge);
            result.Id.Should().Be(expectedId);
            result.Name.Should().Be(expectedName);
        }
        
        [Theory]
        [MemberData(nameof(TestData))]
        public void Can_merge_from_constructor_based_to_constructor_based(
            string sourceId, string sourceName, int? sourceAge,
            string destinationId, string destinationName, int? destinationAge,
            string expectedId, string expectedName, int? expectedAge)
        {
            // Arrange
            var source = new SourceWithConstructor(sourceId, sourceName, sourceAge);
            var destination = new DestinationWithConstructor(destinationId, destinationName, destinationAge);

            // Act
            var result = TypeMerger.Merge<SourceWithConstructor, SourceWithConstructor, DestinationWithConstructor>(source, destination);

            // Assert
            result.Age.Should().Be(expectedAge);
            result.Id.Should().Be(expectedId);
            result.Name.Should().Be(expectedName);
        }
        
        [Fact]
        public void Can_merge_using_constructor_when_right_side_is_missing_properties()
        {
            // Arrange
            var fixture = new Fixture();
            var left = fixture.Create<SourceWithPropertiesAndMissingProperties>();
            var right = fixture.Create<DestinationWithPropertiesAndMissingProperties>();
            
            // Act
            var result = TypeMerger.Merge<SourceWithPropertiesAndMissingProperties, SourceWithPropertiesAndMissingProperties, DestinationWithPropertiesAndMissingProperties>(left, right);
            
            // Assert
            result.Id.Should().Be(right.Id);
            result.Age.Should().Be(left.Age);
            result.Name.Should().Be(left.Name);
            result.Birthdate.Should().Be(left.Birthdate);
        }
        
        [Fact]
        public void Can_merge_using_properties_when_right_side_is_missing_properties()
        {
            // Arrange
            var fixture = new Fixture();
            var left = fixture.Create<SourceWithConstructorAndMissingProperties>();
            var right = fixture.Create<DestinationWithConstructorAndMissingProperties>();
            
            // Act
            var result = TypeMerger.Merge<SourceWithConstructorAndMissingProperties, SourceWithConstructorAndMissingProperties, DestinationWithConstructorAndMissingProperties>(left, right);
            
            // Assert
            result.Id.Should().Be(right.Id);
            result.Age.Should().Be(left.Age);
            result.Name.Should().Be(left.Name);
            result.Birthdate.Should().Be(left.Birthdate);
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

        [Theory]
        [InlineData(null, "RightId", "RightId")]
        [InlineData("LeftId", null, null)]
        [InlineData("LeftId", "RightId", "RightId")]
        public void Takes_value_from_right_side_if_left_side_is_unmatched_using_constructor(string leftId, string rightId, string targetId)
        {
            // Arrange
            var left = new AnotherSideWithConstructor(leftId);
            var right = new OneSideWithConstructor(rightId);

            // Act
            var result = TypeMerger.Merge<TargetOfLeftAndRightWithConstructor, AnotherSideWithConstructor, OneSideWithConstructor>(left, right);

            // Assert
            result.OneTargetId.Should().Be(targetId);
        }
        
        [Theory]
        [InlineData(null, "RightId", "RightId")]
        [InlineData("LeftId", null, null)]
        [InlineData("LeftId", "RightId", "RightId")]
        public void Takes_value_from_right_side_if_left_side_is_unmatched_using_properties(string leftId, string rightId, string targetId)
        {
            // Arrange
            var left = new AnotherSideWithProperties {AnotherTargetId = leftId};
            var right = new OneSideWithProperties {OneTargetId = rightId};

            // Act
            var result = TypeMerger.Merge<TargetOfLeftAndRightWithProperties, AnotherSideWithProperties, OneSideWithProperties>(left, right);

            // Assert
            result.OneTargetId.Should().Be(targetId);
        }
        
        [Theory]
        [InlineData("LeftId", null, "LeftId")]
        [InlineData(null, "RightId", null)]
        [InlineData("LeftId", "RightId", "LeftId")]
        public void Takes_value_from_left_side_if_right_side_is_unmatched_using_constructor(string leftId, string rightId, string targetId)
        {
            // Arrange
            var left = new AnotherSideWithConstructor(leftId);
            var right = new OneSideWithConstructor(rightId);

            // Act
            var result = TypeMerger.Merge<TargetOfLeftAndRightWithConstructor, AnotherSideWithConstructor, OneSideWithConstructor>(left, right);

            // Assert
            result.AnotherTargetId.Should().Be(targetId);
        }
        
        [Theory]
        [InlineData("LeftId", null, "LeftId")]
        [InlineData(null, "RightId", null)]
        [InlineData("LeftId", "RightId", "LeftId")]
        public void Takes_value_from_left_side_if_right_side_is_unmatched_using_properties(string leftId, string rightId, string targetId)
        {
            // Arrange
            var left = new AnotherSideWithProperties {AnotherTargetId = leftId};
            var right = new OneSideWithProperties {OneTargetId = rightId};

            // Act
            var result = TypeMerger.Merge<TargetOfLeftAndRightWithProperties, AnotherSideWithProperties, OneSideWithProperties>(left, right);

            // Assert
            result.AnotherTargetId.Should().Be(targetId);
        }
    }

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
}