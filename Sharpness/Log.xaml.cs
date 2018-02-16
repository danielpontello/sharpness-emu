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
        public event Action<bool> resetBit;
        public Log()
        {
            InitializeComponent();
            dbgText.Text = "Initialized..\n";
        }

        public void LogMessage(string message)
        {
            dbgText.Text += message + "\n";
            dbgText.ScrollToEnd();
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
