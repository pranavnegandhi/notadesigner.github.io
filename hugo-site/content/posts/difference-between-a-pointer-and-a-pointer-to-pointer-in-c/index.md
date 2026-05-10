---
title: "Difference Between a Pointer and a Pointer-to-pointer in C"
date: 2008-11-18T05:33:51Z
slug: difference-between-a-pointer-and-a-pointer-to-pointer-in-c
aliases: ["/difference-between-a-pointer-and-a-pointer-to-pointer-in-c/"]
categories:
  - "Technique"
tags:
  - "c-lang"
wp_post_id: 42
---

C passes variables by value. So if you pass 2 ints x and y to the function square(), it creates a copy of both variables, populates them with the same value as x and y and hands them over to the function to process.

```cpp
int square(int x, int y)
{
  return x * y;
}

int main(void)
{
  square(a, b);
}
```

The same thing happens with pointers. A char \*a points to location 0x000000 (or NULL) in memory. The variable is then passed to a function initString(). Now a copy of the pointer is created, which also points to the value 0x000000 and is processed by the initString() function. Within initString(), new memory is allocated using malloc() to the variable a.

```cpp
void initString(char *x)
{
  x = malloc(6 * sizeof(char));
  sprintf(x, "%s", "Hello");
}

int main(void)
{
  char *a = NULL;
  initString(a);

  printf("%s\n", a);
}
```

Output

```bash
(null)
```

What happens in this case is that a fresh pointer variable is created, which happens to point at the same location as a, which is initially NULL. Then memory is allocated to this pointer in the initString() function, so it begins to point to the location returned by malloc(). When the function returns, the x variable is destroyed (and the memory allocated to it is now unreferenced, which creates a memory leak). In the meantime, a is still pointing to NULL.

Now let us change the signature of the initString() function.

```cpp
void initString(char **x)
{
  *x = malloc(6 * sizeof(char));
  sprintf(*x, "%s", "Hello");
}

int main(void)
{
  char *a = NULL;

  initString(&a);
  printf("%s\n", a);
}
```

Output

```bash
Hello
```

In this case, a is still pointing to NULL. However, instead of passing a directly, initString() is handed over a reference to the pointer a – a pointer to a pointer. This causes a pointer variable to be created which points to the memory location of a. Now, the dereference operator (\*) can be used to locate the address of a. Thus the memory address returned by malloc() can be assigned to a.

This method is not needed if you have already allocated memory to a char array in the calling function. This is because the pointer parameter in the callee function also refers to the same memory address allocated in the caller function.

Here’s a quick working example that demonstrates the differences between a pointer and a pointer-to-pointer when passed on to a function.

```cpp
#include <stdio.h>
#include <stdlib.h>

void initString1(char *a)
{
  a = malloc(6 * sizeof(char));
  strcpy(a, "Hello");
  printf("initString1():\t%s\t%p\t%p\n", a, a, &a);
}

void initString2(char **a)
{
  *a = malloc(6 * sizeof(char));
  strcpy(*a, "World");
  printf("initString2():\t%s\t%p\t%p\n", *a, *a, &*a);
}

int main(int argc, int *argv[])
{
  char *b = NULL;

  printf("Function |\tValue\tPoints to\tAddress\n", b, b, &b);
  initString1(b);
  //strcpy(b, "World"); // Crashes
  printf("main() :\t%s\t%p\t%p\n", b, b, &b);

  initString2(&b);
  strcpy(b, "Hello");
  printf("main() :\t%s\t%p\t%p\n", b, b, &b);

  return 0;
}
```
