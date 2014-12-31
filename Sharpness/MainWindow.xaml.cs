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
        // 6502 ---------------------------------------------
        // Acumulador
        public byte A;

        // Registradores
        public byte X;
        public byte Y;

        // Flags
        public byte P;

        // Stack Pointer
        public byte S;

        // Program Counter
        public ushort PC;

        // Memória Principal
        public byte[] mem = new byte[0x10000];

        // Flags
        public bool N, V, B, D, I, Z, C;

        // ROM ----------------------------------------------
        // Tamanho da ROM PRG (em blocos de 16kb)
        byte PRGSize;

        // Tamanho da ROM CHR (em blocos de 8kb)
        byte CHRSize;

        // Tamango da ROM CHR (em blocos de 8kb; 0 = 8kb)
        byte PRGRamSize;

        // Tipo de TV (0 = NTSC, 1 = PAL)
        byte TVSystem;

        DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += timer_Tick;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                txtDebug.Text = "Opcode: " + mem[PC].ToString("X2") + "\n";
                txtDebug.Text += "PC: " + PC.ToString("X4") + "\n";
                Emulate();
            }));
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.RestoreDirectory = true;

            if (fileDialog.ShowDialog() == true)
            {
                if (File.Exists(fileDialog.FileName))
                {
                    using (BinaryReader reader = new BinaryReader(File.Open(fileDialog.FileName, FileMode.Open)))
                    {
                        byte[] header = reader.ReadBytes(16);
                        PRGSize = header[4];
                        CHRSize = header[5];
                        PRGRamSize = header[8];
                        TVSystem = header[9];

                        string info = "ROM Info" + "\n";
                        info += "PRG Size: " + 16384 * PRGSize + " bytes\n";
                        info += "CHR Size: " + 8192 * CHRSize + " bytes\n";
                        info += "TV System: " + (TVSystem == 1 ? "PAL" : "NTSC") + "\n";

                        byte[] PRG = reader.ReadBytes(16384 * PRGSize);

                        InitVM();

                        if (PRGSize == 1)
                        {
                            for (int i = 0; i < PRG.Length; i++)
                            {
                                info += "PRG Size = " + PRGSize + "; Mirroring PRG...";
                                mem[0x8000 + i] = PRG[i];
                                mem[0xC000 + i] = PRG[i];
                            }
                        }
                        else if (PRGSize == 2)
                        {
                            info += "PRG Size = " + PRGSize + "; Not mirroring PRG...";
                            for (int i = 0; i < PRG.Length; i++)
                            {
                                mem[0x8000 + i] = PRG[i];
                            }
                        }
                        else
                        {
                            for (int i = 0; i < (0x10000 - 0x8000); i++)
                            {
                                mem[0x8000 + i] = PRG[i];
                            }
                        }


                        txtDebug.Text = info;
                    }
                }
            }
        }

        void InitVM()
        {
            A = 0x00;
            X = 0x00;
            Y = 0x00;
            P = 0x34;
            PC = 0x08000;

            for (int i = 0x0000; i < 0x07FF; i++)
            {
                mem[i] = 0xFF;
            }
        }

        private void btnStep_Click(object sender, RoutedEventArgs e)
        {
            txtDebug.Text = "Opcode: " + mem[PC].ToString("X2") + "\n";
            txtDebug.Text += "PC: " + PC.ToString("X4") + "\n";
            Emulate();
        }

        private void Emulate()
        {
            switch (mem[PC])
            {
                case 0x00:
                    txtDebug.Text += "BRK - Break\n";
                    PC += 1;
                    break;

                case 0x01:
                    txtDebug.Text += "ORA - Bitwise OR with ACC | Indirect with X offset\n";
                    PC += 2;
                    break;

                case 0x08:
                    txtDebug.Text += "Stack | Push Processor status (from stack?)\n";
                    PC += 1;
                    break;

                case 0x09:
                    txtDebug.Text += "ORA - Bitwise OR with ACC | Relative\n";
                    PC += 2;
                    break;

                case 0x10:
                    txtDebug.Text += "BPL - Branch on Plus\n";
                    PC += 2;
                    break;

                case 0x18:
                    txtDebug.Text += "Flag | Clear Carry\n";
                    PC += 1;
                    break;

                case 0x20:
                    txtDebug.Text += "JSR - Jump to Subroutine | Absolute\n";
                    PC += 3;
                    break;

                case 0x21:
                    txtDebug.Text += "AND - Bitwise AND with ACC | Indirect with X Offset\n";
                    PC += 2;
                    break;


                case 0x25:
                    txtDebug.Text += "AND - Bitwise AND with ACC Zero Page\n";
                    PC += 2;
                    break;

                case 0x28:
                    txtDebug.Text += "Stack | Pull Processor status (from stack?)\n";
                    PC += 1;
                    break;

                case 0x29:
                    txtDebug.Text += "AND - Bitwise AND with ACC IMEDIATE\n";
                    PC += 2;
                    break;

                case 0x2D:
                    txtDebug.Text += "AND - Bitwise AND with ACC | Absolute \n";
                    PC += 3;
                    break;

                case 0x30:
                    txtDebug.Text += "Branch - on Minus\n";
                    PC += 2;
                    break;

                case 0x31:
                    txtDebug.Text += "AND - Bitwise AND with ACC | Indirect with Y Offset\n";
                    PC += 2;
                    break;

                case 0x38:
                    txtDebug.Text += "Flag | Set Carry\n";
                    PC += 1;
                    break;

                case 0x35:
                    txtDebug.Text += "AND - Bitwise AND with ACC | Zero Page with X offset\n";
                    PC += 2;
                    break;

                case 0x39:
                    txtDebug.Text += "AND - Bitwise AND with ACC | Absolute with Y Offset\n";
                    PC += 3;
                    break;

                case 0x3D:
                    txtDebug.Text += "AND - Bitwise AND with ACC | Absolute with X Offset\n";
                    PC += 3;
                    break;

                case 0x40:
                    txtDebug.Text += "RTI - Return from Interrupt \n";
                    PC += 1;
                    break;

                case 0x41:
                    txtDebug.Text += "EOR - Exclusive OR | Indirect with X offset\n";
                    PC += 2;
                    break;

                case 0x45:
                    txtDebug.Text += "EOR - Exclusive OR | Zero Page\n";
                    PC += 2;
                    break;

                case 0x46:
                    txtDebug.Text += "LSR - Logical Shift Right | Zero Page \n";
                    PC += 2;
                    break;

                case 0x48:
                    txtDebug.Text += "Stack | Push Accumulator (from Stack?)\n";
                    PC += 1;
                    break;

                case 0x49:
                    txtDebug.Text += "EOR - Exclusive OR | Immediate\n";
                    PC += 2;
                    break;

                case 0x50:
                    txtDebug.Text += "Branch - on Overflow Clear\n";
                    PC += 2;
                    break;

                case 0x4A:
                    txtDebug.Text += "LSR - Logical Shift Right | Accumulator \n";
                    PC += 1;
                    break;

                case 0x4C:
                    txtDebug.Text += "JMP - Jump | Absolute \n";
                    PC += 3;
                    break;

                case 0x4D:
                    txtDebug.Text += "EOR - Exclusive OR | Absolute\n";
                    PC += 3;
                    break;

                case 0x4E:
                    txtDebug.Text += "LSR - Logical Shift Right | Absolute \n";
                    PC += 3;
                    break;

                case 0x51:
                    txtDebug.Text += "EOR - Exclusive OR | Indirect with Y offset\n";
                    PC += 2;
                    break;

                case 0x55:
                    txtDebug.Text += "EOR - Exclusive OR | Zero Page with X offset\n";
                    PC += 2;
                    break;

                case 0x56:
                    txtDebug.Text += "LSR - Logical Shift Right | Zero page with X offset \n";
                    PC += 2;
                    break;

                case 0x58:
                    txtDebug.Text += "Flag | Clear Interrupt\n";
                    PC += 1;
                    break;

                case 0x59:
                    txtDebug.Text += "EOR - Exclusive OR | Absolute with Y offset\n";
                    PC += 3;
                    break;

                case 0x5D:
                    txtDebug.Text += "EOR - Exclusive OR | Absolute with X offset\n";
                    PC += 3;
                    break;

                case 0x5E:
                    txtDebug.Text += "LSR - Logical Shift Right | Absolute with X offset \n";
                    PC += 3;
                    break;

                case 0x60:
                    txtDebug.Text += "RTS - Return from Subroutine \n";
                    PC += 1;
                    break;

                case 0x68:
                    txtDebug.Text += "PLA - Pull Accumulator | Implied\n";
                    PC += 1;
                    break;

                case 0x6C:
                    txtDebug.Text += "JMP - Jump | Indirect \n";
                    PC += 3;
                    break;

                case 0x70:
                    txtDebug.Text += "Branch - on Overflow Set\n";
                    PC += 2;
                    break;

                case 0x78:
                    txtDebug.Text += "Flag | Set Interrupt\n";
                    PC += 1;
                    break;

                case 0x81:
                    txtDebug.Text += "STA - Store Accumulator | Indirect with X offset\n";
                    PC += 2;
                    break;

                case 0x85:
                    txtDebug.Text += "STA - Store Accumulator | Zero Page \n";
                    PC += 2;
                    break;

                case 0x88:
                    txtDebug.Text += "DEY - Decrement Y | Implied\n";
                    PC += 1;
                    break;

                case 0x8A:
                    txtDebug.Text += "TXA - Transfer X to A | Implied\n";
                    PC += 1;
                    break;

                case 0x8C:
                    txtDebug.Text += "STY - Store Y | Absolute\n";
                    PC += 3;
                    break;

                case 0x8D:
                    txtDebug.Text += "STA - Store Accumulator\n";
                    PC += 3;
                    break;

                case 0x90:
                    txtDebug.Text += "Branch - on Carry Clear\n";
                    PC += 2;
                    break;

                case 0x91:
                    txtDebug.Text += "STA - Store Accumulator | Indirect with Y offset\n";
                    PC += 2;
                    break;

                case 0x94:
                    txtDebug.Text += "STY - Store Y | Zero Page, X\n";
                    PC += 2;
                    break;

                case 0x95:
                    txtDebug.Text += "STA - Store Accumulator | Zero Page with X offset\n";
                    PC += 2;
                    break;

                case 0x98:
                    txtDebug.Text += "TYA - Transfer Y to A | Implied\n";
                    PC += 1;
                    break;

                case 0x99:
                    txtDebug.Text += "STA - Store Accumulator | Absolute with Y offset\n";
                    PC += 3;
                    break;

                case 0x9A:
                    txtDebug.Text += "TXS - Transfer X to Stack Pointer | Implied";
                    PC += 1;
                    break;

                case 0x9D:
                    txtDebug.Text += "STA - Store Accumulator | Absolute with X offset\n";
                    PC += 3;
                    break;

                case 0xA0:
                    txtDebug.Text += "LDY - Load Y | Immediate \n";
                    PC += 2;
                    break;

                case 0xA1:
                    txtDebug.Text += "LDA - Load Accumulator | Indirect with X offset \n";
                    PC += 2;
                    break;

                case 0xA2:
                    txtDebug.Text += "LDX - Load X | Immediate \n";
                    PC += 2;
                    break;

                case 0xA4:
                    txtDebug.Text += "LDY - Load register Y | Zero Page \n";
                    PC += 2;
                    break;

                case 0xA5:
                    txtDebug.Text += "LDA - Load Accumulator | Zero Page \n";
                    PC += 2;
                    break;

                case 0xA6:
                    txtDebug.Text += "LDX - Load X Register | Zero Page\n";
                    PC += 2;
                    break;

                case 0xA8:
                    txtDebug.Text += "TAY - Transfer A to Y | Implied\n";
                    PC += 1;
                    break;

                case 0xA9:
                    txtDebug.Text += "LDA - Load ACC | Immediate\n";
                    PC += 2;
                    break;

                case 0xAA:
                    txtDebug.Text += "TAX - Transfer A to X | Implied\n";
                    PC += 1;
                    break;

                case 0xAC:
                    txtDebug.Text += "LDY - Load register Y | Absolute\n";
                    PC += 3;
                    break;

                case 0xAD:
                    txtDebug.Text += "LDA - Load ACC | Absolute\n";
                    PC += 3;
                    break;

                case 0xAE:
                    txtDebug.Text += "LDX - Load X | Absolute\n";
                    PC += 3;
                    break;

                case 0xB0:
                    txtDebug.Text += "Branch - on Carry Set\n";
                    PC += 2;
                    break;

                case 0xB1:
                    txtDebug.Text += "LDA - Load Accumulator | Indirect with Y offset \n";
                    PC += 2;
                    break;

                case 0xB4:
                    txtDebug.Text += "LDY - Load register Y | Zero Page with X offset \n";
                    PC += 2;
                    break;

                case 0xB5:
                    txtDebug.Text += "LDA - Load Accumulator | Zero Page with X offset \n";
                    PC += 2;
                    break;

                case 0xB6:
                    txtDebug.Text += "LDX - Load X Register | Zero Page with X offset\n";
                    PC += 2;
                    break;

                case 0xB8:
                    txtDebug.Text += "Flag | Clear overflow\n";
                    PC += 1;
                    break;

                case 0xB9:
                    txtDebug.Text += "LDA - Load Accumulator | Absolute with Y offset \n";
                    PC += 3;
                    break;

                case 0xBA:
                    txtDebug.Text += "Stack | Transfer StackPtr to X\n";
                    PC += 1;
                    break;

                case 0xBC:
                    txtDebug.Text += "LDY - Load register Y | Absolute with X offset \n";
                    PC += 3;
                    break;

                case 0xBD:
                    txtDebug.Text += "LDA - Load ACC | Absolute, X\n";
                    PC += 3;
                    break;

                case 0xBE:
                    txtDebug.Text += "LDX - Load X Register | Absolute with X offset\n";
                    PC += 3;
                    break;

                case 0xC0:
                    txtDebug.Text += "CPY - Compare Y reg | Immediate\n";
                    PC += 2;
                    break;

                case 0xC1:
                    txtDebug.Text += "CMP - Compare Accumulator reg | Indirect with X offset\n";
                    PC += 2;
                    break;

                case 0xC4:
                    txtDebug.Text += "CPY - Compare Y reg | Zero Page\n";
                    PC += 2;
                    break;

                case 0xC5:
                    txtDebug.Text += "CMP - Compare Accumulator reg | Zero Page\n";
                    PC += 2;
                    break;

                case 0xC6:
                    txtDebug.Text += "DEC - Decrement Memory | Zero Page \n";
                    PC += 2;
                    break;

                case 0xC8:
                    txtDebug.Text += "INY - Increment Y | Implied\n";
                    PC += 1;
                    break;

                case 0xC9:
                    txtDebug.Text += "CMP - Compare | Immediate\n";
                    PC += 2;
                    break;

                case 0xCA:
                    txtDebug.Text += "DEX - Decrement X | Implied\n";
                    PC += 1;
                    break;

                case 0xCC:
                    txtDebug.Text += "CPY - Compare Y reg | Absolute \n";
                    PC += 3;
                    break;

                case 0xCD:
                    txtDebug.Text += "CMP - Compare Accumulator reg | Absolute\n";
                    PC += 3;
                    break;

                case 0xCE:
                    txtDebug.Text += "DEC - Decrement Memory | Absolute \n";
                    PC += 3;
                    break;

                case 0xD0:
                    txtDebug.Text += "Branch - on Not Equal (!=)\n";
                    PC += 2;
                    break;

                case 0xD1:
                    txtDebug.Text += "CMP - Compare Accumulator reg | Indirect with Y offset\n";
                    PC += 2;
                    break;

                case 0xD5:
                    txtDebug.Text += "CMP - Compare Accumulator reg | Zero Page with X offset\n";
                    PC += 2;
                    break;

                case 0xD6:
                    txtDebug.Text += "DEC - Decrement Memory | Zero Page with X offset\n";
                    PC += 2;
                    break;

                case 0xD8:
                    txtDebug.Text += "Flag | Clear decimal\n";
                    PC += 1;
                    break;

                case 0xD9:
                    txtDebug.Text += "CMP - Compare Accumulator reg | Absolute with Y offset\n";
                    PC += 2;
                    break;

                case 0xDD:
                    txtDebug.Text += "CMP - Compare Accumulator reg | Absolute with X offset\n";
                    PC += 3;
                    break;

                case 0xDE:
                    txtDebug.Text += "DEC - Decrement Memory | Absolute with X offset\n";
                    PC += 3;
                    break;

                case 0xE0:
                    txtDebug.Text += "CPX - Compare X reg | Immediate\n";
                    PC += 2;
                    break;

                case 0xE4:
                    txtDebug.Text += "CPX - Compare X reg | Zero Page\n";
                    PC += 2;
                    break;

                case 0xE6:
                    txtDebug.Text += "INC - Incremental mem | Zero Page\n";
                    PC += 2;
                    break;

                case 0xE8:
                    txtDebug.Text += "INX - Increment X | Implied\n";
                    PC += 1;
                    break;

                case 0xEA:
                    txtDebug.Text += "NOP - No operation (sleep) \n";
                    PC += 1;
                    break;

                case 0xEC:
                    txtDebug.Text += "CPX - Compare X Register | Absolute\n";
                    PC += 3;
                    break;

                case 0xEE:
                    txtDebug.Text += "INC - Increment Memory | Absolute\n";
                    PC += 3;
                    break;

                case 0xF0:
                    txtDebug.Text += "Branch - on Equal(==)\n";
                    PC += 2;
                    break;

                case 0xF6:
                    txtDebug.Text += "INC - Incremental mem | Zero Page with X offset\n";
                    PC += 2;
                    break;

                case 0xF8:
                    txtDebug.Text += "Flag | Set decimal\n";
                    PC += 1;
                    break;

                case 0xFE:
                    txtDebug.Text += "INC - Incremental mem | Absolute with X offset \n";
                    PC += 3;
                    break;

                default:
                    txtDebug.Text += "Undefined";
                    break;
            }
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            timer.Start();
        }
    }
}
