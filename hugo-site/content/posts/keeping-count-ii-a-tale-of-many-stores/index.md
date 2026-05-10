---
title: "Keeping Count II – A Tale of Many Stores"
date: 2006-09-23T05:30:27Z
slug: keeping-count-ii-a-tale-of-many-stores
aliases: ["/keeping-count-ii-a-tale-of-many-stores/"]
categories:
  - "Construction"
wp_post_id: 37
---

In [my previous post](/keeping-count-i-the-candy-shop/ "Keeping Count I – The Candy Shop") I described Daryl’s experience as a programmer writing an inventory control system for a candy store. Over the next few weeks Betty, the store owner, spread the word about Daryl’s fantastic inventory and billing management software amongst her friends. Daryl was flooded with requests for a computer application “just like the Betty’s”.

So he got down to work again. But Daryl now felt that writing inventory control wasn’t as much fun any more. So he wanted to get away with as little code, in the shortest amount of time as possible. He looked at what he had written for Betty and realised that most of the core inventory code was supposed to work exactly the same. All that was needed was to detail out some business specific stuff such as sale-units. This code could go into a thin layer on top of the code inventory module.

This is what his base Inventory class looks like.

```actionscript
class Inventory
{
    function AddUnits(Units:Number)
    {
        m_units += Units;
    }

    function RemoveUnits(Units:Number)
    {
        m_units -= Units;
    }

    function PrintUnits()
    {
        print("Remaining units: " + m_units);
    }
}
```

Daryl adds a member variable called FunctionReference and a method called SetUnitConvertor() to this class, which accepts a function reference as a parameter.

```actionscript
function SetUnitConvertor(ObjectReference:Object, FunctionReference:Function)
{
    m_unitConvertor = Delegate.create(ObjectReference, FunctionReference)
}
```

Then he modifies the PrintUnits() method to use this function reference.

```actionscript
function PrintUnits()
{
    print("Remaining units: " + m_unitConvertor(m_units));
}
```

He copies the Inventory class into the project for Mr. Coton’s cloth store and adds a class specific to that store inheriting from the Inventory class.

```actionscript
class ClothStoreInventory extends Inventory
{
    function ClothStoreInventory()
    {
        super.SetUnitConvertor(this, ConvertUnitsToLength);
    }

    function ConvertUnitsToLength(Units:Number)
    {
        // Find the total length of cloth sold
        return Units / 1000
    }
}
```

And another class is added for Mr. Chiseller’s consultancy firm.

```actionscript
class ConsultancyFirmInventory extends Inventory
{
    function ConvertUnitsToTime(Units:Number)
    {
        // Show the number of hours worked
        Seconds = Math.floor(Units / 1000);
        Minutes = Math.floor(Seconds / 60);
        Seconds = Seconds % 60;
        Hours = Math.floor(Minutes / 60);
        Minutes = Minutes % 60;
        return Hours + ":" + Minutes + ":" + Seconds;
    }
}
```

The result of this hoopla is that the base class will always call the formatting function from its inherited class (which would be quite impossible otherwise without using a messy callback system, or hard-coding the object reference into the base class, or (gasp!) event generation.

“But why use a delegate, you silly goose! The base class can override the PrintUnits() method with whatever it wants to use in its place.”

Well, that’s true. But when you start overriding methods from the base class, it means that the base class has not been designed to anticipate future use. This is a very basic example. But if your PrintUnits() method becomes more elaborate, such as drawing a dialog box with icons and buttons, or maybe even handling multiple output devices such as printers and LCD tickers, replicating all that code in an inherited class is really bad design. By using a delegate, you also allow yourself room to use static utility classes for mundane functions such as this.
