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

        var b1 = Book.NewBuilder().WithTitle("Mastering VMF").WithPages(350).Build();
        var w1 = Writer.NewBuilder().WithName("The Author").WithBooks(b1).Build();

        library.Authors.Add(w1);
        library.Books.Add(b1);

        // Library must contain a book
        Assert.Equal(new[] { b1 }, library.Books);
        // Library must contain an author
        Assert.Equal(new[] { w1 }, library.Authors);
        // The book must reference its author
        Assert.Equal(new[] { w1 }, b1.Authors);
        // The author must reference his/her books
        Assert.Equal(new[] { b1 }, w1.Books);
    }
}
