using System;
using System.Collections.Generic;

namespace DrakeScript
{
	public struct Scope
	{
		public static Scope Invalid = new Scope {Valid = false, Locals = null};

		public bool Valid;
		public Dictionary<string, Value> Locals;

		public static Scope Create()
		{
			var scope = new Scope();
			scope.Valid = true;
			scope.Locals = new Dictionary<string, Value>();
			return scope;
		}
	}
}

