// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections;
using VMF.NET.Runtime.Internal;

namespace VMF.NET.Runtime;

/// <summary>
/// Static utility methods for operating on VMF objects.
/// </summary>
public static class VObjects
{
    /// <summary>
    /// Returns true if the arguments are equal to each other. If the specified objects are
    /// model instances then the VMF equals method is used. If the specified objects are
    /// collections containing model instances then the VMF equals method is used element-wise.
    /// </summary>
    public static new bool Equals(object? o1, object? o2)
    {
        if (ReferenceEquals(o1, o2)) return true;
        if (o1 is null || o2 is null) return object.Equals(o1, o2);

        if (o1 is IVObject v1 && o2 is IVObject v2)
        {
            if (v1 is IVObjectInternal i1)
            {
                return i1.VmfEquals(o2, new HashSet<long>());
            }
            return object.Equals(o1, o2);
        }

        if (o1 is IList list1 && o2 is IList list2)
        {
            if (list1.Count != list2.Count) return false;
            for (int i = 0; i < list1.Count; i++)
            {
                if (!Equals(list1[i], list2[i])) return false;
            }
            return true;
        }

        return object.Equals(o1, o2);
    }
}
