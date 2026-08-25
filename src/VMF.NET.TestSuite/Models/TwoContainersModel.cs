// A type contained through two DIFFERENT container properties of the SAME containing type.
//
// An object is contained through at most one container property, and which one is recorded in
// the container property id. Both properties here are Shelf?, so a container getter that tests
// the container's runtime type instead of that id cannot tell them apart and answers for both.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.Models.VmfModel;

interface Shelf
{
    [Contains("Book.FrontShelf")]
    Book[] Front { get; }

    [Contains("Book.BackShelf")]
    Book[] Back { get; }

    [Contains("Book.Featured")]
    Book? Featured { get; set; }
}

interface Book
{
    string? Title { get; set; }

    [Container("Shelf.Front")]
    Shelf? FrontShelf { get; set; }

    [Container("Shelf.Back")]
    Shelf? BackShelf { get; set; }

    [Container("Shelf.Featured")]
    Shelf? Featured { get; set; }
}
