using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace TypeMerger.Tests
{
    public class TypeWitherTests
    {
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
            
            var withProperties = new List<(string propertyName, object propertyValue)>
            {
                (nameof(SourceWithSetters.Id), withId),
                (nameof(SourceWithSetters.Name), withName),
                (nameof(SourceWithSetters.Age), withAge)
            };

            // Act
            var result = TypeWither.With(source, withProperties.ToArray());

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
            };

            // Act
            var result = TypeWither.With(source, withProperties.ToArray());

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