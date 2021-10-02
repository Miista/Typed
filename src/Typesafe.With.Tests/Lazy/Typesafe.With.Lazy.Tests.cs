using System;
using AutoFixture.Xunit2;
using FluentAssertions;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Typesafe.With.Lazy.Tests
{
  using Lazy;
  
  public class Tests
  {
    public class Sequence
    {
      internal class TypeWithProperties
      {
        public string String { get; set; }
      }
      
      [Theory, AutoData]
      internal void Sequence_is_not_evaluated(TypeWithProperties instance, string newValue)
      {
        // Act
        var sequence = instance.With(i => i.String, newValue).ToSequence();

        // Assert
        sequence.Should().NotBeNull();
        sequence.Should().BeAssignableTo<LazyWithSequence<TypeWithProperties>>();
      }
      
      [Theory, AutoData]
      internal void Sequence_can_be_applied(TypeWithProperties instance, string newValue)
      {
        // Act
        var sequence = instance.With(i => i.String, newValue).ToSequence();
        var result = sequence.ApplyTo(instance);

        // Assert
        result.Should().NotBeNull();
        result.String.Should().Be(newValue);
      }
    }

    public class Lazy
    {
      internal class TypeWithProperties
      {
        public string String { get; set; }
        public int Int { get; set; }
        public double Double { get; set; }
        public short Short { get; set; }
      }

      [Theory, AutoData]
      internal void Empty_sequence_does_nothing(TypeWithProperties instance)
      {
        // Act
        var sequence = LazyWithSequence<TypeWithProperties>.New();
        var result = sequence.ToSequence().ApplyTo(instance);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(instance);
      }

    
      [Theory, AutoData]
      internal void Can_build_sequence(TypeWithProperties instance, TypeWithProperties newInstance)
      {
        // Act
        var sequence = LazyWithSequence<TypeWithProperties>.New()
          .With(i => i.String, newInstance.String)
          .With(i => i.Int, newInstance.Int)
          .With(i => i.Double, newInstance.Double)
          .With(i => i.Short, newInstance.Short);

        // Assert
        sequence.Should().NotBeNull();
        sequence.Should().BeAssignableTo<LazyWithSequence<TypeWithProperties>>();
      }

    }

    public class NonCategorized
    {

      internal class TypeWithProperties
      {
        public string String { get; set; }
      }

      [Theory, AutoData]
      internal void Sequence_is_not_evaluated(TypeWithProperties instance, string newValue)
      {
        // Act
        var sequence = instance.With(i => i.String, newValue);

        // Assert
        sequence.Should().NotBeNull();
        sequence.Should().BeAssignableTo<InstancedWithSequence<TypeWithProperties>>();
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
        sequence.Should().BeAssignableTo<InstancedWithSequence<TypeWithProperties>>();
      }

      [Theory, AutoData]
      internal void Can_evaluate_sequence(TypeWithProperties instance, string newValue)
      {
        // Act
        var sequence = instance.With(i => i.String, newValue);
        var result = sequence.ApplyTo(instance);

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
    }
  }
}