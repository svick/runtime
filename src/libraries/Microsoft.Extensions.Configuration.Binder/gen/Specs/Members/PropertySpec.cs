// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis;
using SourceGenerators;

namespace Microsoft.Extensions.Configuration.Binder.SourceGeneration
{
    public sealed record PropertySpec : MemberSpec
    {
        public PropertySpec(IPropertySymbol property, TypeRef typeRef) : base(property, typeRef)
        {
            IMethodSymbol? setMethod = property.SetMethod;
            bool setterIsPublic = setMethod?.DeclaredAccessibility is Accessibility.Public;
            bool isInitOnly = setMethod?.IsInitOnly is true;

            IsStatic = property.IsStatic;
            // Only public setters are considered here, consistent with CanSet. A required or init-only property with a
            // non-public (e.g. internal) setter is therefore not treated as SetOnInit: the generator does not set it
            // (matching the reflection binder, which does not bind non-public members by default), and the member keeps
            // its default value.
            SetOnInit = setterIsPublic && (property.IsRequired || isInitOnly);
            CanSet = setterIsPublic && !isInitOnly;
            // An init-only property can only be assigned at construction time through normal C#. Post-construction the
            // generator sets it through an [UnsafeAccessor] setter (or reflection downlevel), which lets absent config
            // keys preserve the property's default value instead of overwriting it.
            CanSetViaAccessor = setterIsPublic && isInitOnly;
            CanGet = property.GetMethod?.DeclaredAccessibility is Accessibility.Public;
            IsRequired = property.IsRequired;
        }

        public ParameterSpec? MatchingCtorParam { get; set; }

        public bool IsIgnored { get; init; }

        public bool IsStatic { get; }

        public bool SetOnInit { get; }

        public bool IsRequired { get; }

        public override bool CanGet { get; }

        public override bool CanSet { get; }

        /// <summary>
        /// Whether the property has a public init-only setter, so it is assignable post-construction only through an
        /// <c>[UnsafeAccessor]</c> setter (or a reflection fallback downlevel) rather than a direct assignment.
        /// </summary>
        public bool CanSetViaAccessor { get; }

        /// <summary>
        /// Whether the property is declared on a base type rather than the type being bound. An <c>[UnsafeAccessor]</c>
        /// setter targets the type that declares the setter, so for an inherited property the extern is emitted against
        /// <see cref="DeclaringTypeRef"/> when that type is usable; otherwise the reflection fallback (which searches
        /// base types) is used.
        /// </summary>
        public bool IsInherited { get; init; }

        /// <summary>
        /// For an inherited init-only property, the declaring (base) type that an <c>[UnsafeAccessor]</c> setter extern
        /// must target, since the setter is declared there rather than on the derived type. Set only when that type can
        /// be used with <c>[UnsafeAccessor]</c> - it can be named (is accessible) and is non-generic. <see langword="null"/>
        /// when the property is not inherited, or when the declaring type is inaccessible or generic, in which case the
        /// reflection fallback (which searches base types) is used instead.
        /// </summary>
        public TypeRef? DeclaringTypeRef { get; init; }
    }
}
