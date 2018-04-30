﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace DrakeScript
{
	public static class CoreLibs
	{
		public static class LibCore
		{
			public static void Register(Context context)
			{
				context.SetGlobal("array", typeof(List<Value>));
				context.SetGlobal("coroutine", typeof(Coroutine));
				context.SetGlobal("function", typeof(Function));
				context.SetGlobal("int", typeof(double));
				context.SetGlobal("number", typeof(double));
				context.SetGlobal("string", typeof(string));
				context.SetGlobal("table", typeof(Table));

				context.SetGlobal("print", context.CreateFunction(Print, 0));
				context.SetGlobal("println", context.CreateFunction(PrintLn, 0));
				context.SetGlobal("time", context.CreateFunction(Time, 0));
				context.SetGlobal("toString", context.CreateFunction(ConvToString, 1));
				context.SetGlobal("type", context.CreateFunction(GetValueType, 1));

				context.SetGlobal("inf", double.PositiveInfinity);
				context.SetGlobal("maxNumber", double.MaxValue);
				context.SetGlobal("minNumber", double.MinValue);
				context.SetGlobal("NaN", double.NaN);
				context.SetGlobal("epsilon", double.Epsilon);
				context.SetGlobal("pi", Math.PI);
				context.SetGlobal("e", Math.E);
			}

			public static Value Print(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				for (var i = 0; i < argCount; i++)
				{
					Console.Write(args[i].ToString());
				}
				return Value.Nil;
			}
			public static Value PrintLn(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				for (var i = 0; i < argCount; i++)
				{
					Console.Write(args[i].ToString());
				}
				Console.WriteLine();
				return Value.Nil;
			}
			public static Value Time(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				return (double)DateTime.Now.Ticks / (double)TimeSpan.TicksPerSecond;
			}
			public static Value ConvToString(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				return args[0].ToString();
			}
			public static Value GetValueType(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				return Value.Create(args[0].ActualType);
			}
		}

		public static class LibCoroutine
		{
			public static void Register(Context context)
			{
				context.SetGlobal("coroutine", context.CreateFunction(Create, 1));
			}

			public static Value Create(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				return interpreter.Context.CreateCoroutine(args[0].VerifyType(Value.ValueType.Function, location));
			}
		}

		public static class LibArray
		{
			public static void Register(Context context)
			{
                context.SetGlobal("arrayOfLength", context.CreateFunction(ArrayOfLength, 1));
                context.SetGlobal("length", context.CreateFunction(Length, 1));
				context.SetGlobal("slice", context.CreateFunction(Slice, 3));
			}

            public static Value ArrayOfLength(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                var count = (int)args[0].Number;
                if (count < 0)
                    count = 0;
                var val = Value.Nil;
                if (argCount > 1)
                    val = args[1];
                var arr = new List<Value>(count);
                for (var i = 0; i < count; i++)
                    arr.Add(val);
                return arr;
            }

            public static Value Length(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				switch (args[0].Type)
				{
					case (Value.ValueType.Array):
						return args[0].ArrayDirect.Count;
					case (Value.ValueType.String):
						return args[0].StringDirect.Length;
					default:
						throw new UnexpectedTypeException(args[0].Type,location);
				}
			}

			public static Value Slice(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				var skip = (int)args[1].VerifyType(Value.ValueType.Number, location).Number;
				var take = (int)args[2].VerifyType(Value.ValueType.Number, location).Number;
				switch (args[0].Type)
				{
					case (Value.ValueType.Array):
						return args[0].ArrayDirect.Skip(skip).Take(take).ToList();
					case (Value.ValueType.String):
						return new String(args[0].StringDirect.Skip(skip).Take(take).ToArray());
					default:
						throw new UnexpectedTypeException(args[0].Type,location);
				}
			}
		}

		public static class LibTable
		{
			public static void Register(Context context)
			{
				context.SetGlobal("count", context.CreateFunction(Count, 1));
			}

			public static Value Count(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				return args[0].VerifyType(Value.ValueType.Table, location).TableDirect.Count;
			}
		}

		public static Random Random = new Random();
		public static class LibMath
		{
			public static void Register(Context context)
			{
				context.SetGlobal("cos", context.CreateFunction(Cos, 1));
				context.SetGlobal("sin", context.CreateFunction(Sin, 1));
				context.SetGlobal("dcos", context.CreateFunction(DCos, 1));
				context.SetGlobal("dsin", context.CreateFunction(DSin, 1));
				context.SetGlobal("degtorad", context.CreateFunction(Rad, 1));
				context.SetGlobal("radtodeg", context.CreateFunction(Deg, 1));
				context.SetGlobal("round", context.CreateFunction(Round, 1));
				context.SetGlobal("floor", context.CreateFunction(Floor, 1));
				context.SetGlobal("ceil", context.CreateFunction(Ceil, 1));
				context.SetGlobal("sqrt", context.CreateFunction(Sqrt, 1));
				context.SetGlobal("sqr", context.CreateFunction(Sqr, 1));
                context.SetGlobal("root", context.CreateFunction(Root, 2));
				context.SetGlobal("pow", context.CreateFunction(Pow, 2));
				context.SetGlobal("rand", context.CreateFunction(Rand, 2));
			}

			public static Value Cos(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				return Math.Cos(args[0].Number);
			}

			public static Value Sin(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				return Math.Sin(args[0].Number);
			}

			public static Value DCos(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				return Math.Cos(args[0].Number / 57.295779515);
			}

			public static Value DSin(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				return Math.Sin(args[0].Number / 57.295779515);
			}

			public static Value Rad(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				return args[0].Number / 57.295779515;
			}

			public static Value Deg(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				return args[0].Number * 57.295779515;
			}

			public static Value Round(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				return Math.Round(args[0].Number);
			}

			public static Value Floor(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				return Math.Floor(args[0].Number);
			}

			public static Value Ceil(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				return Math.Ceiling(args[0].Number);
			}

			public static Value Sqrt(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				return Math.Sqrt(args[0].Number);
			}

			public static Value Sqr(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				return args[0].Number * args[0].Number;
			}

            public static Value Root(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                return Math.Pow(args[0].Number, 1.0 / args[1].Number);
            }

			public static Value Pow(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				return Math.Pow(args[0].Number, args[1].Number);
			}

			public static Value Rand(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				return args[0].Number + (args[1].Number - args[0].Number) * Random.NextDouble();
			}
		}
	}
}

