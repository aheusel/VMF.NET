// Mirrors eu.mihosoft.vmf.tutorial12.vmfmodel (Store, Item), which delegates toString().
//
// Java's own test suite never covers a delegated toString() -- only the tutorial does -- which is
// how VMF.NET shipped 0.3.0 generating uncompilable code for it (the generator emitted its own
// ToString() *and* the delegating one, CS0111).
//
// Crate deliberately does NOT delegate: it exists to check that a parent printing a delegating
// child picks up the child's custom form, which is what Java's __vmf_toString does.

using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.Models.VmfModel;

interface Store
{
    string? Id { get; set; }

    Item[] Items { get; }

    [DelegateTo(typeof(VMF.NET.TestSuite.Models.StoreDelegate))]
    string ToString();
}

interface Item
{
    string? Id { get; set; }

    [DelegateTo(typeof(VMF.NET.TestSuite.Models.ItemDelegate))]
    string ToString();
}

interface Crate
{
    string? Label { get; set; }

    Item[] Items { get; }
}
