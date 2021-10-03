// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

using System;
using System.Collections;
using System.Collections.Generic;
using AutoFixture.Xunit2;
using FluentAssertions;
using Xunit;

namespace Typesafe.With.Tests
{
    public class Tests
    {
        public class Inheritance
        {
            internal class BaseClassWithSetter
            {
                public string String { get; set; }
            }

            internal class ChildClassWithSetter : BaseClassWithSetter
            {
                public int Int { get; set; }
            }

            [Theory, AutoData]
            internal void Can_handle_inherited_property_with_setter(ChildClassWithSetter childClass, string newValue)
            {
                // Act
                var result = childClass.With(c => c.String, newValue);
                
                // Assert
                result.Should().NotBeNull();
                result.Int.Should().Be(childClass.Int);
                result.String.Should().Be(newValue);
            }
            
            internal class BaseClassWithInternalSetter
            {
                public string String { get; internal set; }
            }

            internal class ChildClassWithInheritedInternalSetter : BaseClassWithInternalSetter
            {
                public int Int { get; set; }
            }

            [Theory, AutoData]
            internal void Can_handle_inherited_property_with_inherited_internal_setter(ChildClassWithInheritedInternalSetter childClass, string newValue)
            {
                // Act
                var result = childClass.With(c => c.String, newValue);
                
                // Assert
                result.Should().NotBeNull();
                result.Int.Should().Be(childClass.Int);
                result.String.Should().Be(newValue);
            }
            
            internal class BaseClassWithConstructor
            {
                public string String { get; }

                public BaseClassWithConstructor(string @string) => String = @string;
            }

            internal class ChildClassWithSetterAndInheritedConstructor : BaseClassWithConstructor
            {
                public int Int { get; set; }


                public ChildClassWithSetterAndInheritedConstructor(string @string) : base(@string)
                {
                }
            }

            [Theory, AutoData]
            internal void Can_handle_inherited_property_with_inherited_constructor(ChildClassWithSetterAndInheritedConstructor childClass, string newValue)
            {
                // Act
                var result = childClass.With(c => c.String, newValue);
                
                // Assert
                result.Should().NotBeNull();
                result.Int.Should().Be(childClass.Int);
                result.String.Should().Be(newValue);
            }
            
            internal class ChildClassWithConstructorAndInheritedConstructor : BaseClassWithConstructor
            {
                public int Int { get; }


                public ChildClassWithConstructorAndInheritedConstructor(int @int, string @string) : base(@string) =>
                    Int = @int; 
            }

            [Theory, AutoData]
            internal void Can_handle_class_with_constructor_and_inherited_constructor(ChildClassWithConstructorAndInheritedConstructor childClass, string newValue)
            {
                // Act
                var result = childClass.With(c => c.String, newValue);
                
                // Assert
                result.Should().NotBeNull();
                result.Int.Should().Be(childClass.Int);
                result.String.Should().Be(newValue);
            }

            internal abstract class AbstractBaseClass
            {
                public string String { get; set; }
            }

            internal class ChildClassWithAbstractBaseClass : AbstractBaseClass
            {
                public int Int { get; set; }
            }
            
            [Theory, AutoData]
            internal void Can_handle_class_with_abstract_base_class(ChildClassWithAbstractBaseClass childClass, string newValue)
            {
                // Act
                var result = childClass.With(c => c.String, newValue);
                
                // Assert
                result.Should().NotBeNull();
                result.Int.Should().Be(childClass.Int);
                result.String.Should().Be(newValue);
            }
        }
        
        public class PropertyCopy
        {
            internal class TypeWithNoSetter
            {
                public int Age { get; }

                public TypeWithNoSetter(int age)
                {
                    Age = age;
                }
            }

            [Theory, AutoData]
            internal void Can_handle_properties_with_no_setter(TypeWithNoSetter source, int newValue)
            {
                // Act
                var result = source.With(a => a.Age, newValue);
                
                // Assert
                result.Should().NotBeNull();
                result.Age.Should().Be(newValue);
            }

            internal class TypeWithInternalSetter
            {
                public int Age { get; internal set; }

                public TypeWithInternalSetter(int age)
                {
                    Age = age;
                }
            }

            [Theory, AutoData]
            internal void Can_handle_properties_with_internal_setter(TypeWithInternalSetter source, int newValue)
            {
                // Act
                var result = source.With(a => a.Age, newValue);
                
                // Assert
                result.Should().NotBeNull();
                result.Age.Should().Be(newValue);
            }

            internal class TypeWithExpressionBodiedProperty
            {
                public int Age { get; }

                public TypeWithExpressionBodiedProperty(int age)
                {
                    Age = age;
                }
            }

