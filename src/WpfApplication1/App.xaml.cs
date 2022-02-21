using System;
using System.Net.Mail;
using System.Windows;
using AssetBuilder.Properties;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AssetBuilder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
        }

        public static void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Task t = new Task(() => WriteError(e.Exception));
            t.Start();
            //WriteError(e.Exception);
            e.Handled = true;
        }

		public static void WriteError(Exception ex)
		{
		    var then = DateTime.Now;
			var mailMessage = new MailMessage("assetbuilder@expert-24.com", "helpdesk@expert-24.com");
            //mailMessage.To.Add("helpdesk@expert-24.com");

            var ErrorMessage = "";
            var topMessage = ex.Message;
            var source = ex.Source;
		    var method = ex.TargetSite.ToString();
			var c = 0;
			while (ex != null)
			{
				ErrorMessage += "-------------------------------------------------------------" + Environment.NewLine;
				ErrorMessage += $"-----              Exception {++c}                         ------" + Environment.NewLine;
				ErrorMessage += "-------------------------------------------------------------" + Environment.NewLine + Environment.NewLine;
				ErrorMessage += ExceptionError(ex);
				ex = ex.InnerException;
			}

			ErrorMessage += Environment.NewLine;
			ErrorMessage += "Last procedure call\r\n";
			ErrorMessage += Settings.Default.WebService + "\r\n";
			ErrorMessage += DataAccess.LastCommand;

			mailMessage.Priority = MailPriority.High;
			mailMessage.Body = ErrorMessage;

            DataAccess.AddLastCommand(topMessage, ErrorMessage.CDataWrap(), then - DateTime.Now);

   //         try
   //         {
   //             DataAccess.AddLastCommand(method, ErrorMessage.CDataWrap(), new TimeSpan());
			//    var client = new SmtpClient {Timeout = 5000};
   //             client.EnableSsl = true;
   //             client.Port = 587;
			//    client.Send(mailMessage);
   //         }
			//catch (Exception ee)
			//{
			//    string emailerror = "";
   //             //System.Diagnostics.EventLog.WriteEntry("AssetBuilder", mailMessage.Body, System.Diagnostics.EventLogEntryType.Error, 5001);
   //             if (source == null || source != "mscorlib")
			//	{
   //                 emailerror = "\r\n\r\n-------------------------------------------------------------\r\n";
   //                 emailerror += "-----                   Emailer error                  ------\r\n";
   //                 emailerror += "-------------------------------------------------------------\r\n\r\n" + ee;

   //                 Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
   //                 {
   //                     MessageBox.Show(mailMessage.Body + emailerror, ee.Message, MessageBoxButton.OK);
   //                 }));
			//	}
   //             DataAccess.AddLastCommand(ee.Message, emailerror.CDataWrap(), then - DateTime.Now);
   //         }
        }

        private static string ExceptionError(Exception ex)
        {
            string ErrorMessage = ex.Message + "\r\n\r\nAn error has occurred. Error details to follow:" + Environment.NewLine + Environment.NewLine;
            if(Window1.window != null && Window1.windowTitle != null) ErrorMessage += "Version       : " + Window1.windowTitle + Environment.NewLine;
            ErrorMessage += "Workstation   : " + Environment.MachineName + Environment.NewLine;
            ErrorMessage += "Date          : " + DateTime.Now.ToString() + Environment.NewLine;
            ErrorMessage += "Error Source  : " + ex.Source + Environment.NewLine;
            ErrorMessage += "Error Message : " + ex.Message + Environment.NewLine;
            ErrorMessage += "Stack Trace   : " + Environment.NewLine + ex.StackTrace + Environment.NewLine + Environment.NewLine + Environment.NewLine;
            return ErrorMessage;
        }
    }
}
