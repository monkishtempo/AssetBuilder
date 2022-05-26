using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AssetBuilder.Reports
{
    public class BaseXmlObject
    {
        public virtual int ID { get; }
        public string Type { get; set; }

        public BaseXmlObject(XElement data)
        {
            var props = this.GetType().GetProperties();
            foreach (var field in data.Elements())
            {
                var match = props.FirstOrDefault(p => p.Name == field.Name.ToString().Replace("_x0020_", ""));
                if (match != null)
                {
                    if (match.PropertyType.IsEnum)
                    {
                        var value = Enum.ToObject(match.PropertyType, Convert.ChangeType(field.Value, typeof(int)));
                        match.SetValue(this, value);
                    }
                    else
                    {
                        var value = Convert.ChangeType(field.Value, match.PropertyType);
                        match.SetValue(this, value);
                    }
                }
            }
        }

        public void Munge(BaseXmlObject obj, string suffix = "_Language")
        {
            Type type = this.GetType();
            var props = type.GetProperties().Where(f => f.Name.EndsWith(suffix))
                .Select(f => new { source = type.GetProperty(f.Name.Substring(0, f.Name.Length-suffix.Length)), target = f });
            foreach (var prop in props)
            {
                if (prop.target != null)
                {
                    var t = prop.source.GetValue(this);
                    var s = prop.source.GetValue(obj);
                    if((string)s != (string)t)
                        prop.target.SetValue(this, AssetBuilder.Controls.NLTest.GetNLConditionString((string)s));
                    if (t != null)
                    {
                        t = AssetBuilder.Controls.NLTest.GetNLConditionString((string)t);
                        prop.source.SetValue(this, t);
                    }
                }
            }
        }
    }
}
