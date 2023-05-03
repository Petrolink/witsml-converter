using System;
using System.Collections;
using System.Text;

namespace Altova.Mapforce
{
	public class MFNodeByKindFilter : IEnumerable
	{
		IEnumerable baseSet;
		MFNodeKind nodeKind;

		public MFNodeByKindFilter(IEnumerable baseSet, MFNodeKind nodeKind)
		{
			this.baseSet = baseSet;
			this.nodeKind = nodeKind;
		}

		public IEnumerator GetEnumerator()
		{
			return new Enumerator(baseSet.GetEnumerator(), nodeKind);
		}

		class Enumerator : IMFEnumerator
		{
			IEnumerator baseEnum;
			MFNodeKind nodeKind;
			int pos = 0;
			
			public Enumerator(IEnumerator baseEnum, MFNodeKind nodeKind)
			{
				this.baseEnum = baseEnum;
				this.nodeKind = nodeKind;
			}

			public object Current 
			{ 
				get 
				{
					object o = baseEnum.Current;
					return (o is IMFNode) ? o : new ContentNode(o);
				}  
			}
			
			public int Position { get { return pos; } }
			public void Dispose() { MFEnumerator.Dispose(baseEnum); }			
			public void Reset() { baseEnum.Reset(); pos = 0;}

			public bool MoveNext()
			{
				while (baseEnum.MoveNext())
				{
					IMFNode node = baseEnum.Current as IMFNode;
					if (node != null)
					{
						if ((node.NodeKind & nodeKind) != 0)
						{
							pos++;
							return true;
						}
					}
					else
					{
						// simple value.
						if ((nodeKind & MFNodeKind.Text) != 0)
						{
							pos++;
							return true;
						}
					}
				}
				return false;					
			}
			class ContentNode : IMFNode
			{
				private object o;

				public ContentNode(object o) { this.o = o;}

				public string LocalName { get { return ""; } }
				public string NamespaceURI { get { return ""; } }
				public string Prefix { get { return ""; } }
				public string NodeName { get { return ""; } }
				public MFNodeKind NodeKind { get { return MFNodeKind.Text; } }
				public IEnumerable Select(MFQueryKind kind, object query) { return new MFSingletonSequence(o); }
				public Altova.Types.QName GetQNameValue() { return (Altova.Types.QName) o; }
				public object TypedValue { get { return o; } }
			}
		}
	}
	
	public class SequenceJoin : IEnumerable
	{
		private IEnumerable first;
		private IEnumerable second;

		public SequenceJoin(IEnumerable a, IEnumerable b)
		{
			first = a;
			second = b;
		}

		public IEnumerator GetEnumerator()
		{
			return new Enumerator(first.GetEnumerator(), second.GetEnumerator());
		}

		class Enumerator : IMFEnumerator
		{
			private IEnumerator first;
			private IEnumerator second;
			int pos = 0;

			public Enumerator(IEnumerator a, IEnumerator b)
			{
				first = a;
				second = b;
			}

			public object Current
			{
				get
				{
					return (first != null) ? first.Current : second.Current; 
				}
			}

			public int Position
			{
				get { return pos; }
			}

			public void Reset() { pos = 0; first.Reset(); second.Reset(); }
			public void Dispose() { MFEnumerator.Dispose(first); MFEnumerator.Dispose(second); }

			public bool MoveNext()
			{
				if (first != null)
				{
					bool b = first.MoveNext();
					if (b)
					{
						pos++;
						return true;
					}
					first = null;
				}
				if (second.MoveNext())
				{
					pos++;
					return true;
				}
				return false;
			}
		}
	}
}
