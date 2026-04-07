// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Linq;
using Xunit;
using VMF.NET.Core;

namespace VMF.NET.Tests;

public class ModelAnalyzerTests
{
    private const string Ns = "Test.Models";

    // --- Helper to build a minimal interface ---
    private static TypeSymbolData MakeInterface(string name, params PropertySymbolData[] props)
    {
        var sym = new TypeSymbolData
        {
            Name = name,
            FullName = $"{Ns}.{name}",
            IsInterface = true,
        };
        sym.Properties.AddRange(props);
        return sym;
    }

    private static PropertySymbolData MakeProp(string name, string fullType, string simpleType,
        string? ns = null, bool isPrimitive = false)
    {
        return new PropertySymbolData
        {
            Name = name,
            FullTypeName = fullType,
            SimpleTypeName = simpleType,
            TypeNamespace = ns,
        IsPrimitive = isPrimitive,
        };
    }

    private static PropertySymbolData MakeCollectionProp(string name, string elementSimple, string? elementNs = null)
    {
        return new PropertySymbolData
        {
            Name = name,
            FullTypeName = $"System.Collections.Generic.IList<{elementSimple}>",
            SimpleTypeName = "IList",
            TypeNamespace = "System.Collections.Generic",
            IsCollection = true,
            CollectionElementSimpleName = elementSimple,
            CollectionElementNamespace = elementNs,
        };
    }

    // --- Basic analysis tests ---

    [Fact]
    public void EmptyInterfaces_ProducesError()
    {
        var model = ModelAnalyzer.Analyze(Ns, new List<TypeSymbolData>());
        Assert.True(model.HasErrors);
        Assert.Contains(model.Diagnostics, d => d.Message.Contains("At least one interface"));
    }

    [Fact]
    public void NonInterface_ProducesError()
    {
        var sym = MakeInterface("MyClass");
        sym.IsInterface = false;
        var model = ModelAnalyzer.Analyze(Ns, new[] { sym });
        Assert.True(model.HasErrors);
        Assert.Contains(model.Diagnostics, d => d.Message.Contains("only contain interfaces"));
    }

    [Fact]
    public void SingleInterface_CreatesType()
    {
        var sym = MakeInterface("INode");
        var model = ModelAnalyzer.Analyze(Ns, new[] { sym });
        Assert.False(model.HasErrors);
        Assert.Single(model.Types);
        Assert.Equal("INode", model.Types[0].TypeName);
        Assert.Equal($"{Ns}.INode", model.Types[0].FullTypeName);
    }

    [Fact]
    public void TypeId_IncrementsByTwo()
    {
        var a = MakeInterface("IAlpha");
        var b = MakeInterface("IBeta");
        var model = ModelAnalyzer.Analyze(Ns, new[] { a, b });
        Assert.False(model.HasErrors);
        var ids = model.Types.Select(t => t.TypeId).OrderBy(x => x).ToList();
        Assert.Equal(new[] { 0, 2 }, ids);
    }

    // --- Property classification ---

    [Fact]
    public void PrimitiveProperty_ClassifiedCorrectly()
    {
        var prop = MakeProp("Age", "int", "int", isPrimitive: true);
        var sym = MakeInterface("IPerson", prop);
        var model = ModelAnalyzer.Analyze(Ns, new[] { sym });
        var p = model.Types[0].Properties[0];
        Assert.Equal(PropType.Primitive, p.PropType);
        Assert.Equal("Age", p.Name);
    }

    [Fact]
    public void CollectionProperty_ClassifiedCorrectly()
    {
        var prop = MakeCollectionProp("Children", "INode", Ns);
        var node = MakeInterface("INode", prop);
        var model = ModelAnalyzer.Analyze(Ns, new[] { node });
        var p = model.Types[0].Properties[0];
        Assert.Equal(PropType.Collection, p.PropType);
        Assert.True(p.IsCollectionType);
        Assert.Equal("INode", p.GenericTypeName);
    }

