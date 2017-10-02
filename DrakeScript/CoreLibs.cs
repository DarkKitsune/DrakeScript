using System;
using System.Collections.Generic;

namespace DrakeScript
{
	public static class CoreLibs
	{
		public static class LibCore
		{
			public static void Register(Context context)
			{
				context.SetGlobal("print", context.CreateFunction(Print, 0));
				context.SetGlobal("println", context.CreateFunction(PrintLn, 0));
				context.SetGlobal("nothing", context.CreateFunction(Nothing, 0));
			}

			public static Value Print(Context context, SourceRef location, Value[] args, int argCount)
			{
				for (var i = 0; i < argCount; i++)
				{
					Console.Write(args[i].ToString());
				}
				return Value.Nil;
			}
			public static Value PrintLn(Context context, SourceRef location, Value[] args, int argCount)
			{
				for (var i = 0; i < argCount; i++)
				{
					Console.Write(args[i].ToString());
				}
				Console.WriteLine();
				return Value.Nil;
			}
			public static Value Nothing(Context context, SourceRef location, Value[] args, int argCount)
			{
				
				return Value.Nil;
			}
		}

		public static class LibCoroutine
		{
			public static void Register(Context context)
			{
				var table = new Table();
				context.SetGlobal("coroutine", table);

				table["create"] = Value.Create(context.CreateFunction(Create, 1));
			}

			public static Value Create(Context context, SourceRef location, Value[] args, int argCount)
			{
				return Value.Create(context.CreateCoroutine(args[0].VerifyType(Value.ValueType.Function, location)));
			}
		}
	}
}

