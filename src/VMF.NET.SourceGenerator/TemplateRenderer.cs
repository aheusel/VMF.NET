// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.IO;
using Scriban;
using Scriban.Runtime;
using VMF.NET.Core;

using PropertyInfo = VMF.NET.Core.PropertyInfo;

namespace VMF.NET.SourceGenerator;

/// <summary>
/// Renders Scriban templates for VMF code generation.
/// Templates are loaded as embedded resources from the Templates directory.
/// </summary>
public sealed class TemplateRenderer
{
    private readonly Dictionary<string, Template> _cache = new();

    /// <summary>
    /// Renders all generated source files for a single model type.
    /// Returns filename → source pairs.
    /// </summary>
    public IEnumerable<(string FileName, string Source)> RenderType(ModelTypeInfo type, ModelInfo model)
    {
        if (type.IsInterfaceOnly)
        {
            // Interface-only types: just the writable interface + read-only interface
            yield return ($"{type.TypeName}.g.cs", RenderTemplate("Interface", type, model));
            yield return ($"{type.ReadOnlyInterfaceName}.g.cs", RenderTemplate("ReadOnlyInterface", type, model));
            yield break;
        }

        // Writable interface (typed Clone, AsReadOnly, Builder)
        yield return ($"{type.TypeName}.g.cs", RenderTemplate("Interface", type, model));

        // Implementation class
        yield return ($"{type.ImplClassName}.g.cs", RenderTemplate("Implementation", type, model));

        if (type.IsImmutable)
        {
            // Immutable types don't need read-only wrapper (they are their own read-only view)
            // Still generate the read-only interface as a type alias
            yield return ($"{type.ReadOnlyInterfaceName}.g.cs", RenderTemplate("ReadOnlyInterface", type, model));
        }
        else
        {
            // Read-only interface
            yield return ($"{type.ReadOnlyInterfaceName}.g.cs", RenderTemplate("ReadOnlyInterface", type, model));

            // Read-only implementation
            yield return ($"{type.ReadOnlyImplClassName}.g.cs", RenderTemplate("ReadOnlyImplementation", type, model));
        }
    }

    private string RenderTemplate(string templateName, ModelTypeInfo type, ModelInfo model)
    {
        var template = GetTemplate(templateName);
        var context = CreateContext(type, model);
        return template.Render(context);
    }

    private Template GetTemplate(string name)
    {
        if (_cache.TryGetValue(name, out var cached))
            return cached;

        var source = LoadTemplateSource(name);
        var template = Template.Parse(source, name);
        if (template.HasErrors)
        {
            var errors = string.Join("\n", template.Messages);
            throw new InvalidOperationException($"Template '{name}' has errors:\n{errors}");
        }

        _cache[name] = template;
        return template;
    }

