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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Windows.Threading;

namespace Sharpness
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DispatcherTimer timer;
        Log logwin;

        //Tells whether log window is visible or not
        bool logOpen = false;

        //Actual loaded gamecart
        GameCart game;

        //Load CPU core
        CPU emulator;

        public MainWindow()
        {
            InitializeComponent();
            emulator = new CPU();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += timer_Tick;

        }

        private void Logwin_resetBit(bool obj)
        {
            //Update from another form closing
            logOpen = false;
        }

        private void MainWindow1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (logOpen)
            {
                logwin.Close();
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (logOpen)
                {
                    
                }

                //txtDebug.Text += "PC: " + PC.ToString("X4") + "\n";
                emulator.Emulate();
            }));
        }

        private void Window(object sender, EventArgs e)
        {

        }


        private void viewLog_Checked(object sender, RoutedEventArgs e)
        {
            logwin = new Log();
            logwin.Show();
            Application curApp = Application.Current;
            Window mainWindow = curApp.MainWindow;
            logwin.Left = mainWindow.Left + (mainWindow.ActualWidth);
            logwin.Top = mainWindow.Top;
            logOpen = true;
            logwin.resetBit += Logwin_resetBit;
        }

        private void Emulator_LogExternal(string obj)
        {
            logwin.LogMessage(obj);
        }

        private void runCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            timer.Start();
        }

        private void runCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void viewLog_Unchecked(object sender, RoutedEventArgs e)
        {
            logwin.Close();
            logOpen = false;
        }

        private void Window_Loaded(object sender, EventArgs e)
        {
            if (logOpen)
            {
                Application curApp = Application.Current;
                Window mainWindow = curApp.MainWindow;
                logwin.Left = mainWindow.Left + (mainWindow.ActualWidth);
                logwin.Top = mainWindow.Top;
            }
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.RestoreDirectory = true;
            fileDialog.ShowDialog();

            //Review code below

            if (File.Exists(fileDialog.FileName))
            {
                using (BinaryReader reader = new BinaryReader(File.Open(fileDialog.FileName, FileMode.Open)))
                {
                    game = new GameCart(reader, fileDialog.SafeFileName);
                    logwin.LogMessage(game.GetGameInformation());

                    //Setup game on memory
                    if ((game.NumberOfPRGBanks) == 1)
                    {
                        for (int i = 0; i < game.data.Length; i++)
                        {
                            emulator.mem[0x8000 + i] = game.data[i];
                            emulator.mem[0xC000 + i] = game.data[i];
                        }
                        logwin.LogMessage("PRG Size = " + game.NumberOfPRGBanks + "; Mirroring PRG...");
                    }
                    else if (game.NumberOfPRGBanks == 2)
                    {
                        logwin.LogMessage("PRG Size = " + game.NumberOfPRGBanks + "; Not mirroring PRG...");
                        for (int i = 0; i < game.data.Length; i++)
                        {
                            emulator.mem[0x8000 + i] = game.data[i];
                        }
                    }
                    else
                    {
                        for (int i = 0; i < (0x10000 - 0x8000); i++)
                        {
                            emulator.mem[0x8000 + i] = game.data[i];
                        }
                    }
                }
            }
            //End of ROM read and memory setup
            emulator.LogExternal += Emulator_LogExternal;
            emulator.InitVM();
        }



        private void btnStep_Click(object sender, RoutedEventArgs e)
        {
            //txtDebug.Text = "Opcode: " + mem[PC].ToString("X2") + "\n";
            //txtDebug.Text += "PC: " + PC.ToString("X4") + "\n";
            emulator.Emulate();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            emulator.Emulate();
        }
    }
}
