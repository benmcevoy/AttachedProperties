# TLDR;

**Augment** any object by attaching "stuff" to it in a safe way.  

It's like `dynamic` without the dynamic.

Actually, it's a lot like `ConditionalWeakTable` but different.

See the **Augment** project and tests.

# Attached Properties

Having some thoughts:

### Inverting relationships

An object has properties.

An attached property is taking another object's property and glueing it onto your object.

Sounds like dynamic.

### Storage

Initally thought all objects should have "bag", via inheritance.

A bag being a dictionary <string, object>.

#### WeakReference

A weak reference lets you store the properties on **some other** object, but still 
get collected when the object is out of scope

#### ConditionalWeakTable

dynamically attach object fields to managed objects
sounds a lot more like dynamic

## Implement

Lets try WeakReference.  I have previously used weak reference using Prism's 
event aggregator.

This is a pub/sub mediator that wants to let events it manages die when no-one no longer 
subscribes to them.

Let's have a go then.

AttachedProperty um.. Manager.

    T Set<S,T>(Func<S, PropertyInfo|string> select, T destination)

    T Get<T>(T source, PropertyInfo|string prop)

is that a type union there? yes it is.  Union<PropertyInfo, string>

Needs some weak state now.

```
Dictionary<Union<PropertyInfo, string>, object>
```

Where does that weak reference come in?

```
    Dictionary<Union<PropertyInfo, string>, WeakReference>
```

That's where. I think also the Union type can simply provide the key as string.

```
    Dictionary<string, WeakReference> 
```

Bonza.

## Juypter notebooks

At this point I want to write the `AttachedPropertyManger` class.

Wouldn't it be nice to keep writing here?

code-fence
```
    class AttachedPropertyManager
    {   
        // ahhh.... it is but a dream
    }
```

Hmm.. go then.  let's try Juypiter.  

um. how? is ther c# support?

### trydotnet

So... 
after updating VS2019 to the latest
open terminal and
 
`dotnet tool install --global dotnet-try`

then in the terminal (which you can open in VS as View->Terminal)

cd to my actual (this one) workspace
pwd - print working directory

then start the server

`dotnet try`

## Attached Property Manager

``` cs --region FirstAttempt --source-file .\AttachedPropertyManager.cs --project .\AttachedProperties.csproj 
```

So there are a few issues...  The A1() is a really simple wrapper:

``` cs --region Wrapper --source-file .\AttachedPropertyManager.cs --project .\AttachedProperties.csproj 
```

Ignore the weak part and work out the key.

The dictionary must be keyed on the T destination object. I need a two part 
key, the object ref + the property name.

How do I do that? Bit of equality overloading.

My new "wrapper" is now the multi-part key.  I was surprised and pleased to 
see `HashCode.Combine` which I was unaware of, it appears to have arrived in 
.NET Core 2.1.

I lost the Value part, but I think I might want it later to allow strong(er) typing.

``` cs --region Wrapper2 --source-file .\AttachedPropertyManager.cs --project .\AttachedProperties.csproj 
```

With that in place let's try again.

``` cs --region SecondAttempt --source-file .\AttachedPropertyManager.cs --project .\AttachedProperties.csproj 
```

But does it work?

``` cs --region test1 --source-file .\Program.cs --project .\AttachedProperties.csproj 
```

Well yes it does, but all we've really done is make a dictionary and put something in it.

## Stopping to think

The value is in the storage of the property value, being transient, ephemeral, safe (hopefully)

I had been playing with the sitecore "PropertyStore", which attaches a bag of stuff to various "things" in sitecore.

Enables what scenarios?

1. Dynamically attach context.  

- Stick some meta data on a thing
- Communicate between classes
- Extend objects dynamically but without using dynamic (meh?)

2. Dynamically attach methods

ok....

Is this some steps towards dynamic or duck typing?

What is the point?  Got a killer app?

**Duck typery**

`Duck.Cast<desired type>(some object that might have the characteristics of the desired type)`

**dynamic duo**

Well, it's like dynamic without the dynamic keyword.  is that good?

**Yes**. It is good. There are many *many* objects and types out there and hardly any of them are dynamic.

Primarily I wanted this so I can attach metadata to an object as it goes by.

**Sealed** classes are another scenario.  Can't inherit, but you could decorate a sealed class.

I lost the type union along the way. Oh well.

## Considerations

Serialization? Good luck with that.  The object will serialize, but the attached properties will not.

Error handling 
- if you ask for an attached property that is not attached?
- if you set an attached property with a new value of a different type?

Unit testing
- should be ok i reckon


### Garbage Collection

Need those weak references!

The owning object is not my concern.  That's yours.

The reference we take to the owner must be weak.


### Syntactic sugar

Extension methods

as a map

`x["prop"]`

Well, that's not so possible.

`x.A("prop")`

using func as the attached thing.

provide the owner to the func

`x.A("prop")()`

Not doing much at all that extension methods can't do.


```
 public static TValue _Get<TValue>(this object owner, string memberName)
        => Augment.Instance.Get<TValue>(owner, memberName);

```

Yeah. I'm calling this **Augment**.

### How did WPF do it?

```
public static readonly DependencyProperty IsBubbleSourceProperty = DependencyProperty.RegisterAttached(
  "IsBubbleSource",
  typeof(Boolean),
  typeof(AquariumObject),
  new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender)
);
```

```
public static System.Windows.DependencyProperty RegisterAttached (string name, Type propertyType, Type ownerType);
```
