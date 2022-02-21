using AssetBuilder.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using Visio = Microsoft.Office.Interop.Visio;

namespace AssetBuilder.Classes
{
    static class VisioInterface
    {
        public static Visio.Application GetVisio()
        {
            Visio.Application vis = null;
            MessageBoxResult edr = 0;
            DateTime then = DateTime.Now;

            // find visio
            do
            {
                try
                {
                    vis = (Visio.Application)Marshal.GetActiveObject("Visio.Application");
                }
                catch (Exception ex)
                {
                    string message = "Marshal.GetActiveObject(\"Visio.Application\")"; //"Visio is not available!\n\nIt is either closed or not responding. The error returned from the system was:-\n\n" + ex.Message + "\n\nWould you like to retry?";
                    DataAccess.AddLastCommand(message, ex.ToString().CDataWrap(), then - DateTime.Now);
                    //edr = MessageBox.Show(message, "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                }
            } while (vis == null && edr == MessageBoxResult.Yes);

            return vis;
        }

        static Dictionary<int, Visio.Page> Pages;

        public static Progress Track(string nodes, HashSet<int> ids, System.Windows.Media.Color TrackingColour)
        {
            Visio.Application vis = GetVisio();
            if (vis == null) return null;
            Pages = GetPages(vis);

            var process = new List<Dictionary<string, int>>();
            string[] lines = nodes.Split('\n');
            foreach (string line in lines)
            {
                if (line.IndexOf("NodeType :") == -1) continue;
                Dictionary<string, int> node = new Dictionary<string, int>();
                string[] values = line.Substring(line.IndexOf("NodeType :")).Split(':', ',');
                for (int i = 0; i < values.Length - 1; i += 2)
                {
                    node.Add(values[i].Trim(), int.Parse(values[i + 1]));
                }
                if (node.ContainsKey("Algo") && node.ContainsKey("Node") && node.ContainsKey("AnswerID") && (ids.Count == 0 || ids.Contains(node["Algo"])))
                    process.Add(node);
            }

            using (var bw = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true })
            {
                bw.DoWork += delegate (object sender, DoWorkEventArgs e)
                {
                    var worker = sender as BackgroundWorker;
                    int i = 0;
                    int c = process.Count;
                    foreach (var node in process)
                    {
                        if (worker.CancellationPending) break;
                        HighLightShapes(vis, node["Algo"], node["Node"], node["AnswerID"], TrackingColour);
                        worker.ReportProgress((int)((++i / (float)c) * 100));
                    }
                };
                Progress p = new Progress(bw);
                bw.ProgressChanged += delegate (object sender, ProgressChangedEventArgs e)
                {
                    p.pbStatus.Value = e.ProgressPercentage;
                };
                return p;
            }
        }

        static Dictionary<int, Visio.Page> GetPages(Visio.Application vis)
        {
            var pages = new Dictionary<int, Visio.Page>();
            Visio.Shape shp;
            foreach (Visio.Window win in vis.Windows)
                foreach (Visio.Page pge in win.Document.Pages)
                    if (pge.Shapes.Count > 0)
                    {
                        try { shp = pge.Shapes["Algo Start"]; }
                        catch { shp = null; }
                        if (shp != null)
                        {
                            string mAlgoID = null;
                            int AlgoID = 0;
                            if (shp.get_CellExists("Prop.ID", 0) != 0)
                                mAlgoID = shp.get_Cells("Prop.ID").Formula;
                            else if (shp.get_CellExists("Prop.AlgoID", 0) != 0)
                                mAlgoID = shp.get_Cells("Prop.AlgoID").Formula;
                            if (mAlgoID != null && int.TryParse(mAlgoID, out AlgoID) && !pages.ContainsKey(AlgoID)) pages.Add(AlgoID, pge);
                        }
                    }
            return pages;
        }

