using System;

internal struct P
{
    public P(Type owner, string memberName)
    {
        Owner = owner;
        MemberName = memberName;
    }

    public readonly Type Owner;
    public readonly string MemberName;

    public override bool Equals(object? obj) =>
        obj is P o && o.Owner == Owner && o.MemberName.Equals(MemberName);

    public bool Equals(P other) => Owner == other.Owner && MemberName == other.MemberName;
    public override int GetHashCode() => HashCode.Combine(Owner, MemberName);
}