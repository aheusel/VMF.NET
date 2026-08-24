// Regression tests for container-property identity.
//
// One object is contained through at most one container property, and which one is recorded in
// the container property id. The generated getter used to test the CONTAINER'S RUNTIME TYPE
// instead -- so two container properties naming the same containing type were indistinguishable,
// and both reported the container. Java tests the id (getter-container.vm), and now so does this.

using VMF.NET.Runtime;
using VMF.NET.TestSuite.Models;
using Xunit;

namespace VMF.NET.TestSuite;

public class ContainerPropertyIdTests
{
    [Fact]
    public void ContainerProperty_AnswersOnlyForThePropertyThatContains()
    {
        var shelf = IShelf.NewInstance();
        var book = IBook.NewInstance();
        book.Title = "Moby-Dick";

        shelf.Back.Add(book);

        Assert.Same(shelf, book.BackShelf);
        // both of these reported `shelf`, because IShelf matched on type
        Assert.Null(book.FrontShelf);
        Assert.Null(book.Featured);
    }

    [Fact]
    public void ContainerProperty_FollowsTheObjectWhenItMoves()
    {
        var shelf = IShelf.NewInstance();
        var book = IBook.NewInstance();

        shelf.Front.Add(book);
        Assert.Same(shelf, book.FrontShelf);
        Assert.Null(book.BackShelf);

        // moving between two lists of the SAME shelf changes only which property answers
        shelf.Back.Add(book);
        Assert.Same(shelf, book.BackShelf);
        Assert.Null(book.FrontShelf);
        Assert.Empty(shelf.Front);
    }

    [Fact]
    public void ContainerProperty_ScalarContainmentIsAlsoDistinguished()
    {
        var shelf = IShelf.NewInstance();
        var book = IBook.NewInstance();

        shelf.Featured = book;

        Assert.Same(shelf, book.Featured);
        Assert.Null(book.FrontShelf);
        Assert.Null(book.BackShelf);
    }

    [Fact]
    public void SettingAContainerProperty_DrivesTheMatchingOpposite()
    {
        var shelf = IShelf.NewInstance();
        var book = IBook.NewInstance();

        book.BackShelf = shelf;

        Assert.Single(shelf.Back);
        Assert.Empty(shelf.Front);
        Assert.Same(shelf, book.BackShelf);

        // re-assigning the same shelf through a DIFFERENT property must not return early
        book.FrontShelf = shelf;

        Assert.Single(shelf.Front);
        Assert.Empty(shelf.Back);
        Assert.Same(shelf, book.FrontShelf);
        Assert.Null(book.BackShelf);
    }

    [Fact]
    public void Reflection_AgreesWithTheGetter()
    {
        var shelf = IShelf.NewInstance();
        var book = IBook.NewInstance();
        shelf.Back.Add(book);

        var reflect = book.Vmf().Reflect();

        Assert.Same(shelf, reflect.PropertyByName("BackShelf")!.Get());
        Assert.Null(reflect.PropertyByName("FrontShelf")!.Get());
        Assert.True(reflect.PropertyByName("BackShelf")!.IsSet);
        Assert.False(reflect.PropertyByName("FrontShelf")!.IsSet);
    }
}
