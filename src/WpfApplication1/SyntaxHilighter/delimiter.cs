using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace AssetBuilder.SyntaxHilighter
{
	internal class delimiter
	{
		public string firstString { get; set; }
		public string lastString { get; set; }
		public TextPointer tp1 { get; set; }
		public TextPointer tp2 { get; set; }
		public TextRange textRange
		{
			get
			{
				if (tp1 == null | tp2 == null)
					return null;
				return new TextRange(tp1, tp2);
			}
		}
		public List<string> examples { get; set; }
		/// <summary>
		/// Regular expression to validate the data
		/// </summary>
		public string valExpression { get; set; }
		public bool IsValid(string delimi)
		{
			Regex reg = new Regex(this.valExpression);
			return reg.IsMatch(delimi);
		}

		public bool pos
		{
			get
			{
				return this.firstString.Equals(this.lastString, StringComparison.CurrentCultureIgnoreCase);
			}
		}
		public string keycode { get; set; }

		/// <summary>
		/// Returns true if the tag is closed, used for nested hilighting
		/// </summary>
		public bool isClosed
		{
			get
			{
				return (tp2 != null);
			}
		}
		public Color bgcolor { get; set; }
		public Color forecolor { get; set; }
		public string description { get; set; }
		public string syntaxHelper { get; set; }
		public delimiter Clone()
		{
			delimiter rtn = new delimiter();
			rtn.firstString = this.firstString;
			rtn.lastString = this.lastString;
			rtn.forecolor = this.forecolor;
			rtn.bgcolor = this.bgcolor;
			rtn.tp1 = this.tp1;
			rtn.tp2 = this.tp2;
			return rtn;
		}
		public static string keyConvert(KeyEventArgs e)
		{
			string rtn = e.Key.ToString();
			Key k = e.Key;
			ModifierKeys m = e.KeyboardDevice.Modifiers;

			switch (k)
			{
				case Key.Oem4: // [ | {
					rtn = (m == ModifierKeys.Shift) ? "{" : "[";
					break;
				case Key.Oem7: // ~
					rtn = (m == ModifierKeys.Shift) ? "~" : "#";
					break;
				default:
					rtn = e.Key.ToString();
					break;
			}

			return rtn;
		}
	}
}
