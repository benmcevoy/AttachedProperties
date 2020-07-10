using System;

internal struct A
{
    public A(WeakReference owner, string memberName)
    {
        Owner = owner;
        MemberName = memberName;
    }

    public readonly WeakReference Owner;
    public readonly string MemberName;

    public override bool Equals(object? obj) =>
        Owner.IsAlive && obj is A o && o.Owner.Target.Equals(Owner.Target) && o.MemberName.Equals(MemberName);

    public bool Equals(A other) => Owner.IsAlive && Equals(Owner.Target, other.Owner.Target) && MemberName == other.MemberName;
    public override int GetHashCode() => HashCode.Combine(Owner.Target, MemberName);
}