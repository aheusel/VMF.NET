// Ported from eu.mihosoft.vmf.VMFGenerateRuns -- the facts that set up DaBean:
// testGetterSetterFeature, testCloneFeature, testReadOnlyFeature.
//
// Java drives generated code through VMFTestShell, which compiles the model at test time and
// evaluates Groovy against it. Here the generator runs at build time, so the ports call the
// generated API directly. Java's findGeneratedCode/println calls assert nothing and are dropped.

using System.Reflection;
using Xunit;

namespace VMF.NET.TestSuite.VmfTest.Test1;

public class DaBeanTest
{
    [Fact]
    public void TestGetterSetterFeature()
    {
        var aDaBean = IDaBean.NewInstance();
        aDaBean.Name = "testName";
        Assert.Equal("testName", aDaBean.Name);
    }

    [Fact]
    public void TestCloneFeature()
    {
        var aDaBean = IDaBean.NewInstance();
        aDaBean.Name = "testName";
        var cloneBean = aDaBean.Clone();
        Assert.Equal("testName", cloneBean.Name);
    }

    [Fact]
    public void TestReadOnlyFeature()
    {
        // DEVIATION: Java asserts that roBean.setName("test") raises MissingMethodException,
        // which is a RUNTIME failure only because the shell evaluates Groovy. C# resolves
        // members at compile time, so the equivalent statement would not compile at all. The
        // fact is therefore expressed as what makes it uncompilable: the read-only view exposes
        // no setter for the property.
        var aDaBean = IDaBean.NewInstance();
        var roBean = aDaBean.AsReadOnly();

        var name = roBean.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(name);
        Assert.True(name!.CanRead, "the read-only view must still expose the getter");
        Assert.False(name.CanWrite, "the read-only view must expose no setter");
    }
}
