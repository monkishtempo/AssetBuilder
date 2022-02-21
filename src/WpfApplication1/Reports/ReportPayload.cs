using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AssetBuilder.Reports
{
    [Serializable]
    public class ReportPayload
    {
        //private static Regex _defaultRegex = new Regex(@"<[\/]{0,1}[a-zA-Z]{1}[a-z-A-Z0-9-]*( [a-zA-Z]{1}[a-z-A-Z0-9-]*(=[""][^""]*[""]|=['][^']*[']){0,1})*>|\w*[^\w<]*");
        private static Regex _LineCompare = new Regex(@"(?-s)(?m)^(.*)$[\r\n]*");
        private static Regex _defaultRegex = new Regex(@"(<[\/]{0,1}[a-zA-Z]{1}[a-z-A-Z0-9-]*( [a-zA-Z]{1}[a-z-A-Z0-9-]*(=[""][^""]*[""]|=['][^']*[']){0,1})*[ \/]*>)|(?<=[^a-zA-Z0-9]|^|>|\n)([a-zA-Z0-9]+)(?: |(?=[^a-zA-Z0-9]|<|$|\n))|(?<= |[a-zA-Z0-9>]|^|>|\n)([^a-zA-Z0-9><\n]+)(?: |(?=[a-zA-Z0-9]|$|>|<|\n))|[^a-zA-Z0-9]+");
        //private static Regex _defaultRegex = new Regex(@"(<[\/]{0,1}[a-zA-Z]{1}[a-z-A-Z0-9-]*( [a-zA-Z]{1}[a-z-A-Z0-9-]*(=[""][^""]*[""]|=['][^']*[']){0,1})*[ \/]*>)|(\w+[^<\w]*|[<])");
        private static Regex _containsHtml = new Regex(@"([<]([\/][a-zA-Z]{1}[a-z-A-Z0-9-]*|[a-zA-Z]{1}[a-z-A-Z0-9-]*( [a-zA-Z]{1}[a-z-A-Z0-9-]*=[""][^""]*[""])*)[ \/]*[>])");
        private static Comparer<Match> _defaultComp = Comparer<Match>.Create((x, y) => x == null ? (y == null ? 0 : -1) : x.Value.CompareTo(y?.Value));

        public Regex DefaultRegex;

        public ReportPayload()
        {
            DefaultRegex = _defaultRegex;
        }

        public ReportPayload(CompareType ct)
        {
            if (ct == CompareType.Line) DefaultRegex = _LineCompare;
            else DefaultRegex = _defaultRegex;
        }

        public int Index1 { get; set; }
        public int Index2 { get; set; }
        public string Title { get; set; }
        public string LinkTitle { get; set; }
        public string ScriptData { get; set; }
        public string Description { get; set; }
        public string AlternateDescription { get; set; }
        public string Colour { get; set; }
        public List<ReportPayload> Objects { get; set; }
        public Dictionary<string, string> Properties { get; set; }

        public DateTime Date { get; set; }
        public DateTime AlternateDate { get; set; }

        public string Comments { get; set; }

        private ContentExists _Content = ContentExists.NotDefined;
        public ContentExists Content
        {
            get
            {
                if (_Content != ContentExists.NotDefined) return _Content;
                _Content = (ContentExists)((Objects?.Any(f => !string.IsNullOrEmpty(f.Description)) == true ? 2 : 0) + (Objects?.Any(f => !string.IsNullOrEmpty(f.AlternateDescription)) == true ? 1 : 0));
                return _Content;
            }
        }

        public string htmlHigh => _containsHtml.IsMatch(Description + AlternateDescription) ? " htmlHigh" : "";

        private IList<StringCompare.DifferenceSets<Match>> _Diffs;
        public IList<StringCompare.DifferenceSets<Match>> Diffs
        {
            get
            {
                if (_Diffs != null) return _Diffs;
                return (_Diffs = StringCompare.LD.GetChangeSets<Match>(
                        DefaultRegex.Matches(Description).OfType<Match>().ToList(),
                        DefaultRegex.Matches(AlternateDescription).OfType<Match>().ToList(),
                        _defaultComp));
            }
        }

        public static ReportPayload CreateScaffold(string MainTitle, string SubTitle, string DetailTitle, string Colour = null, CompareType compareType = CompareType.NotDefined)
        {
            return new ReportPayload(compareType)
            {
                Title = MainTitle,
                Objects = new List<ReportPayload>
                {
                    new ReportPayload(compareType)
                    {
                        Title = SubTitle,
                        Objects = new List<ReportPayload>
                        {
                            new ReportPayload(compareType)
                            {
                                Colour = Colour,
                                Title = DetailTitle,
                                Objects = new List<ReportPayload>
                                {
                                }
                            }
                        }
                    }
                }
            };

        }
    }

    public enum ContentExists
    {
        NotDefined = -1,
        None,
        RightOnly,
        LeftOnly,
        LeftAndRight
    }

    public enum CompareType
    {
        NotDefined = -1,
        Word,
        Line
    }
}
