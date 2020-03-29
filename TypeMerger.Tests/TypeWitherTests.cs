using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace TypeMerger.Tests
{
    public class TypeWitherTests
    {
        [Theory]
        [MemberData(nameof(TestData))]
        public void Can_with_using_expressions_using_properties(
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
            SourceWithSetters result = TypeWither.GetBuilder(source)
                .With(_ => _.Id, withId)
                .With(_ => _.Age, withAge)
                .With(_ => _.Name, withName);

            // Assert
            result.Age.Should().Be(expectedAge);
            result.Id.Should().Be(expectedId);
            result.Name.Should().Be(expectedName);
        }
        
        [Theory]
        [MemberData(nameof(TestData))]
        public void Can_with_using_properties(
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
            SourceWithSetters result = TypeWither.GetBuilder(source)
                .With(_ => _.Id, withId)
                .With(_ => _.Name, withName)
                .With(_ => _.Age, withAge);

            // Assert
            result.Age.Should().Be(expectedAge);
            result.Id.Should().Be(expectedId);
            result.Name.Should().Be(expectedName);
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void Can_with_using_constructor(
            string sourceId, string sourceName, int? sourceAge,
            string withId, string withName, int? withAge,
            string expectedId, string expectedName, int? expectedAge)
        {
            // Arrange
            var source = new SourceWithConstructor(sourceId, sourceName, sourceAge);
            var withProperties = new List<(string propertyName, object propertyValue)>
            {
                (nameof(SourceWithConstructor.Id), withId),
                (nameof(SourceWithConstructor.Name), withName),
                (nameof(SourceWithConstructor.Age), withAge)
            }.ToArray();

            // Act
            SourceWithConstructor result = TypeWither.GetBuilder(source)
                .With(_ => _.Id, withId)
                .With(_ => _.Name, withName)
                .With(_ => _.Age, withAge);

            // Assert
            result.Age.Should().Be(expectedAge);
            result.Id.Should().Be(expectedId);
            result.Name.Should().Be(expectedName);
        }
        
        [Theory]
        [MemberData(nameof(TestData))]
        public void Can_with_using_expressions_using_constructor(
            string sourceId, string sourceName, int? sourceAge,
            string withId, string withName, int? withAge,
            string expectedId, string expectedName, int? expectedAge)
        {
            // Arrange
            var source = new SourceWithConstructor(sourceId, sourceName, sourceAge);

            // Act
            SourceWithConstructor result = TypeWither.GetBuilder(source)
                .With(_ => _.Age, withAge)
                .With(_ => _.Id, withId)
                .With(_ => _.Name, withName);

            // Assert
            result.Age.Should().Be(expectedAge);
            result.Id.Should().Be(expectedId);
            result.Name.Should().Be(expectedName);
        }
        
        public static IEnumerable<object[]> TestData
        {
            get
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
        }

    }
}