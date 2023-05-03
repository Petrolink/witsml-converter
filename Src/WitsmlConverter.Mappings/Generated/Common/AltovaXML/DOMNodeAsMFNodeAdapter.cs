using System;
using System.Collections;
using System.Xml;
using Altova.Xml;

namespace Altova.Mapforce
{
	public class DOMNodeAsMFNodeAdapter : IMFNode
	{
		private readonly XmlNode node;

		public DOMNodeAsMFNodeAdapter(XmlNode node)
		{
			this.node = node;
		}
		
		public XmlNode Node { get { return node; } }

		public object TypedValue
		{
			get { return MFNode.CollectTypedValue(Select(MFQueryKind.AllChildren, null)); }
		}
		public IEnumerable Select(MFQueryKind kind, object query)
		{
			switch (kind)
			{
				case MFQueryKind.All:
					switch (node.NodeType)
					{
						case XmlNodeType.Element:
							return new SequenceJoin(new DOMChildrenAsMFNodeSequenceAdapter(node), new DOMAttributesAsMFNodeSequenceAdapter(node));
							
						case XmlNodeType.Document:
							return new DOMChildrenAsMFNodeSequenceAdapter(node);

						case XmlNodeType.Attribute:
							return new MFSingletonSequence(node.Value);

						case XmlNodeType.Text:
						case XmlNodeType.SignificantWhitespace:
						case XmlNodeType.CDATA:
							return new MFSingletonSequence(node.Value);
						default:
							return MFEmptySequence.Instance;

					}
					
				case MFQueryKind.AllChildren:
					switch (node.NodeType)
					{
						case XmlNodeType.Element:
						case XmlNodeType.Document:
							return new DOMChildrenAsMFNodeSequenceAdapter(node);

						case XmlNodeType.Attribute:
							return new MFSingletonSequence(node.Value);

						case XmlNodeType.Text:
						case XmlNodeType.SignificantWhitespace:
						case XmlNodeType.CDATA:
							return new MFSingletonSequence(node.Value);
							
						case XmlNodeType.Comment:
						case XmlNodeType.ProcessingInstruction:
							return new MFSingletonSequence(node.Value);
							
						default:
							return MFEmptySequence.Instance;

					}

				case MFQueryKind.AllAttributes:
					switch (node.NodeType)
					{
						case XmlNodeType.Element:
						case XmlNodeType.Document:
							return new DOMAttributesAsMFNodeSequenceAdapter(node);

						default:
							return MFEmptySequence.Instance;

					}

				case MFQueryKind.ChildrenByQName:
					switch (node.NodeType)
					{
						case XmlNodeType.Element:
						case XmlNodeType.Document:
							return new MFNodeByKindAndQNameFilter(new DOMChildrenAsMFNodeSequenceAdapter(node), MFNodeKind.Element, ((XmlQualifiedName)query).Name, ((XmlQualifiedName)query).Namespace);

						default:
							return MFEmptySequence.Instance;

					}

				case MFQueryKind.ChildrenByNodeName:
					switch (node.NodeType)
					{
						case XmlNodeType.Element:
						case XmlNodeType.Document:
							return new MFNodeByKindAndNodeNameFilter(new DOMChildrenAsMFNodeSequenceAdapter(node), MFNodeKind.Element, (string)query);

						default:
							return MFEmptySequence.Instance;

					}

				case MFQueryKind.SelfByQName:
					switch (node.NodeType)
					{
						case XmlNodeType.Element:
						case XmlNodeType.Attribute:
							if (node.LocalName == ((XmlQualifiedName)query).Name &&
								node.NamespaceURI == ((XmlQualifiedName)query).Namespace)
								return new MFSingletonSequence(this);
							else
								return MFEmptySequence.Instance;

						default:
							return MFEmptySequence.Instance;

					}

				case MFQueryKind.AttributeByQName:
					switch (node.NodeType)
					{
						case XmlNodeType.Element:
							{
								XmlAttribute att = node.Attributes[((XmlQualifiedName)query).Name, ((XmlQualifiedName)query).Namespace];
								if (att != null)
									return new MFSingletonSequence(new DOMNodeAsMFNodeAdapter(att));
								else
									return MFEmptySequence.Instance;
							}

						default:
							return MFEmptySequence.Instance;

					}


				default:
					throw new InvalidOperationException("Unsupported query type.");
			}
		}

