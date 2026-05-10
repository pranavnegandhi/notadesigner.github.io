---
title: "A Hash Table using C#"
date: 2017-08-27T06:41:00Z
slug: a-hash-table-using-c
aliases: ["/a-hash-table-using-c/"]
categories:
  - "Foundation"
tags:
  - "c#"
wp_post_id: 950
---

[Brian Barto recently put up a post](https://medium.com/@bartobri/hash-crash-the-basics-of-hash-tables-bef82a8ea550) where he described how hash tables are implemented. Many modern programming languages have this data structure as a built-in feature, making it a commonly used item in a developer’s toolkit. Due to its ready availability, few programmers ever have the need to roll their implementation. So it comes as no surprise when people who do not have a formal education in software engineering do not understand how it works. Brian's post explains the most essential parts of this magical data structure very well.

After reading about it, I felt the urge to implement the same thing in C# as a learning experience. His post carries an implementation using the C programming language, which could be copied over with some tweaking in order to work with the different semantics of C#. This post documents the exercise and its learnings.

### The Associative Array

A hash table is an implementation of an associative array, which is a collection of key-value pairs. Each key is unique and can appear only once in the collection. The data structure supports the full range of CRUD operations - create, retrieve, update and delete. All operations are constant time on average or linear time in the worst case, making speed the main advantage of this data structure. In some languages, all operations are rolled into the same operator for simplicity. If the key does not exist, it is added. If the key already exists in the collection, its value is overwritten with the new one. Keys are removed by setting their value to null, or having a separate delete operator for associative arrays.

JavaScript objects are an example of associative arrays.

```javascript
o["occupation"] = "Wizard"; // Key created
o["name"] = "Jane Doe"; // Key updated
console.log(o["name"]); // Value retrieval
delete o["occupation"]; // Jane just retired
```

A hash table uses a linear array internally to store values. The key is converted into an integer by running it through a hash function, which is then used as the index into the backing array where the value is stored. A good hash function has two characteristics – **minimal conflicts** and **uniform distribution**.

#### Minimal Conflicts

As explained above, a hash function works by converting an input into an integer. A modular hash function is one of the simplest implementations possible, which returns the remainder of a division (aka modulo) of the key by the table length.

```
int size = 100;int key = 10;int index = key % size; // 10
```

However, this function can end up with a high number of conflicts. The remainder of dividing 10, 110, 210 and so on are are all 10, and require additional steps to resolve the conflict. This condition is minimised by setting the length of the array to a prime number.

```
int size = 97;int key = 10;int index = key % size; // 10key = 110;index = key % size; // 13key = 210;index = key % size; // 16
```

#### Uniform Distribution

Discrete uniform distribution is a statistical term for a condition where all values in a finite set all have an equal probability of being observed. A uniform distribution of hash values over the indexes into an array reduces the possibilities of key conflict, and reduces the cost of frequent conflict resolution. Uniform distribution over an arbitrary set of keys is a difficult mathematical challenge, and I won’t even pretend to understand what I’m talking about. Let it just suffice that a lot of really smart folks have already worked on that and we get to build upon their achievements.

If the keys in a hash table belong to a known and finite set (such as a language dictionary), then the developer can write what is known as a perfect hash function that provides an even distribution of hashes with zero conflicts.

#### Resolving Conflicts

Finally, general purpose hash functions can often result in conflicts, i.e. return the same output for two separate inputs. In this case, the hash table must resolve the conflict. For example, the modular hash function returns the index 13 for the key 13 as well as 110. There must be a way to set both these keys simultaneously in the same collection. Broadly, there are three techniques to achieve this.

##### Separate Chaining

In this technique, the values themselves are not stored in the array. They are stored in a separate data structure such as a linked list, and a pointer or reference to that list is stored in the backing array. When a value is to be located, the function first computes its index by hashing the key, then walks through the list associated with that index until it finds the desired key-value pair.

##### Open Addressing

Open addressing stores all records in the array itself. When a new value is to be inserted, its index position is computed by applying the hash function on the key. If the desired position is occupied, then the rest of the positions are probed. There are several probing algorithms also. Linear probing, the simplest of them, requires walking the array in fixed increments from the desired index until an empty spot is found in the array.

### Implementation

Enough theory. Let us move on to implementation details.

The first step is to define a HashTable class. The size of the hash table is restricted to 100 entries for this exercise and a fixed-length array is initialised to store these entries.

```csharp
public class HashTable
{
    private static readonly int SIZE = 100;
    private object[] _entries = new object[SIZE];
}
```

This is where the implementation of this data structure veers away from the one shown in Brian Barto's post. The original post used explicitly named method for insert, search and delete operations. However, C# allows the nicer and more intuitive use of a square-bracket syntax to perform these same operations.

```csharp
public class HashTable
{
    ...

    public int this[int key]
    {
        get;
        set;
    }
}
```

The client program uses the following syntax to consume this interface, which is much more concise than having separate methods for each operation.

```csharp
HashTable map = new HashTable();
map[key] = value; // Insert
int value = map[key]; Retrieve
map[key] = null; // Delete
```

The HashTable class internally uses a struct called Entry to store individual key-value pairs.

```csharp
struct Entry
{
    internal int key;
    internal int? value;
}
```

The type of the value field in the Entry struct must be set to a nullable int so that the client can set it to null when the value has to be cleared. The declaration of the \_entries field is changed as follows.

```csharp
private Entry?[] _entries = new Entry?[HashTable.SIZE];
```

Finally, we come to the meat and potatoes of this exercise - the accessor & mutator functions, whose signature is also modified to support the nullable type.

```csharp
public int? this[int key]
{
    get
    {
        ...
    }

    set
    {
        ...
    }
}
```

The code for these methods and the hash function is more or less identical to the one described in Brian's post, with some tweaks to accommodate for the nullable value.

```csharp
get
{
    int index = _hash(key);
    
    while (null != _entries[index])
    {
        if (key == _entries[index]?.key)
        {
            break;
        }
        index = (index + 1) % HashTable.SIZE;
    } 
    return _entries[index]?.value;
}

set
{
    if (null == value)
    {
        Delete(key);
    }
    else
    {
        int index = _hash(key);
        while (null != _entries[index])
        {
            if (key == _entries[index]?.key)
            {
                break;
            }
            index = (index + 1) % HashTable.SIZE;
        }

        _entries[index] = new HashEntry { key = key, value = value };
     }
}
```

The one additional member in this class is the private Delete method which is required to delete the entry. This method is called by the mutator function when the client passes in a null value.

```csharp
private void Delete(int key)
{
    int index = _hash(key);

    while (null != _entries[index])
    {
        if (key == _entries[index]?.key)
        {
            break;
        }
        else
        {
            index = (index + 1) % HashTable.SIZE;
        }
    }

    if (null == _entries[index])
    {
        return;
    }

    _entries[index] = null;
}
```

That's all that's needed to implement this data structure. The hash function shown in this example is a very na├»ve implementation. The probability of collisions is very high and the probing function is very slow. It also cannot work with any data type other than int for both keys and their values.

In spite of these limitations, this still made for a very useful learning exercise.
