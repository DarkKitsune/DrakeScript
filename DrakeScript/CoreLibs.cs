﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace DrakeScript
{
	public static class CoreLibs
	{
		public static class LibCore
		{
			public static void Register(Context context)
			{
				context.SetGlobal("Array", typeof(List<Value>));
				context.SetGlobal("Coroutine", typeof(Coroutine));
                context.SetGlobal("Thread", typeof(Thread));
                context.SetGlobal("Mutex", typeof(Mutex));
                context.SetGlobal("Function", typeof(Function));
				context.SetGlobal("Int", typeof(double));
				context.SetGlobal("Number", typeof(double));
				context.SetGlobal("String", typeof(string));
				context.SetGlobal("Table", typeof(Table));

				context.SetGlobal("Print", context.CreateFunction(Print, 0));
				context.SetGlobal("PrintLn", context.CreateFunction(PrintLn, 0));
				context.SetGlobal("Time", context.CreateFunction(Time, 0));
				context.SetGlobal("ToString", context.CreateFunction(ConvToString, 1));
				context.SetGlobal("Type", context.CreateFunction(GetValueType, 1));

				context.SetGlobal("Inf", double.PositiveInfinity);
				context.SetGlobal("MaxNumber", double.MaxValue);
				context.SetGlobal("MinNumber", double.MinValue);
				context.SetGlobal("NaN", double.NaN);
				context.SetGlobal("Epsilon", double.Epsilon);
				context.SetGlobal("Pi", Math.PI);
				context.SetGlobal("E", Math.E);
			}

			public static Value Print(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				for (var i = 0; i < argCount; i++)
				{
					Console.Write(args[i].ToString(interpreter.Context));
				}
				return Value.Nil;
			}
			public static Value PrintLn(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				for (var i = 0; i < argCount; i++)
				{
					Console.Write(args[i].ToString(interpreter.Context));
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
				return args[0].ToString(interpreter.Context);
			}
			public static Value GetValueType(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				return Value.Create(args[0].ActualType);
			}
		}

		public static class LibException
		{
			public static void Register(Context context)
			{
				context.SetGlobal("ThrowException", context.CreateFunction(ThrowException, 1));
			}

			public static Value ThrowException(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
                throw new InterpreterException(args[0].VerifyType(Value.ValueType.String, location).String, location);
			}
		}

        public static class LibCoroutine
        {
            public static void Register(Context context)
            {
                context.SetGlobal("CreateCoroutine", context.CreateFunction(Create, 1));
            }

            public static Value Create(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                return interpreter.Context.CreateCoroutine(args[0].VerifyType(Value.ValueType.Function, location));
            }
        }

        public static class LibThread
        {
            public static void Register(Context context)
            {
                context.SetGlobal("CreateThread", context.CreateFunction(Create, 1));
                context.SetGlobal("Sleep", context.CreateFunction(Sleep, 1));
                context.SetMethod(typeof(Thread), "WaitOn", context.CreateFunction(WaitOn, 0));
                context.SetGlobal("CreateMutex", context.CreateFunction(CreateMutex, 0));
                context.SetMethod(typeof(Mutex), "Lock", context.CreateFunction(Lock, 0));
                context.SetMethod(typeof(Mutex), "Unlock", context.CreateFunction(Unlock, 0));
            }

            public static Value Create(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                var func = args[0].VerifyType(Value.ValueType.Function, location);
                var targs = args.Skip(1).ToArray();
                var thread = new Thread(
                    () =>
                    {
                        func.FunctionDirect.Invoke(targs);
                    }
                );
                thread.Start();
                return Value.Create(thread);
            }

            public static Value Sleep(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                Thread.Sleep((int)args[0].VerifyType(Value.ValueType.Number, location).Number);
                return Value.Nil;
            }

            public static Value WaitOn(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                if (argCount > 1)
                    args[0].VerifyType(Value.ValueType.Thread, location).ThreadDirect.Join((int)args[1].VerifyType(Value.ValueType.Number, location).Number);
                else
                    args[0].VerifyType(Value.ValueType.Thread, location).ThreadDirect.Join();
                return Value.Nil;
            }

            public static Value CreateMutex(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                return Value.Create(new Mutex());
            }

            public static Value Lock(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                if (argCount > 1)
                    args[0].VerifyType(Value.ValueType.Mutex, location).MutexDirect.WaitOne((int)args[0].VerifyType(Value.ValueType.Number).Number);
                else
                    args[0].VerifyType(Value.ValueType.Mutex, location).MutexDirect.WaitOne();
                return Value.Nil;
            }

            public static Value Unlock(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                args[0].VerifyType(Value.ValueType.Mutex, location).MutexDirect.ReleaseMutex();
                return Value.Nil;
            }
        }

        public static class LibArray
		{
			public static void Register(Context context)
			{
                context.SetGlobal("ArrayOfLength", context.CreateFunction(ArrayOfLength, 1));
                context.SetMethod(typeof(List<Value>), "Length", context.CreateFunction(ArrayLength, 1));
                context.SetMethod(typeof(string), "Length", context.CreateFunction(StringLength, 1));
                context.SetMethod(typeof(List<Value>), "Slice", context.CreateFunction(ArraySlice, 3));
                context.SetMethod(typeof(string), "Slice", context.CreateFunction(StringSlice, 3));
                context.SetMethod(typeof(List<Value>), "RemoveAt", context.CreateFunction(ArrayRemoveAt, 2));
                context.SetMethod(typeof(List<Value>), "Remove", context.CreateFunction(ArrayRemove, 2));
                context.SetMethod(typeof(List<Value>), "Clear", context.CreateFunction(ArrayClear, 1));
                context.SetMethod(typeof(List<Value>), "Clone", context.CreateFunction(Clone, 1));
                context.SetMethod(typeof(List<Value>), "Sort", context.CreateFunction(Sort, 1));
                context.SetMethod(typeof(string), "ReplaceAll", context.CreateFunction(ReplaceAll, 3));
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

            public static Value ArrayLength(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
                return args[0].ArrayDirect.Count;
            }

            public static Value StringLength(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                return args[0].StringDirect.Length;
            }

            public static Value ArraySlice(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				var skip = (int)args[1].VerifyType(Value.ValueType.Number, location).Number;
				var take = (int)args[2].VerifyType(Value.ValueType.Number, location).Number;
                return args[0].ArrayDirect.Skip(skip).Take(take).ToList();
            }

            public static Value StringSlice(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                var skip = (int)args[1].VerifyType(Value.ValueType.Number, location).Number;
                var take = (int)args[2].VerifyType(Value.ValueType.Number, location).Number;
                return new string(args[0].StringDirect.Skip(skip).Take(take).ToArray());
            }

            public static Value ArrayRemoveAt(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                args[0].ArrayDirect.RemoveAt(args[1].VerifyType(Value.ValueType.Number, location));
                return Value.Nil;
            }

            public static Value ArrayRemove(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                var array = args[0].ArrayDirect;
                for (var i = 0; i < array.Count; i++)
                    if (array[i].DynamicValue == args[1].DynamicValue)
                    {
                        array.RemoveAt(i);
                        return Value.Nil;
                    }
                return Value.Nil;
            }

            public static Value ArrayClear(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                args[0].ArrayDirect.Clear();
                return Value.Nil;
            }

            public static Value Clone(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                return new List<Value>(args[0].ArrayDirect);
            }

            public static Value Sort(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                if (argCount == 1)
                {
                    args[0].ArrayDirect.Sort(
                        (a, b) =>
                        {
                            if (a.Type == Value.ValueType.Number)
                                return a.Number > b.Number ? 1 : -1;
                            Value gt;
                            var succ = a.TryInvokeMethod(interpreter.Context, "GreaterThan", new[] { b }, out gt);
                            if (succ)
                                return gt.Bool ? 1 : -1;
                            return -1;
                        }
                    );
                }
                else if (argCount > 1)
                {
                    var func = args[1].VerifyType(Value.ValueType.Function, location).FunctionDirect;
                    args[0].ArrayDirect.Sort(
                        (a, b) =>
                        {
                            return func.Invoke(a, b).Bool ? 1 : -1;
                        }
                    );
                }
                return Value.Nil;
            }

            public static Value ReplaceAll(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                var replaceString = args[1].VerifyType(Value.ValueType.String, location).StringDirect;
                if (args[2].Type == Value.ValueType.Function)
                {
                    var str = args[0].StringDirect;
                    var func = args[2].FunctionDirect;
                    var pos = str.IndexOf(replaceString);
                    while (pos >= 0)
                    {
                        var rep = func.Invoke(replaceString).ToString();
                        str = new string(str.Take(pos).Concat(rep).Concat(str.Skip(pos + replaceString.Length)).ToArray());
                        pos = str.IndexOf(replaceString, pos + rep.Length);
                    }
                    return str;
                }
                var replaceWith = args[2].VerifyType(Value.ValueType.String, location).StringDirect;
                return args[0].StringDirect.Replace(replaceString, replaceWith);
            }
        }

		public static class LibTable
		{
			public static void Register(Context context)
			{
				context.SetMethod(typeof(Table), "Count", context.CreateFunction(Count, 0));
                context.SetMethod(typeof(Table), "Clone", context.CreateFunction(Clone, 0));
                context.SetMethod(typeof(Table), "CopyTo", context.CreateFunction(CopyTo, 1));
            }

			public static Value Count(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
			{
				return args[0].TableDirect.Count;
			}

            public static Value Clone(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                return new Table(args[0].TableDirect.InternalDictionary);
            }

            public static Value CopyTo(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                var dest = args[1].VerifyType(Value.ValueType.Table, location).TableDirect.InternalDictionary;
                foreach (var kvp in args[0].TableDirect.InternalDictionary)
                {
                    dest[kvp.Key] = kvp.Value;
                }
                return Value.Nil;
            }
        }

		public static Random Random = new Random();
		public static class LibMath
		{
			public static void Register(Context context)
			{
				context.SetGlobal("Cos", context.CreateFunction(Cos, 1));
				context.SetGlobal("Sin", context.CreateFunction(Sin, 1));
				context.SetGlobal("DCos", context.CreateFunction(DCos, 1));
				context.SetGlobal("DSin", context.CreateFunction(DSin, 1));
                context.SetGlobal("Atan", context.CreateFunction(Atan, 1));
                context.SetGlobal("DAtan", context.CreateFunction(DAtan, 1));
                context.SetGlobal("Atan2", context.CreateFunction(Atan2, 1));
                context.SetGlobal("DAtan2", context.CreateFunction(DAtan2, 1));
                context.SetGlobal("DegToRad", context.CreateFunction(Rad, 1));
				context.SetGlobal("RadToDeg", context.CreateFunction(Deg, 1));
				context.SetGlobal("Round", context.CreateFunction(Round, 1));
				context.SetGlobal("Floor", context.CreateFunction(Floor, 1));
				context.SetGlobal("Ceil", context.CreateFunction(Ceil, 1));
				context.SetGlobal("Sqrt", context.CreateFunction(Sqrt, 1));
				context.SetGlobal("Sqr", context.CreateFunction(Sqr, 1));
                context.SetGlobal("Root", context.CreateFunction(Root, 2));
				context.SetGlobal("Pow", context.CreateFunction(Pow, 2));
				context.SetGlobal("Rand", context.CreateFunction(Rand, 2));
                context.SetGlobal("Abs", context.CreateFunction(Abs, 1));
                context.SetGlobal("Max", context.CreateFunction(Max, 2));
                context.SetGlobal("Min", context.CreateFunction(Min, 2));
                context.SetGlobal("Clamp", context.CreateFunction(Clamp, 3));
                context.SetGlobal("Sign", context.CreateFunction(Sign, 1));
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

            public static Value Atan(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                return Math.Atan(args[0].Number);
            }

            public static Value DAtan(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                return Math.Atan(args[0].Number) * 57.295779515;
            }

            public static Value Atan2(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                return Math.Atan2(args[0].Number, args[1].Number);
            }

            public static Value DAtan2(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                return Math.Atan2(args[0].Number, args[1].Number) * 57.295779515;
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

            public static Value Abs(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                return Math.Abs(args[0].Number);
            }

            public static Value Max(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                return Math.Max(args[0].Number, args[1].Number);
            }

            public static Value Min(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                return Math.Min(args[0].Number, args[1].Number);
            }

            public static Value Clamp(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                return Math.Min(Math.Max(args[0].Number, args[1].Number), args[2].Number);
            }

            public static Value Sign(Interpreter interpreter, SourceRef location, Value[] args, int argCount)
            {
                return Math.Sign(args[0].Number);
            }
        }
	}
}