    [Fact]
    public void ModelTypeProperty_ResolvedCorrectly()
    {
        var childProp = MakeProp("Child", $"{Ns}.IChild", "IChild", Ns);
        var parent = MakeInterface("IParent", childProp);
        var child = MakeInterface("IChild");
        var model = ModelAnalyzer.Analyze(Ns, new[] { parent, child });
        var p = model.Types.First(t => t.TypeName == "IParent").Properties[0];
        Assert.True(p.IsModelType);
        Assert.Equal("IChild", p.ModelType!.TypeName);
    }

    // --- Property flags ---

    [Fact]
    public void PropertyFlags_PreservedFromSymbol()
    {
        var prop = MakeProp("Name", "string", "string", isPrimitive: true);
        prop.IsRequired = true;
        prop.IsIgnoredForEquals = true;
        prop.IsIgnoredForToString = true;
        prop.IsGetterOnly = true;
        prop.DefaultValue = "\"hello\"";
        prop.Documentation = "A name field.";
        var sym = MakeInterface("IFoo");
        sym.IsImmutable = true; // allow getter-only
        sym.Properties.Add(prop);
        var model = ModelAnalyzer.Analyze(Ns, new[] { sym });
        var p = model.Types[0].Properties[0];
        Assert.True(p.IsRequired);
        Assert.True(p.IsIgnoredForEquals);
        Assert.True(p.IsIgnoredForToString);
        Assert.True(p.IsGetterOnly);
        Assert.Equal("\"hello\"", p.DefaultValueAsString);
        Assert.True(p.IsDocumented);
    }

    // --- Property ordering ---

    [Fact]
    public void Properties_SortedAlphabeticallyByDefault()
    {
        var z = MakeProp("Zebra", "string", "string", isPrimitive: true);
        var a = MakeProp("Apple", "string", "string", isPrimitive: true);
        var sym = MakeInterface("IFoo", z, a);
        var model = ModelAnalyzer.Analyze(Ns, new[] { sym });
        Assert.Equal("Apple", model.Types[0].Properties[0].Name);
        Assert.Equal("Zebra", model.Types[0].Properties[1].Name);
    }

    [Fact]
    public void CustomPropertyOrder_Applied()
    {
        var b = MakeProp("Beta", "string", "string", isPrimitive: true);
        b.OrderIndex = 2;
        var a = MakeProp("Alpha", "string", "string", isPrimitive: true);
        a.OrderIndex = 1;
        var sym = MakeInterface("IFoo", b, a);
        var model = ModelAnalyzer.Analyze(Ns, new[] { sym });
        Assert.False(model.HasErrors);
        Assert.Equal("Alpha", model.Types[0].Properties[0].Name);
        Assert.Equal("Beta", model.Types[0].Properties[1].Name);
    }

    [Fact]
    public void IncompletePropertyOrder_ProducesError()
    {
        var a = MakeProp("A", "int", "int", isPrimitive: true);
        a.OrderIndex = 1;
        var b = MakeProp("B", "int", "int", isPrimitive: true);
        // b has no OrderIndex
        var sym = MakeInterface("IFoo", a, b);
        var model = ModelAnalyzer.Analyze(Ns, new[] { sym });
        Assert.True(model.HasErrors);
        Assert.Contains(model.Diagnostics, d => d.Message.Contains("incomplete property order"));
    }

    // --- Containment ---

    [Fact]
    public void ContainsWithOpposite_Resolved()
    {
        var childrenProp = MakeCollectionProp("Children", "IChild", Ns);
        childrenProp.ContainsOpposite = "IChild.Parent";

        var parentProp = MakeProp("Parent", $"{Ns}.IParent", "IParent", Ns);
        parentProp.ContainerOpposite = "IParent.Children";

        var parent = MakeInterface("IParent", childrenProp);
        var child = MakeInterface("IChild", parentProp);
        var model = ModelAnalyzer.Analyze(Ns, new[] { parent, child });

        Assert.False(model.HasErrors);

        var containsProp = model.Types.First(t => t.TypeName == "IParent").Properties[0];
        Assert.True(containsProp.IsContained); // Contains = Contained side in VMF terminology
        Assert.True(containsProp.IsContainmentProperty);

        var containerProp = model.Types.First(t => t.TypeName == "IChild").Properties[0];
        Assert.True(containerProp.IsContainer);
    }

