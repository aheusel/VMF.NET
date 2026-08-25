// Ported from eu.mihosoft.vmftest.complex.library.vmfmodel.Library

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.Library.VmfModel;

interface ILibrary
{
    string? Name { get; set; }

    [Contains("Book.Library")]
    Book[] Books { get; }

    [Contains("Writer.Library")]
    Writer[] Authors { get; }
}

interface Book
{
    string? Title { get; set; }
    int? Pages { get; set; }

    [Container("ILibrary.Books")]
    ILibrary? Library { get; }

    [Refers("Writer.Books")]
    Writer[] Authors { get; }
}

interface Writer
{
    string? Name { get; set; }

    [Container("ILibrary.Authors")]
    ILibrary? Library { get; }

    [Refers("Book.Authors")]
    Book[] Books { get; }
}
