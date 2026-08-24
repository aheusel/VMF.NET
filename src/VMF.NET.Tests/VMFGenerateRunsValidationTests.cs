// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//
// The model-validation facts of eu.mihosoft.vmf.VMFGenerateRuns. Java builds a model at test
// time and asserts that generation does or does not throw; here a model error is a build
// diagnostic, so these assert on ModelAnalyzer's diagnostics instead. They live in
// VMF.NET.Tests rather than VMF.NET.TestSuite because they need no generated code -- and
// because a model VMF must REJECT cannot sit in a compiled project.
//
// Four of the nine validation facts were already covered before this file existed, in
// ModelAnalyzerTests: IncompletePropertyOrder_ProducesError (testIncompleteOrderInfoIsInvalid),
// ImmutableType_CannotExtendMutable (testImmutabilityInvalidMutableInheritance),
// InterfaceOnly_GetterOnly_AllowedOnMutableInterfaceOnlyType
// (testImmutabilityValidInterfaceOnlyGetterOnlyProperty) and CustomPropertyOrder_Applied
// (testCompleteOrderInfoIsValid). The five below had no counterpart.

using System.Linq;
using Xunit;
using VMF.NET.Core;

namespace VMF.NET.Tests;

public class VMFGenerateRunsValidationTests
{
    private const string Ns = "Test.Models";

    private static TypeSymbolData Iface(string name, params PropertySymbolData[] props)
    {
        var sym = new TypeSymbolData { Name = name, FullName = $"{Ns}.{name}", IsInterface = true };
        sym.Properties.AddRange(props);
        return sym;
    }

    private static PropertySymbolData ModelProp(string name, string typeName) =>
        new()
        {
            Name = name,
            FullTypeName = $"{Ns}.{typeName}",
            SimpleTypeName = typeName,
            TypeNamespace = Ns,
        };

    private static PropertySymbolData StringProp(string name) =>
        new() { Name = name, FullTypeName = "System.String", SimpleTypeName = "String" };

    // testInvalidOrderInfoTest
    [Fact]
    public void InvalidOrderInfo_DuplicateIndex_ProducesError()
    {
        var a = StringProp("A"); a.OrderIndex = 1;
        var b = StringProp("B"); b.OrderIndex = 1;   // duplicate of A
        var c = StringProp("C"); c.OrderIndex = 3;

        var model = ModelAnalyzer.Analyze(Ns, new[] { Iface("IInvalidOrderInfo", a, b, c) });

        Assert.True(model.HasErrors, "a duplicated [PropertyOrder] index must be rejected");
        Assert.Contains(model.Diagnostics, d => d.Message.Contains("duplicate property order"));
    }

    // testImmutabilityInvalidMutableProperty
    [Fact]
    public void ImmutableType_CannotHaveMutableModelProperty()
    {
        var mutable = Iface("IMutableProperty", StringProp("Name"));

        var immutable = Iface("IMutablePropertyImmutable", ModelProp("Property", "IMutableProperty"));
        immutable.IsImmutable = true;

        var model = ModelAnalyzer.Analyze(Ns, new[] { mutable, immutable });

        Assert.True(model.HasErrors, "an immutable type must not hold a property of a mutable model type");
        Assert.Contains(model.Diagnostics, d => d.Message.Contains("cannot have mutable property"));
    }

    // testImmutabilityInalidIndirectInheritedMutableProperty
    [Fact]
    public void ImmutableType_CannotHaveIndirectInheritedMutableProperty()
    {
        // interface-only base declaring a settable member
        var mutableBase = Iface("IMyMutableProperty", StringProp("Name"));
        mutableBase.IsInterfaceOnly = true;

        // interface-only type that re-declares it as getter-only -- but still INHERITS the
        // settable one, so it is not actually immutable
        var getterOnlyName = StringProp("Name");
        getterOnlyName.IsGetterOnly = true;
        var property = Iface("IMyProperty", getterOnlyName);
        property.IsInterfaceOnly = true;
        property.BaseTypeNames.Add($"{Ns}.IMyMutableProperty");

        var immutable = Iface("IImmutableObj", ModelProp("Property", "IMyProperty"));
        immutable.IsImmutable = true;

        var model = ModelAnalyzer.Analyze(Ns, new[] { mutableBase, property, immutable });

        Assert.True(model.HasErrors,
            "an immutable type must not hold a property whose type inherits a mutable member");
        Assert.Contains(model.Diagnostics, d => d.Message.Contains("cannot have mutable property"));
    }

    // testImmutabilityInalidIndirectMutableProperty
    [Fact]
    public void ImmutableType_CannotHaveIndirectMutableProperty()
    {
        var mutableBase = Iface("IMyMutableProperty", StringProp("Name"));
        mutableBase.IsInterfaceOnly = true;

        // getter-only, but its TYPE is mutable -- so the graph below it is still mutable
        var nameOfMutableType = ModelProp("Name", "IMyMutableProperty");
        nameOfMutableType.IsGetterOnly = true;
        var property = Iface("IMyProperty", nameOfMutableType);
        property.IsInterfaceOnly = true;

        var immutable = Iface("IImmutableObj", ModelProp("Property", "IMyProperty"));
        immutable.IsImmutable = true;

        var model = ModelAnalyzer.Analyze(Ns, new[] { mutableBase, property, immutable });

        Assert.True(model.HasErrors,
            "an immutable type must not reach a mutable type through a getter-only property");
        Assert.Contains(model.Diagnostics, d => d.Message.Contains("cannot have mutable property"));
    }

    // testGetterOnlyInterfaceOnlyWithModifiableProperties
    [Fact]
    public void InterfaceOnly_GetterOnly_PropertyOfMutableType_IsValid()
    {
        var normal = Iface("INormalProperty", StringProp("Name"));

        var parent = ModelProp("Parent", "INormalProperty");
        parent.IsGetterOnly = true;
        var interfaceOnly = Iface("IInterfaceOnlyGetterOnlyType", parent);
        interfaceOnly.IsInterfaceOnly = true;

        var model = ModelAnalyzer.Analyze(Ns, new[] { normal, interfaceOnly });

        Assert.False(model.HasErrors,
            "an interface-only type may declare a getter-only property of a mutable type");
    }
}