            [Theory, AutoData]
            internal void Can_handle_expression_bodied_property(TypeWithExpressionBodiedProperty source, int newValue)
            {
                // Act
                var result = source.With(a => a.Age, newValue);
                
                // Assert
                result.Should().NotBeNull();
                result.Age.Should().Be(newValue);
            }

            internal class TypeWithPrivateSetter
            {
                public int Age { get; private set; }

                public TypeWithPrivateSetter(int age)
                {
                    Age = age;
                }
            }

            [Theory, AutoData]
            internal void Can_handle_properties_with_private_setter(TypeWithPrivateSetter source, int newValue)
            {
                // Act
                var result = source.With(a => a.Age, newValue);
                
                // Assert
                result.Should().NotBeNull();
                result.Age.Should().Be(newValue);
            }
        }

        public class MemberAccess
        {
            /***
             * Verify support for the following combinations:
             *
             * If there is no public getter for the property, there is no way we can set it.
             * Therefore, all cases not having a public getter are not supported.
             * 
             *          Combinations: Property has
             * ------------------------------------------------------------------------------
             * Public setter        Constructor parameter   Supported?  Notes
             * No                   No                      No          There is no setter/constructor for the property
             * No                   Yes                     Yes         There is a constructor argument for the property
             * Yes                  No                      Yes         There is a setter for the property
             * Yes                  Yes                     Yes         There is a setter/constructor for the property
             *
             *          Combinations: Property has
             * ------------------------------------------------------------------------------
             * Private setter       Constructor parameter   Supported?  Notes
             * No                   No                      No          There is no setter/constructor for the property
             * No                   Yes                     Yes         There is a constructor argument for the property
             * Yes                  No                      Yes         There is a setter for the property
             * Yes                  Yes                     Yes         There is a setter/constructor for the property
             *
             *          Combinations: Property has
             * ------------------------------------------------------------------------------
             * Protected setter     Constructor parameter   Supported?  Notes
             * No                   No                      No          There is no setter/constructor for the property
             * No                   Yes                     Yes         There is a constructor argument for the property
             * Yes                  No                      Yes         There is a setter for the property
             * Yes                  Yes                     Yes         There is a setter/constructor for the property
             *
             *          Combinations: Property has
             * ------------------------------------------------------------------------------
             * Internal setter      Constructor parameter   Supported?  Notes
             * No                   No                      No          There is no setter/constructor for the property
             * No                   Yes                     Yes         There is a constructor argument for the property
             * Yes                  No                      Yes         There is a setter for the property
             * Yes                  Yes                     Yes         There is a setter/constructor for the property
             *
             * Conclusion
             * ==========
             * It all boils down to whether there is:
             * - A public getter, and
             * - A public/private/internal setter, or
             * - A constructor parameter
             *
             * As long as there is either a constructor parameter or a public/private/internal setter,
             * the property can be set.
             */

            public class PublicSetter
            {
                public class TypeWithoutSetterAndConstructor
                {
                    public int Property { get; }
                }
                
                [Theory, AutoData]
                public void Does_not_support_property_with_no_setter_nor_constructor_argument(TypeWithoutSetterAndConstructor source, int newValue)
                {
                    // Act
                    Action act = () => source.With(a => a.Property, newValue);
                    
                    // Assert
                    act.Should().Throw<Exception>(because: "the property has neither a constructor parameter nor setter");
                }
                
                public class TypeWithoutSetterButConstructor
                {
                    public int Property { get; }

                    public TypeWithoutSetterButConstructor(int property)
                    {
                        Property = property;
                    }
                }
                
                [Theory, AutoData]
                public void Supports_property_with_no_setter_but_constructor_argument(TypeWithoutSetterButConstructor source, int newValue)
                {
                    // Act
                    Action act = () => source.With(a => a.Property, newValue);
                    
                    // Assert
                    act.Should().NotThrow<Exception>(because: "the property has a constructor parameter");
                }
                
                public class TypeWithPublicSetter
                {
                    public int Property { get; set; }
                }
                
                [Theory, AutoData]
                public void Supports_property_with_public_setter(TypeWithPublicSetter source, int newValue)
                {
                    // Act
                    Action act = () => source.With(a => a.Property, newValue);
                    
                    // Assert
                    act.Should().NotThrow<Exception>(because: "the property has a public setter");
                }
                
                public class TypeWithPublicSetterAndConstructor
                {
                    public int Property { get; set; }

                    public TypeWithPublicSetterAndConstructor(int property)
                    {
                        Property = property;
                    }
                }
                
                [Theory, AutoData]
                public void Supports_property_with_public_setter_and_constructor(TypeWithPublicSetterAndConstructor source, int newValue)
                {
                    // Act
                    Action act = () => source.With(a => a.Property, newValue);
                    
                    // Assert
                    act.Should().NotThrow<Exception>(because: "the property has a public setter and constructor parameter");
                }
            }
        }

