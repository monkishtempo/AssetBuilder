using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

namespace NaturalLanguageWizard
{          
    public static class AssetReader
    {        
        private static XmlDocument getAlgosDoc;
        private static XmlDocument getAssetDoc;

        static AssetReader()
        {
            SetUpAssetDoc();

            SetUpAlgosDoc();
        }

        private static void SetUpAssetDoc()
        {
            getAssetDoc = new XmlDocument();
            var root = getAssetDoc.CreateNode(XmlNodeType.Element, "root", "");
            var asset = getAssetDoc.CreateNode(XmlNodeType.Element, "Asset", "");
            var ID = getAssetDoc.CreateAttribute("ID");
            var Type = getAssetDoc.CreateAttribute("Type");            
            asset.Attributes.Append(ID);
            asset.Attributes.Append(Type);
            root.AppendChild(asset);
            getAssetDoc.AppendChild(root);
        }

        private static void SetUpAlgosDoc()
        {
            getAlgosDoc = new XmlDocument();
            var root = getAlgosDoc.CreateNode(XmlNodeType.Element, "root", "");
            var algos = getAlgosDoc.CreateNode(XmlNodeType.Element, "GetAlgos", "");
            root.AppendChild(algos);
            getAlgosDoc.AppendChild(root);
        }

        private class NodeDescriptor
        {            
            public NodeType Type { get; set; }
            public string Name { get; set; }
        }

        [Flags]
        private enum NodeType
        {
            ID = 1,
            Description = 2,
            Type = 4,
            DescriptionAndType = Description | Type
        }

        private static bool ContainsNodeType(this NodeType haystack, NodeType needle)
        {
            return (haystack & needle) == needle;
        }

        private static bool ContainsNodeType(this NodeDescriptor[] nodes, NodeType type)
        {
            return nodes.Any(n => n.Type == type);
        }

        private static NodeDescriptor CreateNodeDescriptor(string name, NodeType type)
        {
            return new NodeDescriptor { Name = name, Type = type };
        }

        private static string GetNodeValue(this XPathNavigator xpn, NodeType type, params NodeDescriptor[] nodes)
        {
            try
            {
                var nodeName = nodes.Single(n => n.Type == type).Name;

                return xpn.GetNodeValue(nodeName);
            }
            catch
            {
                throw new Exception("No node of type " + type);
            }
        }

        private static string GetNodeValue(this XPathNavigator xpn, string nodeName)
        {
            return xpn.SelectSingleNode(nodeName).Value;
        }

        private delegate T CreateAssetDelegate<T>(XPathNavigator xpn, int id, params NodeDescriptor[] nodes) where T : Asset, new();        

        /// <summary>
        /// Default creates asset with an ID and a description.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xpn"></param>
        /// <param name="id"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        private static T CreateAsset<T>(XPathNavigator xpn, int id, params NodeDescriptor[] nodes)
            where T : Asset, new()
        {
            var asset = new T();

            asset.ID = id;

            if (nodes.ContainsNodeType(NodeType.Description))
            {
                asset.Description = xpn.GetNodeValue(NodeType.Description, nodes);
            }

            return asset;
        }                    
        
        private static IEnumerable<T> GetAssets<T>(XmlNode xn, AssetInfoTable table, 
            CreateAssetDelegate<T> CreateAsset, NodeDescriptor idNode, params NodeDescriptor[] nodes)
            where T : Asset, new()
        {
            var navigator = xn.CreateNavigator();

            var path = "//Table" + (table != null ? table : null);

            var iterator = navigator.Select(path);

            var assets = new List<T>();

            while (iterator.MoveNext())
            {
                var current = iterator.Current;

                var id = int.Parse(current.GetNodeValue(NodeType.ID, idNode));

                if (!assets.Exists(a => a.ID == id))
                {
                    var asset = CreateAsset(current, id, nodes);

                    assets.Add(asset);
                }
            }

            return assets.OrderBy(a => a.ID.ToString());
        }       

        private static XmlNode GetAssetInfo(XmlNode doc)
        {
            return AssetBuilder.DataAccess.getDataNode("nl_AssetInfo", new string[] { "@xml", doc.OuterXml }, false);
        }

        private static XmlNode GetAssetDoc(int id, AssetType type)
        {
            var doc = getAssetDoc.Clone();

            var assetNode = doc.FirstChild.FirstChild;

            assetNode.Attributes["ID"].Value = id.ToString();
            assetNode.Attributes["Type"].Value = type.ToString();

            return doc;
        }

        public static IEnumerable<Algo> GetAlgos()
        {
            var xn = GetAssetInfo(getAlgosDoc);

            var idNode = CreateNodeDescriptor("AlgoID", NodeType.ID);
            var descriptionNode = CreateNodeDescriptor("Algo_Name", NodeType.Description);

            return GetAssets<Algo>(xn, AssetInfoTable.Algo, CreateAlgo, idNode, descriptionNode);
        }

        private static Algo CreateAlgo(XPathNavigator xpn, int id, params NodeDescriptor[] nodes)
        {
            return CreateAsset<Algo>(xpn, id, nodes);
        }

