using System;

namespace DrakeScript
{
	public class Source
	{
		public string Name {get; private set;}
		public string Code {get; private set;}
		public SourceRef[] SourceRefs {get; private set;}

		public Source(string name, string code)
		{
			Name = name;
			Code = code.Replace("\r", "");
			SourceRefs = new SourceRef[Code.Length];

			int line = 0;
			int column = 0;
			for (var i = 0; i < Code.Length; i++)
			{
				SourceRefs[i] = new SourceRef(this, line, column);
				if (Code[i] == '\n')
				{
					line++;
					column = 0;
				}
				else
					column++;
            }
		}

		public override string ToString()
		{
			return Name;
		}
	}
}

