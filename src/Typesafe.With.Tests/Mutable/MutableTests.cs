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
            }

            [Theory, AutoData]
            internal void Mutates_existing_instance(ClassWithSetter instance, string newValue)
            {
                // Arrange

                // Act
                var updatedInstance = instance.With(i => i.String, newValue);

                // Assert
                updatedInstance.String.Should().BeEquivalentTo(newValue, because: "the value has changed");
                updatedInstance.Should().BeSameAs(instance, because: "we are mutating the existing instance");
            }
        }
    }
}