// Ported from eu.mihosoft.vmftest.complex.account.vmfmodel.Account

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.Account.VmfModel;

interface IAccountModel
{
    [Contains("ICustomer.Model")]
    VList<ICustomer> Customers { get; }

    [Contains("IAccount.Model")]
    VList<IAccount> Accounts { get; }
}

[Doc("A bank account has one or more authorized signatories.")]
interface IAccount
{
    string? Name { get; set; }

    [Refers("ICustomer.Accounts")]
    VList<ICustomer> AuthorizedSignatories { get; }

    [Container("IAccountModel.Accounts")]
    IAccountModel? Model { get; }
}

[Doc("A customer can have one or more bank accounts.")]
[InterfaceOnly]
interface ICustomer
{
    [Doc("Returns all bank accounts of this customer.")]
    [Refers("IAccount.AuthorizedSignatories")]
    VList<IAccount> Accounts { get; }

    [Container("IAccountModel.Customers")]
    IAccountModel? Model { get; }
}

[Doc("A private customer has a name and a residential address.")]
interface IPrivateCustomer : ICustomer
{
    string? FirstName { get; set; }
    string? LastName { get; set; }
    IAddress? ResidentialAddress { get; set; }
}

[Doc("A business customer is a company.")]
interface IBusinessCustomer : ICustomer
{
    string? CompanyName { get; set; }
    IAddress? CompanyAddress { get; set; }
}

[Doc("An address for customers.")]
interface IAddress
{
    string? Street { get; set; }
    string? City { get; set; }
    string? Postal { get; set; }
}