        public class Validation
        {
            internal class TypeWithConstructor_PublicGetter_NoSetter
            {
                public int Property { get; }

                public TypeWithConstructor_PublicGetter_NoSetter(int property)
                {
                    Property = property;
                }
            }
            
            [Theory, AutoData]
            internal void Can_handle_property_with_constructor_argument(TypeWithConstructor_PublicGetter_NoSetter source, int newValue)
            {
                // Act
                Action act = () => source.With(a => a.Property, newValue);
                
                // Assert
                act.Should().NotThrow<Exception>(because: "the property has a constructor parameter");
            }
            
            internal class TypeWithConstructor_PublicGetter_PublicSetter
            {
                public int Property { get; set; }

                public TypeWithConstructor_PublicGetter_PublicSetter(int property)
                {
                    Property = property;
                }
            }
            
            [Theory, AutoData]
            internal void Can_handle_property_with_constructor_argument_and_public_getter_setter(TypeWithConstructor_PublicGetter_PublicSetter source, int newValue)
            {
                // Act
                Action act = () => source.With(a => a.Property, newValue);
                
                // Assert
                act.Should().NotThrow<Exception>(because: "the property has a constructor parameter/setter");
            }
            
            internal class TypeWithConstructorAndPublicGetterAndPrivateSetter
            {
                public int Property { get; private set; }

                public TypeWithConstructorAndPublicGetterAndPrivateSetter(int property)
                {
                    Property = property;
                }
            }
            
            [Theory, AutoData]
            internal void Can_handle_property_with_constructor_argument_and_public_getter_and_private_setter(TypeWithConstructorAndPublicGetterAndPrivateSetter source, int newValue)
            {
                // Act
                Action act = () => source.With(a => a.Property, newValue);
                
                // Assert
                act.Should().NotThrow<Exception>(because: "the property has a constructor parameter");
            }
            
            internal class TypeWithConstructorAndGetter
            {
                public int Property { get; }

                public TypeWithConstructorAndGetter(int property)
                {
                    Property = property;
                }
            }
            
            [Theory, AutoData]
            internal void Can_handle_property_with_constructor_argument_and_getter(TypeWithConstructorAndGetter source, int newValue)
            {
                // Act
                Action act = () => source.With(a => a.Property, newValue);
                
                // Assert
                act.Should().NotThrow<Exception>(because: "the property has a constructor parameter");
            }
            
            internal class TypeWithGetterSetter
            {
                public int Property { get; set; }
            }
            
            [Theory, AutoData]
            internal void Can_handle_property_with_getter_setter_only(TypeWithGetterSetter source, int newValue)
            {
                // Act
                Action act = () => source.With(a => a.Property, newValue);
                
                // Assert
                act.Should().NotThrow<Exception>(because: "the property has a setter");
            }
            
            internal class TypeWithGetterOnly
            {
                public int Property { get; }
            }
            
            [Theory, AutoData]
            internal void Cannot_handle_property_with_getter_only(TypeWithGetterOnly source, int newValue)
            {
                // Act
                Action act = () => source.With(a => a.Property, newValue);
                
                // Assert
                act.Should().Throw<Exception>(because: "the property has no setter");
            }
            
            internal class TypeWithExpressionBodiedProperty
            {
                public int ExpressionBodiedProperty => int.MaxValue;
            }
            
            [Theory, AutoData]
            internal void Cannot_handle_expression_bodied_property(TypeWithExpressionBodiedProperty source, int newValue)
            {
                // Act
                Action act = () => source.With(a => a.ExpressionBodiedProperty, newValue);
                
                // Assert
                act.Should().Throw<Exception>(because: "an expression-bodied property cannot be written to");
            }
        }

        public class General
        {
            internal class TypeWithoutWritableProperty
            {
                public string Text { get; }
            }
            
            [Theory, AutoData]
            internal void With_fails_if_property_is_not_writable(TypeWithoutWritableProperty source, string newValue)
            {
                // Act
                Func<TypeWithoutWritableProperty> act = () => source.With(s => s.Text, newValue);
                
                // Assert
                act.Should()
                    .Throw<InvalidOperationException>(
                        because: $"the property '{nameof(TypeWithoutWritableProperty.Text)}' is not writable")
                    .And.Message.Should().Contain(nameof(TypeWithoutWritableProperty.Text));
            }

            internal class TypeWithoutMatchingConstructorArgument
            {
                public string FullName { get; }

                public TypeWithoutMatchingConstructorArgument(string name)
                {
                    FullName = name;
                }
            }
        
