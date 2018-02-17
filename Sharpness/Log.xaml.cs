using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Sharpness
{
    /// <summary>
    /// Lógica interna para Log.xaml
    /// </summary>
    public partial class Log : Window
    {
        private List<string> logMessages;
        public event Action<bool> resetBit;
        public Log()
        {
            InitializeComponent();
            logMessages = new List<string>();
            dbgText.Text = "Initialized..\n";
            
        }

        public void EnableModuleSubscriptions()
        {
            ((MainWindow)Application.Current.MainWindow).emulator.LogExternal += Emulator_LogExternal;
        }

        private void Emulator_LogExternal(string obj)
        {
            this.LogMessage(obj);
        }

        public void LogMessage(string message)
        {
            logMessages.Add(message);
            dbgText.Text = GenerateString();
            dbgText.ScrollToEnd();
        }

        private string GenerateString()
        {
            string finalmessage = "";
            int size = 0;

            //Render a maximum of 120 messages on log
            if (logMessages.Count > 120)
            {
                size = 120;
            }
            else
            {
                size = logMessages.Count;
            }

            for (int i = 0; i < size; i++)
            {
                finalmessage += logMessages[i] + "\n";
            }
            return finalmessage;
        }

        private void logWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(resetBit != null)
            {
                resetBit(false);
            }
            
        }
    }
}