    private static string LoadTemplateSource(string name)
    {
        var assembly = typeof(TemplateRenderer).Assembly;
        var resourceName = $"VMF.NET.SourceGenerator.Templates.{name}.sbn";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException(
                $"Embedded template '{resourceName}' not found. Available: {string.Join(", ", assembly.GetManifestResourceNames())}");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static TemplateContext CreateContext(ModelTypeInfo type, ModelInfo model)
    {
        var scriptObject = new ScriptObject();

        // Import helper functions with snake_case names (Scriban convention)
        var helpers = new TemplateHelpers();
        scriptObject.Import("prop_type_name", new Func<PropertyInfo, string>(TemplateHelpers.PropTypeName));
        scriptObject.Import("read_only_prop_type_name", new Func<PropertyInfo, string>(TemplateHelpers.ReadOnlyPropTypeName));
        scriptObject.Import("field_name", new Func<PropertyInfo, string>(TemplateHelpers.FieldName));
        scriptObject.Import("raw_list_field_name", new Func<PropertyInfo, string>(TemplateHelpers.RawListFieldName));
        scriptObject.Import("prop_type_full_name", new Func<PropertyInfo, string>(TemplateHelpers.PropTypeFullName));
        scriptObject.Import("prop_type_code", new Func<PropertyInfo, int>(TemplateHelpers.PropTypeCode));
        scriptObject.Import("needs_null_check", new Func<PropertyInfo, bool>(TemplateHelpers.NeedsNullCheck));
        scriptObject.Import("default_value", new Func<PropertyInfo, string>(TemplateHelpers.DefaultValue));
        scriptObject.Import("strip_i", new Func<string, string>(TemplateHelpers.StripI));
        scriptObject.Import("camel_case", new Func<string, string>(TemplateHelpers.CamelCase));
        scriptObject.Import("int_array", new Func<IEnumerable<int>, string>(TemplateHelpers.IntArray));
        scriptObject.Import("string_array", new Func<IEnumerable<string>, string>(TemplateHelpers.StringArray));
        scriptObject.Import("is_primitive", new Func<PropertyInfo, bool>(p => p.PropType == PropType.Primitive));
        scriptObject.Import("is_nullable", new Func<PropertyInfo, bool>(p => p.PropType != PropType.Primitive));

        scriptObject.Add("type", type);
        scriptObject.Add("model", model);
        scriptObject.Add("ns", type.NamespaceName);

        // Pre-compute useful lists
        var allProps = type.AllProperties;
        var ownProps = type.Properties;
        scriptObject.Add("all_props", allProps);
        scriptObject.Add("own_props", ownProps);

        // Pre-compute string arrays for static readonly fields
        scriptObject.Add("prop_names_literal", TemplateHelpers.StringArray(allProps.Select(p => p.Name)));
        scriptObject.Add("prop_type_names_literal", TemplateHelpers.StringArray(allProps.Select(p => TemplateHelpers.PropTypeFullName(p))));
        scriptObject.Add("super_type_names_literal", TemplateHelpers.StringArray(type.Implements.Select(t => t.FullTypeName)));

        // Property classifications
        var containedProps = new List<PropertyInfo>();
        var containerProps = new List<PropertyInfo>();
        var crossRefProps = new List<PropertyInfo>();
        var simpleProps = new List<PropertyInfo>();
        var collectionProps = new List<PropertyInfo>();
        var scalarProps = new List<PropertyInfo>();
        var modelTypeIndices = new List<int>();
        var modelElementIndices = new List<int>();
        var childIndices = new List<int>();
        var parentIndices = new List<int>();

        for (int i = 0; i < allProps.Count; i++)
        {
            var p = allProps[i];
            if (p.IsContained) containedProps.Add(p);
            else if (p.IsContainer) containerProps.Add(p);
            else if (p.IsCrossRefProperty) crossRefProps.Add(p);
            else simpleProps.Add(p);

            if (p.IsCollectionType) collectionProps.Add(p);
            else scalarProps.Add(p);

            if (p.IsModelType || (p.IsCollectionType && p.GenericModelType != null)) modelTypeIndices.Add(i);
            if (p.IsCollectionType && p.GenericModelType != null) modelElementIndices.Add(i);
            if (p.IsContained) childIndices.Add(i);
            if (p.IsContainer) parentIndices.Add(i);
        }

        scriptObject.Add("contained_props", containedProps);
        scriptObject.Add("container_props", containerProps);
        scriptObject.Add("cross_ref_props", crossRefProps);
        scriptObject.Add("simple_props", simpleProps);
        scriptObject.Add("collection_props", collectionProps);
        scriptObject.Add("scalar_props", scalarProps);
        scriptObject.Add("model_type_indices", modelTypeIndices);
        scriptObject.Add("model_element_indices", modelElementIndices);
        scriptObject.Add("child_indices", childIndices);
        scriptObject.Add("parent_indices", parentIndices);

        // Equals-relevant properties
        var equalsProps = new List<PropertyInfo>();
        foreach (var p in allProps)
        {
            if (p.IsIgnoredForEquals) continue;
            if (type.IsEqualsContainmentAndExternal && !p.IsContained && p.IsModelType) continue;
            equalsProps.Add(p);
        }
        scriptObject.Add("equals_props", equalsProps);

        // ToString-relevant properties
        var toStringProps = new List<PropertyInfo>();
        foreach (var p in allProps)
        {
            if (!p.IsIgnoredForToString) toStringProps.Add(p);
        }
        scriptObject.Add("to_string_props", toStringProps);

        // Types that contain this type (for UnregisterFromContainers)
        var containingPropsWithOpposite = model.FindAllPropsThatContainType(type, true);
        var containingPropsWithoutOpposite = model.FindAllPropsThatContainType(type, false);
        scriptObject.Add("containing_props_with_opposite", containingPropsWithOpposite);
        scriptObject.Add("containing_props_without_opposite", containingPropsWithoutOpposite);

        var context = new TemplateContext();
        context.PushGlobal(scriptObject);
        context.MemberRenamer = member => member.Name;
        return context;
    }
}

/// <summary>
/// Helper functions available in Scriban templates.
/// </summary>
internal sealed class TemplateHelpers
{
    /// <summary>
    /// Returns the C# type name for a property, suitable for generated code.
    /// For collections, returns VList&lt;ElementType&gt;.
    /// </summary>
    public static string PropTypeName(PropertyInfo prop)
    {
        if (prop.IsCollectionType)
        {
            var elementType = prop.GenericModelType != null
                ? prop.GenericModelType.TypeName
                : prop.GenericTypeName ?? "object";
            return $"VList<{elementType}>";
        }

        if (prop.IsModelType)
            return prop.ModelType!.TypeName;

        return prop.SimpleTypeName;
    }

