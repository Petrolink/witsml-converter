using System;
using System.Collections;
using System.Text;

namespace Altova.Mapforce
{
	public class MFNodeByKindAndQNameFilter : IEnumerable
	{
		IEnumerable baseSet;
		MFNodeKind nodeKind;
		string localName;
		string namespaceURI;

		public MFNodeByKindAndQNameFilter(IEnumerable baseSet, MFNodeKind nodeKind, string localName, string namespaceURI)
		{
			this.baseSet = baseSet;
			this.nodeKind = nodeKind;
			this.localName = localName;
			this.namespaceURI = namespaceURI;
		}

		public IEnumerator GetEnumerator()
		{
			return new Enumerator(baseSet.GetEnumerator(), nodeKind, localName, namespaceURI);
		}

		class Enumerator : IMFEnumerator
		{
			IEnumerator baseEnum;
			MFNodeKind nodeKind;
			string localName;
			string namespaceURI;
			int pos = 0;
			
			public Enumerator(IEnumerator baseEnum, MFNodeKind nodeKind, string localName, string namespaceURI)
			{
				this.baseEnum = baseEnum;
				this.nodeKind = nodeKind;
				this.localName = localName;
				this.namespaceURI = namespaceURI;
			}

			public object Current { get { return baseEnum.Current; } }
			public int Position { get { return pos; } }
			public void Reset() { baseEnum.Reset(); pos = 0;}
			public void Dispose() { MFEnumerator.Dispose(baseEnum); }
			public bool MoveNext()
			{
				while (baseEnum.MoveNext())
				{
					IMFNode node = baseEnum.Current as IMFNode;
					if (node != null && (node.NodeKind & nodeKind) != 0 && node.LocalName == localName && node.NamespaceURI == namespaceURI)
					{
						pos++;
						return true;
					}
				}
				return false;					
			}
		}
	}

	public class MFNodeByKindAndNodeNameFilter : IEnumerable
	{
		IEnumerable baseSet;
		MFNodeKind nodeKind;
		string name;

		public MFNodeByKindAndNodeNameFilter(IEnumerable baseSet, MFNodeKind nodeKind, string name)
		{
			this.baseSet = baseSet;
			this.nodeKind = nodeKind;
			this.name = name;
		}

		public IEnumerator GetEnumerator()
		{
			return new Enumerator(baseSet.GetEnumerator(), nodeKind, name);
		}

		class Enumerator : IMFEnumerator
		{
			IEnumerator baseEnum;
			MFNodeKind nodeKind;
			string name;
			int pos = 0;

			public Enumerator(IEnumerator baseEnum, MFNodeKind nodeKind, string name)
			{
				this.baseEnum = baseEnum;
				this.nodeKind = nodeKind;
				this.name = name;
			}

			public object Current { get { return baseEnum.Current; } }
			public int Position { get { return pos; } }
			public void Reset() { baseEnum.Reset(); pos = 0; }
			public void Dispose() { MFEnumerator.Dispose(baseEnum); }
			public bool MoveNext()
			{
				while (baseEnum.MoveNext())
				{
					IMFNode node = baseEnum.Current as IMFNode;
					if (node != null && (node.NodeKind & nodeKind) != 0 && node.NodeName == name)
					{
						pos++;
						return true;
					}
				}
				return false;
			}
		}
	}
}
