using System;
using System.Collections.Generic;

namespace DrakeScript
{
	public class Context
	{
		public Dictionary<string, Value> Globals = new Dictionary<string, Value>();

		public Context()
		{
			CoreLibs.Core.Register(this);
		}
	}
}