        public static IEnumerable<Question> GetQuestions(int? algoID)        
        {
            if (algoID == null)
            {
                return null;
            }

            var doc = GetAssetDoc(algoID.Value, AssetType.Algo);            

            var xn = GetAssetInfo(doc);

            var idNode = CreateNodeDescriptor("QuestionID", NodeType.ID);
            var descriptionNode = CreateNodeDescriptor("Clinical_Statement", NodeType.Description);

            return GetAssets<Question>(xn, AssetInfoTable.AlgoQuestion, CreateQuestion, idNode, descriptionNode);
        }        

        private static Question CreateQuestion(XPathNavigator n, int id, params NodeDescriptor[] nodes)
        {
            var question = CreateAsset<Question>(n, id, nodes);

            if (nodes.ContainsNodeType(NodeType.Type))
            {
                question.Type = Enums<QuestionType>.Parse(n.GetNodeValue(NodeType.Type, nodes));
            }

            return question;
        }

        public static Question TryGetQuestion(int questionID)
        {
            var doc = GetAssetDoc(questionID, AssetType.Question);

            var xn = GetAssetInfo(doc);

            var idNode = CreateNodeDescriptor("id", NodeType.ID);
            var descriptionNode = CreateNodeDescriptor("Clinical_Statement", NodeType.Description);
            var typeNode = CreateNodeDescriptor("NodeTypeID", NodeType.Type);

            return GetAssets<Question>(xn, AssetInfoTable.Question, CreateQuestion, idNode, descriptionNode, typeNode).FirstOrDefault();
        }

        public static Conclusion TryGetConclusion(int conclusionID)
        {
            var doc = GetAssetDoc(conclusionID, AssetType.Conclusion);

            var xn = GetAssetInfo(doc);

            var idNode = CreateNodeDescriptor("id", NodeType.ID);
            var descriptionNode = CreateNodeDescriptor("Possible_Condition", NodeType.Description);

            return GetAssets<Conclusion>(xn, null, CreateConclusion, idNode, descriptionNode).FirstOrDefault();
        }        

        public static IEnumerable<Conclusion> GetConclusions(int? algoID)
        {
            if (algoID == null)
            {
                return null;
            }

            var doc = GetAssetDoc(algoID.Value, AssetType.Algo);

            var xn = GetAssetInfo(doc);

            var idNode = CreateNodeDescriptor("RecID", NodeType.ID);
            var descriptionNode = CreateNodeDescriptor("Possible_Condition", NodeType.Description);

            return GetAssets<Conclusion>(xn, AssetInfoTable.AlgoConclusion, CreateAsset<Conclusion>, idNode, descriptionNode);
        }

        public static IEnumerable<Answer> GetAnswers(int questionID)
        {
            var doc = GetAssetDoc(questionID, AssetType.Question);            

            var xn = GetAssetInfo(doc);

            var idNode = CreateNodeDescriptor("AnsID", NodeType.ID);            
            var descriptionNode = CreateNodeDescriptor("Clinical_Answer", NodeType.Description);
            var typeNode = CreateNodeDescriptor("AnswerTypeID", NodeType.Type);

            return GetAssets<Answer>(xn, AssetInfoTable.Question, CreateAnswer, idNode, descriptionNode, typeNode);
        }        

        public static char GetQuestionChar(this Question question)
        {
            var type = question.Type;

            switch (type)
            {
                case QuestionType.SingleAnswer:
                case QuestionType.MultipleAnswer:
                case QuestionType.ImageQuestion:
                    return 'a';

                case QuestionType.FreeText:
                    return 's';

                case QuestionType.SingleTable:
                case QuestionType.MultipleTable:
                    return 't';

				case QuestionType.ValueEntry:
				case QuestionType.Calculated:
					return GetValueQuestionChar(question.ID);

                default:
                    throw new Exception("Unrecognised question type");
            }
        }

        private static char GetValueQuestionChar(int questionID)
        {
            var answers = GetAnswers(questionID);

            // ignore any Value Calculated answers
            answers = answers.Where(a => a.Type != AnswerType.ValueCalculated);

            // get distinct types
            var types = (from a in answers select a.Type).Distinct();

			if (types.Count() == 0)
			{
				return 'v'; // default to value
			}


            if (types.Count() > 1)
            {
                return 's'; // default to string
            }

            else
            {
                var type = types.Single();

                switch (type)
                {
                    case AnswerType.ValueEnter:
                        return 'v';

                    case AnswerType.StringEnter:
                        return 's';

                    case AnswerType.DateEnter:
                        return 'd';

                    default:
                        throw new Exception("Unknown answer type" + type);
                }
            }
        }

        private static Answer CreateAnswer(XPathNavigator n, int id, params NodeDescriptor[] nodes)
        {
            var answer = CreateAsset<Answer>(n, id, nodes);            

            if (nodes.ContainsNodeType(NodeType.Type))
            {
                answer.Type = Enums<AnswerType>.Parse(n.GetNodeValue(NodeType.Type, nodes));
            }

            return answer;
        }

        private static Conclusion CreateConclusion(XPathNavigator n, int id, params NodeDescriptor[] nodes)
        {
            return CreateAsset<Conclusion>(n, id, nodes);
        }
    }
}
