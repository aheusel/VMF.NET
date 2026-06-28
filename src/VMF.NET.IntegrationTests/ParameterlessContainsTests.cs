// Runtime acceptance for parameterless [Contains] (opposite-less containment).
// Requires the model to compile (Tier 1 green) — i.e. ContainsAttribute must expose a parameterless
// constructor. These assert the containment semantics that the generator's "without opposite" path
// (containing_props_without_opposite → UnregisterFromContainers) is responsible for.

using System.Text.Json;
using VMF.NET.IntegrationTests.Models;
using VMF.NET.Json;
using VMF.NET.Runtime;
using Xunit;

namespace VMF.NET.IntegrationTests;

public class ParameterlessContainsTests
{
    private static JsonSerializerOptions Options()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        options.Converters.Add(new VmfJsonConverterFactory());
        return options;
    }

    [Fact]
    public void Items_can_be_added_and_read()
    {
        var box = IBox.NewInstance();
        box.Label = "crate";

        var a = IBoxItem.NewInstance(); a.Name = "a";
        var b = IBoxItem.NewInstance(); b.Name = "b";
        box.Items.Add(a);
        box.Items.Add(b);

        Assert.Equal(2, box.Items.Count);
        Assert.Equal("a", box.Items[0].Name);
        Assert.Equal("b", box.Items[1].Name);
    }

    [Fact]
    public void Adding_a_contained_item_to_another_container_moves_it()
    {
        // The defining behavior of containment: an item already contained is automatically removed
        // from its previous container when added to a new one. With NO opposite back-reference this
        // is driven entirely by the internal parent link + the containing_props_without_opposite
        // cleanup in UnregisterFromContainers.
        var box1 = IBox.NewInstance(); box1.Label = "first";
        var box2 = IBox.NewInstance(); box2.Label = "second";

        var item = IBoxItem.NewInstance(); item.Name = "movable";
        box1.Items.Add(item);
        Assert.Equal(1, box1.Items.Count);

        box2.Items.Add(item);

        Assert.Equal(0, box1.Items.Count);   // moved out of box1 (no back-ref needed)
        Assert.Equal(1, box2.Items.Count);
        Assert.Same(item, box2.Items[0]);
    }

    [Fact]
    public void Removing_an_item_detaches_it()
    {
        var box = IBox.NewInstance(); box.Label = "crate";
        var item = IBoxItem.NewInstance(); item.Name = "x";
        box.Items.Add(item);
        Assert.Equal(1, box.Items.Count);

        box.Items.Remove(item);
        Assert.Equal(0, box.Items.Count);

        // Detached → can be placed into another container without complaint.
        var other = IBox.NewInstance(); other.Label = "other";
        other.Items.Add(item);
        Assert.Equal(1, other.Items.Count);
    }

    [Fact]
    public void Items_round_trip_through_json()
    {
        var box = IBox.NewInstance();
        box.Label = "crate";
        var a = IBoxItem.NewInstance(); a.Name = "a";
        var b = IBoxItem.NewInstance(); b.Name = "b";
        box.Items.Add(a);
        box.Items.Add(b);

        var options = Options();
        var json = JsonSerializer.Serialize<IVObject>(box, options);
        var restored = JsonSerializer.Deserialize<IBox>(json, options)!;

        Assert.Equal("crate", restored.Label);
        Assert.Equal(2, restored.Items.Count);
        Assert.Equal("a", restored.Items[0].Name);
        Assert.Equal("b", restored.Items[1].Name);
    }

    [Fact]
    public void Containment_is_reestablished_after_deserialize()
    {
        // There is no public back-reference to assert the parent directly, so prove the internal
        // containment link was rebuilt on read: moving a restored item into a new container must
        // remove it from the deserialized one.
        var box = IBox.NewInstance();
        box.Label = "crate";
        var a = IBoxItem.NewInstance(); a.Name = "a";
        var b = IBoxItem.NewInstance(); b.Name = "b";
        box.Items.Add(a);
        box.Items.Add(b);

        var options = Options();
        var json = JsonSerializer.Serialize<IVObject>(box, options);
        var restored = JsonSerializer.Deserialize<IBox>(json, options)!;

        var dest = IBox.NewInstance(); dest.Label = "dest";
        dest.Items.Add(restored.Items[0]);   // move 'a' out of the restored box

        Assert.Equal(1, dest.Items.Count);
        Assert.Equal(1, restored.Items.Count);     // containment was rebuilt → 'a' left the restored box
        Assert.Equal("b", restored.Items[0].Name);
    }
}
