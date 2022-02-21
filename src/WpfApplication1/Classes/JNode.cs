using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Web.Script.Serialization;

namespace AssetBuilder
{
    [DebuggerDisplay("{ToString()}")]
    public class JNode : IEnumerable<JNode>
    {
        private readonly Dictionary<string, object> _Dictionary;
        private readonly ArrayList _Array;
        //private readonly object[] _Objects;
        private readonly string _Value;

        private readonly string _Key;
        public string Key { get { return _Key; } }

        private readonly int _Index;
        public int Index { get { return _Index; } }

        private readonly JNode _Parent;
        public JNode Parent { get { return _Parent; } }

        public bool IsDictionary { get { return _Dictionary != null; } }
        public bool IsArray { get { return _Array != null; } }
        public bool IsValue { get { return !IsDictionary && !IsArray; } }
        public bool HasValue { get { return _Value != null; } }
        public string Value { get { return _Value; } }

        public int Count
        {
            get
            {
                if (IsDictionary) return _Dictionary.Count;
                if (IsArray) return _Array.Count;
                return 0;
            }
        }

        public int Length
        {
            get
            {
                if (IsArray) return _Array.Count;
                if (HasValue) return _Value.Length;
                return 0;
            }
        }

        public string[] Keys
        {
            get
            {
                if (IsDictionary)
                {
                    return _Dictionary.Select(f => f.Key).ToArray();
                }
                return new string[0];
            }
        }

        public JNode[] Values
        {
            get
            {
                if (IsArray)
                {
                    int i = 0;
                    return (from object o in _Array select this[i++]).ToArray();
                }
                if (IsDictionary)
                    return _Dictionary.Select(f => this[f.Key]).ToArray();
                if (IsValue) return new JNode[0]; // new JNode[] { this };
                return new JNode[0];
            }
        }

        public static JNode FromData(Dictionary<string, List<Dictionary<string, object>>> data)
        {
            return new JNode(data.ToDictionary((x) => x.Key, (x) => (object)new ArrayList(x.Value)));
        }

        public static JNode FromObject(object o)
        {
            var dict = new Dictionary<string, object>();

            if (o != null)
            {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(o))
                {
                    object obj2 = descriptor.GetValue(o);
                    dict.Add(descriptor.Name, obj2);
                }
            }

            return new JNode(dict);
        }

        public static JNode FromArray(IEnumerable o)
        {
            var array = new ArrayList();

            if (o != null)
            {
                foreach (var a in o)
                {
                    array.Add(a);
                }
            }

            return new JNode(array);
        }

        public static JNode CreateFromJson(string json)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            string jsonTrim;
            if (json != null && (jsonTrim = json.Trim()).Length > 0)
            {
                if (jsonTrim[0] == '{')
                    return new JNode(js.Deserialize<Dictionary<string, object>>(json));
                else if (jsonTrim[0] == '[')
                    return new JNode(js.Deserialize<ArrayList>(json));
                else
                    return new JNode((object)json);
            }
            return new JNode((object)null);
        }

        public JNode(Dictionary<string, object> obj)
        {
            _Index = -1;
            _Dictionary = obj;
        }

        private JNode(object o, JNode parent = null, int index = -1, string key = null)
        {
            _Parent = parent;
            _Index = index;
            _Key = key;
            if (o is Dictionary<string, object>) _Dictionary = o as Dictionary<string, object>;
            else if (o is ArrayList) _Array = o as ArrayList;
            else if (o != null) _Value = o.ToString();
        }

        public JNode this[string s]
        {
            get
            {
                if (IsDictionary && _Dictionary.ContainsKey(s)) return new JNode(_Dictionary[s], parent: this, key: s);
                return new JNode((object)null);
            }
        }

        public JNode this[int i]
        {
            get
            {
                if (IsArray && i >= 0 && _Array.Count > i) return new JNode(_Array[i], parent: this, key: $"{Key}[{i}]", index: i);
                return new JNode((object)null);
            }
        }



        public static implicit operator bool(JNode d)
        {
            bool b = false;
            bool.TryParse(d, out b);
            return b;
        }

        public static implicit operator string(JNode d)
        {
            return d == null ? null : d._Value;
        }

        public static implicit operator int(JNode d)
        {
            int r = 0;
            int.TryParse(d, out r);
            return r;
        }

        public static implicit operator Dictionary<string, object>(JNode d)
        {
            return d._Dictionary ?? new Dictionary<string, object>();
        }

        public static bool operator ==(JNode d1, JNode d2)
        {
            if (object.ReferenceEquals(d1, null))
            {
                if (object.ReferenceEquals(d2, null)) return true;
                return d2.Equals(d1);
            }
            return d1.Equals(d2);
        }

        public static bool operator !=(JNode d1, JNode d2)
        {
            return !(d1 == d2);
        }

        public override string ToString()
        {
            if (IsDictionary) return $"{(Index >= 0 ? $"[{Index}] " : "")}" + string.Join(", ", _Dictionary.Select(f => $"[{f.Key}: {f.Value}]"));
            if (IsArray) return $"{(Index >= 0 ? $"[{Index}] " : "")}{Key}: [{_Array.Count}]";
            if (IsValue) return $"{(Index >= 0 ? $"[{Index}] " : "")}[{Key}: {(HasValue ? _Value : "null")}]";

            return null;
        }

        public string ToJson()
        {
            if (IsDictionary) return new JavaScriptSerializer().Serialize(_Dictionary);
            if (IsArray) return new JavaScriptSerializer().Serialize(_Array);
            if (IsValue) return new JavaScriptSerializer().Serialize(_Value);

            return null;
        }

        public override int GetHashCode()
        {
            if (IsDictionary) return _Dictionary.GetHashCode();
            if (IsArray) return _Array.GetHashCode();
            if (HasValue) return _Value.GetHashCode();

            return base.GetHashCode();
        }

        public override bool Equals(object o)
        {
            if (o is JNode)
            {
                var dav = o as JNode;
                if (dav.IsDictionary) return this.Equals(dav._Dictionary);
                if (dav.IsArray) return this.Equals(dav._Array);
                return this.Equals(dav._Value);
            }
            if (object.ReferenceEquals(o, null))
            {
                if (this.IsDictionary) return false;
                if (this.IsArray) return false;
                return this._Value == null;
            }

            if (o is Dictionary<string, object>) return _Dictionary == o as Dictionary<string, object>;
            if (o is ArrayList) return _Array == o as ArrayList;
            if (o is string) return _Value == o as string;

            return base.Equals(o);
        }

        public IEnumerator<JNode> GetEnumerator()
        {
            if (IsArray)
                for (int i = 0; i < _Array.Count; i++)
                {
                    yield return this[i];
                }
            if (IsDictionary)
                foreach (var key in Keys)
                {
                    yield return this[key];
                }
            //if (IsValue) yield return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