    [Fact]
    public void ContainsWithoutOpposite_Allowed()
    {
        var childrenProp = MakeCollectionProp("Children", "IChild", Ns);
        childrenProp.ContainsOpposite = "";

        var parent = MakeInterface("IParent", childrenProp);
        var child = MakeInterface("IChild");
        var model = ModelAnalyzer.Analyze(Ns, new[] { parent, child });

        Assert.False(model.HasErrors);
        var prop = model.Types.First(t => t.TypeName == "IParent").Properties[0];
        Assert.True(prop.IsContainmentProperty);
        Assert.True(prop.Containment.IsWithoutOpposite);
    }

    // --- Cross-references ---

    [Fact]
    public void CrossReference_Resolved()
    {
        var refProp = MakeProp("Friend", $"{Ns}.IBeta", "IBeta", Ns);
        refProp.RefersOpposite = "IBeta.Buddy";

        var buddyProp = MakeProp("Buddy", $"{Ns}.IAlpha", "IAlpha", Ns);
        buddyProp.RefersOpposite = "IAlpha.Friend";

        var alpha = MakeInterface("IAlpha", refProp);
        var beta = MakeInterface("IBeta", buddyProp);
        var model = ModelAnalyzer.Analyze(Ns, new[] { alpha, beta });

        Assert.False(model.HasErrors);

        var friendProp = model.Types.First(t => t.TypeName == "IAlpha").Properties[0];
        Assert.True(friendProp.IsCrossRefProperty);
        Assert.Equal("IBeta", friendProp.Reference!.OppositeType.TypeName);
    }

    // --- Inheritance ---

    [Fact]
    public void Implements_Resolved()
    {
        var baseType = MakeInterface("IBase");
        var derived = MakeInterface("IDerived");
        derived.BaseTypeNames.Add($"{Ns}.IBase");

        var model = ModelAnalyzer.Analyze(Ns, new[] { baseType, derived });
        Assert.False(model.HasErrors);
        var d = model.Types.First(t => t.TypeName == "IDerived");
        Assert.Single(d.Implements);
        Assert.Equal("IBase", d.Implements[0].TypeName);
    }

    [Fact]
    public void AllInheritedTypes_Transitive()
    {
        var a = MakeInterface("IA");
        var b = MakeInterface("IB");
        b.BaseTypeNames.Add($"{Ns}.IA");
        var c = MakeInterface("IC");
        c.BaseTypeNames.Add($"{Ns}.IB");

        var model = ModelAnalyzer.Analyze(Ns, new[] { a, b, c });
        Assert.False(model.HasErrors);
        var cType = model.Types.First(t => t.TypeName == "IC");
        Assert.Equal(2, cType.AllInheritedTypes.Count);
        Assert.Contains(cType.AllInheritedTypes, t => t.TypeName == "IA");
        Assert.Contains(cType.AllInheritedTypes, t => t.TypeName == "IB");
    }

    [Fact]
    public void InheritedProperties_CollectedInAllProperties()
    {
        var baseProp = MakeProp("Name", "string", "string", isPrimitive: true);
        var baseType = MakeInterface("IBase", baseProp);

        var ownProp = MakeProp("Value", "int", "int", isPrimitive: true);
        var derived = MakeInterface("IDerived", ownProp);
        derived.BaseTypeNames.Add($"{Ns}.IBase");

        var model = ModelAnalyzer.Analyze(Ns, new[] { baseType, derived });
        Assert.False(model.HasErrors);
        var d = model.Types.First(t => t.TypeName == "IDerived");
        Assert.Equal(2, d.AllProperties.Count);
        Assert.Contains(d.AllProperties, p => p.Name == "Name");
        Assert.Contains(d.AllProperties, p => p.Name == "Value");
    }

    // --- Computed names ---

