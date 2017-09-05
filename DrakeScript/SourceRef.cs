using System;

namespace DrakeScript
{
	public struct SourceRef
	{
		public static SourceRef Invalid = new SourceRef {Valid = false, Source = null, Line = 0, Column = 0};

		public bool Valid {get; private set;}
		public Source Source {get; private set;}
		public int Line {get; private set;}
		public int Column {get; private set;}

		public SourceRef(Source source, int line, int column)
		{
			Valid = true;
			Source = source;
			Line = line;
			Column = column;
		}

		public override string ToString()
		{
			return string.Format("{0}:{1}:{2}", Source, Line + 1, Column + 1);
		}
	}
}

