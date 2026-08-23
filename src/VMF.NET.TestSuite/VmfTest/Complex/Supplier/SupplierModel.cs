// Ported from eu.mihosoft.vmftest.complex.supplier.vmfmodel.Supplier
// java.util.Date maps to System.DateTime.

using System;
using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.Supplier;

[VmfModel]
[Doc("Supplier has customers and processes orders.")]
public partial interface ISupplier
{
    string? Name { get; set; }

    [Contains("ICustomer.Supplier")]
    VList<ICustomer> Customers { get; }

    [Contains("IPurchaseOrder.Supplier")]
    VList<IPurchaseOrder> Orders { get; }
}

[VmfModel]
[Doc("Customer of a supplier. It has a unique id.")]
public partial interface ICustomer
{
    [Container("ISupplier.Customers")]
    ISupplier? Supplier { get; }

    int? CustomerID { get; set; }

    [Refers("IPurchaseOrder.Customer")]
    VList<IPurchaseOrder> Orders { get; }
}

[VmfModel]
[Doc("A purchase order.")]
public partial interface IPurchaseOrder
{
    string? Comment { get; set; }
    DateTime? Date { get; set; }
    string? Status { get; set; }

    [Refers("ICustomer.Orders")]
    ICustomer? Customer { get; set; }

    IPurchaseOrder? PreviousOrder { get; set; }

    [Contains("IItem.PurchaseOrder")]
    VList<IItem> Items { get; }

    [Contains]
    IAddress? BillTo { get; set; }

    [Contains]
    IAddress? ShipTo { get; set; }

    [Container("ISupplier.Orders")]
    ISupplier? Supplier { get; }
}

[VmfModel]
[Doc("Item provided by a supplier.")]
public partial interface IItem
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

[VmfModel]
[Doc("An address used for shippment and billing.")]
[InterfaceOnly]
public partial interface IAddress
{
    string? Name { get; set; }
}

[VmfModel]
[Doc("US address")]
public partial interface IUSAddress : IAddress
{
    int? Zip { get; set; }
    string? City { get; set; }
    string? Street { get; set; }
    string? State { get; set; }
}

[VmfModel]
[Doc("Global address.")]
public partial interface IGlobalAddress : IAddress
{
    string? Country { get; set; }
    int? Zip { get; set; }
    string? City { get; set; }
    string? Street { get; set; }
}
