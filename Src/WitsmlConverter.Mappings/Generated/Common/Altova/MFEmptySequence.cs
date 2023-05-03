using System;
using System.Collections;
using System.Text;

namespace Altova.Mapforce
{
	public class MFEmptySequence : IEnumerable
	{
		private MFEmptySequence()
		{
		}

		public static MFEmptySequence Instance = new MFEmptySequence();

		public IEnumerator GetEnumerator()
		{
			return Enumerator.Instance;
		}

		class Enumerator : IMFEnumerator
		{
			public static Enumerator Instance = new Enumerator();

			private Enumerator() { }
			public object Current { get { throw new InvalidOperationException(); } }
			public int Position { get { throw new InvalidOperationException(); } }
			public bool MoveNext() { return false; }
			public void Reset() { }
			public void Dispose() {}
		}
	}


}
