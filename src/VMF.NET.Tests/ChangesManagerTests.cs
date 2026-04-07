using Xunit;
using VMF.NET.Runtime;
using VMF.NET.Runtime.Internal;

namespace VMF.NET.Tests;

public class ChangesManagerTests
{
    [Fact]
    public void AddListener_NotifiesOnPropertyChange()
    {
        var obj = new FakeVObject();
        var mgr = new ChangesManager(obj);
        IChange? captured = null;
        mgr.AddListener(c => captured = c);

        mgr.FirePropertyChange(obj, "Name", 0, "old", "new");

        Assert.NotNull(captured);
        Assert.Equal("Name", captured.PropertyName);
        Assert.Equal(ChangeType.Property, captured.ChangeType);
        Assert.NotNull(captured.PropertyChange);
        Assert.Equal("old", captured.PropertyChange!.OldValue);
        Assert.Equal("new", captured.PropertyChange!.NewValue);
    }

    [Fact]
    public void Recording_TrackChanges()
    {
        var obj = new FakeVObject();
        var mgr = new ChangesManager(obj);
        mgr.Start();

        mgr.FirePropertyChange(obj, "Name", 0, null, "a");
        mgr.FirePropertyChange(obj, "Name", 0, "a", "b");

        Assert.Equal(2, mgr.All().Count);
        Assert.True(mgr.IsModelVersioningEnabled);
    }

    [Fact]
    public void Recording_NotStarted_NoChangesCollected()
    {
        var obj = new FakeVObject();
        var mgr = new ChangesManager(obj);

        mgr.FirePropertyChange(obj, "Name", 0, null, "a");

        Assert.Empty(mgr.All());
    }

    [Fact]
    public void Transactions_GroupChanges()
    {
        var obj = new FakeVObject();
        var mgr = new ChangesManager(obj);
        mgr.Start();

        mgr.StartTransaction();
        mgr.FirePropertyChange(obj, "Name", 0, null, "a");
        mgr.FirePropertyChange(obj, "Value", 1, 0, 42);
        mgr.PublishTransaction();

        Assert.Single(mgr.Transactions());
        Assert.Equal(2, mgr.Transactions()[0].Changes.Count);
    }

    [Fact]
    public void Stop_PublishesUnpublishedTransaction()
    {
        var obj = new FakeVObject();
        var mgr = new ChangesManager(obj);
        mgr.Start();

        mgr.StartTransaction();
        mgr.FirePropertyChange(obj, "Name", 0, null, "a");
        mgr.Stop();

        Assert.Single(mgr.Transactions());
    }

    [Fact]
    public void Clear_RemovesAllRecordedChanges()
    {
        var obj = new FakeVObject();
        var mgr = new ChangesManager(obj);
        mgr.Start();
        mgr.FirePropertyChange(obj, "Name", 0, null, "a");
        mgr.Clear();

        Assert.Empty(mgr.All());
    }

    [Fact]
    public void Unsubscribe_StopsNotifications()
    {
        var obj = new FakeVObject();
        var mgr = new ChangesManager(obj);
        int count = 0;
        var sub = mgr.AddListener(_ => count++);

        mgr.FirePropertyChange(obj, "Name", 0, null, "a");
        Assert.Equal(1, count);

        sub.Dispose();
        mgr.FirePropertyChange(obj, "Name", 0, "a", "b");
        Assert.Equal(1, count);
    }

    [Fact]
    public void ModelVersion_IncreasesWithChanges()
    {
        var obj = new FakeVObject();
        var mgr = new ChangesManager(obj);
        mgr.Start();

        var v1 = mgr.ModelVersion();
        mgr.FirePropertyChange(obj, "Name", 0, null, "a");
        var v2 = mgr.ModelVersion();

        Assert.Equal(0, v1.VersionNumber);
        Assert.Equal(1, v2.VersionNumber);
    }

    [Fact]
    public void ListChange_NotifiesWithCorrectType()
    {
        var obj = new FakeVObject();
        var mgr = new ChangesManager(obj);
        IChange? captured = null;
        mgr.AddListener(c => captured = c);

        var evt = VListChangeEvent.CreateAddEvent(["item"], 0);
        mgr.FireListChange(obj, "Items", evt);

        Assert.NotNull(captured);
        Assert.Equal(ChangeType.List, captured.ChangeType);
        Assert.NotNull(captured.ListChange);
        Assert.Null(captured.PropertyChange);
    }
}

/// <summary>
/// Minimal fake IVObject for testing the changes manager.
/// </summary>
internal class FakeVObject : IVObject
{
    public IVmf Vmf() => throw new NotImplementedException();
    public IVObject Clone() => throw new NotImplementedException();
    public IVObject AsReadOnly() => throw new NotImplementedException();
}