    /// <summary>
    /// Returns the read-only type name for a property.
    /// </summary>
    public static string ReadOnlyPropTypeName(PropertyInfo prop)
    {
        if (prop.IsCollectionType)
        {
            var elementType = prop.GenericModelType != null
                ? prop.GenericModelType.ReadOnlyInterfaceName
                : prop.GenericTypeName ?? "object";
            return $"IReadOnlyList<{elementType}>";
        }

        if (prop.IsModelType)
            return prop.ModelType!.ReadOnlyInterfaceName;

        return prop.SimpleTypeName;
    }

    /// <summary>
    /// Returns the field name for a property.
    /// </summary>
    public static string FieldName(PropertyInfo prop)
    {
        return $"__vmf_prop_{prop.Name}";
    }

    /// <summary>
    /// Returns the raw list field name for a collection property.
    /// </summary>
    public static string RawListFieldName(PropertyInfo prop)
    {
        return $"__vmf_prop_{prop.Name}_RawList";
    }

    /// <summary>
    /// Returns the VmfType full name string for a property (for reflection).
    /// </summary>
    public static string PropTypeFullName(PropertyInfo prop)
    {
        if (prop.IsCollectionType)
        {
            var elementType = prop.GenericTypeName ?? "object";
            var elementNs = prop.GenericPackageName;
            var fullElement = string.IsNullOrEmpty(elementNs) ? elementType : $"{elementNs}.{elementType}";
            return $"VList<{fullElement}>";
        }
        return prop.TypeName;
    }

    /// <summary>
    /// Returns the property type code for reflection (-1 for external, -2 for list, typeId for model).
    /// </summary>
    public static int PropTypeCode(PropertyInfo prop)
    {
        return prop.GetTypeId();
    }

    /// <summary>
    /// Whether the property needs a null check in equality comparison.
    /// </summary>
    public static bool NeedsNullCheck(PropertyInfo prop)
    {
        return !prop.PropType.Equals(PropType.Primitive);
    }

    /// <summary>
    /// Returns the C# default value expression for a property.
    /// </summary>
    public static string DefaultValue(PropertyInfo prop)
    {
        return prop.GetDefaultValueForCodeGen();
    }

    /// <summary>
    /// Strips leading "I" from interface names (e.g., "IParent" → "Parent").
    /// </summary>
    public static string StripI(string name)
    {
        if (name.Length > 1 && name[0] == 'I' && char.IsUpper(name[1]))
            return name.Substring(1);
        return name;
    }

    /// <summary>
    /// Converts a property name to camelCase for parameter names.
    /// </summary>
    public static string CamelCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }

    /// <summary>
    /// Returns a C# array literal from a list of integers.
    /// </summary>
    public static string IntArray(IEnumerable<int> values)
    {
        return "new int[] { " + string.Join(", ", values) + " }";
    }

    /// <summary>
    /// Returns a C# string array literal.
    /// </summary>
    public static string StringArray(IEnumerable<string> values)
    {
        return "new string[] { " + string.Join(", ", values.Select(v => $"\"{v}\"")) + " }";
    }
}