            [Theory, AutoData]
            internal void With_fails_if_property_has_no_matching_constructor_argument(TypeWithoutMatchingConstructorArgument source, string newValue)
            {
                // Act
                Action act = () => source.With(_ => _.FullName, newValue);

                // Assert
                act.Should()
                    .Throw<Exception>(because: $"there is no matching constructor parameter for property '{nameof(TypeWithoutMatchingConstructorArgument.FullName)}'")
                    .WithMessage("Property '*' cannot be set via constructor or property setter.");
            }

            private class Container<T>
            {
                public T Value { get; set; }
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
                    yield return new object[] {0M, 1M, 1M};
                    yield return new object[] {0d, 1d, 1d};
                    yield return new object[] {0f, 1f, 1f};
                    yield return new object[] {false, true, true};
                    yield return new object[] {"Hello", "World", "World"};
                    yield return new object[] {'a', 'b', 'b'};
                    yield return new object[] {(byte) 0, (byte) 1, (byte) 1};
                }
            }

            private class SourceWithSetters
            {
                public string Id { get; set; }
                public string Name { get; set; }
                public int? Age { get; set; }
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

            private class SourceWithConstructor
            {
                public string Id { get; }
                public string Name { get; }
                public int? Age { get; }

                public SourceWithConstructor(string id, string name, int? age) => (Id, Name, Age) = (id, name, age);
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

            private class SourceWithMixedConstructorAndSetters
            {
                public string Id { get; }
                public string Name { get; }
                public int? Age { get; set; }

                public SourceWithMixedConstructorAndSetters(string id, string name) => (Id, Name) = (id, name);
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

            private class TestData : IEnumerable<object[]>
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

            internal class SimpleType
            {
                public string String { get; set; }
            }

            [Theory, AutoData]
            internal void Can_take_method_group_as_value_for_property_value(SimpleType instance, string newValue)
            {
                // Arrange
                string ReturnNewValue() => newValue;
                
                // Act
                var result = instance.With(i => i.String, ReturnNewValue());
                
                // Assert
                result.Should().NotBeNull();
                result.String.Should().Be(newValue);
            }

            internal class TypeWithMultipleConstructors
            {
                public string String { get; set; }
                public int Int { get; set; }

                public TypeWithMultipleConstructors(string @string) : this(@string, int.MaxValue)
                {
                }

                public TypeWithMultipleConstructors(string @string, int @int)
                {
                    String = @string;
                    Int = @int;
                }
            }
            
            [Theory, AutoData]
            internal void Supports_type_with_multiple_constructors(TypeWithMultipleConstructors instance, string newValue)
            {
                // Arrange
                string ReturnNewValue() => newValue;
                
                // Act
                var result = instance.With(i => i.String, ReturnNewValue());
                
                // Assert
                result.Should().NotBeNull();
                result.String.Should().Be(newValue);
                result.Int.Should().Be(instance.Int);
            }

        }

        public class Casing
        {
            internal class TypeWithPropertiesHavingSameNameButDifferentCasing
            {
                public string Fullname { get; }
                public string FullName { get; }

                public TypeWithPropertiesHavingSameNameButDifferentCasing(string fullname, string fullName) => (Fullname, FullName) = (fullname, fullName);
            }
        
            [Theory, AutoData]
            internal void Correctly_set_properties_with_same_name_but_different_casing(TypeWithPropertiesHavingSameNameButDifferentCasing source, string newFullname, string newFullName)
            {
                // Act
                var result = source
                    .With(_ => _.Fullname, newFullname)
                    .With(_ => _.FullName, newFullName);
            
                // Assert
                result.Should().NotBeNull();
                result.Fullname.Should().Be(newFullname);
                result.FullName.Should().Be(newFullName);
            }

            internal class TypeWithDifferentCasingInConstructor
            {
                public string SSN { get; }

                public TypeWithDifferentCasingInConstructor(string ssn)
                {
                    SSN = ssn;
                }
            }
        
            [Theory, AutoData]
            internal void Can_set_property_which_has_different_casing_in_the_constructor(TypeWithDifferentCasingInConstructor source, string newValue)
            {
                // Act
                var result = source.With(_ => _.SSN, newValue);

                // Assert
                result.SSN.Should().Be(newValue, because: "the property is set via constructor");
            }

            internal class TypeCreatesNewInstance
            {
                public string Id { get; }

                public TypeCreatesNewInstance(string id) => Id = id;
            }
            
            [Theory, AutoData]
            internal void Calling_With_creates_a_new_instance(TypeCreatesNewInstance source, string newValue)
            {
                // Act
                var result = source.With(s => s.Id, newValue);

                // Assert
                result.GetHashCode().Should().NotBe(source.GetHashCode());
            }
        }
    }
}