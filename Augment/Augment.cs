using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
[assembly: InternalsVisibleTo("AugmentTests")]
public sealed class Augment
{
    internal static Dictionary<A, object> Instances = new Dictionary<A, object>();
    internal static Dictionary<P, object> Prototypes = new Dictionary<P, object>();

    // ReSharper disable once ObjectCreationAsStatement
    private Augment() => new GCMonitor();
    public static Augment Instance = new Augment();

    internal static void Compact()
    {
        Instances = Instances
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
        // if prototype exists then types must match
        var prototypeKey = new P(typeof(TOwner), memberName);
        var protoTypeExists = Prototypes.TryGetValue(prototypeKey, out var p);

        if (protoTypeExists && p.GetType() != typeof(TValue))
        {
            throw new InvalidOperationException($"value does not match the attached member type");
        }

        var instanceKey = new A(new WeakReference(owner), memberName);

        if (protoTypeExists)
        {
            // shadow the prototype
            Instances[instanceKey] = value;
            return owner;
        }

        var instanceExist = Instances.TryGetValue(instanceKey, out var x);

        if (instanceExist && x.GetType() != typeof(TValue))
        {
            throw new InvalidOperationException($"value does not match the attached member type");
        }

        Instances[instanceKey] = value;

        return owner;
    }

    /// <summary>
    /// Get the attached property for this object
    /// </summary>
    /// <exception cref="InvalidOperationException">member did not exist</exception>
    public TValue Get<TValue>(object owner, string memberName) =>
        Instances.TryGetValue(new A(new WeakReference(owner), memberName), out var x)
            ? (TValue)x
            : Prototypes.TryGetValue(new P(owner.GetType(), memberName), out var y)
                ? (TValue)y
                : default;

    /// <summary>
    /// Try get the attached property for this object
    /// </summary>
    public bool TryGet<TOwner, TValue>(TOwner owner, string memberName, out TValue value)
        where TOwner : class
    {
        value = Get<TValue>(owner, memberName);

        return value != null;
    }

    /// <summary>
    /// Test if the object has an attached property with the given member name
    /// </summary>
    public bool Has<TOwner>(TOwner owner, string memberName)
        where TOwner : class =>
        Instances.TryGetValue(new A(new WeakReference(owner), memberName), out _) ||
        Prototypes.TryGetValue(new P(typeof(TOwner), memberName), out _);

    /// <summary>
    /// Set attached prototype member on the type
    /// </summary>
    public TOwner SetPrototype<TOwner, TValue>(TOwner owner, string memberName, TValue value)
        where TOwner : class
    {
        var key = new P(typeof(TOwner), memberName);
        var exist = Prototypes.TryGetValue(key, out var x);

        if (exist && x.GetType() != typeof(TValue))
        {
            throw new InvalidOperationException($"value does not match the attached member type");
        }

        Prototypes[key] = value;

        return owner;
    }

    /// <summary>
    /// Get all attached member names for this object, including any prototype members
    /// </summary>
    public IReadOnlyCollection<string> Entries<TOwner>(TOwner owner)
        where TOwner : class
        =>
            Prototypes
                .Where(pair => pair.Key.Owner == typeof(TOwner))
                .Select(pair => pair.Key.MemberName)
                .Union(
                        Instances
                            .Where(pair => pair.Key.Owner.IsAlive && pair.Key.Owner.Target.Equals(owner))
                            .Select(pair => pair.Key.MemberName)
                        )
                .Distinct()
                .ToList();


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
}