
namespace NaturalLanguageWizard
{  
    public enum AssetType
    {
        Question,
        Conclusion,
        Answer,
        Algo
    }

    public class AssetInfoTable
    {
        private int ID { get; set; }

        public override string ToString()
        {
            return ID.ToString();
        }

        public static readonly AssetInfoTable Algo = new AssetInfoTable { ID = 2 };
        public static readonly AssetInfoTable AlgoQuestion = new AssetInfoTable { ID = 2 };
        public static readonly AssetInfoTable AlgoConclusion = new AssetInfoTable { ID = 3 };
        public static readonly AssetInfoTable Question = new AssetInfoTable { ID = 1 };
    }

    abstract public class Asset
    {
        public int ID { get; set; }

        public string Description { get; set; }

        abstract public AssetType AssetType { get; }

        public override string ToString()
        {
            return ID.ToString();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj) || EqualsByID(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private bool EqualsByID(object obj)
        {            
            if (obj is Asset)
            {
                return (obj as Asset).ID == ID;
            }
            else if (obj is string)
            {
                return int.Parse(obj as string) == ID;
            }
            else if (obj is int)
            {
                return (int)obj == ID;
            }
            else
            {
                return false;
            }
        }
    }

    public class Algo : Asset
    {
        public override AssetType AssetType
        {
            get { return AssetType.Algo; }
        }
    }

    public enum QuestionType
    {
        SingleAnswer = 32,
        MultipleAnswer = 33,
		ValueEntry = 36,
		Calculated = 37,
		FreeText = 40,
        ImageQuestion = 42,
        SingleTable = 52,
        MultipleTable = 53
    }   

    public class Question : Asset
    {        
        public QuestionType Type { get; set; }

        public override AssetType AssetType
        {
            get { return AssetType.Question; }
        }
    }

    public enum AnswerType
    {
        BasicDefined = 64,
        MultipleCategory = 65,
        ValueCalculated = 81,        
        ValueEnter = 83,
        StringEnter = 84,
        DateEnter = 86
    }

    public class Answer : Asset
    {
        public AnswerType Type { get; set; }

        public override AssetType AssetType
        {
            get { return AssetType.Answer; }
        }
    }

    public class Conclusion : Asset
    {        
        public override AssetType AssetType
        {
            get { return AssetType.Conclusion; }
        }
    }    
}
