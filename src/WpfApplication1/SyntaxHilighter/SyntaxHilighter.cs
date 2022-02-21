using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace AssetBuilder.SyntaxHilighter
{
	class SyntaxHilighter
	{
		private static System.Windows.Media.Color ConvertHexToColour(string hexColour)
		{
			return (System.Windows.Media.Color)ColorConverter.ConvertFromString(hexColour);
		}
		private static List<delimiter> getDelimiters()
		{
			List<delimiter> arrdelimiter = new List<delimiter>();
			arrdelimiter.Add(new delimiter() { firstString = "{qt", lastString = "}", bgcolor = Colors.Orange, forecolor = Colors.White, description = "Table question", syntaxHelper = "{qt<QuestionID>}", valExpression = "{qt[0-9]{1,}}" });
			arrdelimiter.Add(new delimiter() { firstString = "{qd", lastString = "}", bgcolor = Colors.Plum, forecolor = Colors.White, description = "Date entry", syntaxHelper = "{qd<QuestionID>}", valExpression = "{qd[0-9]{1,}}" });
			arrdelimiter.Add(new delimiter() { firstString = "{qs", lastString = "}", bgcolor = Colors.CornflowerBlue, forecolor = Colors.White, description = "Text entry", syntaxHelper = "{qs<QuestionID>}", valExpression = "{qs[0-9]{1,}}" });
			arrdelimiter.Add(new delimiter() { firstString = "{qv", lastString = "}", bgcolor = Colors.Beige, forecolor = Colors.Black, description = "Value entry", syntaxHelper = "{qv<QuestionID>}", valExpression = "{qv[0-9]{1,}}" });
			arrdelimiter.Add(new delimiter() { firstString = "{cc", lastString = "}", bgcolor = Colors.Teal, forecolor = Colors.White, description = "Conclusion", syntaxHelper = "{cc<ConclusionID>|<Text to display if conclusion exists>}", valExpression = "{cc[0-9]{1,}\\|.*}" });
			arrdelimiter.Add(new delimiter() { firstString = "{lb", lastString = "}", bgcolor = Colors.Tomato, forecolor = Colors.White, description = "Bulleted list", syntaxHelper = "{lb<first item>|<second item>...", valExpression = "{lb.*}" });
			arrdelimiter.Add(new delimiter() { firstString = "{lo", lastString = "}", bgcolor = Colors.YellowGreen, forecolor = Colors.White, description = "OR list", syntaxHelper = "{lo<first item>|<second item>|...}", valExpression = "{lo.*}" });
			arrdelimiter.Add(new delimiter() { firstString = "{la", lastString = "}", bgcolor = Colors.Magenta, forecolor = Colors.White, description = "AND list", syntaxHelper = "{la<first item>|<second item>|...}", valExpression = "{la.*}" });
			arrdelimiter.Add(new delimiter() { firstString = "{lr", lastString = "}", bgcolor = Colors.Yellow, forecolor = Colors.Black, description = "Carridge return list", syntaxHelper = "{lr<first item>|<second item>|...}", valExpression = "{lr.*}" });
			arrdelimiter.Add(new delimiter() { firstString = "{ll", lastString = "}", bgcolor = Colors.Orange, forecolor = Colors.Black, description = "Comma delimited list", syntaxHelper = "{ll<first item>|<second item>|...}", valExpression = "{ll.*}" });
			arrdelimiter.Add(new delimiter() { firstString = "{qp", lastString = "}", bgcolor = ConvertHexToColour("#FFD3D3D3"), forecolor = Colors.Black, description = "Question answer pair", syntaxHelper = "{qp<QuestionId>|<AnswerId>|<Text to display if question answer pair exists>}", valExpression = "{qp[0-9]{1,}\\|[0-9]{1,}\\|.*}" });
			arrdelimiter.Add(new delimiter() { firstString = "{qa", lastString = "}", bgcolor = ConvertHexToColour("#FF20B2AA"), forecolor = Colors.White, description = "Answer to a question", syntaxHelper = "{qa<QuestionID>}", valExpression = "{qa[0-9]{3,}}" });
			arrdelimiter.Add(new delimiter() { firstString = "{", lastString = "}", bgcolor = ConvertHexToColour("#FF00FF7F"), forecolor = Colors.Black, description = "Age range", syntaxHelper = "{<from>-<to>y<text>}", valExpression = "{[0-9]{3}-[0-9]{3}.*}" });
			arrdelimiter.Add(new delimiter() { firstString = "[", lastString = "]", bgcolor = Colors.Green, forecolor = Colors.White, description = "2nd and 3rd person", syntaxHelper = "[<2nd>/<3rd>]", valExpression = "\\[.*/.*\\]" });
			arrdelimiter.Add(new delimiter() { firstString = "~", lastString = "~", bgcolor = ConvertHexToColour("#FF87CEEB"), forecolor = Colors.White, description = "Gender", syntaxHelper = "~<F or M><text>~", valExpression = "~(m|f).*~" });
			
			return arrdelimiter;
		}
		static internal void SyntaxHilight(RichTextBox richTextBox1, List<delimiter> arrdelimiter)
		{
			TextPointer caretPosition = richTextBox1.CaretPosition;

			int foundLength = 1;
			DateTime dtStart = DateTime.Now;

			// clear properties
			TextRange fullContent = new TextRange(richTextBox1.Document.ContentStart, richTextBox1.Document.ContentEnd);
			string originalString = fullContent.Text;
			fullContent.ClearAllProperties();

			// setup delimiters
			if (arrdelimiter == null)
			{
				arrdelimiter = new List<delimiter>();
				arrdelimiter = getDelimiters();
			}

			string d = string.Empty;
			TextPointer pointer = richTextBox1.Document.ContentStart;

			List<delimiter> arrFounddelimiter = new List<delimiter>();

			while (pointer != null)
			{
				foundLength = 1;
				if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
				{
					// found some text
					string run = pointer.GetTextInRun(LogicalDirection.Forward);
					//System.Diagnostics.Debug.WriteLine(run);
					foreach (delimiter delim in arrdelimiter)
					{
						//System.Diagnostics.Debug.WriteLine(delim.firstString);
						if (run.StartsWith(delim.firstString))
						{
							foundLength = delim.firstString.Length;
							delimiter arrD = arrFounddelimiter.Where(dfilter => dfilter.firstString == delim.firstString & dfilter.tp2 == null & dfilter.pos).FirstOrDefault();
							if (arrD != null)
							{
								if (arrD.tp1 == null)
									arrD.tp1 = pointer.GetPositionAtOffset(1);
								else if (arrD.tp2 == null)
									arrD.tp2 = pointer.GetPositionAtOffset(1);
							}
							else
							{
								// this is a new range
								delim.tp1 = pointer;
								delimiter delimColour = arrFounddelimiter.Where(dfilter => dfilter.tp2 == null && dfilter.firstString == delim.firstString).Reverse().FirstOrDefault();
								if (delimColour != null)
								{
									if (delimColour.forecolor != Colors.Black)
										delim.forecolor = Colors.Black;
									else
										delim.forecolor = Colors.White;
								}
								arrFounddelimiter.Add(delim.Clone());
							}
							break;
						}
						if (run.StartsWith(delim.lastString))
						{
							foundLength = delim.lastString.Length;
							delimiter delims = arrFounddelimiter.Where(dfilter => dfilter.lastString == delim.lastString & dfilter.tp2 == null).Reverse().FirstOrDefault();
							if (delims == null)
							{
								//System.Diagnostics.Debug.Write("KJKJ");
							}
							else
							{
								delims.tp2 = pointer.GetPositionAtOffset(delim.lastString.Length);
							}
							break;
						}
					}
				}
				System.Diagnostics.Debug.WriteLine(foundLength.ToString());
				pointer = pointer.GetPositionAtOffset(foundLength);
			}

			System.Diagnostics.Debug.WriteLine(originalString);
			string lastHighlightText = string.Empty;
			TextPointer lastPointer = null;

			//arrFounddelimiter.ForEach(delegate(delimiter founddelimit)
			foreach (delimiter founddelimit in arrFounddelimiter)
			{
				try
				{
					lastHighlightText = founddelimit.firstString;
					lastPointer = founddelimit.tp1;
					System.Diagnostics.Debug.WriteLine(string.Format("s: {0} f: {1} Text: {2}", founddelimit.firstString, founddelimit.lastString, founddelimit.textRange.Text));
					founddelimit.textRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(founddelimit.forecolor));
					founddelimit.textRange.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(founddelimit.bgcolor));
				}
				catch (NullReferenceException)
				{
					TextRange tr = new TextRange(lastPointer, lastPointer.GetNextInsertionPosition(LogicalDirection.Forward));
					showError(tr);
				}
			}
			richTextBox1.CaretPosition = caretPosition;

			//debug(DateTime.Now.Subtract(dtStart).TotalMilliseconds.ToString());
			//System.Diagnostics.Debug.WriteLine(DateTime.Now.Subtract(dtStart).TotalMilliseconds.ToString());
		}
		private static void showError(TextRange tr)
		{
			tr.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Red));
			tr.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.White));
		}
	}
}