    [Fact]
    public void ImplClassName_StripsLeadingI()
    {
        var sym = MakeInterface("IParent");
        var model = ModelAnalyzer.Analyze(Ns, new[] { sym });
        var t = model.Types[0];
        Assert.Equal("ParentImpl", t.ImplClassName);
        Assert.Equal("IReadOnlyParent", t.ReadOnlyInterfaceName);
        Assert.Equal("ReadOnlyParentImpl", t.ReadOnlyImplClassName);
    }

    // --- Equals strategy ---

    [Fact]
    public void EqualsStrategy_DefaultsToInstance()
    {
        var sym = MakeInterface("IFoo");
        var model = ModelAnalyzer.Analyze(Ns, new[] { sym });
        Assert.Equal(EqualsStrategy.Instance, model.Types[0].EffectiveEqualsStrategy);
        Assert.False(model.Types[0].IsEqualsAndHashCode);
    }

    [Fact]
    public void VmfModel_SetsDefaultEqualsStrategy()
    {
        var sym = MakeInterface("IFoo");
        sym.VmfModelAttribute = new VmfModelData { Value = EqualsStrategy.All };
        var other = MakeInterface("IBar");

        var model = ModelAnalyzer.Analyze(Ns, new[] { sym, other });
        Assert.Equal(EqualsStrategy.All, model.Config.EqualsDefault);
        Assert.Equal(EqualsStrategy.All, model.Types.First(t => t.TypeName == "IBar").EffectiveEqualsStrategy);
    }

    [Fact]
    public void VmfEquals_OverridesDefault()
    {
        var sym = MakeInterface("IFoo");
        sym.VmfModelAttribute = new VmfModelData { Value = EqualsStrategy.All };
        var bar = MakeInterface("IBar");
        bar.VmfEqualsAttribute = new VmfEqualsData { Value = EqualsStrategy.Instance };

        var model = ModelAnalyzer.Analyze(Ns, new[] { sym, bar });
        Assert.Equal(EqualsStrategy.Instance, model.Types.First(t => t.TypeName == "IBar").EffectiveEqualsStrategy);
    }

    // --- External types ---

    [Fact]
    public void ExternalType_RegisteredNotAsModelType()
    {
        var ext = MakeInterface("IExternal");
        ext.ExternalTypeNamespace = "Some.External.Ns";
        var normal = MakeInterface("IFoo");

        var model = ModelAnalyzer.Analyze(Ns, new[] { ext, normal });
        Assert.False(model.HasErrors);
        Assert.Single(model.Types); // Only IFoo
        Assert.True(model.IsExternalType("IExternal"));
    }

    // --- Immutability ---

    [Fact]
    public void ImmutableType_MarkedCorrectly()
    {
        var sym = MakeInterface("IPoint");
        sym.IsImmutable = true;
        var model = ModelAnalyzer.Analyze(Ns, new[] { sym });
        Assert.True(model.Types[0].IsImmutable);
    }

    // --- Delegation ---

    [Fact]
    public void MethodDelegation_Parsed()
    {
        var sym = MakeInterface("IFoo");
        sym.MethodDelegations.Add(new DelegationSymbolData
        {
            FullTypeName = "Test.FooBehavior",
            MethodName = "DoSomething",
            ReturnType = "void",
            ParamTypes = new List<string> { "int" },
            ParamNames = new List<string> { "count" },
        });

        var model = ModelAnalyzer.Analyze(Ns, new[] { sym });
        Assert.False(model.HasErrors);
        var t = model.Types[0];
        Assert.Single(t.MethodDelegations);
        Assert.Equal("DoSomething", t.MethodDelegations[0].MethodName);
    }

    [Fact]
    public void ConstructorDelegation_Parsed()
    {
        var sym = MakeInterface("IFoo");
        sym.ConstructorDelegation = new DelegationSymbolData
        {
            FullTypeName = "Test.FooInit",
            MethodName = "",
            ReturnType = "void",
        };

        var model = ModelAnalyzer.Analyze(Ns, new[] { sym });
        Assert.False(model.HasErrors);
        var t = model.Types[0];
        Assert.Single(t.ConstructorDelegations);
        Assert.True(t.ConstructorDelegations[0].IsConstructorDelegation);
    }

