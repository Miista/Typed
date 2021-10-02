// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

using AutoFixture.Xunit2;
using FluentAssertions;
using Xunit;

namespace Typesafe.With.Sequence.Tests
{
  public class SequenceTests
  {
    public class Sequence
    {
      internal class TypeWithProperties
      {
        public string String { get; set; }
        public int Int { get; set; }
        public double Double { get; set; }
        public short Short { get; set; }
      }
      
      [Theory, AutoData]
      internal void Sequence_is_not_evaluated(string newValue)
      {
        // Act
        var sequence = WithSequence.New<TypeWithProperties>().With(i => i.String, newValue).ToSequence();
    
        // Assert
        sequence.Should().NotBeNull();
        sequence.Should().NotBeAssignableTo<TypeWithProperties>();
      }
      
      [Theory, AutoData]
      internal void Sequence_can_be_applied(TypeWithProperties instance, string newValue)
      {
        // Act
        var sequence = WithSequence.New<TypeWithProperties>().With(i => i.String, newValue).ToSequence();
        var result = sequence.ApplyTo(instance);
    
        // Assert
        result.Should().NotBeNull();
        result.String.Should().Be(newValue);
      }
      
      [Theory, AutoData]
      internal void Empty_sequence_does_nothing(TypeWithProperties instance)
      {
        // Act
        var sequence = WithSequence.New<TypeWithProperties>().ToSequence();
        var result = sequence.ApplyTo(instance);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(instance);
      }
      
      [Theory, AutoData]
      internal void Can_build_sequence(TypeWithProperties newInstance)
      {
        // Act
        var sequence = WithSequence.New<TypeWithProperties>()
          .With(i => i.String, newInstance.String)
          .With(i => i.Int, newInstance.Int)
          .With(i => i.Double, newInstance.Double)
          .With(i => i.Short, newInstance.Short)
          .ToSequence();

        // Assert
        sequence.Should().NotBeNull();
        sequence.Should().NotBeAssignableTo<TypeWithProperties>();
      }
    }
  }
}