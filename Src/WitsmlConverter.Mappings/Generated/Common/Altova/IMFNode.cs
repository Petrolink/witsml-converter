using System;
using System.Collections;
using System.Text;

namespace Altova.Mapforce
{
	public enum MFQueryKind
	{
		All,
		AllChildren,
		AllAttributes,
		AttributeByQName,
		ChildrenByQName,
		SelfByQName,
		ChildrenByDbCommand,
		ChildrenByNodeName,
		AttributeByNodeName,
	}

	public enum MFNodeKind
	{
		Attribute = 1 << 0,
		Field = 1 << 1,
		Element = 1 << 2,
		Record = 1 << 3,
		Text = 1 << 4,
		CData = 1 << 5,
		Comment = 1 << 6,
		ProcessingInstruction = 1 << 7,
		Document = 1 << 8,
		Connection = 1 << 9,

		Children = Element|Text|CData|Comment|ProcessingInstruction,
		AllChildren = Children|Attribute
	}

	public interface IMFEnumerator : IEnumerator, IDisposable
	{
		int Position { get; }
	}
	
	public class MFEnumerator
	{
		public static void Dispose(IEnumerator e)
		{
			if (e == null)
				return;
			IDisposable i = e as IDisposable;
			if (i != null)
				i.Dispose();
		}
	}

	public interface IMFNode
	{
		MFNodeKind NodeKind { get; }
		string LocalName { get; }
		string NamespaceURI { get; }
		string Prefix { get; }
		string NodeName { get; }
		IEnumerable Select(MFQueryKind kind, object query);
		Altova.Types.QName GetQNameValue();
		object TypedValue { get; }
	}
	
	public interface IMFDocumentNode
	{
		string GetDocumentUri();
	}
	
	public class MFNode
	{
		public static string GetValue(object o)
		{
			if (o is Altova.Mapforce.IMFNode) 
			{
				Altova.Mapforce.IMFNode node = (Altova.Mapforce.IMFNode)o;
				string s = "";
				foreach (object v in node.Select(Altova.Mapforce.MFQueryKind.AllChildren, null))
				{
					s += GetValue(v);
				}
				return s;
					
			}
			return o.ToString();
		}

		static object UnboxTypedValue(object o)
		{
			if (o is IMFNode) return ((IMFNode)o).TypedValue;
			return o;
		}

		static void Add(ArrayList a, object o)
		{
			if (o is ICollection)
				a.AddRange((ICollection)o);
			else
				a.Add(o);
		}

		public static object CollectTypedValue(IEnumerable from)
		{
			IEnumerator en = from.GetEnumerator();
			if (!en.MoveNext())
				return null;
			object first = UnboxTypedValue(en.Current);
			if (!en.MoveNext())
				return first;

			ArrayList a = new ArrayList();
			Add(a, first);
			Add(a, en.Current);
			while (en.MoveNext())
				Add(a, en.Current);
			return a.ToArray();
		}
	}
	
	public class Group
	{
		private object key;
		private IEnumerable items;
		
		public Group (object k, IEnumerable i) 
		{
			key = k;
			items = i;
		}

		public object Key { get { return key; } }
		public IEnumerable Items { get { return items; } }
	}
	
	public delegate IEnumerable MFInvoke(object o);

	public class MFInvokeWithParams
	{
		public MFInvoke f;
		public delegate IEnumerable MFInvokeP(object o, params object[] p);

		public MFInvokeWithParams(MFInvokeP _f, params object[] p) { f = o => _f(o, p); }
		public IEnumerable Invoke(object o)
		{
			return f(o);
		}
	}
}
