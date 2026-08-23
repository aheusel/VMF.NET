// Ported from eu.mihosoft.vmftest.complex.library.LibraryTest

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.Complex.Library;

public class LibraryTest
{
    [Fact]
    public void CreateLibraryTest()
    {
        var library = ILibrary.NewInstance();
        library.Name = "My Library";

        var b1 = IBook.NewBuilder().WithTitle("Mastering VMF").WithPages(350).Build();
        var w1 = IWriter.NewBuilder().WithName("The Author").WithBooks(b1).Build();

        library.Authors.Add(w1);
        library.Books.Add(b1);

        Assert.Contains(b1, library.Books);
        Assert.Contains(w1, library.Authors);
        Assert.Contains(w1, b1.Authors);
        Assert.Contains(b1, w1.Books);
    }
}
