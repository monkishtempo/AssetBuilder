using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetBuilder.Extensions
{
    public static class StringExtensions
    {
        public static Dictionary<char, char> BaseReplacements = new Dictionary<char, char> {
            { (char)8239, ' ' },
            { (char)0x2018, '\'' },
            { (char)0x2019, '\'' },
            { (char)0x201C, '\"' },
            { (char)0x201D, '\"' },
            { (char)0x2013, '-' }
        };

        public static string ReplaceChars(this string input)
        {
            return input.ReplaceChars(BaseReplacements);
        }

        public static string ReplaceChars(this string input, Dictionary<char, char> replacementChars)
        {
            var chars = new List<char>();
            foreach (var c in input)
            {
                switch (c > 255)
                {
                    case true when replacementChars.ContainsKey(c):
                    {
                        var nc = replacementChars[c];
                        chars.Add(nc);
                        break;
                    }
                    case true:
                    {
                        var bytes = Encoding.UTF8.GetBytes(c.ToString());
                        chars.AddRange(bytes.Select(b => (char)b));
                        break;
                    }
                    default:
                        chars.Add(c);
                        break;
                }
            }

            return new string(chars.ToArray());
        }

        public static bool Contains(this string str, string substring, StringComparison comp)
        {
            if (substring == null)
            {
                throw new ArgumentNullException(nameof(substring), "substring cannot be null.");
            }

            if (!Enum.IsDefined(typeof(StringComparison), comp))
            {
                throw new ArgumentException("comp is not a member of StringComparison", nameof(comp));
            }

            return str.IndexOf(substring, comp) >= 0;
        }

        public static string RemoveLineEndings(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            var lineSeparator = ((char)0x2028).ToString();
            var paragraphSeparator = ((char)0x2029).ToString();

            return value.Replace("\r\n", string.Empty)
                .Replace("\n", string.Empty)
                .Replace("\r", string.Empty)
                .Replace(lineSeparator, string.Empty)
                .Replace(paragraphSeparator, string.Empty);
        }
    }
}