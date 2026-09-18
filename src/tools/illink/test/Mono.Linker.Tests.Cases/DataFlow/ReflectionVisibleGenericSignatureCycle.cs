// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using Mono.Linker.Tests.Cases.Expectations.Assertions;

namespace Mono.Linker.Tests.Cases.DataFlow
{
    [ExpectedNoWarnings]
    public class ReflectionVisibleGenericSignatureCycle
    {
        public static void Main()
        {
            _ = typeof(SelfReferentialMethod).GetMethods();
            _ = typeof(SelfReferentialField).GetFields();
        }

        [Kept]
        class SelfReferentialMethod
        {
            [Kept]
            public static AnnotatedWithPublicMethods<SelfReferentialMethod> Method()
            {
                return null;
            }
        }

        [Kept]
        class SelfReferentialField
        {
            [Kept]
            public static AnnotatedWithPublicFields<SelfReferentialField> Field;
        }

        [Kept]
        class AnnotatedWithPublicMethods<
            [KeptAttributeAttribute(typeof(DynamicallyAccessedMembersAttribute))]
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>
        {
        }

        [Kept]
        class AnnotatedWithPublicFields<
            [KeptAttributeAttribute(typeof(DynamicallyAccessedMembersAttribute))]
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>
        {
        }
    }
}
