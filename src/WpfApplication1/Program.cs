using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Linq;

namespace AssetBuilder
{
	class Program
	{
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static private extern IntPtr SendMessage(IntPtr hwnd, uint Msg, int wParam, int lParam);
		private const int RF_TESTMESSAGE = 0xA123;
		static Process process = Process.GetCurrentProcess();
		static public bool newVersionAvailable = false;

        private static string _AssetBuilderTitle;

        public static string AssetBuilderTitle
        {
            get
            {
                if (_AssetBuilderTitle == null)
                {
                    if (ApplicationDeployment.IsNetworkDeployed) _AssetBuilderTitle += " " + ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                    else _AssetBuilderTitle += " " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    _AssetBuilderTitle += " - " + Properties.Settings.Default.WebService;
                }
                return _AssetBuilderTitle;
            }
            set
            {
                _AssetBuilderTitle = value;
            }
        }

        public static int GetNodeType(string nodetype)
        {
            if (nodetype == "Algo") return 1;
            if (nodetype == "Question") return 2;
            if (nodetype == "Answer") return 3;
            if (nodetype == "Conclusion") return 4;
            if (nodetype == "Bullet") return 5;
            if (nodetype == "Track") return -1;
            if (nodetype == "NewTrack") return -2;
            if (nodetype == "ScriptAssets") return -3;
            return 0;
        }

        [STAThread]
		static void Main()
		{
            //System.Windows.Application app = new System.Windows.Application() { StartupUri = new Uri("Window2.xaml", UriKind.Relative) };
            //app.Run();
            //return;
            System.Net.ServicePointManager.SecurityProtocol &= ~System.Net.SecurityProtocolType.Ssl3;
            System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;

            string path = Path.GetDirectoryName(Application.ExecutablePath);
			Directory.SetCurrentDirectory(path);
			SetRegistry();

			string[] cmdLineArgs = System.Environment.GetCommandLineArgs();
			if (cmdLineArgs.Length > 1)
			{
				//string msg = "";
				//foreach (var item in cmdLineArgs)
				//{
				//    msg += item + Environment.NewLine;
				//}
				//MessageBox.Show(msg);
				string[] param = cmdLineArgs[1].Split(':', '.');
				if (param.Length < 3) return;
			    int nt = GetNodeType(param[1]);
				if (nt > 0)
				{
					List<Process> processes = new List<Process>(Process.GetProcessesByName("AssetBuilder"));
					if (System.Diagnostics.Debugger.IsAttached)
						processes.AddRange(Process.GetProcessesByName("AssetBuilder.vshost"));

					processes.RemoveAll(f => f.SessionId != process.SessionId || f.Id == process.Id);

					int id = 0;
					if (!int.TryParse(param[2], out id)) return;

					if (processes.Count > 0)
					{
						SendMessage(processes[0].MainWindowHandle, RF_TESTMESSAGE, (int)id, (int)nt);
					}
					else
					{
						Window1.StartupType = nt;
						Window1.StartupID = id;
						App at = new App();
						at.InitializeComponent();
						at.Run();
					}
				}
                else if (nt == -1)
                {
                    qcat.BuilderDefaults = DataAccess.getDataNode("ab_builderdefaults", null, false);
                    App at = new App();
                    at.Run(new Controls.AlgoTracker { Url = cmdLineArgs[1].Substring(19) });
                }
                else if (nt == -2)
                {
                    qcat.BuilderDefaults = DataAccess.getDataNode("ab_builderdefaults", null, false);
                    App at = new App();
                    at.Run(new Controls.AlgoTracker());
                }
                else if (nt == -3)
                {
                    qcat.BuilderDefaults = DataAccess.getDataNode("ab_builderdefaults", null, false);
                    App at = new App();
                    at.Run(new Controls.AlgoLoader { Url = cmdLineArgs[1].Substring(26) });
                }
                return;
			}

			System.Windows.SplashScreen s = new System.Windows.SplashScreen("Splash2022.png");
			s.Show(true);
			try
			{
				if (ApplicationDeployment.IsNetworkDeployed)
				{
					ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
					UpdateCheckInfo uci = ad.CheckForDetailedUpdate(false);
					if (uci.UpdateAvailable) newVersionAvailable = true;
				}
			}
			catch { }
			//App a = new App();
			//a.Run(new Window1());
			bool ok = true;
			//var m = new Mutex(true, "AssetBuilderMutex", out ok);
			if (!ok && !System.Diagnostics.Debugger.IsAttached)
			{
				System.Windows.MessageBox.Show("Another instance is already running.");
				return;
			}
			App a = new App();
			a.InitializeComponent();
			a.Run();
		}

		static private void SetRegistry()
		{
			string path = Assembly.GetAssembly(typeof(Window1)).Location;
			RegistryKey rk = Registry.CurrentUser.CreateSubKey("Software\\Classes\\assetbuilder");
			rk.SetValue("URL Protocol", "");
			rk.CreateSubKey("DefaultIcon").SetValue("", path);
			rk.CreateSubKey("Shell").CreateSubKey("Open").CreateSubKey("Command").SetValue("", string.Format("\"{0}\" \"%1\"", path));

            var fileName = System.IO.Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
            if (String.Compare(fileName, "devenv.exe", true) == 0 || String.Compare(fileName, "XDesProc.exe", true) == 0)
                return;
            using (var key = Registry.CurrentUser.CreateSubKey(
                String.Concat(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\", "FEATURE_BROWSER_EMULATION"),
                RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                key.SetValue(fileName, (UInt32)0x00002ee1, RegistryValueKind.DWord);
            }
        }
    }
}
