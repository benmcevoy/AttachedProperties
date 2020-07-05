# AttachedProperties
Dynamically attach object fields to managed objects

You have an object come by:

`myObject`

You like it, but wish it had more.

`myObject._Set("AwesomePropertyNameWeAgreedOnEarlier", "have alittle more");`

As long as that instance is alive, being passed around your code, you can now:

`var awesome = myObject._Get<string>("AwesomePropertyNameWeAgreedOnEarlier");`

When the object goes out of scope and get garbage collected, so does the **Augmented* properties (or methods or whatever you attached to it).


