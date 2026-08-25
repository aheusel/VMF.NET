// A type contained through two DIFFERENT container properties of the SAME containing type.
//
// An object is contained through at most one container property, and which one is recorded in
// the container property id. Both properties here are IShelf?, so a container getter that tests
// the container's runtime type instead of that id cannot tell them apart and answers for both.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.Models.VmfModel;

interface IShelf
{
    [Contains("IBook.FrontShelf")]
    IBook[] Front { get; }

    [Contains("IBook.BackShelf")]
    IBook[] Back { get; }

    [Contains("IBook.Featured")]
    IBook? Featured { get; set; }
}

interface IBook
{
    string? Title { get; set; }

    [Container("IShelf.Front")]
    IShelf? FrontShelf { get; set; }

    [Container("IShelf.Back")]
    IShelf? BackShelf { get; set; }

    [Container("IShelf.Featured")]
    IShelf? Featured { get; set; }
}
