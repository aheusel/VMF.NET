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

    [Contains("ICustomer.Supplier")]
    ICustomer[] Customers { get; }

    [Contains("IPurchaseOrder.Supplier")]
    IPurchaseOrder[] Orders { get; }
}

[Doc("Customer of a supplier. It has a unique id.")]
interface ICustomer
{
    [Container("ISupplier.Customers")]
    ISupplier? Supplier { get; }

    int? CustomerID { get; set; }

    [Refers("IPurchaseOrder.Customer")]
    IPurchaseOrder[] Orders { get; }
}

[Doc("A purchase order.")]
interface IPurchaseOrder
{
    string? Comment { get; set; }
    DateTime? Date { get; set; }
    string? Status { get; set; }

    [Refers("ICustomer.Orders")]
    ICustomer? Customer { get; set; }

    IPurchaseOrder? PreviousOrder { get; set; }

    [Contains("IItem.PurchaseOrder")]
    IItem[] Items { get; }

    [Contains]
    IAddress? BillTo { get; set; }

    [Contains]
    IAddress? ShipTo { get; set; }

    [Container("ISupplier.Orders")]
    ISupplier? Supplier { get; }
}

[Doc("Item provided by a supplier.")]
interface IItem
{
    string? ProductName { get; set; }
    int? Quantity { get; set; }
    double? USPrice { get; set; }
    string? Comment { get; set; }
    DateTime? ShipDate { get; set; }
    string? PartNumber { get; set; }

    [Container("IPurchaseOrder.Items")]
    IPurchaseOrder? PurchaseOrder { get; }
}

[Doc("An address used for shippment and billing.")]
[InterfaceOnly]
interface IAddress
{
    string? Name { get; set; }
}

[Doc("US address")]
interface IUSAddress : IAddress
{
    int? Zip { get; set; }
    string? City { get; set; }
    string? Street { get; set; }
    string? State { get; set; }
}

[Doc("Global address.")]
interface IGlobalAddress : IAddress
{
    string? Country { get; set; }
    int? Zip { get; set; }
    string? City { get; set; }
    string? Street { get; set; }
}