		public MFNodeKind NodeKind
		{
			get
			{
				switch (node.NodeType)
				{
					case XmlNodeType.Attribute:
						return MFNodeKind.Attribute; // also field?
					case XmlNodeType.CDATA:
						return MFNodeKind.Text | MFNodeKind.CData;
					case XmlNodeType.Comment:
						return MFNodeKind.Comment;
					case XmlNodeType.Document:
						return MFNodeKind.Document;
					case XmlNodeType.Element:
						return MFNodeKind.Element;
					case XmlNodeType.Text:
					case XmlNodeType.SignificantWhitespace:
						return MFNodeKind.Text;
					case XmlNodeType.ProcessingInstruction:
						return MFNodeKind.ProcessingInstruction;
					default:
						return 0;
				}
			}
		}

		public string LocalName
		{
			get { return node.NodeType == XmlNodeType.Document || node.NodeType == XmlNodeType.Text || node.NodeType == XmlNodeType.CDATA ? "" : node.LocalName; }
		}

		public string NamespaceURI
		{
			get { return node.NamespaceURI; }
		}
		
		public string Prefix
		{
			get { return node.Prefix; }
		}

		public string NodeName { get { return node.Name; } }
		
		public Altova.Types.QName GetQNameValue() 
		{			
			return Altova.Xml.XmlTreeOperations.CastToQName(node, null);
		}
	}
	
	public static class MFDomWriter
	{
		public static void Write(IEnumerable what, XmlNode where)
		{
			XmlDocument owner = (where.NodeType == XmlNodeType.Document) ? where as XmlDocument : where.OwnerDocument;
			foreach (object o in what)
			{
				if (o is Altova.Mapforce.IMFNode)
				{
					Altova.Mapforce.IMFNode el = (Altova.Mapforce.IMFNode)o;
					if ((el.NodeKind & Altova.Mapforce.MFNodeKind.Element) != 0)
					{
						string prefix = null;
						XmlElement xel = null;

						if (el.Prefix == null) // lookup
						{
							var navigator = where.CreateNavigator();
							if (el.NamespaceURI != navigator.LookupNamespace(System.String.Empty))
								prefix = XmlTreeOperations.FindPrefixForNamespace(where, el.NamespaceURI);

							if (prefix == null || prefix == "")
								xel = owner.CreateElement(el.LocalName, el.NamespaceURI);
							else
								xel = owner.CreateElement(prefix, el.LocalName, el.NamespaceURI);
						}
						else if (el.Prefix == "")
							xel = owner.CreateElement(el.LocalName, el.NamespaceURI);
						else
							xel = owner.CreateElement(el.Prefix, el.LocalName, el.NamespaceURI);

						where.AppendChild(xel);
						
						Write(el.Select(Altova.Mapforce.MFQueryKind.All, null), xel);
					}
					else if ((el.NodeKind & Altova.Mapforce.MFNodeKind.Attribute) != 0)
					{
						if (el.NamespaceURI == "http://www.w3.org/XML/1998/namespace")
							((XmlElement)where).SetAttribute("xml:" + el.LocalName, GetValue(el, where));

						else if (el.NamespaceURI == "http://www.w3.org/2000/xmlns/")
							((XmlElement)where).SetAttribute((el.Prefix == null || el.Prefix == "") ? el.LocalName : el.Prefix + ":" + el.LocalName, GetValue(el, where));

						else
						{
							if (el.Prefix == null)
							{
								String prefix = XmlTreeOperations.FindPrefixForNamespace(where, el.NamespaceURI);
								if (prefix == null || prefix == "")
									prefix = GetPrefixForW3URIs( el.NamespaceURI, where );
								if (prefix == null || prefix == "")
									((XmlElement)where).SetAttribute(el.LocalName, el.NamespaceURI, GetValue(el, where));
								else
									SetAttribute(where, prefix, el.LocalName, el.NamespaceURI, GetValue(el, where));
							}
							else if (el.Prefix == "")
								((XmlElement)where).SetAttribute(el.LocalName, el.NamespaceURI, GetValue(el, where));
							else
								SetAttribute(where, el.Prefix, el.LocalName, el.NamespaceURI, GetValue(el, where));
						}
					}
					else if ((el.NodeKind & Altova.Mapforce.MFNodeKind.Comment) != 0)
					{
						where.AppendChild(owner.CreateComment(GetValue(el, where)));
					}
					else if ((el.NodeKind & Altova.Mapforce.MFNodeKind.ProcessingInstruction) != 0)
					{
						where.AppendChild(owner.CreateProcessingInstruction(el.LocalName, GetValue(el, where)));
					}
					else if ((el.NodeKind & Altova.Mapforce.MFNodeKind.CData) != 0)
					{
						where.AppendChild(owner.CreateCDataSection(GetValue(el, where)));
					}
					else if ((el.NodeKind & Altova.Mapforce.MFNodeKind.Text) != 0)
					{
						string s = GetValue(el, where);
						if (s != "")
							where.AppendChild(owner.CreateTextNode(s));
					}
				}
				else
				{
					string s = GetValue(o, where);
					if (s != "")
						where.AppendChild(owner.CreateTextNode(s));
				}
			}
		}

