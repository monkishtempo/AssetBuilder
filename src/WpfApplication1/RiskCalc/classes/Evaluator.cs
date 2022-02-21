using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Evaluator
{
    /// <summary>
    /// Array of the valid function names.
    /// </summary>
    /// <remarks>The supported functions are:
    /// <list type="bullet">
    /// <item><c>log</c> As detailed in <see cref="System.Math.Log"></see></item>
    /// <item><c>int</c> As detailed in <see cref="System.Math.Floor"></see></item>
    /// <item><c>exp</c> As detailed in <see cref="System.Math.Exp"></see></item>
    /// <item><c>sqr</c> As detailed in <see cref="System.Math.Sqrt"></see></item>
    /// <item><c>abs</c> As detailed in <see cref="System.Math.Abs"></see></item>
    /// </list>
    /// </remarks>
    private static readonly string[] Functions = {
			"log",
			"int",
			"exp",
			"sqr",
			"abs",
			"min",
			"max",
			"if",
            "gety"
		};


    /// <summary>
    /// Evaluates a <see cref="Functions">function</see>.
    /// </summary>
    /// <param name="o">Either a <see cref="System.Double">double</see> or a
    ///		<see cref="System.String">string</see>
    ///	</param>
    /// <returns>The result returned by the overload <see cref="funcEval"/>
    ///		method called.
    ///	</returns>
    /// <remarks>This checks whether the input <see cref="System.Object">object
    ///		</see> is a <see cref="System.Double">double</see> or a 
    ///		<see cref="System.String">string</see>.  The function then returns the 
    ///		result of the <see cref="funcEval"/> method called.
    /// </remarks>
    private static double funcEval(object o)
    {
        if (o.GetType() == typeof(string))
        {
            return funcEval((string)o);
        }
        else if (o.GetType() == typeof(double))
        {
            return funcEval((double)o);
        }
        return 0;
    }
    /// <summary>
    /// Evaluates a <see cref="Functions">function</see>.
    /// </summary>
    /// <param name="s">The text to evaluate</param>
    /// <returns>The value of the evaluation</returns>
    /// <remarks>This will return the result of one of the valid <see cref="Hudson.Common.Evaluator.Functions">functions</see></remarks>
    private static double funcEval(string s)
    {
        double d;

        try
        {
            if (s.ToLower().IndexOf("log") != -1 && s.Length > 3)
            {
                return Math.Log(double.Parse(s.Substring(s.ToLower().IndexOf("log") + 3)));
            }
            else if (s.ToLower().IndexOf("int") != -1 && s.Length > 3)
            {
                return Math.Floor(double.Parse(s.Substring(s.ToLower().IndexOf("int") + 3)));
            }
            else if (s.ToLower().IndexOf("exp") != -1 && s.Length > 3)
            {
                return Math.Exp(double.Parse(s.Substring(s.ToLower().IndexOf("exp") + 3)));
            }
            else if (s.ToLower().IndexOf("sqr") != -1 && s.Length > 3)
            {
                return Math.Sqrt(double.Parse(s.Substring(s.ToLower().IndexOf("sqr") + 3)));
            }
            else if (s.ToLower().IndexOf("abs") != -1 && s.Length > 3)
            {
                return Math.Abs(double.Parse(s.Substring(s.ToLower().IndexOf("abs") + 3)));
            }
            else if (s.ToLower().IndexOf("min") != -1 && s.Length > 3)
            {
                string[] args = s.Substring(s.ToLower().IndexOf("min") + 3).Split('|');
                return Math.Min(double.Parse(args[0]), double.Parse(args[1]));
            }
            else if (s.ToLower().IndexOf("max") != -1 && s.Length > 3)
            {
                string[] args = s.Substring(s.ToLower().IndexOf("max") + 3).Split('|');
                return Math.Max(double.Parse(args[0]), double.Parse(args[1]));
            }
            else if (s.ToLower().IndexOf("if") != -1 && s.Length > 2)
            {
                char op = s[s.IndexOfAny(ifseparators)];
                string[] args = s.Substring(s.ToLower().IndexOf("if") + 2).Split(ifseparators);
                if (args.Length > 1 && 
                    ((op == '=' || op == (char)1) && args[0] == args[1] ||
                    (op == '>' || op == (char)2) && double.Parse(args[0]) > double.Parse(args[1]) ||
                    (op == '<' || op == (char)3) && double.Parse(args[0]) < double.Parse(args[1]) ||
                    (op == '!' || op == (char)4) && args[0] != args[1])) return double.Parse(args[2]);
                else if (args.Length > 2) return double.Parse(args[3]);
                return 0;
            }
            else if (s.ToLower().IndexOf("gety") != -1 && s.Length > 4)
            {
                string[] args = s.Substring(s.ToLower().IndexOf("gety") + 4).Split('|');
                if (args.Length > 1)
                    return (double)UserDefinedFunctions.GetY(double.Parse(args[0]), double.Parse(args[1]), string.Join(",", args.Skip(2)));
                else return 0;
            }
            else if (s == "")
            {
                d = 0;
            }
            else
            {
                if (!double.TryParse(s, out d)) d = 0;
            }
        }
        catch
        {
            d = 0;
        }
        return d;
    }

    /// <summary>
    /// Evaluates a <see cref="Functions">function</see>.
    /// </summary>
    /// <param name="d">The number</param>
    /// <returns>The number</returns>
    /// <remarks>This will return the number passed to it.</remarks>
    private static double funcEval(double d)
    {
        return d;
    }


    /// <summary>
    ///		Check if a <see cref="Functions">function</see> exists at a 
    ///		given place in the arithmetic expression.
    /// </summary>
    /// <param name="s">The arithmetic expression</param>
    /// <param name="x">The current position of the parser</param>
    /// <returns>
    ///		True if no function call exists after the current position
    /// </returns>
    /// <remarks>This in only used in <see cref="subEval"/>.</remarks>
    private static bool noFunc(string s, int x)
    {
        for (int i = 0; i < Functions.Length; i++)
        {
            if ((s.ToLower().IndexOf(Functions[i]) == x - Functions[i].Length) && (x >= Functions[i].Length))
            {
                return false;
            }
        }
        if (x > 0)
            for (int i = 0; i < ifseparators.Length; i++)
                if (s[x - 1] == ifseparators[i]) return false;
        return true;
    }

    static char[] nonoperators = { 'E', '|', (char)1, (char)2, (char)3, (char)4, '=', '>', '<', '!' };
    static char[] operators = { '+', '-', '*', '/', '^', '%' };

    /// <summary>Evaluate a sub expression</summary>
    /// <param name="s">The expression</param>
    /// <returns>The evaluation of the expression</returns>
    /// <remarks>This is where the supported operators are analysed.  
    ///		<see cref="funcEval"/> is called to evaluate functions.
    ///	</remarks>
    private static double subEval(string s)
    {
        s = s.Replace("--", "+");
        ArrayList op = new ArrayList();
        ArrayList ex = new ArrayList();
        int x = s.IndexOfAny(operators);
        int y = 0;

        while (x != -1)
        {
            if (((s.IndexOfAny(nonoperators) != x - 1) && (x != 0)) && noFunc(s, x))
            {
                ex.Add(s.Substring(y, x - y));
                op.Add(s.Substring(x, 1));
                y = x + 1;
                x = s.IndexOfAny(operators, y);
            }
            else
            {
                x = s.IndexOfAny(operators, x + 1);
            }
            if (x == y)
            {
                x = s.IndexOfAny(operators, y + 1);
            }
        }

        ex.Add(s.Substring(y));

        while (op.Contains("^"))
        {
            x = op.IndexOf("^");
            ex[x + 1] = Math.Pow(funcEval(ex[x]), funcEval(ex[x + 1]));
            ex.RemoveAt(x);
            op.RemoveAt(x);
        }

        while (op.Contains("*") || op.Contains("/"))
        {
            x = op.IndexOf("*");
            y = op.IndexOf("/");
            if (((x < y) && (x != -1)) || (y == -1))
            {
                ex[x + 1] = funcEval(ex[x]) * funcEval(ex[x + 1]);
                ex.RemoveAt(x);
                op.RemoveAt(x);
            }
            else
            {
                ex[y + 1] = funcEval(ex[y]) / funcEval(ex[y + 1]);
                ex.RemoveAt(y);
                op.RemoveAt(y);
            }
        }

        while (op.Contains("%"))
        {
            x = op.IndexOf("%");
            ex[x + 1] = funcEval(ex[x]) % funcEval(ex[x + 1]);
            ex.RemoveAt(x);
            op.RemoveAt(x);
        }

        while (op.Contains("+") || op.Contains("-"))
        {
            x = op.IndexOf("+");
            y = op.IndexOf("-");
            if (((x < y) && (x != -1)) || (y == -1))
            {
                ex[x + 1] = funcEval(ex[x]) + funcEval(ex[x + 1]);
                ex.RemoveAt(x);
                op.RemoveAt(x);
            }
            else
            {
                ex[y + 1] = funcEval(ex[y]) - funcEval(ex[y + 1]);
                ex.RemoveAt(y);
                op.RemoveAt(y);
            }
        }

        return funcEval(ex[0]);
    }
    static char[] ifseparators = { '|', (char)1, (char)2, (char)3, (char)4, ',', '=', '>', '<', '!' };
    static char[] separators = { ',', '=', '>', '<', '!' };
    static List<char> ifreplacers = new List<char>(new char[] { ',', '=', '>', '<', '!' });

    /// <summary>
    /// Evaluates the arithmetic expression.
    /// </summary>
    /// <param name="s">The arithmetic expression</param>
    /// <returns>The evaluation of the input expression.</returns>
    /// <remarks>This will evaluate simple arithmetic expressions and also a 
    ///		limited list of <see cref="Functions">functions</see>
    ///	</remarks>
    public static double Eval(string s)
    {
        int x = 0;
        int[] pos = new int[3];
        string temp;
        s = s.Replace(" ", "");
        s = s.Replace("mod", "%");
        s = s.Replace("'", "").Replace("\"", "");

        try
        {
            while (s.IndexOf("(", x) != -1)
            {
                pos[0] = s.IndexOf("(", x);
                pos[1] = s.IndexOf("(", pos[0] + 1);
                pos[2] = s.IndexOf(")", pos[0] + 1);
                if (pos[2] > -1 && (pos[1] == -1 || pos[2] < pos[1]))
                {
                    x = 0;
                    string eval = s.Substring(pos[0] + 1, pos[2] - (pos[0] + 1));
                    if (eval.IndexOfAny(separators) != -1)
                    {
                        int p = -1;
                        string pop = "";
                        while ((p = eval.IndexOfAny(separators, p + 1)) != -1)
                        {
                            pop += eval[p];
                        }
                        p = 0;
                        string[] evals = eval.Split(separators);
                        temp = "";
                        foreach (string subeval in evals)
                        {
                            if (temp != "") temp += ifreplacers.Contains(pop[p++]) ? ifseparators[ifreplacers.IndexOf(pop[p - 1])] : pop[p - 1];
                            temp += subEval(subeval).ToString();
                        }
                    }
                    else
                        temp = subEval(eval).ToString();
                    s = s.Replace(s.Substring(pos[0], pos[2] - (pos[0] - 1)), temp);
                }
                else
                {
                    if (pos[0] == -1 && pos[1] == -1 && pos[2] > -1) return 0;
                    if (pos[0] > -1 && pos[1] == -1 && pos[2] == -1) return 0;
                    x = pos[0] + 1;
                }
            }

            return subEval(s);
        }
        catch
        {
            return 0;
        }
    }

}
