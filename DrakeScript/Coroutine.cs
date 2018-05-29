using System;
using System.Linq;

namespace DrakeScript
{
	public class Coroutine
	{
		public Context Context;
		public Interpreter Interpreter;
		public Function Function;
		public CoroutineStatus Status;
		public Coroutine(Context context, Function function)
		{
			Context = context;
			Interpreter = new Interpreter(context, true);
			Function = function;
			Status = CoroutineStatus.Ready;
		}


        internal Value Resume(Value[] args, int count, Value[] parentLocals)
		{
			
			Value ret = Value.Nil;
			switch (Status)
			{
				case (CoroutineStatus.Ready):
				case (CoroutineStatus.Stopped):
					if (args.Length != count)
						ret = Function.Invoke(Interpreter, args.Take(count).ToArray());
					else
						ret = Function.Invoke(Interpreter, args);
					break;
				case (CoroutineStatus.Yielded):
					ret = Function.Invoke(Interpreter);
					break;
			}

			
			if (Interpreter.PauseStatus.Paused)
				Status = CoroutineStatus.Yielded;
			else
				Status = CoroutineStatus.Stopped;
			
			return ret;
		}

        public Value Resume(Value[] args, int count)
        {

            return Resume(args, count);
        }

        public Value Resume(params Value[] args)
		{
			Status = CoroutineStatus.Ready;
			return Resume(args, args.Length);
		}

		public void Yield()
		{
			if (Status == CoroutineStatus.Ready)
			{
				Interpreter.Yield();
				Status = CoroutineStatus.Yielded;
			}
		}
	}
}

