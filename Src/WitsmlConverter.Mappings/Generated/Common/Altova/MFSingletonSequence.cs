using System;
using System.Collections;
using System.Text;

namespace Altova.Mapforce
{
	public class MFSingletonSequence : IEnumerable
	{
		object item;

		public MFSingletonSequence(object item)
		{
			this.item = item;
		}

		public IEnumerator GetEnumerator()
		{
			return new Enumerator(item);
		}

		class Enumerator : IMFEnumerator
		{
			object item;
			bool b = true;
			public Enumerator(object item) { this.item = item; }
			public object Current { get { return item; } }
			public int Position { get { return 1; } }
			public bool MoveNext()
			{
				if (b) { b = false; return true; }
				return false;
			}
			public void Reset() { b = true; }
			public void Dispose() {}			
		}
	}
	
}
