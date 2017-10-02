using System;

namespace DrakeScript
{
	public struct InterpreterPause
	{
		public static InterpreterPause Finished = new InterpreterPause {Paused = false};

		public bool Paused;
		public int Location;
		public Value[] Locals;
		public Value[] Args;

		public InterpreterPause(int location, Value[] locals, Value[] args)
		{
			Paused = true;
			Location = location;
			Locals = locals;
			Args = args;
		}
	}
}

