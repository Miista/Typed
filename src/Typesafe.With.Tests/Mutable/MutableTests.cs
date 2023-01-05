using AutoFixture.Xunit2;
using FluentAssertions;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Typesafe.With.Tests
{
    using Mutable;
    
    public class MutableTests
    {
        public class Mutable
        {
            internal class ClassWithSetter
            {
                public string String { get; set; }
                public int Age { get; set; }
            }

            [Theory, AutoData]
            internal void Mutates_existing_instance(ClassWithSetter originalInstance, string newValue)
            {
                // Arrange + Act
                var updatedInstance = originalInstance.With(i => i.String, newValue);

                // Assert
                updatedInstance.String.Should().BeEquivalentTo(newValue, because: "the value has changed");
                updatedInstance.Age.Should().Be(originalInstance.Age, because: "the value has not changed");
                updatedInstance.Should().BeSameAs(originalInstance, because: "we are mutating the existing instance");
            }
            
            [Theory, AutoData]
            internal void Mutates_existing_instance1(ClassWithSetter originalInstance, string newValue)
            {
                // Arrange
                var expectedValue = originalInstance.String.Substring(0, 2);
                
                // Act
                var updatedInstance = originalInstance.With(i => i.String, existingValue => existingValue.Substring(0, 2));

                // Assert
                updatedInstance.String.Should().BeEquivalentTo(expectedValue, because: "the value has been 'substringed'");
                updatedInstance.Age.Should().Be(originalInstance.Age, because: "the value has not changed");
                updatedInstance.Should().BeSameAs(originalInstance, because: "we are mutating the existing instance");
            }
        }
    }
}