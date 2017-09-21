using System;
using System.Collections.Generic;
using DrakeScript;

namespace DrakeScriptTester
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("You must specify a script!");
				return;
			}
			if (args[0] == "tests")
			{
				RunTests();
			}
			else if (args[0] == "compile")
			{
				if (!System.IO.File.Exists(args[1]))
				{
					Console.WriteLine("Script does not exist!");
					return;
				}
				var context = new Context();
				var code = context.LoadFile(args[1]);
				var optimizer = new Optimizer();
				optimizer.Optimize(code);
				Console.WriteLine(code.Code.ToStringFormatted() + "\n");
				if (System.IO.File.Exists("bytecode.bin"))
					System.IO.File.Delete("bytecode.bin");
				System.IO.File.WriteAllBytes("bytecode.bin", code.GetBytecode());
			}
			else if (args[0] == "load")
			{
				if (!System.IO.File.Exists(args[1]))
				{
					Console.WriteLine("File does not exist!");
					return;
				}
				var context = new Context();
				var code = context.LoadBytecode(args[1]);
				Console.WriteLine(code.Code.ToStringFormatted() + "\n");
				var interpreter = new Interpreter(context);
				code.Invoke(interpreter);
				if (interpreter.Stack.Count > 0)
					Console.WriteLine("result: " + interpreter.Stack.Peek(0).ToString());
				else
					Console.WriteLine("no result");
			}
			else
			{
				if (!System.IO.File.Exists(args[0]))
				{
					Console.WriteLine("Script does not exist!");
					return;
				}
				var context = new Context();
				var code = context.LoadFile(args[0]);
				var optimizer = new Optimizer();
				optimizer.Optimize(code);
				Console.WriteLine(code.Code.ToStringFormatted() + "\n");
				var interpreter = new Interpreter(context);
				code.Invoke(interpreter);
				if (interpreter.Stack.Count > 0)
					Console.WriteLine("result: " + interpreter.Stack.Peek(0).ToString());
				else
					Console.WriteLine("no result");
			}
		}



		public static Test[] Tests = new Test[]
		{
			new Test(
				"simple math expression 1",
				Value.Create(100.2 * (241.22 / 5.4) - 2.3 + 9.99),
				"local a = 241.22; b = 5.4; local c = 9.99; return 100.2 * (a / b) - 2.3 + c;"
			),
			new Test(
				"simple math expression 2",
				Value.Create(1.0240251 / 7.35020232 / 35.23235),
				"local a = 1.0240251; b = 7.35020232; c = 35.23235; return a / b / c;"
			),
			new Test(
				"concat 1",
				Value.Create("hello, world!"),
				"return \"hello, \" ~ \"world!\";"
			),
			new Test(
				"concat 2",
				Value.Create("hello, world!"),
				"local a = \"hello, \"; local b = \"world!\"; return a ~ b;"
			),
			new Test(
				"local noarg function 1",
				Value.Create("Hello, world!"),
				"local function a() {return \"Hello, world!\";} return a();"
			),
			new Test(
				"local noarg function 2",
				Value.Create("Hello, world!"),
				"local function a(str) {return str;} return a(\"Hello, world!\");"
			),
			new Test(
				"local arg function 1",
				Value.Create(21.4 + 47.0 / 124.0 * 93.2),
				"local function a(a, b, c, d) {return a + b / c * d;} return a(21.4, 47, 124, 93.2);"
			),
			new Test(
				"global noarg function 1",
				Value.Create("Hello, world!"),
				"function a() {return \"Hello, world!\";} return a();"
			),
			new Test(
				"global noarg function 2",
				Value.Create("Hello, world!"),
				"function a(str) {return str;} return a(\"Hello, world!\");"
			),
			new Test(
				"global arg function 1",
				Value.Create(21.4 + 47.0 / 124.0 * 93.2),
				"function a(a, b, c, d) {return a + b / c * d;} return a(21.4, 47, 124, 93.2);"
			),
			new Test(
				"1000 local increments 1",
				Value.Create(1000),
				"local a = 0; loop (1000) {a += 1;} return a;"
			),
			new Test(
				"1000 local increments 2",
				Value.Create(2000),
				"local a = 0; loop (1000) {a += 2;} return a;"
			),
			new Test(
				"1000 local increments 3",
				Value.Create(1000),
				"local a = 0; local function add(a, b) {return a + b;} loop (1000) {a = add(a, 1);} return a;"
			),
			new Test(
				"1000 global increments 1",
				Value.Create(1000),
				"a = 0; loop (1000) {a += 1;} return a;"
			),
			new Test(
				"1000 global increments 2",
				Value.Create(2000),
				"a = 0; loop (1000) {a += 2;} return a;"
			),
			new Test(
				"1000 global increments 3",
				Value.Create(1000),
				"a = 0; local function add(a, b) {return a + b;} loop (1000) {a = add(a, 1);} return a;"
			),
			new Test(
				"function as argument",
				Value.Create(99.2),
				"function testrun(otherFunc) {return otherFunc(99.2);} return testrun(function (num) {return num});"
			),
			new Test(
				"two functions as arguments",
				Value.Create(4.0 + (12.2 + 2.0)),
				"function testrun(otherFunc, otherFunc2, arg1, arg2) {return otherFunc(arg1) + otherFunc2(arg2);} return testrun(function (num) {return num}, function (num) {return num + 2}, 4, 12.2);"
			),
			new Test(
				"if statement 1",
				Value.Create(2),
				"local b = 5; if (b == 4) {return 1;} return 2;"
			),
			new Test(
				"if statement 2",
				Value.Create(1),
				"local b = 5; if (b == 5) {return 1;} return 2;"
			),
			new Test(
				"if-else statement 1",
				Value.Create(2),
				"local b = 5; if (b == 4) {return 1;}else {return 2;}"
			),
			new Test(
				"if-else statement 2",
				Value.Create(1),
				"local b = 5; if (b == 5) {return 1;}else {return 2;}"
			),
			new Test(
				"nested if-else 1",
				Value.Create(1),
				"local b = 5; if (b == 5) {return 1;}else {if (b == 4) {return 2;}}"
			),
			new Test(
				"nested if-else 2",
				Value.Create(2),
				"local b = 4; if (b == 5) {return 1;}else {if (b == 4) {return 2;}else {return 3;}}"
			),
			new Test(
				"nested if-else 3",
				Value.Create(3),
				"local b = 3; if (b == 5) {return 1;}else {if (b == 4) {return 2;}else {return 3;}}"
			),
			new Test(
				"while loop 1",
				Value.Create(1.0 + 20.0 * 2.5),
				"local a = 1; local i = 0; while (i < 20) {a += 2.5; i += 1;} return a;"
			),
			new Test(
				"empty array",
				Value.Create(new List<Value> {}),
				"return [];"
			),
			new Test(
				"simple array",
				Value.Create(new List<Value> {1, 2, 3, 4}),
				"return [1, 2, 3, 4];"
			),
			new Test(
				"array with expressions",
				Value.Create(new List<Value> {3.2, 235.4, 238.6, 4}),
				"local a = 3.2; local b = 235.4; return [a, b, a + b, 4];"
			),
			new Test(
				"array nils",
				Value.Create(new List<Value> {Value.Nil, 235.4, 238.6, 4}),
				"local a = 3.2; local b = 235.4; return [c, b, a + b, 4];"
			),
			new Test(
				"array set",
				Value.Create(new List<Value> {1, 2, 3}),
				"local arr = [1]; arr[2] = 3; arr[1] = 2; return arr;"
			),
			new Test(
				"array get",
				Value.Create(new List<Value> {12, 4, 7}),
				"local arr = [3 * 4, 7, 4]; return [arr[0], arr[2], arr[1]];"
			),
			new Test(
				"simple table",
				Value.Create(new Table(new Dictionary<object, Value>{{1.0, 5.0}, {5.0, 2.0}})),
				"return {1: 5, 5: 2};"
			),
			new Test(
				"empty table",
				Value.Create(new Table()),
				"return {};"
			),
			new Test(
				"table set",
				Value.Create(new Table(new Dictionary<object, Value>{{"woof", "meow"}})),
				"local a = {}; local catSound = \"meow\"; a[\"woof\"] = catSound; return a;"
			),
			new Test(
				"table set dot",
				Value.Create(new Table(new Dictionary<object, Value>{{"woof", "meow"}})),
				"local a = {}; local catSound = \"meow\"; a.woof = catSound; return a;"
			),
			new Test(
				"table get 1",
				Value.Create(1),
				"local a = {\"a\": 1, \"b\": 2.34}; return a[\"a\"];"
			),
			new Test(
				"table get 2",
				Value.Create(2.34),
				"local a = {\"a\": 1, \"b\": 2.34}; return a[\"b\"];"
			),
			new Test(
				"table get dot 1",
				Value.Create(1),
				"local a = {\"a\": 1, \"b\": 2.34}; return a.a;"
			),
			new Test(
				"table get dot 2",
				Value.Create(2.34),
				"local a = {\"a\": 1, \"b\": 2.34}; return a.b;"
			),
			new Test(
				"table key math and concatenation",
				Value.Create(new Table(new Dictionary<object, Value>{{50.0, "Yellow"}, {"Test Key", 5.0}})),
				"return {25 * 2: \"Yellow\", \"Test \" ~ \"Key\": 5};"
			),
		};

		public static void RunTests()
		{
			var count = 0;
			foreach (var test in Tests)
			{
				if (!test.Run())
				{
					break;
				}
				count++;
			}

			Console.WriteLine("Tests " + count + "/" + Tests.Length + " completed successfully");
		}
	}
}
