using System;
using System.Collections;
using System.Text;

namespace Altova.Mapforce
{
	public class MFAttribute : IMFNode
	{
		private readonly string nodeName;
		private readonly string namespaceURI;
		private readonly IEnumerable children;

		public MFAttribute(string localName, string namespaceURI, string prefix, IEnumerable children)
		{
			this.nodeName = prefix == null || prefix == "" ? localName : prefix + ":" + localName;
			this.namespaceURI = namespaceURI;
			this.children = children;
		}

		public MFAttribute(string nodeName, IEnumerable children)
		{
			this.nodeName = nodeName;
			this.children = children;
		}

		public string LocalName { get { int sep = nodeName.IndexOf(':'); return sep > 0 ? nodeName.Substring(sep + 1) : nodeName; } }
		public string NamespaceURI { get { return namespaceURI; } }
		public string Prefix { get { int sep = nodeName.IndexOf(':'); return sep > 0 ? nodeName.Substring(0, sep) : ""; } }
		public string NodeName { get { return nodeName; } }

		public MFNodeKind NodeKind { get { return MFNodeKind.Attribute | MFNodeKind.Field; } }
		
		public IEnumerable Select(MFQueryKind kind, object query)
		{
			switch (kind)
			{
				case MFQueryKind.All:
				case MFQueryKind.AllChildren:
					return new MFNodeByKindFilter(children, MFNodeKind.Text);
				
				case MFQueryKind.AllAttributes:
				case MFQueryKind.AttributeByQName:
					return MFEmptySequence.Instance;

				case MFQueryKind.ChildrenByQName:
				case MFQueryKind.AttributeByNodeName:
				case MFQueryKind.ChildrenByNodeName:
					return MFEmptySequence.Instance;

				case MFQueryKind.SelfByQName:
					if (LocalName == ((System.Xml.XmlQualifiedName)query).Name &&
						NamespaceURI == ((System.Xml.XmlQualifiedName)query).Namespace)
						return new MFSingletonSequence(this);
					else
						return MFEmptySequence.Instance;

				default:
					throw new InvalidOperationException("Unsupported query type.");
			}
		}

		public Altova.Types.QName GetQNameValue() 
		{
			IEnumerable children = Select(MFQueryKind.AllChildren, null);

			IEnumerator en = children.GetEnumerator();
			if (!en.MoveNext())
				throw new Exception("Trying to convert NULL to QName.");

			Altova.Types.QName qn;
			if (en.Current is IMFNode)
				qn = ((IMFNode)en.Current).GetQNameValue();
			else
				qn = (Altova.Types.QName)en.Current;

			if (en.MoveNext())
				throw new Exception("Trying to convert multiple values to QName.");

			return qn;
		}
		
		public object TypedValue
		{
			get { return MFNode.CollectTypedValue(Select(MFQueryKind.AllChildren, null)); }
		}
	}
}
