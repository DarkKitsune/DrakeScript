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
			if (!System.IO.File.Exists(args[0]))
			{
				Console.WriteLine("Script does not exist!");
				return;
			}
			var context = new Context();
			var source = new Source(args[0], System.IO.File.ReadAllText(args[0]));
			var scanner = new Scanner();
			var tokens = scanner.Scan(source);
			Console.WriteLine(tokens.ToStringFormatted() + "\n");
			var parser = new Parser();
			var tree = parser.Parse(tokens);
			var analyzer = new Analyzer();
			tree = analyzer.Analyze(tree);
			Console.WriteLine(tree + "\n");
			var generator = new CodeGenerator();
			var code = generator.Generate(tree);
			var optimizer = new Optimizer();
			optimizer.Optimize(code);
			Console.WriteLine(code.Code.ToStringFormatted() + "\n");
			var interpreter = new Interpreter(context);
			var sw = new System.Diagnostics.Stopwatch();
			for (var i = 0; i < 10; i++)
			{
				sw.Reset();
				sw.Start();
				code.Invoke(interpreter);
				sw.Stop();
			}
			if (interpreter.Stack.Count > 0)
				Console.WriteLine("result: " + interpreter.Stack.Peek(0).DynamicValue);
			else
				Console.WriteLine("no result");
			Console.WriteLine("time taken: " + sw.ElapsedTicks + " ticks (" + ((double)sw.ElapsedTicks / (double)TimeSpan.TicksPerSecond) + "s)");
		}
	}
}
