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
			Console.WriteLine(code.ToStringFormatted() + "\n");
			var interpreter = new Interpreter(context);
			interpreter.Interpret(code);
			Console.WriteLine("result: " + interpreter.Stack.Peek(0).DynamicValue);
		}
	}
}
