using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace AssetBuilder.Controls
{
	static class NLExtensions
	{
		static Dictionary<char, char> pairs = new Dictionary<char, char>();
		static Dictionary<char, bool> forwards = new Dictionary<char, bool>();
		static Dictionary<char, bool> html = new Dictionary<char, bool>();
		static Dictionary<char, string> replace = new Dictionary<char, string>();
		static Dictionary<char, string> valid = new Dictionary<char, string>();
		static Dictionary<char, List<NLError>> NLErrors = new Dictionary<char, List<NLError>>();
		static List<string> tags = new List<string>(new string[] { "a", "div", "p", "span", "font", "strong", "b", "i", "object", "param", "embed", "br", "u", "ul", "li", "img" });
		public static List<TextBox> errors = new List<TextBox>();

		static NLExtensions()
		{
			pairs.Add('{', '}');
			pairs.Add('}', '{');
			pairs.Add('[', ']');
			pairs.Add(']', '[');
			pairs.Add('<', '>');
			pairs.Add('>', '<');
			pairs.Add('(', ')');
			pairs.Add(')', '(');
			pairs.Add('~', '~');
			forwards.Add('{', true);
			forwards.Add('}', false);
			forwards.Add('[', true);
			forwards.Add(']', false);
			forwards.Add('<', true);
			forwards.Add('>', false);
			forwards.Add('(', true);
			forwards.Add(')', false);
			forwards.Add('~', true);
			html.Add('<', true);
			html.Add('>', true);
			replace.Add('{', @"(?<=\{.*)\{[^\{\}]*\}(?=.*\})");
			//replace.Add('>', @"(?<=<[^<]*)(<\w+\b[^<>]*/>|<(\w+)\b[^<>]*>([^<>]*?)</\2>)(?=[^>]*>)");
            valid.Add('{', @"\{((qv|qs|qd)\d+(\|\d+){0,1}|(qv|qs|qd|qt|nt)\d+|(ca|ga|qa|ce)[-]{0,1}\d+|((qp(\d+\|)([-]{0,1}\d+\|)|(cc|gc|qc|qn)(\d+\|){1})(?s:([^\|\}]*\|){0,1})|(lb|ln|ll|lr|la|lo)(?s:[^\}]*)|(xf|xm|xx|xy|xz)([^\|\}]*(\|[^\|\}]*){0,1})|(tc|tp)|(tp)((\d+)(\|\d+)*)|\d{3}-\d{3}(?i:[ymwd]))(?s:[^\|\}]*))\}");
			valid.Add('[', @"\[(?![A-Z][^/]*/[a-z])(?![a-z][^/]*/[A-HJ-Z])[^/]*/((?!\b(?i:he)\b)(?!\bi\b)[^/])*\]");
			valid.Add('~', @"~((T|E|I)[^~#]*#|W\d{2}|M|F|P)[^~]*~");
			valid.Add('<', @"(<\w+\b([ ]+\w[\w-]+=""[^""<>]*"")*[ ]*(>|/>)|</\w+>)");
			NLErrors.Add('{', new List<NLError>());
			NLErrors.Add('[', new List<NLError>());
            NLErrors['{'].Add(new NLError { Key = @"\{(qc|qn|qv|qs|qd|ca|ga|qa|ce|qt|qp|cc|gc|nt|lb|ln|ll|lr|la|lo|tc|tp|xf|xm|xx|xy|xz)[^\}]*\}", Value = "Missing clause descriptor. (i.e. 'qp', 'qa', etc.)" });
            NLErrors['{'].Add(new NLError { Key = @"\{((qv|qs|qd|ca|ga|qa|ce|qt|nt)\d+|(qc|qn|qp|cc|gc)\d+\||(tc|tp|lb|ln|ll|lr|la|lo|xf|xm|xx|xy|xz))[^\}]*\}", Value = "Missing id and/or pipe after clause descriptor. (i.e. 'cc1234|...')" });
            NLErrors['{'].Add(new NLError { Key = @"\{(qp(\d+\|)([-]{0,1}\d+\|)|(tc|tp|qc|qn|qv|qs|qd|ca|ga|qa|ce|qt|nt|lb|ln|ll|lr|la|lo|xf|xm|xx|xy|xz))[^\}]*\}", Value = "Missing answerid and/or pipe after questionid. (i.e. 'qp1234|2|...')" });
            NLErrors['{'].Add(new NLError { Key = @"\{(qv|qs|qd)\d+(\|\d+){0,1}|(qp(\d+\|)([-]{0,1}\d+\|)([^\|]*\|){0,1}|(cc|gc|qc|qn)(\d+\|){1}([^\|]*\|){0,1}|(ca|ga|qa|ce|qt|nt)\d+|(lb|ln|ll|lr|la|lo)([^\|]*\|)*)[^\}\|]*\}|(xf|xm|xx|xy|xz)([^\|\}]*(\|[^\|\}]*){0,1})|(tc)(\})|(tp)((\d+)(\|\d+)*\})", Value = "Too many inputs. (i.e. 'qp1234|2|...|...|')" });
			NLErrors['['].Add(new NLError { Key = @"\[((?=[A-Z][^/]*/[a-z])|(?=[a-z][^/]*/[A-Z])).*\]", Value = "Case doesn't match", Match = true });
			NLErrors['['].Add(new NLError { Key = @"\[([^/]*/){2}.*\]", Value = "Too many '/' characters", Match = true });
			NLErrors['['].Add(new NLError { Key = @"\[[^/]*\]", Value = "Missing '/' separator", Match = true });
			//valid.Add('<', @"(<\w+\b[^<>]*/>|<(\w+)\b[^<>]*>([^<>]*?)</\2>)"); \[[^/]*\]
		}

		public static void textBox_AdornAndValidate(object sender, RoutedEventArgs e)
		{
			TextBox t = sender as TextBox;
			if(t.IsVisible) t.validateTextBox();
		}

		public static void textBox_ReAdorn(object sender, RoutedEventArgs e)
		{
			if (sender is TextBox)
			{
				TextBox t = sender as TextBox;
				AdornerLayer al = AdornerLayer.GetAdornerLayer(t);
				Adorner[] aa = al.GetAdorners(t);
				if (aa != null)
					foreach (Adorner item in aa)
					{
						if (item is HighLightAdorner)
						{
							HighLightAdorner ha = item as HighLightAdorner;
							ha.ReDraw();
						}
						if (item is TextAdorner)
						{
							TextAdorner ta = item as TextAdorner;
							ta.ReDraw();
						}
					}

			}
		}

        public static object lockObj = new object();

		public static bool validateTextBox(this TextBox t)
		{
			errors.Remove(t);
			AdornerLayer al = clearAdornerLayer(t);

			if (al == null) return true;
            var assetcontrol = t.TemplatedParent as AssetBuilder.AssetControls.assetControl;

            if (Window1.ShowTranslation && assetcontrol?.Changes != null)
            {
                TextBox t2 = null;
                bool isLanguage = t.Name.EndsWith("Language");
                if (isLanguage && assetcontrol.TextChildren.ContainsKey(t.Name.Substring(0, t.Name.Length - 8))) t2 = assetcontrol.TextChildren[t.Name.Substring(0, t.Name.Length - 8)];
                else if (assetcontrol.TextChildren.ContainsKey(t.Name + "Language")) t2 = assetcontrol.TextChildren[t.Name + "Language"];
                if (t2 != null && t2.Text != "")
                {
                    if (!assetcontrol.Changes.ContainsKey(t.Name))
                    {
                        lock (lockObj)
                        {
                            var w1 = Compare._defaultRegex.Matches(t.Text).OfType<Match>().ToArray();
                            var w2 = Compare._defaultRegex.Matches(t2.Text).OfType<Match>().ToArray();
                            assetcontrol.Changes.Add(t.Name, StringCompare.LD.GetChangeSets(w1, w2, Compare._defaultComp));
                        }
                    }
                    foreach (var item in assetcontrol.Changes[t.Name])
                    {
                        var type = "edit";
                        if (item.NewValues == null && item.OldValues != null) type = isLanguage ? "add" : "delete";
                        if (item.NewValues != null && item.OldValues == null) type = isLanguage ? "delete" : "add";
                        if(isLanguage && item.OldValues != null)
                            al.Add(new HighLightAdorner(t, item.OldValues.First().Index, item.OldValues.Sum(f => f.Length), type));
                        else if(!isLanguage && item.OldValues != null)
                            al.Add(new HighLightAdorner(t, item.OldValues.First().Index, item.OldValues.Sum(f => f.Length), type));
                    }
                }
            }
			else if (qcat.CurrentSearch.Replace(@"\b", "").Replace(@"(?-i)", "") != "")
			{
				Regex r = new Regex(qcat.CurrentSearch, RegexOptions.Multiline | RegexOptions.IgnoreCase);
				foreach (Match match in r.Matches(t.Text))
				{
					if (match.Length > 0)
					{
						HighLightAdorner ha = new HighLightAdorner(t, match.Index, match.Length, "SearchWord");
						al.Add(ha);
					}
				}
			}

			if (t.Name.EndsWith("Language"))
			{
				TextBox tb = (TextBox)t.FindName(t.Name.Replace("Language", ""));
				if (tb != null && t.Text == "")
				{
					inherits ih = (inherits)t.FindName("list" + tb.Name.Substring(3));
					string content = ih?.OriginalText;
					if (tb.IsReadOnly || ih?.getCheckedLanguages().Contains(Window1.TranslationLanguage) == true || !Window1.window.AlternateLanguages.Contains(Window1.TranslationLanguage)) content = tb.Text;
					TextAdorner ta = new TextAdorner(t, content, Colors.Gray);
					//AdornerLayer.GetAdornerLayer(tb).Add(ta);
					al.Add(ta);
				}
			}

			if (!Window1.DisableValidation)
			{
				if (t.SelectionLength == 0 && t.SelectionStart > 0)
				{
					char pair = t.Text.Substring(t.SelectionStart - 1, 1)[0];
					if (pairs.ContainsKey(pair))
					{
						int start = findPair(t.Text, pair, t.SelectionStart - 1);
						if (start > -1)
						{
							al.Add(new HighLightAdorner(t, getCharRect(t, start), start));
							al.Add(new HighLightAdorner(t, getCharRect(t, t.SelectionStart - 1), t.SelectionStart - 1));
						}
					}
				}

				NLFail[] ra = t.validateText();
				if (ra != null && ra.Length > 0)
				{
					foreach (var item in ra)
					{
						al.Add(new HighLightAdorner(t, item, true));
					}
					errors.Add(t);
					Window1.setStatus("Natural Language Errors!");
					return false;
				}
			}

			if (errors.Count == 0) Window1.setStatus("");
			return true;
		}

		public static AdornerLayer clearAdornerLayer(this TextBox t)
		{
			AdornerLayer al = AdornerLayer.GetAdornerLayer(t);
			//al.IsHitTestVisible = false;
			if (al == null) return null;
			Adorner[] aa = al.GetAdorners(t);
			if (aa != null)
				foreach (Adorner item in aa)
					al.Remove(item);
			return al;
		}

		public static NLFail[] validateText(this TextBox t)
		{
			string s = t.Text;
            if (t.IsFocused) Window1.NLI.list.Items.Clear();

            if (s.Length == 0 || s.Substring(0, 1) == "?") return null;

            var sa = ValidateText(s, out Dictionary<int, string> tag, out List<string> clauses);
            List<NLFail> ra = new List<NLFail>();

            foreach (var item in sa)
            {
                ra.Add(new NLFail { Position = t.getCharRect(item.Start), Message = item.Message, Start = item.Start });
            }
            if(t.IsFocused)
            {
                foreach (var item in clauses)
                {
                    Window1.NLI.list.Items.Add(item);
                }
            }

            if (!Window1.DisableHTMLValidation) validateHTML(ra, tag, t);

			return ra.ToArray();
		}

        public static List<(string Message, int Start)> ValidateText(string s, out Dictionary<int, string> tag, out List<string> clauses)
        {
            List<(string Message, int Start)> sa = new List<(string Message, int Start)>();
            tag = new Dictionary<int, string>();
            clauses = new List<string>();

            int y = 0;

            foreach (var item in pairs)
            {
                Regex rp = null;
                Regex vl = null;
                if (replace.ContainsKey(item.Key)) rp = new Regex(replace[item.Key], RegexOptions.Singleline);
                if (valid.ContainsKey(item.Key)) vl = new Regex(valid[item.Key]);
                int x = 0;
                while ((y = s.IndexOf(item.Key, x)) > -1)
                {
                    int z = findPair(s, item.Key, y);
                    if (z == -1 && !html.ContainsKey(item.Key)) sa.Add(("Missing " + (forwards[item.Key] ? "closing '" : "opening '") + pairs[item.Key] + "' character", y));
                    else if (z > -1 && forwards[item.Key] && z > y)
                    {
                        string innerclause = s.Substring(y, z - (y - 1));
                        while (rp != null && rp.IsMatch(innerclause)) innerclause = rp.Replace(innerclause, "");
                        clauses.Add(innerclause);
                        if (html.ContainsKey(item.Key))
                        {
                            tag.Add(y, innerclause);
                        }
                        if (vl != null && !vl.IsMatch(innerclause))
                        {
                            string error = "Error inside clause";
                            if (NLErrors.ContainsKey(item.Key))
                                foreach (var sc in NLErrors[item.Key])
                                {
                                    if (Regex.IsMatch(innerclause, sc.Key) == sc.Match)
                                    {
                                        error = sc.Value;
                                        break;
                                    }
                                }
                            sa.Add((error, y));
                            sa.Add((error, z));
                        }
                    }
                    x = y + 1;
                }
            }

            return sa;
        }

		static List<string> nonHTMLTags = new List<string>() { "age", "algo", "c", "fullname", "name", "ntage", "problem", "subcat1", "subcat2", "you", "your" };

		static Stack<T> MakeStack<T>(T itemOfType)
		{
			return new Stack<T>();
		}

		public static string OpenElement = "";

		static void validateHTML(List<NLFail> ra, Dictionary<int, string> tag, TextBox tb)
		{
			// || t.Value.Length > 2 
			List<int> remove = new List<int>(from t in tag where t.Value.EndsWith("/>") || nonHTMLTags.Exists(f => t.Value.Substring(1).StartsWith(f)) select t.Key);
			var lastOpenTags = MakeStack(new { Index = 0, Name = "" });
			var firstCloseTag = new { Index = 0, Name = "" };

			foreach (var t in remove)
			{
				tag.Remove(t);
			}
			var openTags = MakeStack(new { Index = 0, Name = "" });
			var unclosedTags = MakeStack(new { Index = 0, Name = "" });
			bool findOpen = true;
			OpenElement = "";
			foreach (var item in tag)
			{
				MatchCollection mc = Regex.Matches(item.Value, @"(?![</])(\w*)([^ />])");
				if (mc.Count == 0) continue;
				if (item.Value.StartsWith("</"))
				{
					if (item.Key >= tb.SelectionStart && firstCloseTag.Index == -1) firstCloseTag = new { Index = item.Key, Name = mc[0].Value };
					if (openTags.Count == 0 || openTags.Peek().Name != mc[0].Value)
					{
						string message = string.Format("Invalid End Tag Expected {0}",
							openTags.Count == 0 ? "Start tag" : "</" + openTags.Peek().Name + ">");
						ra.Add(new NLFail { Position = tb.getCharRect(item.Key), Message = message, Start = item.Key });
						int close = tb.Text.IndexOf('>', item.Key);
						if(close > -1)
							ra.Add(new NLFail { Position = tb.getCharRect(close), Message = message, Start = close });
						if (openTags.Any(f => f.Name == mc[0].Value))
							while (openTags.Count > 0 && openTags.Peek().Name != mc[0].Value)
								unclosedTags.Push(openTags.Pop());
					}
					if (openTags.Count > 0 && openTags.Peek().Name == mc[0].Value)
					{
						if (lastOpenTags.Count > 0 && openTags.Peek().Index == lastOpenTags.Peek().Index)
						{
							lastOpenTags.Pop();
							if (item.Key >= tb.SelectionStart) findOpen = false;
						}
						openTags.Pop();
					}
				}
				else
				{
					if (item.Key < tb.SelectionStart) lastOpenTags.Push(new { Index = item.Key, Name = mc[0].Value });
					openTags.Push(new { Index = item.Key, Name = mc[0].Value });
				}
			}
			var lastOpenTag = new { Index = 0, Name = "" };
			if(lastOpenTags.Count > 0) lastOpenTag = lastOpenTags.Pop();
			while (openTags.Count + unclosedTags.Count > 0)
			{
				var openTag = openTags.Count > 0 ? openTags.Pop() : unclosedTags.Pop();
				if (findOpen && OpenElement == "" && openTag.Index < tb.SelectionStart && lastOpenTag.Name == openTag.Name && openTag.Name != firstCloseTag.Name) OpenElement = openTag.Name;
				string message = string.Format("Missing End Tag for {0}", openTag.Name);
				ra.Add(new NLFail { Position = tb.getCharRect(openTag.Index), Message = message, Start = openTag.Index });
				int close = tb.Text.IndexOf('>', openTag.Index);
				if (close > -1)
					ra.Add(new NLFail { Position = tb.getCharRect(close), Message = message, Start = close });
			}
			//KeyValuePair<int, string>[] kvp = tag.ToArray();
			//for (int i = 0; i < tag.Count; i++)
			//{
			//    MatchCollection mc = Regex.Matches(kvp[i].Value, @"(?![</])(\w*)([^ />])");
			//    MatchCollection cc = Regex.Matches(kvp[tag.Count - (i + 1)].Value, @"(?![</])(\w*)([^ />])");
			//    if (mc[0].Value != cc[0].Value || i == tag.Count - (i + 1))
			//    {
			//        ra.Add(new NLFail { Position = tb.getCharRect(kvp[i].Key), Message = "Invalid HTML", Start = kvp[i].Key });
			//        ra.Add(new NLFail { Position = tb.getCharRect(kvp[i].Key + kvp[i].Value.Length - 1), Message = "Invalid HTML", Start = kvp[i].Key + kvp[i].Value.Length - 1 });
			//    }
			//}
		}

		public static int findPair(this string p, char c, int pos)
		{
			char find = pairs[c];
			bool searchForwards = forwards[c];
			if (find == c)
			{
				int count = 0;
				int x = 0;
				int y = 0;
				while ((y = p.IndexOf(c, x)) > -1 && y < pos)
				{
					count++;
					x = y + 1;
				}
				if (count % 2 == 1) searchForwards = false;
			}
			int step = 1;
			if (!searchForwards) step = -1;
			if (find == (char)0) return -1;
			int miss = 0;
			for (int i = pos + step; (searchForwards ? i < p.Length : i >= 0); i += step)
			{
				if (p[i] == c && find != c) miss++;
				else if (p[i] == find)
					if (miss == 0) return i; else miss--;
			}
			return -1;
		}

		public static Rect getCharRect(this TextBox t, int start)
		{
			return getCharRect(t, start, 1);
		}

        public static Rect getCharRect(this TextBox t, int start, int length)
		{
			Rect r = t.GetRectFromCharacterIndex(start, false);
			if (r == Rect.Empty) return r;
			string chr = t.Text.Substring(start, length);
			Typeface tf = new Typeface(t.FontFamily, t.FontStyle, t.FontWeight, t.FontStretch);
			FormattedText tb = new FormattedText(
				chr, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
				tf, t.FontSize, Brushes.Black, VisualTreeHelper.GetDpi(t).PixelsPerDip);
			if (r.Y < 0)
			{
				double newHeight = r.Height + r.Y;
				r.Height = Math.Max(1, newHeight);
				r.Y = 0;
			}
			if (r.Y + r.Height > t.ActualHeight)
			{
				double newHeight = r.Height - ((r.Y + r.Height) - t.ActualHeight);
				r.Height = Math.Max(1, newHeight);
				r.Y = Math.Min(t.ActualHeight, r.Y);
			}
			//r.X -= tb.Width;
			r.Width = tb.Width;
			return r;
		}
	}
}
