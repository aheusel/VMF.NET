// Ported from eu.mihosoft.vmftest.complex.account.vmfmodel.Account

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.Account;

[VmfModel]
public partial interface IAccountModel
{
    [Contains("ICustomer.Model")]
    VList<ICustomer> Customers { get; }

    [Contains("IAccount.Model")]
    VList<IAccount> Accounts { get; }
}

[VmfModel]
[Doc("A bank account has one or more authorized signatories.")]
public partial interface IAccount
{
    string? Name { get; set; }

    [Refers("ICustomer.Accounts")]
    VList<ICustomer> AuthorizedSignatories { get; }

    [Container("IAccountModel.Accounts")]
    IAccountModel? Model { get; }
}

[VmfModel]
[Doc("A customer can have one or more bank accounts.")]
[InterfaceOnly]
public partial interface ICustomer
{
    [Doc("Returns all bank accounts of this customer.")]
    [Refers("IAccount.AuthorizedSignatories")]
    VList<IAccount> Accounts { get; }

    [Container("IAccountModel.Customers")]
    IAccountModel? Model { get; }
}

[VmfModel]
[Doc("A private customer has a name and a residential address.")]
public partial interface IPrivateCustomer : ICustomer
{
    string? FirstName { get; set; }
    string? LastName { get; set; }
    IAddress? ResidentialAddress { get; set; }
}

[VmfModel]
[Doc("A business customer is a company.")]
public partial interface IBusinessCustomer : ICustomer
{
    string? CompanyName { get; set; }
    IAddress? CompanyAddress { get; set; }
}

[VmfModel]
[Doc("An address for customers.")]
public partial interface IAddress
{
    string? Street { get; set; }
    string? City { get; set; }
    string? Postal { get; set; }
}
