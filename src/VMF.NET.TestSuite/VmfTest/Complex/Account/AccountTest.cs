// Ported from eu.mihosoft.vmftest.complex.account.AccountTest

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.Complex.Account;

public class AccountTest
{
    [Fact]
    public void TestCrossReference()
    {
        var model = IAccountModel.NewInstance();

        var a1 = IAccount.NewInstance();
        model.Accounts.Add(a1);
        var a2 = IAccount.NewInstance();
        model.Accounts.Add(a2);

        var c1 = IPrivateCustomer.NewBuilder().WithFirstName("John").WithLastName("Potter").Build();

        a1.AuthorizedSignatories.Add(c1);
        model.Customers.Add(c1);
        a2.AuthorizedSignatories.Add(c1);
        model.Customers.Add(c1);

        Assert.Contains(c1, a1.AuthorizedSignatories);
        Assert.Single(a1.AuthorizedSignatories);
        Assert.Contains(c1, a2.AuthorizedSignatories);
        Assert.Single(a2.AuthorizedSignatories);

        // the customer sees both accounts through the opposite side
        Assert.Contains(a1, c1.Accounts);
        Assert.Contains(a2, c1.Accounts);
        Assert.Equal(2, c1.Accounts.Count);

        a1.AuthorizedSignatories.Remove(c1);

        Assert.Single(c1.Accounts);
        Assert.Contains(a2, c1.Accounts);
    }
}
