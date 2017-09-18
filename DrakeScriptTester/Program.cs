using System;
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
				var sw = new System.Diagnostics.Stopwatch();
				var count = 0;
				sw.Start();
				while (sw.ElapsedMilliseconds < 5000)
				{
					code.Invoke(interpreter);
					count++;
				}
				sw.Stop();
				if (interpreter.Stack.Count > 0)
					Console.WriteLine("result: " + interpreter.Stack.Peek(0).ToString());
				else
					Console.WriteLine("no result");
				Console.WriteLine("average time per run: " + (5000.0 / (double)count) + " ms over " + count + " runs");
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
