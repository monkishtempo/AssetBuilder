using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AssetBuilder.Reports
{
    internal class AssetReportContent
    {
        Dictionary<string, Dictionary<int, BaseXmlObject>> objects = new Dictionary<string, Dictionary<int, BaseXmlObject>>();

        public Dictionary<int, BaseXmlObject>.ValueCollection Algos => objects.ContainsKey("Algo") ? objects["Algo"].Values : null;
        public Dictionary<int, BaseXmlObject>.ValueCollection Questions => objects.ContainsKey("Question") ? objects["Question"].Values : null;
        public Dictionary<int, BaseXmlObject>.ValueCollection Answers => objects.ContainsKey("Answer") ? objects["Answer"].Values : null;
        public Dictionary<int, BaseXmlObject>.ValueCollection Conclusions => objects.ContainsKey("Conclusion") ? objects["Conclusion"].Values : null;
        public Dictionary<int, BaseXmlObject>.ValueCollection Bullets => objects.ContainsKey("Bullet") ? objects["Bullet"].Values : null;

        public bool HasAlgos => Algos?.Any() ?? false;
        public bool HasQuestions => Questions?.Any() ?? false;
        public bool HasAnswers => Answers?.Any() ?? false;
        public bool HasConclusions => Conclusions?.Any() ?? false;
        public bool HasBullets => Bullets?.Any() ?? false;

        public AssetReportContent(XElement baseContent, XElement languageContent)
        {
            foreach (var item in baseContent.Elements())
            {
                AddItem(item);
            }
            if (languageContent != null)
            {
                foreach (var item in languageContent.Elements())
                {
                    AddItemLanguage(item);
                }
            }
            else
            {
                foreach (var item in objects.SelectMany(f => f.Value.Values))
                {
                    item.NLExpand();
                }
            }
        }

        private void AddItemLanguage(XElement item)
        {
            BaseXmlObject obj = GetObject(item);
            if (objects.ContainsKey(obj.Type) && objects[obj.Type].ContainsKey(obj.ID)) 
            {
                objects[obj.Type][obj.ID].Munge(obj);
            }
            //objects.
        }

        void AddItem(XElement item)
        {
            BaseXmlObject obj = GetObject(item);
            if (obj != null)
            {
                if (!objects.ContainsKey(obj.Type)) objects.Add(obj.Type, new Dictionary<int, BaseXmlObject>() { { obj.ID, obj } });
                else objects[obj.Type].Add(obj.ID, obj);
            }
        }

        private BaseXmlObject GetObject(XElement item)
        {
            BaseXmlObject obj = null;
            var name = item.Name.ToString();
            if (name == "Table") obj = new ARAlgo(item);
            else if (name == "Table1") obj = new ARQuestion(item);
            else if (name == "Table2") obj = new ARAnswer(item);
            else if (name == "Table3") obj = new ARConclusion(item);
            else if (name == "Table4") obj = new ARBullet(item);
            return obj;
        }
    }
}
