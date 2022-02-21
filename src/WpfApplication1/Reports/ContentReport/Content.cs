using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AssetBuilder.Reports
{
    public class Content
    {
        public List<Question> Questions { get; set; }
        public List<Conclusion> Conclusions { get; set; }

        public IEnumerable<Conclusion> NonSilentConclusions { get { return Conclusions?.Where(f => !f.Silent) ?? null; } }
        public IEnumerable<Question> NonSilentQuestions { get { return Questions?.Where(f => f.Type != "Calculated") ?? null; } }

        public bool HasConclusions => NonSilentConclusions?.Any() ?? false;
        public bool HasQuestions => NonSilentQuestions?.Any() ?? false;

        private XElement _QuestionAnswerXml;
        public XElement QuestionAnswerXml
        {
            get { return _QuestionAnswerXml; }
            set
            {
                Questions = new List<Question>();
                HashSet<int> qids = new HashSet<int>();
                HashSet<int> aids = new HashSet<int>();
                HashSet<string> tables = new HashSet<string>();
                Dictionary<string, string> NL = new Dictionary<string, string>();
                _QuestionAnswerXml = value;
                Question currentQuestion = null;
                SetTitle(_QuestionAnswerXml);

                foreach (var item in _QuestionAnswerXml.Elements("Table1"))
                {
                    var questionid = item.ElementIntValue("QuestionID");
                    var answerid = item.ElementIntValue("AnswerID");
                    var tablequestion = item.ElementValue("TableQuestion");
                    if (!qids.Contains(questionid))
                    {
                        currentQuestion = new Question
                        {
                            AssetID = questionid,
                            Expert = item.ElementValue("Expert_x0020_Statement"),
                            Lay = item.ElementValue("Lay_x0020_Statement"),
                            QuestionText = item.ElementValue("Question"),
                            Explanation = item.ElementValue("Explanation"),
                            Type = item.ElementValue("Question_x0020_Type"),
                            State = item.ElementValue("State"),
                            Category = item.ElementValue("Category"),
                            SubCat1 = item.ElementValue("Sub_x0020_Category_x0020_1"),
                            Tables = new List<Table>(),
                            Answers = new List<Answer>(),
                            NLText = AssetBuilder.Controls.NLTest.GetNLConditionString(item.ElementValue("Question"))
                        };
                        Questions.Add(currentQuestion);
                        qids.Add(questionid);
                        aids.Clear();
                        tables.Clear();
                    }
                    if (!string.IsNullOrWhiteSpace(tablequestion) && !tables.Contains(tablequestion))
                    {
                        currentQuestion.Tables.Add(new Table
                        {
                            Question = item.ElementValue("TableQuestion"),
                            Explanation = item.ElementValue("TableExplanation")
                        });
                        tables.Add(tablequestion);
                    }
                    if (!aids.Contains(answerid))
                    {
                        var text = item.ElementValue("Answer");
                        if (!NL.ContainsKey(text))
                        {
                            var nl = AssetBuilder.Controls.NLTest.GetNLConditionString(text);
                            NL.Add(text, nl);
                        }
                        currentQuestion.Answers.Add(new Answer
                        {
                            AssetID = item.ElementIntValue("AnswerID"),
                            Expert = item.ElementValue("Expert_x0020_Statement1"),
                            Lay = item.ElementValue("Lay_x0020_Statement1"),
                            AnswerText = text,
                            NLText = NL[text],
                            Explanation = item.ElementValue("Explanation1"),
                        });
                        aids.Add(answerid);
                    }
                }
            }
        }

        private XElement _ConclusionXml;
        public XElement ConclusionXml
        {
            get { return _ConclusionXml; }
            set
            {
                _ConclusionXml = value;
                Conclusions = new List<Conclusion>();
                SetTitle(_ConclusionXml);

                foreach (var item in _ConclusionXml.Elements("Table1"))
                {
                    var conclusionid = item.ElementIntValue("RecID");
                    var nlExpert = AssetBuilder.Controls.NLTest.GetNLConditionString(item.ElementValue("Expert_x0020_Statement"));
                    var nlLay = AssetBuilder.Controls.NLTest.GetNLConditionString(item.ElementValue("Lay_x0020_Statement"));
                    var nlExplanation = AssetBuilder.Controls.NLTest.GetNLConditionString(item.ElementValue("Explanation"));
                    Conclusions.Add(new Conclusion
                    {
                        AssetID = conclusionid,
                        Expert = item.ElementValue("Expert_x0020_Statement"),
                        Lay = item.ElementValue("Lay_x0020_Statement"),
                        Explanation = item.ElementValue("Explanation"),
                        Category = item.ElementValue("Category"),
                        SubCat1 = item.ElementValue("Sub_x0020_Category_x0020_1"),
                        SubCat2 = item.ElementValue("Sub_x0020_Category_x0020_2"),
                        Silent = item.ElementBoolValue("Silent"),
                        NLExpert = nlExpert,
                        NLLay = nlLay,
                        NLText = nlExplanation
                    });
                }

                Conclusions.Sort((f, s) => f.CategoryNumberSort.CompareTo(s.CategoryNumberSort) * 10 + f.SubCat1NumberSort.CompareTo(s.SubCat1NumberSort));
                //Conclusions = Conclusions.OrderBy(f => f.CategoryNumberSort).ThenBy(f => f.SubCat1NumberSort).ToList();
            }
        }

        public string Title
        {
            get
            {
                if (Questions != null && Conclusions != null) return "Content Report";
                else if (Questions != null) return "Question Answer Report";
                else if (Conclusions != null) return "Conclusion Report";
                return "";
            }
        }

        private void SetTitle(XElement xml)
        {
            if (string.IsNullOrEmpty(SubTitle))
            {
                var st = "";
                var ai = 0;
                var tt = xml.Elements("Table").Count();
                foreach (var item in xml.Elements("Table"))
                {
                    if (++ai == tt && tt > 1) st += " and ";
                    else if (ai > 1) st += ", ";
                    var algoid = item.ElementValue("AlgoID");
                    st += item.ElementValue("Algo_Name");
                    st += string.IsNullOrWhiteSpace(algoid) ? "" : $" ({algoid})";
                }
                SubTitle = st;
            }
        }

        public string SubTitle { get; set; }
    }

    public class Question : CategoryAsset
    {
        public string QuestionText { get; set; }
        public string Type { get; set; }
        public string State { get; set; }

        public List<Table> Tables { get; set; }

        public List<Answer> Answers { get; set; }

        public bool IsMultiSelect { get { return Type == ">1 Answer"; } }
    }

    public class Answer : Asset
    {
        public string AnswerText { get; set; }
    }

    public class Conclusion : CategoryAsset
    {
        public string SubCat2 { get; set; }
        public string SubCat2NumberSort { get { int i; if (int.TryParse(SubCat2, out i)) return $"{i:D8}"; else return SubCat2; } }
        public string NLExpert { get; set; }
        public string NLLay { get; set; }
        public bool Silent { get; set; }
    }

    public class CategoryAsset : Asset
    {
        public string Category { get; set; }
        public string SubCat1 { get; set; }
        public string CategoryNumberSort { get { int i; if (int.TryParse(Category, out i)) return $"{i:D8}"; else return Category; } }
        public string SubCat1NumberSort { get { int i; if (int.TryParse(SubCat1, out i)) return $"{i:D8}"; else return SubCat1; } }
    }

    public class Asset
    {
        public int AssetID { get; set; }
        public string Expert { get; set; }
        public string Lay { get; set; }
        public string Explanation { get; set; }
        public string NLText { get; set; }
    }

    public class Table
    {
        public string Question { get; set; }
        public string Explanation { get; set; }
    }
}
