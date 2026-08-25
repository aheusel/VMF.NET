// Ported from eu.mihosoft.vmftest.complex.horses.vmfmodel.Horses

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.Horses.VmfModel;

[Doc("A barn for horses.")]
interface HorseBarn
{
    [Doc("The horses contained in this barn.")]
    [Contains]
    Horse[] Horses { get; }
}

[Doc("Owner of a horse or multiple horses.")]
interface Owner
{
    [Doc("Name of the owner.")]
    string? Name { get; set; }

    [Doc("Horses owned by this owner.")]
    [Refers("Horse.Owner")]
    Horse[] Horses { get; }
}

[Doc("A horse.")]
interface Horse
{
    string? Name { get; set; }

    [Doc("Owner of this horse.")]
    [Refers("Owner.Horses")]
    Owner? Owner { get; set; }

    [Doc("Tournaments this horse attends.")]
    [Refers("Tournament.Horses")]
    Tournament[] Tournaments { get; }
}

[Doc("Tournament a horse can attend.")]
interface Tournament
{
    [Doc("Name of the tournament.")]
    string? Name { get; set; }

    [Doc("Horses that attend this tournament.")]
    [Refers("Horse.Tournaments")]
    Horse[] Horses { get; }
}