		private static string GetPrefixForW3URIs(String uri, XmlNode node)
		{
			if ( uri == "http://www.w3.org/2001/XMLSchema-instance" )
			{
				String existing = node.GetNamespaceOfPrefix("xsi");
				if (existing == null || existing == "")
					return "xsi";
			}
			else if ( uri == "http://www.w3.org/2001/XMLSchema" )
			{
				String existing = node.GetNamespaceOfPrefix("xs");
				if (existing == null || existing == "")
					return "xs";
			}
			return "";
		}

		private static void SetAttribute (XmlNode node, String prefix, String localname, String namespaceUri, String value)
		{
			XmlElement xel = (XmlElement) node;
			if (prefix == null || prefix == "")
			{
				xel.SetAttribute(localname, namespaceUri, value);
				return;
			}

			String namespaceUriOfPrefix = xel.GetNamespaceOfPrefix(prefix);

			if (namespaceUriOfPrefix != namespaceUri)
				xel.SetAttribute("xmlns:" + prefix, namespaceUri);
				
			xel.SetAttribute(localname, namespaceUri, value);
		}
		
		public static string GetValue(object o, XmlNode n)
		{
			if (o is Altova.Types.QName)
			{
				Altova.Types.QName q = (Altova.Types.QName)o;
				if (q.Uri == null || q.Uri.Length == 0)
					return q.LocalName;

				String prefix = XmlTreeOperations.FindPrefixForNamespace(n, q.Uri);
				if (prefix == null || prefix.Length == 0)
				{
					if (q.Prefix != null && n.GetNamespaceOfPrefix(q.Prefix) == q.Uri)
						prefix = q.Prefix;
					else
					{
						prefix = GetPrefixForW3URIs( q.Uri, n );
						if ( prefix == null || prefix == "" )
							prefix = Altova.Xml.XmlTreeOperations.FindUnusedPrefix(n, q.Prefix);

						((XmlElement)n).SetAttribute("xmlns:" + prefix, q.Uri);
					}
				}

				if (prefix == null || prefix.Length == 0)
					return q.LocalName;
				
				int i = q.LocalName.IndexOf(':');
				if (i == -1)
					return prefix + ":" + q.LocalName;
				return prefix + ":" + q.LocalName.Substring(i+1);
			}
			if (o is Altova.Mapforce.IMFNode) 
			{
				Altova.Mapforce.IMFNode node = (Altova.Mapforce.IMFNode)o;

				string s = "";
				if ( ( node.NodeKind & Altova.Mapforce.MFNodeKind.Attribute ) != 0 )
				{
					bool isfirst = true;
					foreach (object v in node.Select(Altova.Mapforce.MFQueryKind.AllChildren, null))
					{
						if (isfirst)
							isfirst = false;
						else
							s += " ";
						s += GetValue(v, n);
					}
				}
				else
				{
					foreach (object v in node.Select(Altova.Mapforce.MFQueryKind.AllChildren, null))
						s += GetValue(v, n);
				}
				return s;
			}
			return o.ToString();
		}
	}
	
	public class DOMDocumentNodeAsMFNodeAdapter : DOMNodeAsMFNodeAdapter, IMFDocumentNode
	{
		private readonly string filename;

		public DOMDocumentNodeAsMFNodeAdapter(XmlNode document, string filename)
			: base(document)
		{
			this.filename = filename;
		}

		public string GetDocumentUri()
		{
			return filename;
		}
	}
}
