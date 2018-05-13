DrakeScript
========

DrakeScript is an embedded scripting language for .NET applications.


Usage examples:
==
Hello world
```csharp
var context = new Context();
var testScript = context.LoadString(
    @"PrintLn(""Hello, world!"");"
);
testScript.Invoke();```

Simple function
```csharp
var context = new Context();
var testScript = context.LoadString(
    @"local function add3(a, b, c)
    {
        return a + b + c;
    }

    return add3(1, 2, 6);"
);
var result = testScript.Invoke();
Console.WriteLine(result);```

Sum of array elements that are numbers
```csharp
var context = new Context();
var testScript = context.LoadString(
    @"local function arraySum(array)
    {
    	local sum = 0;
    	local i = 0;
        loop (lengthof array)
        {
        	local e = array[i];
        	if (e is Number)
        	{
        		sum += e;
        	}
        	i += 1;
        }
        return sum;
    }

    return arraySum([24, 53, 1, 9, ""foo"", nil]);"
);
var result = testScript.Invoke();
Console.WriteLine(result);```

Sum of array elements that are numbers (then-otherwise version)
```csharp
var context = new Context();
var testScript = context.LoadString(
    @"local function arraySum(array)
    {
    	local sum = 0;
    	local i = 0;
        loop (lengthof array)
        {
        	local e = array[i];
    		sum += e is Number then e otherwise 0;
        	i += 1;
        }
        return sum;
    }

    return arraySum([24, 53, 1, 9, ""foo"", nil]);"
);
var result = testScript.Invoke();
Console.WriteLine(result);```

Setting global variable from host application
```csharp
var context = new Context();
context.SetGlobal("Foo", 5.1);
var testScript = context.LoadString(
    @"PrintLn2(Foo);"
);
testScript.Invoke();```

Registering global function from host application
```csharp
var context = new Context();
context.SetGlobal(
    "PrintLn2", context.CreateFunction(
        (interpreter, location, args, argCount) =>
        {
            for (var i = 0; i < argCount; i++)
			{
				Console.Write(args[i].ToString());
			}
			Console.WriteLine();
			return Value.Nil;
        },
        1 // *MINUMUM* argument count required
    )
);
var testScript = context.LoadString(
    @"PrintLn2(1, 2, 3);"
);
testScript.Invoke();```

Creating table
```csharp
var context = new Context();
var testScript = context.LoadString(
    @"local tableKey = { 1: [2, 4] };
	PrintLn(
		{
			""foo"": ""bar"",
			""hello"": 4,
			89.2: 2,
			tableKey: 2
		}
	);"
);
testScript.Invoke();```