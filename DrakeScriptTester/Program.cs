﻿using System;
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
                Function code;
                try
                {
                    code = context.LoadFile(args[1]);
                }
                catch (DrakeScript.CompileException e)
                {
                    Console.WriteLine("Compile error: " + e);
                    return;
                }
				var optimizer = new Optimizer();
				optimizer.Optimize(code);
				Console.WriteLine(code.ToStringFormatted() + "\n");
				if (System.IO.File.Exists(args[2]))
					System.IO.File.Delete(args[2]);
				using (var writer = new System.IO.BinaryWriter(System.IO.File.OpenWrite(args[2])))
				{
					code.WriteByteCodeFile(writer);
				}
			}
			else if (args[0] == "load")
			{
                if (args.Length == 1)
                {
                    Console.WriteLine("File not specified!");
                    return;
                }
                if (!System.IO.File.Exists(args[1]))
				{
					Console.WriteLine("File does not exist!");
					return;
				}
				var context = new Context();
				var code = context.LoadBytecode(args[1]);
				Console.WriteLine(code.ToStringFormatted() + "\n");
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
                Function code;
                try
                {
                    code = context.LoadFile(args[0]);
                }
                catch (DrakeScript.CompileException e)
                {
                    Console.WriteLine("Compile error: " + e);
                    return;
                }
                var optimizer = new Optimizer();
				optimizer.Optimize(code);
				Console.WriteLine(code.ToStringFormatted() + "\n");
				var interpreter = new Interpreter(context);
				code.Invoke(interpreter);
				if (interpreter.Stack.Count > 0)
					Console.WriteLine("result: " + interpreter.Stack.Peek(0).ToString());
				else
					Console.WriteLine("no result");
			}
		}



        class IndexableTest : IIndexable
        {
            Table InternalTable = new Table();

            public Value GetValue(Value key, SourceRef location)
            {
                return InternalTable[key];
            }

            public void SetValue(Value key, Value value, SourceRef location)
            {
                InternalTable[key] = value;
            }
        }
		public static Test[] Tests = new Test[]
		{
			new Test(
				"nothing",
				Value.Nil,
				""
			),
			new Test(
				"only return nil",
				Value.Nil,
				"return nil;"
			),
			new Test(
				"only return number 1",
				Value.Create(20.0),
				"return 20;"
			),
			new Test(
				"only return number 2",
				Value.Create(20.1261),
				"return 20.1261;"
			),
			new Test(
				"simple math expression 1",
				Value.Create(100.2 * (241.22 / 5.4) - 2.3 + 9.99),
				"let a = 241.22; b = 5.4; let c = 9.99; return 100.2 * (a / b) - 2.3 + c;"
			),
			new Test(
				"simple math expression 2",
				Value.Create(1.0240251 / 7.35020232 / 35.23235),
				"let a = 1.0240251; b = 7.35020232; c = 35.23235; return a / b / c;"
			),
			new Test(
				"comment 1",
				Value.Create(1.0240251 / 7.35020232 / 35.23235),
				"let a = 1.0240251; b = 7.35020232; c = 35.23235; return a / b / c; //aaaaa"
			),
			new Test(
				"comment 2",
				Value.Create(1.0240251 / 7.35020232 / 35.23235),
				"//aaaaaaaaaa safasf safa asf\nlet a = 1.0240251; b = 7.35020232; c = 35.23235; return a / b / c;"
			),
			new Test(
				"comment 3",
				Value.Create(1.0240251 / 7.35020232 / 35.23235),
				"/*test comment*/ let a = 1.0240251; b = 7.35020232; c = 35.23235; return a / b / c;"
			),
			new Test(
				"comment 4",
				Value.Create(1.0240251 / 7.35020232 / 35.23235),
				"let a = 1.0240251; b = /*test comment*/7.35020232; c = 35.23235; return a / b / c;"
			),
			new Test(
				"concat 1",
				Value.Create("hello, world!"),
				"return \"hello, \" .. \"world!\";"
			),
			new Test(
				"concat 2",
				Value.Create("hello, world!"),
				"let a = \"hello, \"; let b = \"world!\"; return a .. b;"
			),
			new Test(
				"concat 3",
				Value.Create("hello, 6!"),
				"let a = \"hello, \"; let b = 6; return a .. b .. \"!\";"
			),
			new Test(
				"concat 4",
				Value.Create("hello, [1, 2, 3]!"),
				"let a = \"hello, \"; let b = [1, 2, 3]; return a .. b .. \"!\";"
			),
			new Test(
				"concat 6",
				Value.Create("hello, {1:4, 5:5, test:6}!"),
				"let a = \"hello, \"; let b = {1 => 4, 5 => 5, \"test\" => 6}; return a .. b .. \"!\";"
			),
			new Test(
				"let noarg function 1",
				Value.Create("Hello, world!"),
				"let function a() {return \"Hello, world!\";} return a();"
			),
			new Test(
				"let noarg function 2",
				Value.Create("Hello, world!"),
				"let function a(str) {return str;} return a(\"Hello, world!\");"
			),
			new Test(
				"let arg function 1",
				Value.Create(21.4 + 47.0 / 124.0 * 93.2),
				"let function a(a, b, c, d) {return a + b / c * d;} return a(21.4, 47, 124, 93.2);"
			),
			new Test(
				"global noarg function 1",
				Value.Create("Hello, world!"),
				"function a() {return \"Hello, world!\";} return a();"
			),
			new Test(
				"global arg function 1",
				Value.Create("Hello, world!"),
				"function a(str) {return str;} return a(\"Hello, world!\");"
			),
			new Test(
				"global arg function 2",
				Value.Create(21.4 + 47.0 / 124.0 * 93.2),
				"function a(a, b, c, d) {return a + b / c * d;} return a(21.4, 47, 124, 93.2);"
			),
			new Test(
				"1000 let increments 1",
				Value.Create(1000),
				"let a = 0; loop (1000) {a += 1;} return a;"
			),
			new Test(
				"1000 let increments 2",
				Value.Create(2000),
				"let a = 0; loop (1000) {a += 2;} return a;"
			),
			new Test(
				"1000 let increments 3",
				Value.Create(1000),
				"let a = 0; let function add(a, b) {return a + b;} loop (1000) {a = add(a, 1);} return a;"
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
				"a = 0; let function add(a, b) {return a + b;} loop (1000) {a = add(a, 1);} return a;"
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
				"let b = 5; if (b == 4) {return 1;} return 2;"
			),
			new Test(
				"if statement 2",
				Value.Create(1),
				"let b = 5; if (b == 5) {return 1;} return 2;"
			),
			new Test(
				"if-else statement 1",
				Value.Create(2),
				"let b = 5; if (b == 4) {return 1;}else {return 2;}"
			),
			new Test(
				"if-else statement 2",
				Value.Create(1),
				"let b = 5; if (b == 5) {return 1;}else {return 2;}"
			),
			new Test(
				"nested if-else 1",
				Value.Create(1),
				"let b = 5; if (b == 5) {return 1;}else {if (b == 4) {return 2;}}"
			),
			new Test(
				"nested if-else 2",
				Value.Create(2),
				"let b = 4; if (b == 5) {return 1;}else {if (b == 4) {return 2;}else {return 3;}}"
			),
			new Test(
				"nested if-else 3",
				Value.Create(3),
				"let b = 3; if (b == 5) {return 1;}else {if (b == 4) {return 2;}else {return 3;}}"
			),
			new Test(
				"while loop 1",
				Value.Create(1.0 + 20.0 * 2.5),
				"let a = 1; let i = 0; while (i < 20) {a += 2.5; i += 1;} return a;"
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
				"let a = 3.2; let b = 235.4; return [a, b, a + b, 4];"
			),
			new Test(
				"array nils",
				Value.Create(new List<Value> {Value.Nil, 235.4, 238.6, 4}),
				"let a = 3.2; let b = 235.4; return [c, b, a + b, 4];"
			),
			new Test(
				"array set",
				Value.Create(new List<Value> {1, 2, 3}),
				"let arr = [1]; arr[2] = 3; arr[1] = 2; return arr;"
			),
			new Test(
				"array get",
				Value.Create(new List<Value> {12, 4, 7}),
				"let arr = [3 * 4, 7, 4]; return [arr[0], arr[2], arr[1]];"
			),
			new Test(
				"simple table",
				Value.Create(new Table(new Dictionary<Value, Value>{{1.0, 5.0}, {5.0, 2.0}})),
				"return {1 => 5, 5 => 2};"
			),
			new Test(
				"empty table",
				Value.Create(new Table()),
				"return {};"
			),
			new Test(
				"table set",
				Value.Create(new Table(new Dictionary<Value, Value>{{"woof", "meow"}})),
				"let a = {}; let catSound = \"meow\"; a[\"woof\"] = catSound; return a;"
			),
			new Test(
				"table set dot",
				Value.Create(new Table(new Dictionary<Value, Value>{{"woof", "meow"}})),
				"let a = {}; let catSound = \"meow\"; a.woof = catSound; return a;"
			),
			new Test(
				"table get 1",
				Value.Create(1),
                "let a = {\"a\" => 1, \"b\" => 2.34, \"width\" => worldWidth, \"height\" => worldHeight}; return a[\"a\"];"
            ),
			new Test(
				"table get 2",
				Value.Create(2.34),
                "let a = {\"a\" => 1, \"b\" => 2.34, \"width\" => worldWidth, \"height\" => worldHeight}; return a[\"b\"];"
            ),
			new Test(
				"table get dot 1",
				Value.Create(1),
                "let a = {\"a\" => 1, \"b\" => 2.34, \"width\" => worldWidth, \"height\" => worldHeight}; return a.a;"
            ),
			new Test(
				"table get dot 2",
				Value.Create(2.34),
                "let a = {\"a\" => 1, \"b\" => 2.34, \"width\" => worldWidth, \"height\" => worldHeight}; return a.b;"
            ),
			new Test(
				"table key math and concatenation",
				Value.Create(new Table(new Dictionary<Value, Value>{{50.0, "Yellow"}, {"Test Key", 5.0}})),
				"return {25 * 2 => \"Yellow\", \"Test \" .. \"Key\" => 5};"
			),
			new Test(
				"table dot call",
				Value.Create(168.0),
				"let Vector = {}; Vector.Create = function(x, y) { return {\"x\" => x, \"y\" => y}; }; return Vector.Create(192, 168).y;"
			),
			new Test(
				"table dot bracket index",
				Value.Create(5.0),
				"let a = {}; a.b = {}; a.b[\"c\"] = 5; return a.b[\"c\"];"
			),
			new Test(
				"table mixed 1",
				Value.Create(23.0),
				"let Vector = {\"Create\" => function(x, y) { return {\"x\" => x, \"y\" => y}; } }; return Vector.Create(23, 212).x;"
			),
			new Test(
				"table concat",
				Value.Create(new Table(new Dictionary<Value, Value> {{"a", 5}, {"b", 7}, {"c", 5}, {"d", 7}})),
				"return {\"a\" => 5, \"b\" => 7} .. {\"c\" => 5, \"d\" => 7};"
			),
			new Test(
				"coroutine 1",
				Value.Create(new List<Value> {1.0, 2.0, 3.0}),
				"let cr = CreateCoroutine(function() {yield 1.0; yield 2.0; return 3.0;}); return [cr(), cr(), cr()];"
			),
			new Test(
				"coroutine 2",
				Value.Create(new List<Value> {3.0, 7.0, 9.0, 4.5, 8.5, 10.5}),
				"let cr = CreateCoroutine(function(n) {yield n + 2.0; yield n + 6.0; return n + 8.0;}); return [cr(1.0), cr(), cr(), cr(2.5), cr(), cr()];"
			),
			new Test(
				"coroutine 3",
				Value.Create(new List<Value> {3.0, 7.0, 9.0, 4.5, 8.5, 10.5}),
				"let cr = CreateCoroutine(function(n) {yield n + 2.0; yield n + 6.0; return n + 8.0;}); return [cr(1.0 + 2.5 - 2.5), cr(2.0), cr(), cr(2.5), cr(), cr()];"
			),
			new Test(
				"string char get 1",
				Value.Create("e"),
				"let str = \"test\"; return str[1];"
			),
			new Test(
				"string char get 2",
				Value.Create("s"),
				"return \"test\"[2];"
			),
			new Test(
				"string slice",
				Value.Create("es"),
                "return \"test\":Slice(1, 2);"
            ),
			new Test(
				"array slice",
				Value.Create(new List<Value> {3, 4}),
                "return [1, 2, 3, 4, 5]:Slice(2, 2);"
            ),
            new Test(
                "new array with length 1",
                Value.Create(new List<Value> {Value.Nil, Value.Nil, Value.Nil, Value.Nil}),
                "return ArrayOfLength(4);"
            ),
            new Test(
                "new array with length 2",
                Value.Create(new List<Value> {1.0, 1.0, 1.0, 1.0}),
                "return ArrayOfLength(4, 1);"
            ),
            new Test(
                "new array with length 3",
                Value.Create(new List<Value> {}),
                "return ArrayOfLength(0);"
            ),
            new Test(
                "new array with length 4",
                Value.Create(new List<Value> {}),
                "return ArrayOfLength(-1);"
            ),
            new Test(
				"type check number 1",
				Value.Create(true),
				"return 1 is Number;"
			),
			new Test(
				"type check number 2",
				Value.Create(false),
				"return \"Test\" is Number;"
			),
			new Test(
				"type check number 3",
				Value.Create(true),
				"return 4 + 1 is Number;"
			),
			new Test(
				"type check array 1",
				Value.Create(true),
				"return [1, 2, 3, 4] is Array;"
			),
			new Test(
				"type check array 2",
				Value.Create(false),
				"return 9 is Array;"
			),
			new Test(
				"type check array 3",
				Value.Create(false),
				"return \"Test\" is Array;"
			),
			new Test(
				"type check array 4",
				Value.Create(true),
				"return [1, 2] .. [3, 4] is Array;"
			),
			new Test(
				"then 1",
				Value.Create(1),
				"return true then 1;"
			),
			new Test(
				"then 2",
				Value.Nil,
				"return false then 1;"
			),
			new Test(
				"then 3",
				Value.Create(7),
				"return true then 1 + 6;"
			),
			new Test(
				"then 4",
				Value.Create(new List<Value>{1, 2, 3}),
				"return true then [1] .. [2, 3];"
			),
			new Test(
				"then otherwise 1",
				Value.Create(1),
				"return true then 1 otherwise 2;"
			),
			new Test(
				"then otherwise 2",
				Value.Create(2),
				"return false then 1 otherwise 2;"
			),
			new Test(
				"then otherwise 3",
				Value.Create(""),
				"let var = 5; return var is String then var otherwise \"\";"
			),
			new Test(
				"then otherwise 4",
				Value.Create("Hello"),
				"let var = \"Hello\"; return var is String then var otherwise \"\";"
			),
			new Test(
				"lengthof 1",
				Value.Create(3),
					"return lengthof [1, 4, 87];"
			),
			new Test(
				"lengthof 2",
				Value.Create(3),
				"return lengthof {\"aa\" => 1, 4 => 4, \"nn\" => 87};"
			),
			new Test(
				"lengthof 3",
				Value.Create(-5),
				"return -lengthof \"abcde\";"
			),
			new Test(
				"break 1",
				Value.Create(6),
				"let a = 0;loop(30){a += 1; if (a > 5) {break;}}return a;"
			),
			new Test(
				"break 2",
				Value.Create(new List<Value> {6, 6, 6, 6}),
				"let a = 0;let b = [];loop(4){a = 0; loop (30) {if (a > 5) {break;} a += 1;} b = b .. [a];}return b;"
			),
			new Test(
				"method 1",
				Value.Create(new List<Value> {22, 23, 24}),
				"let t = {\"v\" => 21, \"getV\" => function(this) { this.v = this.v + 1; return this.v; }}; return [t:getV(), t:getV(), t:getV()];"
			),
			new Test(
				"method 2",
				Value.Create(35),
				"Number.add = function(num, amount) {return num + amount;}; return 34:add(1);"
			),
            new Test(
                "nested functions and writing to own argument",
                Value.Create(3),
                "let function test_nest(x) { let function test_square(x) { return x+1; } x = test_square(x); return x; } return test_nest(2);"
            ),
            new Test(
                "sequence equals 1",
                Value.Create(1),
                "return [1, 2, 5, 7] === [1, 2, 5, 7];"
            ),
            new Test(
                "sequence equals 2",
                Value.Create(0),
                "return [1, 2, 5, 7] === [1, 2, 5, 7, 0];"
            ),
            new Test(
                "sequence equals 3",
                Value.Create(0),
                "return [1, 2, nil, 7] === [1, 2, 5, 7];"
            ),
            new Test(
                "sequence equals 4",
                Value.Create(1),
                "return [] === [];"
            ),
            new Test(
                "sequence equals 5",
                Value.Create(1),
                "return {\"cat\" => 1, \"mouse\" => 2} === {\"mouse\" => 2, \"cat\" => 1};"
            ),
            new Test(
                "sequence equals 6",
                Value.Create(0),
                "return {\"cat\" => 2, \"mouse\" => 2} === {\"mouse\" => 2, \"cat\" => 1};"
            ),
            new Test(
                "sequence equals 7",
                Value.Create(1),
                "return {\"cat\" => nil, \"mouse\" => 2} === {\"mouse\" => 2, \"cat\" => nil};"
            ),
            new Test(
                "sequence equals 8",
                Value.Create(0),
                "return {\"cat\" => nil, \"mouse\" => 2, 3 => 5} === {\"mouse\" => 2, \"cat\" => nil};"
            ),
            new Test(
                "sequence equals 9",
                Value.Create(1),
                "return {} === {};"
            ),
            new Test(
                "indexable 1",
                Value.Create(2),
                "indexableTest.foo = 1; indexableTest.bar = 2; return indexableTest.bar;",
                (c) => { c.SetGlobal("indexableTest", new IndexableTest()); }
            ),
            new Test(
                "indexable 2",
                Value.Create("hello"),
                "indexableTest.foo = 1; indexableTest.bar = 2; indexableTest[2] = \"hello\"; return indexableTest[2];",
                (c) => { c.SetGlobal("indexableTest", new IndexableTest()); }
            )
            ,
            new Test(
                "variable scope 1",
                Value.Create(new List<Value> { 4, 5, 6, 7, 8, 9 }),
                "let ret = []; let a = 4; let function aTest() { ret[lengthof ret] = a; a += 1; if (a < 10) { aTest(); } } aTest(); return ret;"
            ),
            new Test(
                "array clone 1",
                Value.Create(new List<Value> { 1, 2, 3, 4, 10, "orange" }),
                "return [1, 2, 3, 4, 10, \"orange\"]:Clone();"
            ),
            new Test(
                "table clone 2",
                Value.Create(new Table(new Dictionary<Value, Value> { { 6.0, 1.0 }, { 2.0, 2.0 }, { "merp", 3.0 }, { 9.0, 4.0 }, { 123.0, "orange" } })),
                "return {6 => 1, 2 => 2, \"merp\" => 3, 9 => 4, 123 => \"orange\"}:Clone();"
            ),
            new Test(
                "table CopyTo",
                Value.Create(new Table(new Dictionary<Value, Value> { { "foo", "bar" }, { 6.0, 1.0 }, { 2.0, 2.0 }, { "merp", 3.0 }, { 9.0, 4.0 }, { 123.0, "orange" } })),
                "let a = { \"foo\" => \"bar\" }; {6 => 1, 2 => 2, \"merp\" => 3, 9 => 4, 123 => \"orange\"}:CopyTo(a); return a;"
            ),
            new Test(
                "table foreach",
                Value.Create(new List<Value> {6, 2, "merp", 9, 123}),
                "let a = []; foreach ( k, v, { 6 => 1, 2 => 2, \"merp\" => 3, 9 => 4, 123 => \"orange\" } ) { a[lengthof a] = k; } return a;"
            ),
            new Test(
                "string ReplaceAll 1",
                Value.Create("Hello, World. World, Hello!"),
                "return \"1, 2. 2, 1!\":ReplaceAll(\"1\", \"Hello\"):ReplaceAll(\"2\", \"World\");"
            ),
            new Test(
                "string ReplaceAll 2",
                Value.Create("Hello, World. World, Hello!"),
                "let function repFunc() { return \"Hello\"; }; return \"1, 2. 2, 1!\":ReplaceAll(\"1\", repFunc):ReplaceAll(\"2\", \"World\");"
            )
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
