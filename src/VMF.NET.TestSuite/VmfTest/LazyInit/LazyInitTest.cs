// Ported from eu.mihosoft.vmftest.lazyinit.LazyInitTest
//
// Collections are lazily created. Two otherwise-identical objects must compare equal
// whether or not their list has been materialised by a prior read.

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.LazyInit;

public class LazyInitTest
{
    [Fact]
    public void TestLazyInitOfLists1()
    {
        var o1 = IObj.NewInstance();
        var o2 = IObj.NewInstance();

        // read Entries so o1's list is materialised; o2's stays null
        Assert.Empty(o1.Entries);

        Assert.Equal(o1, o2);
    }

    [Fact]
    public void TestLazyInitOfLists2()
    {
        var o1 = IObj.NewInstance();
        var o2 = IObj.NewInstance();

        // the same, with the roles reversed
        Assert.Empty(o2.Entries);

        Assert.Equal(o1, o2);
    }
}
