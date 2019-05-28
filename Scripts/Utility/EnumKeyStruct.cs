#region Namespace

using System.Collections.Generic;

#endregion

// <summary>
// 优化enum 作为字典key 造成的gc alloc 
// 用于定义enum的几个几个结构接口
// </summary>
// <author>zhulin</author>

public struct LongComparer : IEqualityComparer<long>
{
    public bool Equals(long a, long b)
    {
        return a == b;
    }

    public int GetHashCode(long obj)
    {
        return (int) obj;
    }
}

public struct RoleAnimationTypeCompare : IEqualityComparer<RoleAnimationType>
{
    public bool Equals(RoleAnimationType a, RoleAnimationType b)
    {
        return a == b;
    }

    public int GetHashCode(RoleAnimationType obj)
    {
        return (int) obj;
    }
}