        public static bool HighLightShapes(Visio.Application vis, int AlgoID, int NodeID, int AnswerID, System.Windows.Media.Color TrackingColour)
        {
            if (Pages.ContainsKey(AlgoID))
            {
                if (Pages[AlgoID] == null) return false;
                return FindShape(NodeID, AnswerID, Pages[AlgoID], TrackingColour);
            }
            else return false;

            //Visio.Shape shp;
            ////bool visioFound = false;

            //foreach (Visio.Window win in vis.Windows)
            //	foreach (Visio.Page pge in win.Document.Pages)
            //		if (pge.Shapes.Count > 0)
            //		{
            //			try { shp = pge.Shapes["Algo Start"]; }
            //			catch { shp = null; }
            //			if (shp != null)
            //			{
            //				string mAlgoID;
            //				if (shp.get_CellExists("Prop.ID", 0) != 0)
            //					mAlgoID = shp.get_Cells("Prop.ID").Formula;
            //				else if (shp.get_CellExists("Prop.AlgoID", 0) != 0)
            //                             mAlgoID = shp.get_Cells("Prop.AlgoID").Formula;
            //				if (mAlgoID == AlgoID.ToString())
            //                         {
            //                             //visioFound = true;
            //                             Pages.Add(AlgoID, pge);
            //                             return FindShape(NodeID, AnswerID, pge, TrackingColour);
            //                         }
            //                     }
            //		}
            //         Pages.Add(AlgoID, null);
            //return false;
        }

        private static bool FindShape(int NodeID, int AnswerID, Visio.Page pge, System.Windows.Media.Color TrackingColour)
        {
            Visio.Shape shp;
            try { shp = pge.Shapes.ItemFromID[NodeID]; }
            catch { shp = null; }
            if (shp != null && shp.get_CellExists("Prop.ID", 0) != 0)
            {
                HighLightShape(shp, TrackingColour);
                if (AnswerID > 0)
                {
                    Visio.Connects con = shp.FromConnects;
                    for (int i = 1; i <= con.Count; i++)
                    {
                        if (con[i].FromSheet.Connects.Count > 1)
                        {
                            Visio.Shape cshp = con[i].FromSheet.Connects[2].ToSheet;
                            if (cshp.get_CellExists("Prop.ID", 0) != 0 && cshp.get_Cells("Prop.ID").Formula == AnswerID.ToString())
                            {
                                HighLightShape(con[i].FromSheet.Connects[2].ToSheet, TrackingColour);
                                break;
                            }
                        }
                    }
                }
                return true;
            }

            return false;
        }

        private static void SelectShape(Visio.Shape shp, Visio.Window win, Visio.Page pge)
        {
            win.Activate();
            win.Page = pge.Name;
            win.Select(shp, 258);
            double X = shp.get_Cells("PinX").ResultIU;
            double Y = shp.get_Cells("PinY").ResultIU;

            win.ScrollViewTo(X, Y);
        }

        private static void HighLightShape(Visio.Shape shp, System.Windows.Media.Color TrackingColour)
        {
            shp.get_Cells("LineWeight").Formula = "5 pt";
            shp.get_Cells("LineColor").Formula = formulaAdd(shp.get_Cells("LineColor").Formula, TrackingColour);
        }

        static string formulaAdd(string formula, System.Windows.Media.Color TrackingColour)
        {
            //formula = string.Format("RGB({0},{1},{2})", colorDialog1.Color.R, colorDialog1.Color.G, colorDialog1.Color.B);
            int i = 0;
            var split = formula.Split('(', ',', ')').Where(f => int.TryParse(f, out i)).Select(f => i).ToArray();
            var r = TrackingColour.R;
            var g = TrackingColour.G;
            var b = TrackingColour.B;
            if (split.Length == 3)
            {
                r = (byte)Math.Min(255, r + split[0]);
                g = (byte)Math.Min(255, g + split[1]);
                b = (byte)Math.Min(255, b + split[2]);
            }
            formula = string.Format("RGB({0},{1},{2})", r, g, b);
            return formula;
        }
    }
}
