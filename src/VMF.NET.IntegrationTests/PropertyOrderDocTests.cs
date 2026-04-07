using VMF.NET.IntegrationTests.Models;
using VMF.NET.Runtime;
using VMF.NET.Runtime.Internal;
using Xunit;

namespace VMF.NET.IntegrationTests;

/// <summary>
/// Tests for property ordering and doc annotations.
/// </summary>
public class PropertyOrderDocTests
{
    [Fact]
    public void PropertyOrder_ReflectedInCorrectOrder()
    {
        var config = IConfig.NewInstance();
        var reflect = config.Vmf().Reflect();
        var props = reflect.Properties();

        Assert.Equal(3, props.Count);
        Assert.Equal("Host", props[0].Name);      // order 0
        Assert.Equal("Protocol", props[1].Name);   // order 1
        Assert.Equal("Port", props[2].Name);       // order 2
    }

    [Fact]
    public void PropertyOrder_BuilderSetsInAnyOrder()
    {
        var config = IConfig.NewBuilder()
            .WithPort(8080)
            .WithHost("localhost")
            .WithProtocol("https")
            .Build();

        Assert.Equal("localhost", config.Host);
        Assert.Equal("https", config.Protocol);
        Assert.Equal(8080, config.Port);
    }

    [Fact]
    public void PropertyOrder_GetPropertyIdByName_RespectsOrder()
    {
        var config = IConfig.NewInstance();
        var intern = (IVObjectInternal)config;

        // Host is order 0, Protocol is order 1, Port is order 2
        Assert.Equal(0, intern.GetPropertyIdByName("Host"));
        Assert.Equal(1, intern.GetPropertyIdByName("Protocol"));
        Assert.Equal(2, intern.GetPropertyIdByName("Port"));
    }

    [Fact]
    public void Doc_TypeAnnotation_ReflectedInReadOnlyInterface()
    {
        // The [Doc] attribute should be available via the model's annotations
        var config = IConfig.NewInstance();
        var intern = (IVObjectInternal)config;

        // Verify type info is accessible
        var vmfType = intern.GetVmfType();
        Assert.NotNull(vmfType);
    }

    [Fact]
    public void Config_PropertiesWork()
    {
        var config = IConfig.NewInstance();
        config.Host = "example.com";
        config.Protocol = "http";
        config.Port = 443;

        Assert.Equal("example.com", config.Host);
        Assert.Equal("http", config.Protocol);
        Assert.Equal(443, config.Port);
    }

    [Fact]
    public void Config_ToString_ContainsValues()
    {
        var config = IConfig.NewBuilder()
            .WithHost("localhost")
            .WithProtocol("https")
            .WithPort(8080)
            .Build();

        var str = config.ToString();
        Assert.Contains("localhost", str);
        Assert.Contains("https", str);
        Assert.Contains("8080", str);
    }
}
