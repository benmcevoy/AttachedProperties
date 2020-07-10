using System.Collections.Generic;

public static class AugmentExtensions
{
    public static TOwner _Set<TOwner, TValue>(this TOwner owner, string memberName, TValue value)
        where TOwner : class
        => Augment.Instance.Set(owner, memberName, value);

    public static TOwner _SetPrototype<TOwner, TValue>(this TOwner owner, string memberName, TValue value)
        where TOwner : class
        => Augment.Instance.SetPrototype(owner, memberName, value);

    public static IReadOnlyCollection<string> _Entries<TOwner>(this TOwner owner)
        where TOwner : class
        => Augment.Instance.Entries(owner);

    public static TValue _Get<TValue>(this object owner, string memberName)
        => Augment.Instance.Get<TValue>(owner, memberName);

    public static bool _TryGet<TOwner, TValue>(this TOwner owner, string memberName, out TValue value)
        where TOwner : class
        => Augment.Instance.TryGet(owner, memberName, out value);

    public static bool _Has<TOwner>(this TOwner owner, string memberName)
        where TOwner : class
        => Augment.Instance.Has(owner, memberName);
}