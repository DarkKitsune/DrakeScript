using System;
using DrakeScript;
using System.Collections.Generic;

namespace DrakeScriptTester
{
	public struct Test
	{
		string Name;
		Value SuccessReturn;
		string Code;
        Action<Context> RunBefore;
        string SourceName;

        public Test(string name, Value successReturn, string code, Action<Context> runBefore = null,
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
		{
			Name = name;
			SuccessReturn = successReturn;
			Code = code;
            RunBefore = runBefore;
            SourceName = "(Test at " + System.IO.Path.GetFileName(sourceFilePath) + ":" + sourceLineNumber + ")";
        }

		public bool Run()
		{
			Context context;
			Function func;
			try
			{
				context = new Context();
				func = context.LoadString(SourceName, Code);
			}
			catch (Exception e)
			{
				var oldColor = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Test \"" + Name + "\" failed because of exception:\n " + e.ToString());
				Console.ForegroundColor = oldColor;
				return false;
			}
			try
			{
                if (RunBefore != null)
                    RunBefore(context);
				var ret = func.Invoke();
				var success = false;
				if (ret.Type == SuccessReturn.Type)
				{
                    if (ret.Type == Value.ValueType.Array)
					{
						success = true;
						if (ret.ArrayDirect.Count != SuccessReturn.ArrayDirect.Count)
							success = false;
						else
							for (var i = 0; i < ret.ArrayDirect.Count; i++)
							{
								if (!ret.ArrayDirect[i].Equals(SuccessReturn.ArrayDirect[i]))
								{
                                    success = false;
									break;
								}
							}
					}
					else if (ret.Type == Value.ValueType.Table)
					{
						success = true;
						if (ret.TableDirect.Count != SuccessReturn.TableDirect.Count)
							success = false;
						else
						{
							var keys = ret.TableDirect.Keys;
							foreach (var key in keys)
							{
                                if (!ret.TableDirect[key].Equals(SuccessReturn.TableDirect[key]))
								{
                                    success = false;
									break;
								}
							}
						}
					}
                    else
						success = ret.Equals(SuccessReturn);
				}
				if (!success)
				{
					var oldColor = Console.ForegroundColor;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Test \"" + Name + "\" failed; expected " + SuccessReturn.ToString() + " but got " + ret.ToString());
					Console.WriteLine("Code = " + func.ToStringFormatted());
					Console.ForegroundColor = oldColor;
				}
				else
				{
					var oldColor = Console.ForegroundColor;
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("Test \"" + Name + "\" succeeded");
					Console.ForegroundColor = oldColor;
				}
				return success;
			}
			catch (Exception e)
			{
				var oldColor = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Test \"" + Name + "\" failed because of exception:\n " + e.ToString());
				Console.WriteLine("Code = " + func.ToStringFormatted());
				Console.ForegroundColor = oldColor;
				return false;
			}
		}
	}
}

