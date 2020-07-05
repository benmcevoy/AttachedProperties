using System;
using System.Collections.Generic;
using System.Linq;

namespace AttachedProperties
{
    #region Wrapper
    // a wrapper object
    internal class A1 { public object Owner; public object Value; public string Name; }
    #endregion

    #region FirstAttempt
    public class AttachedPropertyManager1
    {
        // The key here needs to be a reference to the destination object
        private readonly Dictionary<string, WeakReference> State = new Dictionary<string, WeakReference>();

        public T Set<S, T>(T destination, Func<S, string> select, object value)
        {
            // I don't know what to the with the source.  I wanted to be able to select the property off it
            var key = select(default(S));

            State[key] = new WeakReference(new A1 { Owner = destination, Name = key, Value = value });

            return destination;
        }

        public T Get<T>(T source, string prop) => default;
    }
    #endregion

    #region Wrapper2
    // a wrapper object
    internal struct A2
    {
        public A2(object owner, string name)
        {
            Owner = owner;
            Name = name;
        }

        public readonly object Owner;
        public readonly string Name;

        public override bool Equals(object? obj) => obj is A2 o && o.Owner.Equals(Owner) && o.Name.Equals(Name);
        public bool Equals(A2 other) => Equals(Owner, other.Owner) && Name == other.Name;
        public override int GetHashCode() => HashCode.Combine(Owner, Name);
    }
    #endregion

    #region SecondAttempt
    public class AttachedPropertyManager2
    {
        // lets not care about weak references or garbage collection at first
        private readonly Dictionary<A2, object> State = new Dictionary<A2, object>();

        // just tell me what the propertyName is for now
        public T Set<T>(T owner, string propertyName, object value)
        {
            State[new A2(owner, propertyName)] = value;

            return owner;
        }

        public object Get<T>(T owner, string propertyName)
        {
            var key = new A2(owner, propertyName);

            var exist = State.TryGetValue(key, out var x);

            return exist ? x : null;
        }
    }
    #endregion

    #region ThirdAttempt

    internal struct A3
    {
        public A3(WeakReference owner, string name)
        {
            Owner = owner;
            Name = name;
        }

        public readonly WeakReference Owner;
        public readonly string Name;

        public override bool Equals(object? obj) => obj is A3 o && o.Owner.Equals(Owner) && o.Name.Equals(Name);
        public bool Equals(A3 other) => Equals(Owner, other.Owner) && Name == other.Name;
        public override int GetHashCode() => HashCode.Combine(Owner, Name);
    }

    public class AttachedPropertyManager3
    {
        private static readonly Dictionary<A3, WeakReference> State = new Dictionary<A3, WeakReference>();
        
        public AttachedPropertyManager3() => new GCMonitor();

        public static void Compact()
        {
            foreach (var stateKey in State.Keys.Where(stateKey => !stateKey.Owner.IsAlive))
            {
                State.Remove(stateKey);
            }
        }

        public T Set<T>(T owner, string propertyName, object value)
        {
            State[new A3(new WeakReference(owner), propertyName)] = new WeakReference(value);

            return owner;
        }

        public object Get<T>(T owner, string propertyName)
        {
            var key = new A3(new WeakReference(owner), propertyName);

            var exist = State.TryGetValue(key, out var x);

            return exist && x.IsAlive ? x.Target : null;
        }

        public sealed class GCMonitor
        {
            ~GCMonitor()
            {
                if (!AppDomain.CurrentDomain.IsFinalizingForUnload() && !Environment.HasShutdownStarted)
                {
                    AttachedPropertyManager3.Compact();
                    new GCMonitor();
                }
            }
        }
    }
    #endregion
}


