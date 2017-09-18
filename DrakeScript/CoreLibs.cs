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

			public static Value Print(Value[] args, int argCount)
			{
				for (var i = 0; i < argCount; i++)
				{
					Console.Write(args[i].ToString());
				}
				return Value.Nil;
			}
			public static Value PrintLn(Value[] args, int argCount)
			{
				for (var i = 0; i < argCount; i++)
				{
					Console.Write(args[i].ToString());
				}
				Console.WriteLine();
				return Value.Nil;
			}
			public static Value Nothing(Value[] args, int argCount)
			{
				
				return Value.Nil;
			}
		}
	}
}

