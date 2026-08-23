// Ported from eu.mihosoft.vmftest.complex.horses.vmfmodel.Horses

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.Horses;

[VmfModel]
[Doc("A barn for horses.")]
public partial interface IHorseBarn
{
    [Doc("The horses contained in this barn.")]
    [Contains]
    VList<IHorse> Horses { get; }
}

[VmfModel]
[Doc("Owner of a horse or multiple horses.")]
public partial interface IOwner
{
    [Doc("Name of the owner.")]
    string? Name { get; set; }

    [Doc("Horses owned by this owner.")]
    [Refers("IHorse.Owner")]
    VList<IHorse> Horses { get; }
}

[VmfModel]
[Doc("A horse.")]
public partial interface IHorse
{
    string? Name { get; set; }

    [Doc("Owner of this horse.")]
    [Refers("IOwner.Horses")]
    IOwner? Owner { get; set; }

    [Doc("Tournaments this horse attends.")]
    [Refers("ITournament.Horses")]
    VList<ITournament> Tournaments { get; }
}

[VmfModel]
[Doc("Tournament a horse can attend.")]
public partial interface ITournament
{
    [Doc("Name of the tournament.")]
    string? Name { get; set; }

    [Doc("Horses that attend this tournament.")]
    [Refers("IHorse.Tournaments")]
    VList<IHorse> Horses { get; }
}
