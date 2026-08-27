// Ported from eu.mihosoft.vmftest.complex.account.AccountTest

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.Complex.Account;

public class AccountTest
{
    [Fact]
    public void TestCrossReference()
    {
        var model = AccountModel.NewInstance();

        var a1 = IAccount.NewInstance();
        model.Accounts.Add(a1);
        var a2 = IAccount.NewInstance();
        model.Accounts.Add(a2);

        var c1 = PrivateCustomer.NewBuilder()
            .WithFirstName("John").WithLastName("Potter").Build();

        a1.AuthorizedSignatories.Add(c1);
        model.Customers.Add(c1);
        a2.AuthorizedSignatories.Add(c1);
        model.Customers.Add(c1);

        // account 1 must contain our customer / has exactly one customer
        Assert.Equal(new[] { c1 }, a1.AuthorizedSignatories);
        Assert.Single(a1.AuthorizedSignatories);
        // account 2 must contain our customer / has exactly one customer
        Assert.Equal(new[] { c1 }, a2.AuthorizedSignatories);
        Assert.Single(a2.AuthorizedSignatories);

        // our customer has both accounts, in that order / has exactly two accounts
        Assert.Equal(new[] { a1, a2 }, c1.Accounts);
        Assert.Equal(2, c1.Accounts.Count);

        a1.AuthorizedSignatories.Remove(c1);

        // after removing our customer from a1 our customer has exactly one account,
        // and it is a2
        Assert.Single(c1.Accounts);
        Assert.Equal(new[] { a2 }, c1.Accounts);
    }
}
