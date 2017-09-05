using System;
using DrakeScript;

namespace DrakeScriptTester
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			var source = new Source("test.script", System.IO.File.ReadAllText("test.script"));
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
		}
	}
}
