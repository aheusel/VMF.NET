// Ported from eu.mihosoft.vmftest.complex.supplier.SupplierTest
//
// The Java fact has an empty body: it exists so the supplier model is generated and loaded.
// The port keeps that intent but at least touches the model, so it fails if generation breaks.

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.Complex.Supplier;

public class SupplierTest
{
    [Fact]
    public void SupplierTest_ModelIsUsable()
    {
        var supplier = ISupplier.NewInstance();
        supplier.Name = "ACME";

        var customer = ICustomer.NewInstance();
        customer.CustomerID = 1;
        supplier.Customers.Add(customer);

        var order = IPurchaseOrder.NewInstance();
        supplier.Orders.Add(order);

        Assert.Equal("ACME", supplier.Name);
        Assert.Single(supplier.Customers);
        Assert.Single(supplier.Orders);
        Assert.Same(supplier, customer.Supplier);
        Assert.Same(supplier, order.Supplier);
    }
}
