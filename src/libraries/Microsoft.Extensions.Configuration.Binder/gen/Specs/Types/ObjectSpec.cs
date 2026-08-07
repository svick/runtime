// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis;
using SourceGenerators;

namespace Microsoft.Extensions.Configuration.Binder.SourceGeneration
{
    public sealed record ObjectSpec : ComplexTypeSpec
    {
        public ObjectSpec(
            INamedTypeSymbol type,
            ObjectInstantiationStrategy instantiationStrategy,
            ImmutableEquatableArray<PropertySpec>? properties,
            ImmutableEquatableArray<ParameterSpec>? constructorParameters,
            string? initExceptionMessage) : base(type)
        {
            InstantiationStrategy = instantiationStrategy;
            Properties = properties;
            ConstructorParameters = constructorParameters;
            InitExceptionMessage = initExceptionMessage;
        }

        public ObjectInstantiationStrategy InstantiationStrategy { get; }

        public ImmutableEquatableArray<PropertySpec>? Properties { get; }

        public ImmutableEquatableArray<ParameterSpec>? ConstructorParameters { get; }

        public string? InitExceptionMessage { get; }

        /// <summary>
        /// Whether the target framework and this type support <c>[UnsafeAccessor]</c> for setting init-only members and
        /// bypassing the required-member check when constructing. When <see langword="false"/> (downlevel frameworks, or
        /// a generic type, which pre-.NET 9 <c>[UnsafeAccessor]</c> does not support), the generator falls back to
        /// reflection.
        /// </summary>
        public bool CanUseUnsafeAccessors { get; init; }

        /// <summary>
        /// Whether the type has required members that are not satisfied by a <c>[SetsRequiredMembers]</c> constructor, so
        /// it cannot be created with a plain <c>new T(...)</c> (which would require an object initializer, CS9035).
        /// Construction goes through an accessor (<c>[UnsafeAccessor(Constructor)]</c> or reflection) that bypasses the
        /// check, then the required members are set post-construction, preserving their defaults for absent config keys.
        /// </summary>
        public bool ConstructionRequiresAccessor { get; init; }

        /// <summary>
        /// Whether the type is a value type with required members not satisfied by a <c>[SetsRequiredMembers]</c>
        /// constructor. Such a struct cannot be created with <c>new T()</c> (CS9035) and has no constructor an accessor
        /// could target, so it is constructed with <c>default(T)</c> (which bypasses the required-member check) and its
        /// required members are set post-construction.
        /// </summary>
        public bool ConstructValueTypeWithDefault { get; init; }
    }

    public enum ObjectInstantiationStrategy
    {
        None = 0,
        ParameterlessConstructor = 1,
        ParameterizedConstructor = 2,
    }
}
