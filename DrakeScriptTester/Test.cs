using System;
using DrakeScript;

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
				var success = ret.Equals(SuccessReturn);
				if (!success)
				{
					var oldColor = Console.ForegroundColor;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Test \"" + Name + "\" failed; expected " + SuccessReturn + " but got " + ret);
					Console.WriteLine("Code = " + func.Code.ToStringFormatted());
					Console.WriteLine("Globals = " + String.Join(", ", context.Globals));
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
				Console.WriteLine("Globals = " + String.Join(", ", context.Globals));
				Console.ForegroundColor = oldColor;
				return false;
			}
		}
	}
}

