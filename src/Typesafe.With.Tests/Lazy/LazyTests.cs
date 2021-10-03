// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

using System;
using AutoFixture.Xunit2;
using FluentAssertions;
using Xunit;

namespace Typesafe.With.Lazy.Tests
{
  using Lazy;
  
  public class LazyTests
  {
    public class NonCategorized
    {
      internal class TypeWithProperties
      {
        public string String { get; set; }
        public int Int { get; set; }
      }

      [Theory, AutoData]
      internal void Sequence_is_not_evaluated(TypeWithProperties instance, string newValue)
      {
        // Act
        var sequence = instance.With(i => i.String, newValue);

        // Assert
        sequence.Should().NotBeNull();
        sequence.Should().NotBeAssignableTo<TypeWithProperties>();
      }

      [Theory, AutoData]
      internal void Can_call_with_on_sequence(TypeWithProperties instance, string newValue)
      {
        // Act
        var sequence = instance.With(i => i.String, newValue);
        Action act = () => sequence.With(i => i.String, newValue);

        // Assert
        act.Should().NotThrow();
        sequence.Should().NotBeNull();
        sequence.Should().NotBeAssignableTo<TypeWithProperties>();
      }

      [Theory, AutoData]
      internal void Can_evaluate_sequence(TypeWithProperties instance, string newValue)
      {
        // Act
        TypeWithProperties result = instance.With(i => i.String, newValue);

        // Assert
        result.Should().NotBeNull();
        result.String.Should().Be(newValue);
        result.Should().BeAssignableTo<TypeWithProperties>();
      }

      [Theory, AutoData]
      internal void Can_evaluate_sequence_via_implicit_cast(TypeWithProperties instance, string newValue)
      {
        TypeWithProperties Apply() => instance.With(i => i.String, newValue);

        // Act
        var result = Apply();

        // Assert
        result.Should().NotBeNull();
        result.String.Should().Be(newValue);
        result.Should().BeAssignableTo<TypeWithProperties>();
      }
      
      [Theory, AutoData]
      internal void Supports_taking_a_value_factory(TypeWithProperties instance, string newValue, int newInt)
      {
        // Act
        TypeWithProperties result = instance
          .With(i => i.String, newValue)
          .With(i => i.String, () => newValue)
          .With(i => i.Int, () => newInt);

        // Assert
        result.Should().NotBeNull();
        result.String.Should().BeEquivalentTo(newValue);
      }
      
      [Theory, AutoData]
      internal void Does_not_evaluate_lazy_sequence_until_instructed_to(TypeWithProperties instance, string newValue)
      {
        // Arrange
        var sequence = instance
          .With(i => i.String, newValue)
          .With(i => i.String, () => throw new Exception());
        
        // Act
        Action act = () => { TypeWithProperties result = sequence; };

        // Assert
        act.Should().Throw<Exception>();
      }
    }
  }
}