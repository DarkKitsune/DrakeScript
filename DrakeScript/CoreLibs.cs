using System;
using System.Collections.Generic;

namespace DrakeScript
{
	public static class CoreLibs
	{
		public static class Core
		{
			public static void Register(Context context)
			{
				context.Globals["print"] = Value.Create(new Function(Print));
				context.Globals["println"] = Value.Create(new Function(PrintLn));
				context.Globals["nothing"] = Value.Create(new Function(Nothing));
			}

			public static Value Print(List<Value> args)
			{
				foreach (var arg in args)
				{
					Console.Write(arg.DynamicValue.ToString());
				}
				return Value.Nil;
			}
			public static Value PrintLn(List<Value> args)
			{
				foreach (var arg in args)
				{
					Console.Write(arg.DynamicValue.ToString());
				}
				Console.WriteLine();
				return Value.Nil;
			}
			public static Value Nothing(List<Value> args)
			{
				
				return Value.Nil;
			}
		}
	}
}

