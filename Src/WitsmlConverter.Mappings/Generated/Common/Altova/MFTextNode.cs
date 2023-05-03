using System;
using System.Collections;
using System.Text;

namespace Altova.Mapforce
{
	public class MFTextNode : IMFNode
	{
		MFNodeKind kind;
		IEnumerable children;

		public MFTextNode(bool cdata, IEnumerable children)
		{
			this.kind = MFNodeKind.Text | (cdata ? MFNodeKind.CData : 0);
			this.children = children;
		}

		public string LocalName { get { return ""; } }
		public string NamespaceURI { get { return ""; } }
		public string Prefix { get { return ""; } }
		public MFNodeKind NodeKind { get { return kind; } }
		public string NodeName { get { return ""; } }
		
		public IEnumerable Select(MFQueryKind kind, object query)
		{
			switch (kind)
			{
				case MFQueryKind.All:
				case MFQueryKind.AllChildren:
					return new MFNodeByKindFilter(children, MFNodeKind.Text);
				
				case MFQueryKind.AttributeByQName:
					return MFEmptySequence.Instance;

				case MFQueryKind.ChildrenByQName:
					return MFEmptySequence.Instance;

				case MFQueryKind.SelfByQName:
					return MFEmptySequence.Instance;

				default:
					throw new InvalidOperationException("Unsupported query type.");
			}
		}
		
		public Altova.Types.QName GetQNameValue() {return null;}
		public object TypedValue
		{
			get { return MFNode.CollectTypedValue(Select(MFQueryKind.AllChildren, null)); }
		}
	}
}
