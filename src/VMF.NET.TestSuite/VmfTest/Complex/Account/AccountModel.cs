// Ported from eu.mihosoft.vmftest.complex.account.vmfmodel.Account

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.Account.VmfModel;

interface AccountModel
{
    [Contains("Customer.Model")]
    Customer[] Customers { get; }

    [Contains("Account.Model")]
    Account[] Accounts { get; }
}

[Doc("A bank account has one or more authorized signatories.")]
interface Account
{
    string? Name { get; set; }

    [Refers("Customer.Accounts")]
    Customer[] AuthorizedSignatories { get; }

    [Container("AccountModel.Accounts")]
    AccountModel? Model { get; }
}

[Doc("A customer can have one or more bank accounts.")]
[InterfaceOnly]
interface Customer
{
    [Doc("Returns all bank accounts of this customer.")]
    [Refers("Account.AuthorizedSignatories")]
    Account[] Accounts { get; }

    [Container("AccountModel.Customers")]
    AccountModel? Model { get; }
}

[Doc("A private customer has a name and a residential address.")]
interface PrivateCustomer : Customer
{
    string? FirstName { get; set; }
    string? LastName { get; set; }
    Address? ResidentialAddress { get; set; }
}

[Doc("A business customer is a company.")]
interface BusinessCustomer : Customer
{
    string? CompanyName { get; set; }
    Address? CompanyAddress { get; set; }
}

[Doc("An address for customers.")]
interface Address
{
    string? Street { get; set; }
    string? City { get; set; }
    string? Postal { get; set; }
}
