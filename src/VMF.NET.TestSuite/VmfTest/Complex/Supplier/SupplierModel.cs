// Ported from eu.mihosoft.vmftest.complex.supplier.vmfmodel.Supplier
// java.util.Date maps to System.DateTime.

using System;
using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.Supplier.VmfModel;

[Doc("Supplier has customers and processes orders.")]
interface ISupplier
{
    string? Name { get; set; }

    [Contains("Customer.Supplier")]
    Customer[] Customers { get; }

    [Contains("PurchaseOrder.Supplier")]
    PurchaseOrder[] Orders { get; }
}

[Doc("Customer of a supplier. It has a unique id.")]
interface Customer
{
    [Container("ISupplier.Customers")]
    ISupplier? Supplier { get; }

    int? CustomerID { get; set; }

    [Refers("PurchaseOrder.Customer")]
    PurchaseOrder[] Orders { get; }
}

[Doc("A purchase order.")]
interface PurchaseOrder
{
    string? Comment { get; set; }
    DateTime? Date { get; set; }
    string? Status { get; set; }

    [Refers("Customer.Orders")]
    Customer? Customer { get; set; }

    PurchaseOrder? PreviousOrder { get; set; }

    [Contains("Item.PurchaseOrder")]
    Item[] Items { get; }

    [Contains]
    Address? BillTo { get; set; }

    [Contains]
    Address? ShipTo { get; set; }

    [Container("ISupplier.Orders")]
    ISupplier? Supplier { get; }
}

[Doc("Item provided by a supplier.")]
interface Item
{
    string? ProductName { get; set; }
    int? Quantity { get; set; }
    double? USPrice { get; set; }
    string? Comment { get; set; }
    DateTime? ShipDate { get; set; }
    string? PartNumber { get; set; }

    [Container("PurchaseOrder.Items")]
    PurchaseOrder? PurchaseOrder { get; }
}

[Doc("An address used for shippment and billing.")]
[InterfaceOnly]
interface Address
{
    string? Name { get; set; }
}

[Doc("US address")]
interface USAddress : Address
{
    int? Zip { get; set; }
    string? City { get; set; }
    string? Street { get; set; }
    string? State { get; set; }
}

[Doc("Global address.")]
interface GlobalAddress : Address
{
    string? Country { get; set; }
    int? Zip { get; set; }
    string? City { get; set; }
    string? Street { get; set; }
}
