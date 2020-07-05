using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
[assembly: InternalsVisibleTo("AugmentTests")]
public sealed class Augment
{
    internal static Dictionary<A, object> State = new Dictionary<A, object>();

    // ReSharper disable once ObjectCreationAsStatement
    private Augment() => new GCMonitor();
    public static Augment Instance = new Augment();

    internal static void Compact()
    {
        State = State
            .Where(stateKey => stateKey.Key.Owner.IsAlive)
            .ToDictionary(a => a.Key, a => a.Value);
    }

    /// <summary>
    /// Set an attached property on this object
    /// </summary>
    /// <exception cref="InvalidOperationException">value does not match the attached member type</exception>
    public TOwner Set<TOwner, TValue>(TOwner owner, string memberName, TValue value) 
        where TOwner : class
    {
        var key = new A(new WeakReference(owner), memberName);
        var exist = State.TryGetValue(key, out var x);

        if (exist && x.GetType() != typeof(TValue))
        {
            throw new InvalidOperationException($"value does not match the attached member type");
        }

        State[new A(new WeakReference(owner), memberName)] = value;

        return owner;
    }

    /// <summary>
    /// Get the attached property for this object
    /// </summary>
    /// <exception cref="InvalidOperationException">member did not exist</exception>
    public TValue Get<TValue>(object owner, string memberName)
    {
        var key = new A(new WeakReference(owner), memberName);
        var exist = State.TryGetValue(key, out var x);

        return exist ? (TValue)x : throw new InvalidOperationException("member did not exist");
    }

    /// <summary>
    /// Try get the attached property for this object
    /// </summary>
    public bool TryGet<TValue>(object owner, string memberName, out TValue value)
    {
        var key = new A(new WeakReference(owner), memberName);
        var exist = State.TryGetValue(key, out var x);

        value = (TValue)x;

        return exist;
    }

    /// <summary>
    /// Test if the object has an attached property with the given member name
    /// </summary>
    public bool Has(object owner, string memberName)
    {
        var key = new A(new WeakReference(owner), memberName);
        var exist = State.TryGetValue(key, out _);

        return exist;
    }

    // ReSharper disable once InconsistentNaming
    private sealed class GCMonitor
    {
        ~GCMonitor()
        {
            if (AppDomain.CurrentDomain.IsFinalizingForUnload() || Environment.HasShutdownStarted) return;

            Compact();

            // ReSharper disable once ObjectCreationAsStatement
            new GCMonitor();
        }
    }

    internal struct A
    {
        public A(WeakReference owner, string memberName)
        {
            Owner = owner;
            _memberName = memberName;
        }

        public readonly WeakReference Owner;
        private readonly string _memberName;

        public override bool Equals(object? obj) =>
            obj is A o && o.Owner.Target.Equals(Owner.Target) && o._memberName.Equals(_memberName);

        public bool Equals(A other) => Equals(Owner.Target, other.Owner.Target) && _memberName == other._memberName;
        public override int GetHashCode() => HashCode.Combine(Owner.Target, _memberName);
    }
}

public static class AugmentExtensions
{
    public static TOwner _Set<TOwner, TValue>(this TOwner owner, string memberName, TValue value)
        where TOwner : class
        => Augment.Instance.Set(owner, memberName, value);

    public static TValue _Get<TValue>(this object owner, string memberName)
        => Augment.Instance.Get<TValue>(owner, memberName);

    public static bool _TryGet<TValue>(this object owner, string memberName, out TValue value)
        => Augment.Instance.TryGet(owner, memberName, out value);

    public static bool _Has(this object owner, string memberName)
        => Augment.Instance.Has(owner, memberName);
}