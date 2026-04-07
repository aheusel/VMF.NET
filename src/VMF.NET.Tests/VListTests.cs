using Xunit;
using VMF.NET.Runtime;

namespace VMF.NET.Tests;

public class VListTests
{
    [Fact]
    public void Add_FiresChangeEvent()
    {
        var list = new VList<string>();
        VListChangeEvent? captured = null;
        list.AddChangeListener(e => captured = e);

        list.Add("hello");

        Assert.NotNull(captured);
        Assert.Equal(VListChangeType.Add, captured.ChangeType);
        Assert.Single(captured.Added);
        Assert.Equal("hello", captured.Added[0]);
        Assert.Equal(0, captured.Index);
    }

    [Fact]
    public void Remove_FiresChangeEvent()
    {
        var list = new VList<string> { "a", "b", "c" };
        VListChangeEvent? captured = null;
        list.AddChangeListener(e => captured = e);

        list.RemoveAt(1);

        Assert.NotNull(captured);
        Assert.Equal(VListChangeType.Remove, captured.ChangeType);
        Assert.Single(captured.Removed);
        Assert.Equal("b", captured.Removed[0]);
        Assert.Equal(1, captured.Index);
    }

    [Fact]
    public void Set_FiresChangeEvent()
    {
        var list = new VList<string> { "a", "b" };
        VListChangeEvent? captured = null;
        list.AddChangeListener(e => captured = e);

        list[0] = "z";

        Assert.NotNull(captured);
        Assert.True(captured.WasSet);
        Assert.Equal("a", captured.Removed[0]);
        Assert.Equal("z", captured.Added[0]);
    }

    [Fact]
    public void Clear_FiresRemoveEventsPerElement()
    {
        var list = new VList<string> { "a", "b", "c" };
        var events = new List<VListChangeEvent>();
        list.AddChangeListener(e => events.Add(e));

        list.Clear();

        Assert.Equal(3, events.Count);
        Assert.All(events, e => Assert.Equal(VListChangeType.Remove, e.ChangeType));
    }

    [Fact]
    public void Unsubscribe_StopsNotifications()
    {
        var list = new VList<string>();
        int callCount = 0;
        var sub = list.AddChangeListener(_ => callCount++);

        list.Add("a");
        Assert.Equal(1, callCount);

        sub.Dispose();
        list.Add("b");
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void ElementCallbacks_Invoked()
    {
        var list = new VList<string>();
        string? addedItem = null;
        string? removedItem = null;
        list.SetOnElementAdded((item, _) => addedItem = item);
        list.SetOnElementRemoved((item, _) => removedItem = item);

        list.Add("hello");
        Assert.Equal("hello", addedItem);

        list.RemoveAt(0);
        Assert.Equal("hello", removedItem);
    }

    [Fact]
    public void Constructor_WithItems_Works()
    {
        var list = new VList<int>(new[] { 1, 2, 3 });
        Assert.Equal(3, list.Count);
        Assert.Equal(2, list[1]);
    }
}
