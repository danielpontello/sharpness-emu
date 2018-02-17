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
    /// Lógica interna para Disassembler.xaml
    /// </summary>
    public partial class Disassembler : Window
    {
        
        public Disassembler()
        {
            InitializeComponent();
            ((MainWindow)Application.Current.MainWindow).emulator.CpuStats += Emulator_CpuStats; ;

        }

        private void Emulator_CpuStats(byte[] arg1, ushort arg2)
        {
            memoryList.Items.Clear();
            for (int i = 0; i < arg1.Length; i++)
            {
                var row = new { Address = i.ToString("X4"), Data = arg1[i].ToString("X2") };
                memoryList.Items.Add(row);
                
            }
            memoryList.SelectedIndex = arg2;
            memoryList.ScrollIntoView(memoryList.Items[arg2]);
            PCBox.Text = Convert.ToString(arg2);
        }
    }
}
