using System;
using System.Collections;
using System.Text;

namespace Altova.Mapforce
{
	public class MFDocument : IMFNode, IMFDocumentNode
	{
		private readonly string filename;
		private readonly IEnumerable children;

		public MFDocument(string filename, IEnumerable children)
		{
			this.filename = filename;
			this.children = children;
		}
		
		public string GetDocumentUri()
		{
			return filename;
		}

		public string LocalName { get { return ""; } }
		public string NamespaceURI { get { return ""; } }
		public string Prefix { get { return ""; } }
		public string NodeName { get { return ""; } }
		public MFNodeKind NodeKind { get { return MFNodeKind.Document; } }
		public string Filename { get { return filename; } }

		public IEnumerable Select(MFQueryKind kind, object query)
		{
			switch (kind)
			{
				case MFQueryKind.All:
					return children;
				default:
					throw new InvalidOperationException("Unsupported query type.");
			}
		}

		public Altova.Types.QName GetQNameValue()
		{
			return null;
		}

	public object TypedValue { get { return null; } }
	}

	public class MFElement : IMFNode
	{
		private readonly string namespaceURI;
		private readonly string nodeName;
		private IEnumerable children;
		private ArrayList childrenCache;

		public MFElement(string localName, string namespaceURI, string prefix, IEnumerable children)
		{
			this.namespaceURI = namespaceURI;
			this.nodeName = prefix == null || prefix == "" ? localName : prefix + ":" + localName;
			this.children = children;
		}

		public MFElement(string nodeName, IEnumerable children)
		{
			this.nodeName = nodeName;
			this.children = children;
		}

		public string LocalName { get {
			int sep = nodeName.IndexOf(':');
			if (sep > 0)
				return nodeName.Substring(sep + 1);
			return nodeName;
		} }
		public string NamespaceURI { get { return namespaceURI; } }
		public string Prefix { get {
			int sep = nodeName.IndexOf(':');
			if (sep > 0)
				return nodeName.Substring(0, sep);
			return "";
		} }
		public string NodeName { get { return nodeName; } }
		
		public MFNodeKind NodeKind { get { return MFNodeKind.Element; } }
		
		void createCache()
		{
			if (childrenCache == null)
			{
				childrenCache = new ArrayList();
				foreach (object o in children)
					childrenCache.Add(o);
				children = childrenCache;
			}
		}
		
		public IEnumerable Select(MFQueryKind kind, object query)
		{
			switch (kind)
			{
				case MFQueryKind.All:
					return new MFNodeByKindFilter(children, MFNodeKind.AllChildren);
				case MFQueryKind.AllChildren:
					createCache();
					return new MFNodeByKindFilter(children, MFNodeKind.Children);
				case MFQueryKind.AllAttributes:
					createCache();
					return new MFNodeByKindFilter(children, MFNodeKind.Attribute | MFNodeKind.Field);
				
				case MFQueryKind.AttributeByQName:
					createCache();
					return new MFNodeByKindAndQNameFilter(children, MFNodeKind.Attribute|MFNodeKind.Field,
						((System.Xml.XmlQualifiedName)query).Name,
						((System.Xml.XmlQualifiedName)query).Namespace);

				case MFQueryKind.ChildrenByQName:
					createCache();
					return new MFNodeByKindAndQNameFilter(children, MFNodeKind.Element,
						((System.Xml.XmlQualifiedName)query).Name,
						((System.Xml.XmlQualifiedName)query).Namespace);

				case MFQueryKind.ChildrenByNodeName:
					createCache();
					return new MFNodeByKindAndNodeNameFilter(children, MFNodeKind.Element, (string)query);

				case MFQueryKind.AttributeByNodeName:
					createCache();
					return new MFNodeByKindAndNodeNameFilter(children, MFNodeKind.Attribute|MFNodeKind.Field, (string)query);

				case MFQueryKind.SelfByQName:
					if (LocalName == ((System.Xml.XmlQualifiedName)query).Name &&
						namespaceURI == ((System.Xml.XmlQualifiedName)query).Namespace)
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
	
	public class MFComment : IMFNode
	{
		private readonly string content;

		public MFComment(string content)
		{
			this.content = content;
		}

		public MFNodeKind NodeKind { get { return MFNodeKind.Comment; } }

		public string LocalName { get { return ""; } }
		public string NamespaceURI { get { return ""; } }
		public string Prefix { get { return ""; } }
		public string NodeName { get { return "comment"; } }

		void createCache()
		{
		}

		public IEnumerable Select(MFQueryKind kind, object query)
		{
			return new MFSingletonSequence(content);
		}

		public Altova.Types.QName GetQNameValue()
		{
			throw new Exception("Trying to read QName of a comment.");
		}

		public object TypedValue
		{
			get { return MFNode.CollectTypedValue(Select(MFQueryKind.AllChildren, null)); }
		}
	}
	
	public class MFProcessingInstruction : IMFNode
	{
		private readonly string name;
		private readonly string content;
		
		public MFProcessingInstruction(string name, string content)
		{
			this.name = name;
			this.content = content;
		}

		public MFNodeKind NodeKind { get { return MFNodeKind.ProcessingInstruction; } }

		public string LocalName { get { return name; } }
		public string NamespaceURI { get { return ""; } }
		public string Prefix { get { return ""; } }
		public string NodeName { get { return "PI"; } }

		void createCache()
		{
		}

		public IEnumerable Select(MFQueryKind kind, object query)
		{
			return new MFSingletonSequence(content);
		}

		public Altova.Types.QName GetQNameValue()
		{
			throw new Exception("Trying to read QName of a processing instruction.");
		}

		public object TypedValue
		{
			get { return MFNode.CollectTypedValue(Select(MFQueryKind.AllChildren, null)); }
		}
	}

	public class MFCData : IMFNode
	{
		private readonly string content;

		public MFCData(string content)
		{
			this.content = content;
		}

		public MFNodeKind NodeKind { get { return MFNodeKind.CData; } }

		public string LocalName { get { return ""; } }
		public string NamespaceURI { get { return ""; } }
		public string Prefix { get { return ""; } }
		public string NodeName { get { return "cdata"; } }

		void createCache()
		{
		}

		public IEnumerable Select(MFQueryKind kind, object query)
		{
			return new MFSingletonSequence(content);
		}

		public Altova.Types.QName GetQNameValue()
		{
			throw new Exception("Trying to read QName of a cdata.");
		}

		public object TypedValue
		{
			get { return MFNode.CollectTypedValue(Select(MFQueryKind.AllChildren, null)); }
		}
	}
}
