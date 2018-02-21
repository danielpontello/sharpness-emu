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

        private void Emulator_CpuStats(byte[] arg1, ushort arg2, bool c, bool z, bool i, bool d, bool v, bool n, byte x, byte y, byte a)
        {
            memoryList.Items.Clear();
            for (int m = 0; m < arg1.Length; m++)
            {
                var row = new { Address = m.ToString("X4"), Data = arg1[m].ToString("X2") };
                memoryList.Items.Add(row);
                
            }
            memoryList.SelectedIndex = arg2;
            memoryList.ScrollIntoView(memoryList.Items[arg2]);
            PCBox.Text = arg2.ToString("X2");

            carryFlag.Text = c.ToString();
            zeroFlag.Text = z.ToString();
            interruptDisableFlag.Text = i.ToString();
            decimalModeFlag.Text = d.ToString();
            overflowFlag.Text = v.ToString();
            negativeFlag.Text = n.ToString();

            XBox.Text = x.ToString("X2");
            YBox.Text = y.ToString("X2");

            accumulatorBox.Text = a.ToString("X2");

        }
    }
}
