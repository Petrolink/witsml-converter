using System;
using System.Collections;
using System.Xml;

namespace Altova.Mapforce
{
	internal class DOMChildrenAsMFNodeSequenceAdapter : IEnumerable
	{
		private readonly XmlNode from;

		public DOMChildrenAsMFNodeSequenceAdapter(XmlNode from)
		{
			this.from = from;
		}

		public IEnumerator GetEnumerator()
		{
			return new Enumerator(from);
		}

		public class Enumerator : IMFEnumerator
		{
			private readonly Stack enumStack = new Stack();
			private int pos = 0;

			public Enumerator(XmlNode from)
			{
				enumStack.Push(from.ChildNodes.GetEnumerator());
			}

			public object Current
			{
				get
				{
					if (enumStack == null)
						throw new InvalidOperationException("No current.");

					return new DOMNodeAsMFNodeAdapter((XmlNode)((IEnumerator)enumStack.Peek()).Current);
				}
			}
			
			public int Position { get { return pos; } }

			public bool MoveNext()
			{
				while (true)
				{
					IEnumerator en = (IEnumerator)enumStack.Peek();
					if (!en.MoveNext())
					{
						if (enumStack.Count > 1)
							enumStack.Pop();
						else
							return false;
					}
					else
					{
						XmlNode node = (XmlNode)en.Current;
						if (node.NodeType == XmlNodeType.EntityReference)
							enumStack.Push(node.ChildNodes.GetEnumerator());
						else
						{
							++pos;
							return true;
						}
					}
				}
			}

			public void Reset()
			{
				//current = null;
				while (enumStack.Count > 1)
					enumStack.Pop();
				((IEnumerator)enumStack.Peek()).Reset();
				pos = 0;
			}
			
			public void Dispose() {}
		}
	}

	internal class DOMAttributesAsMFNodeSequenceAdapter : IEnumerable
	{
		private readonly XmlNode from;

		public DOMAttributesAsMFNodeSequenceAdapter(XmlNode from)
		{
			this.from = from;
		}

		public IEnumerator GetEnumerator()
		{
			return new Enumerator(from);
		}

		public class Enumerator : IMFEnumerator
		{
			private readonly XmlNode from;
			private int current = -1;
			private int pos = 0;

			public Enumerator(XmlNode from)
			{
				this.from = from;
			}

			public object Current
			{
				get
				{
					if (current == -1) throw new InvalidOperationException("No current.");
					return new DOMNodeAsMFNodeAdapter(from.Attributes[current]);
				}
			}

			public int Position { get { return pos; } }

			public bool MoveNext()
			{
				++current;
				if(current < from.Attributes.Count)
				{
					pos++;
					return true;
				}
				return false;
			}

			public void Reset()
			{
				current = 0;
				pos = 0;
			}
			
			public void Dispose() {}
		}
	}
}
