// Ported from eu.mihosoft.vmftest.complex.library.vmfmodel.Library

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.Library.VmfModel;

interface ILibrary
{
    string? Name { get; set; }

    [Contains("IBook.Library")]
    VList<IBook> Books { get; }

    [Contains("IWriter.Library")]
    VList<IWriter> Authors { get; }
}

interface IBook
{
    string? Title { get; set; }
    int? Pages { get; set; }

    [Container("ILibrary.Books")]
    ILibrary? Library { get; }

    [Refers("IWriter.Books")]
    VList<IWriter> Authors { get; }
}

interface IWriter
{
    string? Name { get; set; }

    [Container("ILibrary.Authors")]
    ILibrary? Library { get; }

    [Refers("IBook.Authors")]
    VList<IBook> Books { get; }
}
