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

		public Test(string name, Value successReturn, string code)
		{
			Name = name;
			SuccessReturn = successReturn;
			Code = code;
		}

		public bool Run()
		{
			Context context;
			Function func;
			try
			{
				context = new Context();
				func = context.LoadString(Code);
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
				var ret = func.Invoke();
				var success = false;
				if (ret.Type == SuccessReturn.Type)
				{
					if (ret.Type == Value.ValueType.Array)
					{
						success = true;
						if (ret.Array.Count != SuccessReturn.Array.Count)
							success = false;
						else
							for (var i = 0; i < ret.Array.Count; i++)
							{
								if (!ret.Array[i].Equals(SuccessReturn.Array[i]))
								{
									success = false;
									break;
								}
							}
					}
					else if (ret.Type == Value.ValueType.Table)
					{
						success = true;
						if (ret.Table.Count != SuccessReturn.Table.Count)
							success = false;
						else
						{
							var keys = ret.Table.Keys;
							foreach (var key in keys)
							{
								if (!ret.Table[key].Equals(SuccessReturn.Table[key]))
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
					Console.WriteLine("Code = " + func.Code.ToStringFormatted());
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
				Console.WriteLine("Code = " + func.Code.ToStringFormatted());
				Console.ForegroundColor = oldColor;
				return false;
			}
		}
	}
}