    // --- Validation ---

    [Fact]
    public void MutableType_CannotHaveGetterOnly()
    {
        var prop = MakeProp("Value", "int", "int", isPrimitive: true);
        prop.IsGetterOnly = true;
        var sym = MakeInterface("IFoo", prop);
        // not immutable, not interface-only -> error
        var model = ModelAnalyzer.Analyze(Ns, new[] { sym });
        Assert.True(model.HasErrors);
        Assert.Contains(model.Diagnostics, d => d.Message.Contains("getter-only"));
    }

    [Fact]
    public void MutableType_CannotExtendImmutable()
    {
        var immutable = MakeInterface("IBase");
        immutable.IsImmutable = true;
        var mutable = MakeInterface("IDerived");
        mutable.BaseTypeNames.Add($"{Ns}.IBase");

        var model = ModelAnalyzer.Analyze(Ns, new[] { immutable, mutable });
        Assert.True(model.HasErrors);
        Assert.Contains(model.Diagnostics, d => d.Message.Contains("cannot extend immutable"));
    }

    [Fact]
    public void ImmutableType_CannotExtendMutable()
    {
        var mutable = MakeInterface("IBase");
        var immutable = MakeInterface("IDerived");
        immutable.IsImmutable = true;
        immutable.BaseTypeNames.Add($"{Ns}.IBase");

        var model = ModelAnalyzer.Analyze(Ns, new[] { mutable, immutable });
        Assert.True(model.HasErrors);
        Assert.Contains(model.Diagnostics, d => d.Message.Contains("cannot extend mutable"));
    }

    // --- Annotations ---

    [Fact]
    public void Annotations_ParsedAndSorted()
    {
        var sym = MakeInterface("IFoo");
        sym.Annotations.Add(new AnnotationData { Key = "z-key", Value = "z-val" });
        sym.Annotations.Add(new AnnotationData { Key = "a-key", Value = "a-val" });

        var model = ModelAnalyzer.Analyze(Ns, new[] { sym });
        var t = model.Types[0];
        Assert.Equal(2, t.Annotations.Count);
        Assert.Equal("a-key", t.Annotations[0].Key);
        Assert.Equal("z-key", t.Annotations[1].Key);
    }

    // --- PropId assignment ---

    [Fact]
    public void PropIds_AssignedSequentially()
    {
        var a = MakeProp("Alpha", "int", "int", isPrimitive: true);
        var b = MakeProp("Beta", "int", "int", isPrimitive: true);
        var c = MakeProp("Gamma", "int", "int", isPrimitive: true);
        var sym = MakeInterface("IFoo", a, b, c);

        var model = ModelAnalyzer.Analyze(Ns, new[] { sym });
        Assert.False(model.HasErrors);
        var props = model.Types[0].AllProperties;
        Assert.Equal(3, props.Count);
        Assert.Equal(0, props[0].PropId);
        Assert.Equal(1, props[1].PropId);
        Assert.Equal(2, props[2].PropId);
    }

    // --- InterfaceOnly ---

    [Fact]
    public void InterfaceOnly_GetterOnly_AllowedOnMutableInterfaceOnlyType()
    {
        var prop = MakeProp("Value", "int", "int", isPrimitive: true);
        prop.IsGetterOnly = true;
        var sym = MakeInterface("IFoo", prop);
        sym.IsInterfaceOnly = true;
        var model = ModelAnalyzer.Analyze(Ns, new[] { sym });
        Assert.False(model.HasErrors);
    }

    // --- Documentation ---

    [Fact]
    public void Documentation_PreservedOnTypeAndProperty()
    {
        var prop = MakeProp("Name", "string", "string", isPrimitive: true);
        prop.Documentation = "The name.";
        var sym = MakeInterface("IFoo", prop);
        sym.Documentation = "A foo type.";

        var model = ModelAnalyzer.Analyze(Ns, new[] { sym });
        var t = model.Types[0];
        Assert.True(t.IsDocumented);
        Assert.Equal("A foo type.", t.Documentation);
        Assert.True(t.Properties[0].IsDocumented);
    }
}
