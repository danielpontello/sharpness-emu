using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharpness
{
    public class CPU
    {
        //Internals
        public event Action<string> LogExternal;
        public event Action<byte[], ushort, bool, bool, bool, bool, bool, bool, byte, byte, byte> CpuStats; //I know, this is ugly af

        // 6502 ---------------------------------------------
        const int CPUfreq = 1789773;
        private int cycles;

        // Acumulador
        private byte A;

        // Registradores
        private byte X;
        private byte Y;

        // Flags
        private byte P;

        // Stack Pointer
        private byte S;

        // Program Counter
        public ushort PC;

        // Memória Principal
        public byte[] mem = new byte[0x10000];

        // Flags
        private bool N, V, B, D, I, Z, C;
        private bool NMIReq, ResetReq, IRQReq = false;

        // Auxiliar para decodificação do Opcode com mais de um argumento
        private ushort argop;

        public void Emulate()
        {
            if (LogExternal != null)
            {
                LogExternal("");
                LogExternal("Opcode: " + mem[PC].ToString("X2"));
                LogExternal("PC: " + PC.ToString("X4"));
            }

            //Emulation routine begins
            if (NMIReq)
            {

            }
            if (IRQReq)
            {
                LogExternal("IRQ Handle");

            }

            switch (mem[PC])
            {
                case 0x00:
                    LogExternal("BRK - Break | Implicit ");
                    PC += 1;
                    PackCPUToByte();
                    push16(PC);
                    push(P);
                    PC = (ushort)(read16(0xFFFD));
                    B = true;

                    break;

                case 0x01:
                    LogExternal("ORA - Bitwise OR with ACC | Indirect with X offset -- Unreliable");
                    PC += 1;
                    byte hi = (byte)(mem[PC]);
                    A = (byte)(A | mem[(hi << 8) | X]);
                    checkZN(A);
                    PC += 1;
                    break;

                case 0x08:
                    LogExternal("PHP - Push Processor status (from stack?) | Implicit");
                    PackCPUToByte();
                    push(P);
                    PC += 1;
                    break;

                case 0x09:
                    LogExternal("ORA - Bitwise OR with ACC | Immediate");
                    PC += 1;
                    A = (byte)(A | mem[PC]);
                    checkZN(A);
                    PC += 1;
                    break;

                case 0x10:
                    LogExternal("BPL - Branch on Plus | Implicit ");
                    PC += 1;
                    var offset = mem[PC];
                    if (!N)
                    {
                        LogExternal("Jumped to " + (PC + offset) + " .");
                        PC += offset;
                    }
                    PC += 1;
                    break;

                case 0x18:
                    LogExternal("CLC - Clear Carry | Implicit");
                    C = false;
                    PC += 1;
                    break;

                case 0x20:
                    LogExternal("JSR - Jump to Subroutine | Absolute -- Unreliable execution order!");
                    PC += 2;
                    push16(PC);
                    PC = read16(PC);
                    break;

                case 0x21:
                    LogExternal("AND - Bitwise AND with ACC | Indirect with X Offset");
                    PC += 1;
                    A = (byte)(A & mem[(ushort)((mem[PC] << 8) | X)]);
                    checkZN(A);
                    PC += 1;
                    break;

                case 0x24:
                    LogExternal("BIT - Bit Test | Zero Page");
                    PC += 1;
                    if((byte)(A & mem[(ushort)(mem[PC])]) == 0)
                    {
                        Z = true;
                    }
                    else
                    {
                        Z = false;
                    }
                    if ((mem[(ushort)(mem[PC])] & 0b10000000) == 0b10000000)
                    {
                        N = true;
                    }
                    else
                    {
                        N = false;
                    }
                    if ((mem[(ushort)(mem[PC])] & 0b01000000) == 0b01000000)
                    {
                        V = true;
                    }
                    else
                    {
                        V = false;
                    }
                        PC += 1;
                    break;

                case 0x25:
                    LogExternal("AND - Bitwise AND with ACC | Zero Page");
                    PC += 1;
                    A = (byte)(A & mem[(ushort)(mem[PC])]);
                    checkZN(A);
                    PC += 1;
                    break;

                case 0x28:
                    LogExternal("PLP - Pull Processor status (from stack?) | Implicit");
                    UnpackByteToCpu();
                    PC += 1;
                    break;

                case 0x29:
                    LogExternal("AND - Bitwise AND with ACC | Immediate");
                    PC += 1;
                    A = (byte)(A & mem[PC]);
                    checkZN(A);
                    PC += 1;
                    break;

                case 0x2D:
                    LogExternal("AND - Bitwise AND with ACC | Absolute ");
                    A = (byte)(A & mem[read16(PC)]);
                    checkZN(A);
                    PC += 3;
                    break;

                case 0x30:
                    LogExternal("BMI - Branch on Minus | Implicit ");
                    PC += 1;
                    if (N)
                    {
                        PC = (ushort)(PC + mem[PC]);
                        LogExternal("Jumped to " + PC);
                    }
                    PC += 1;
                    break;

                case 0x31:
                    //txtDebug.Text += "AND - Bitwise AND with ACC | Indirect with Y Offset\n";
                    PC += 2;
                    break;

                case 0x38:
                    //txtDebug.Text += "SEC - Set Carry | Implicit \n";
                    C = true;
                    PC += 1;
                    break;

                case 0x35:
                    //txtDebug.Text += "AND - Bitwise AND with ACC | Zero Page with X offset\n";
                    PC += 2;
                    break;

                case 0x39:
                    //txtDebug.Text += "AND - Bitwise AND with ACC | Absolute with Y Offset\n";
                    PC += 3;
                    break;

                case 0x3D:
                    //txtDebug.Text += "AND - Bitwise AND with ACC | Absolute with X Offset\n";
                    PC += 3;
                    break;

                case 0x40:
                    //txtDebug.Text += "RTI - Return from Interrupt | Implicit \n";
                    PC += 1;
                    break;

                case 0x41:
                    //txtDebug.Text += "EOR - Exclusive OR | Indirect with X offset\n";
                    PC += 2;
                    break;

                case 0x45:
                    //txtDebug.Text += "EOR - Exclusive OR | Zero Page\n";
                    PC += 2;
                    break;

                case 0x46:
                    //txtDebug.Text += "LSR - Logical Shift Right | Zero Page \n";
                    PC += 2;
                    break;

                case 0x48:
                    //txtDebug.Text += "PHA - Push Accumulator (from Stack?) | Implicit \n";
                    PC += 1;
                    break;

                case 0x49:
                    //txtDebug.Text += "EOR - Exclusive OR | Immediate \n";
                    PC += 2;
                    break;

                case 0x50:
                    //txtDebug.Text += "BVC - Branch on Overflow Clear | Implicit \n";
                    PC += 2;
                    break;

                case 0x4A:
                    //txtDebug.Text += "LSR - Logical Shift Right | Accumulator \n";
                    PC += 1;
                    break;

                case 0x4C:
                    LogExternal("JMP - Jump | Absolute ");
                    PC = mem[read16(PC)];
                    PC += 3;
                    break;

                case 0x4D:
                    //txtDebug.Text += "EOR - Exclusive OR | Absolute \n";
                    PC += 3;
                    break;

                case 0x4E:
                    //txtDebug.Text += "LSR - Logical Shift Right | Absolute \n";
                    PC += 3;
                    break;

                case 0x51:
                    //txtDebug.Text += "EOR - Exclusive OR | Indirect with Y offset \n";
                    PC += 2;
                    break;

                case 0x55:
                    //txtDebug.Text += "EOR - Exclusive OR | Zero Page with X offset \n";
                    PC += 2;
                    break;

                case 0x56:
                    //txtDebug.Text += "LSR - Logical Shift Right | Zero page with X offset \n";
                    PC += 2;
                    break;

                case 0x58:
                    //txtDebug.Text += "CLI - Clear Interrupt | Implicit \n";
                    I = false;
                    LogExternal("Interrupt flag cleared.");
                    PC += 1;
                    break;

                case 0x59:
                    //txtDebug.Text += "EOR - Exclusive OR | Absolute with Y offset \n";
                    PC += 3;
                    break;

                case 0x5D:
                    //txtDebug.Text += "EOR - Exclusive OR | Absolute with X offset \n";
                    PC += 3;
                    break;

                case 0x5E:
                    //txtDebug.Text += "LSR - Logical Shift Right | Absolute with X offset \n";
                    PC += 3;
                    break;

                case 0x60:
                    LogExternal("RTS - Return from Subroutine | Implicit ");
                    PC = pop16();
                    PC += 1;
                    break;

                case 0x68:
                    //txtDebug.Text += "PLA - Pull Accumulator | Implied \n";
                    PC += 1;
                    break;

                case 0x6C:
                    //txtDebug.Text += "JMP - Jump | Indirect \n";
                    PC += 3;
                    break;

                case 0x70:
                    //txtDebug.Text += "BVS - Branch on Overflow Set | Implicit \n";
                    PC += 2;
                    break;

                case 0x78:
                    //txtDebug.Text += "SEI - Set Interrupt | Implicit \n";
                    I = true;
                    PC += 1;
                    cycles += 2;
                    LogExternal("Set interrupt flag.");
                    break;

                case 0x81:
                    //txtDebug.Text += "STA - Store Accumulator | Indirect with X offset \n";
                    PC += 2;
                    break;

                case 0x84:
                    //txtDebug.Text += "STY - Store Y register | Zero Page";
                    mem[PC + 1] = Y;
                    PC += 2;
                    break;

                case 0x85:
                    LogExternal("STA - Store Accumulator | Zero Page ");
                    PC += 1;
                    mem[PC] = A;
                    PC += 1;
                    break;

                case 0x86:
                    LogExternal("STX - Store X Register | Zero Page");
                    PC += 1;
                    mem[PC] = X; //Probably works like this. I gotta get used to the variables declared.
                    PC += 1;
                    break;

                case 0x88:
                    //txtDebug.Text += "DEY - Decrement Y | Implied \n";
                    Y--;

                    // Set zero flag
                    if (Y == 0)
                        Z = true;

                    // Set Negative flag
                    if (Y >> 7 == 1)
                        N = true;

                    PC += 1;
                    break;

                case 0x8A:
                    LogExternal("TXA - Transfer X to A | Implied ");
                    A = X;
                    checkZN(A);
                    PC += 1;
                    break;

                case 0x8C:
                    //txtDebug.Text += "STY - Store Y | Absolute \n";
                    argop = (ushort)((mem[PC + 1] << 8) + (mem[PC + 2]));
                    mem[argop] = Y;
                    PC += 3;
                    break;

                case 0x8D:
                    //txtDebug.Text += "STA - Store Accumulator | Absolute \n";
                    PC += 1;
                    var val1 = mem[PC];
                    PC += 1;
                    var val2 = mem[PC];
                    var value = (val1 << 8) | val2;
                    mem[value] = A;
                    LogExternal("Stored " + A + " on memory location $" + value.ToString("X2"));
                    PC += 1;
                    break;

                case 0x8E:
                    LogExternal("STX - Store X Register | Absolute");
                    argop = (ushort)((mem[PC + 1] << 8) + (mem[PC + 2]));
                    mem[argop] = X;
                    PC += 3;
                    break;

                case 0x90:
                    LogExternal("BCC - Branch on Carry Clear | Implicit ");
                    PC += 1;
                    if (!C)
                    {
                        PC = (ushort)(PC + mem[PC]);
                        LogExternal("Branching.");
                    }
                    PC += 1;
                    break;

                case 0x91:
                    //txtDebug.Text += "STA - Store Accumulator | Indirect with Y offset \n";
                    PC += 2;
                    break;

                case 0x94:
                    //txtDebug.Text += "STY - Store Y | Zero Page, X \n";
                    mem[(PC + 1) + X] = Y;
                    PC += 2;
                    break;

                case 0x95:
                    LogExternal("STA - Store Accumulator | Zero Page with X offset ");
                    cycles += 4;
                    PC += 1;
                    value = mem[PC];
                    PC += 1;
                    A = mem[X | value];
                    break;

                case 0x96:
                    //txtDebug.Text += "STX - Store X Register | Zero Page with Y offset";
                    mem[(PC + 1) + Y] = X;
                    PC += 2;
                    break;

                case 0x98:
                    //txtDebug.Text += "TYA - Transfer Y to A | Implied \n";
                    PC += 1;
                    break;

                case 0x99:
                    //txtDebug.Text += "STA - Store Accumulator | Absolute with Y offset \n";
                    PC += 3;
                    break;

                case 0x9A:
                    LogExternal("TXS - Transfer X to Stack Pointer | Implied ");
                    S = X;
                    PC += 1;
                    break;

                case 0x9D:
                    LogExternal("STA - Store Accumulator | Absolute with X offset ");
                    mem[read16(PC) | X] = A;
                    PC += 3;
                    break;

                case 0xA0:
                    //txtDebug.Text += "LDY - Load Y | Immediate \n";
                    PC += 2;
                    break;

                case 0xA1:
                    //txtDebug.Text += "LDA - Load Accumulator | Indirect with X offset \n";
                    PC += 2;
                    break;

                case 0xA2:
                    LogExternal("LDX - Load X | Immediate");
                    PC += 1;
                    checkZN(mem[PC]);
                    X = mem[PC];
                    cycles += 2;
                    PC += 1;
                    break;

                case 0xA4:
                    //txtDebug.Text += "LDY - Load register Y | Zero Page \n";
                    PC += 2;
                    break;

                case 0xA5:
                    LogExternal("LDA - Load Accumulator | Zero Page");
                    PC += 1;
                    A = mem[(ushort)(mem[PC])];
                    checkZN(A);
                    PC += 1;
                    break;

                case 0xA6:
                    //txtDebug.Text += "LDX - Load X Register | Zero Page \n";
                    PC += 2;
                    break;

                case 0xA8:
                    //txtDebug.Text += "TAY - Transfer A to Y | Implied \n";
                    PC += 1;
                    break;

                case 0xA9:
                    //txtDebug.Text += "LDA - Load ACC | Immediate \n";
                    PC += 1;
                    checkZN(mem[PC]);
                    LogExternal("Stored " + mem[PC] + " on accumulator.");
                    A = mem[PC];
                    PC += 1;
                    break;

                case 0xAA:
                    //txtDebug.Text += "TAX - Transfer A to X | Implied \n";
                    PC += 1;
                    break;

                case 0xAC:
                    //txtDebug.Text += "LDY - Load register Y | Absolute \n";
                    PC += 3;
                    break;

                case 0xAD:
                    //txtDebug.Text += "LDA - Load ACC | Absolute \n";
                    //Read 16 bytes of data
                    PC += 1;
                    var val1_ = mem[PC];
                    PC += 1;
                    var val2_ = mem[PC];
                    var value_ = (val1_ << 8) + val2_;

                    //Check affected flags
                    checkZN(mem[value_]);

                    A = mem[value_];
                    LogExternal("Loaded " + mem[value_].ToString("X2") + " to accumulator.");
                    PC += 1;
                    cycles += 4;
                    break;

                case 0xAE:
                    //txtDebug.Text += "LDX - Load X | Absolute \n";
                    PC += 3;
                    break;

                case 0xB0:
                    //txtDebug.Text += "BCS - Branch on Carry Set | Implicit \n";
                    PC += 2;
                    break;

                case 0xB1:
                    LogExternal("LDA - Load Accumulator | Indirect with Y offset");
                    PC += 1;
                    A = mem[(ushort)(((Y << 8) | (mem[PC] >> 8)))];
                    checkZN(A);
                    PC += 1;
                    break;

                case 0xB4:
                    //txtDebug.Text += "LDY - Load register Y | Zero Page with X offset \n";
                    PC += 2;
                    break;

                case 0xB5:
                    LogExternal("LDA - Load Accumulator | Zero Page with X offset --Unreliable operand");
                    A = mem[(ushort)((PC + 1) + X)];
                    checkZN(A);
                    PC += 2;
                    break;

                case 0xB6:
                    //txtDebug.Text += "LDX - Load X Register | Zero Page with X offset \n";
                    PC += 2;
                    break;

                case 0xB8:
                    //txtDebug.Text += "CLV - Clear overflow | Implied \n";
                    V = false;
                    PC += 1;
                    break;

                case 0xB9:
                    //txtDebug.Text += "LDA - Load Accumulator | Absolute with Y offset \n";
                    PC += 3;
                    break;

                case 0xBA:
                    //txtDebug.Text += "TSX - Transfer StackPtr to X | Implicit \n";
                    PC += 1;
                    break;

                case 0xBC:
                    //txtDebug.Text += "LDY - Load register Y | Absolute with X offset \n";
                    PC += 3;
                    break;

                case 0xBD:
                    LogExternal("LDA - Load ACC | Absolute, X");
                    A = mem[read16(PC) + X];
                    PC += 2;
                    checkZN(A);
                    PC += 1;
                    break;

                case 0xBE:
                    //txtDebug.Text += "LDX - Load X Register | Absolute with X offset \n";
                    PC += 3;
                    break;

                case 0xC0:
                    //txtDebug.Text += "CPY - Compare Y reg | Immediate \n";
                    PC += 2;
                    break;

                case 0xC1:
                    //txtDebug.Text += "CMP - Compare Accumulator reg | Indirect with X offset \n";
                    PC += 2;
                    break;

                case 0xC4:
                    //txtDebug.Text += "CPY - Compare Y reg | Zero Page \n";
                    PC += 2;
                    break;

                case 0xC5:
                    LogExternal("CMP - Compare Accumulator reg | Zero Page ");
                    PC += 1;
                    compare(A, mem[PC]);
                    PC += 1;
                    break;

                case 0xC6:
                    //txtDebug.Text += "DEC - Decrement Memory | Zero Page \n";
                    PC += 2;
                    break;

                case 0xC8:
                    //txtDebug.Text += "INY - Increment Y | Implied \n";

                    Y++;

                    // Set Zero flag
                    if (Y == 0)
                        Z = true;

                    // Set Negative flag
                    if (Y >> 7 == 1)
                        N = true;

                    PC += 1;
                    break;

                case 0xC9:
                    LogExternal("CMP - Compare | Immediate ");
                    compare(A, mem[PC + 1]);
                    PC += 2;
                    break;

                case 0xCA:
                    //txtDebug.Text += "DEX - Decrement X | Implied \n";
                    X--;
                    PC += 1;
                    break;

                case 0xCC:
                    //txtDebug.Text += "CPY - Compare Y reg | Absolute \n";
                    PC += 3;
                    break;

                case 0xCD:
                    //txtDebug.Text += "CMP - Compare Accumulator reg | Absolute \n";
                    PC += 3;
                    break;

                case 0xCE:
                    //txtDebug.Text += "DEC - Decrement Memory | Absolute \n";
                    PC += 3;
                    break;

                case 0xD0:
                    LogExternal("BNE - Branch on Not Equal (!=) | Implicit");
                    PC += 1;
                    if (!Z)
                    {
                        PC = (ushort)(PC + mem[PC]);
                        LogExternal("Branching.");
                    }
                    PC += 1;
                    break;

                case 0xD1:
                    //txtDebug.Text += "CMP - Compare Accumulator reg | Indirect with Y offset \n";
                    PC += 2;
                    break;

                case 0xD5:
                    //txtDebug.Text += "CMP - Compare Accumulator reg | Zero Page with X offset \n";
                    PC += 2;
                    break;

                case 0xD6:
                    //txtDebug.Text += "DEC - Decrement Memory | Zero Page with X offset \n";
                    PC += 2;
                    break;

                case 0xD8:
                    //txtDebug.Text += "CLD - Clear decimal | Implicit \n";
                    D = false;
                    LogExternal("Set decimal flag to zero.");
                    PC += 1;
                    cycles += 2;
                    break;

                case 0xD9:
                    //txtDebug.Text += "CMP - Compare Accumulator reg | Absolute with Y offset \n";
                    PC += 2;
                    break;

                case 0xDD:
                    //txtDebug.Text += "CMP - Compare Accumulator reg | Absolute with X offset \n";
                    compare(A, mem[read16((ushort)(PC + X))]);
                    PC += 3;
                    LogExternal("CMP - Compare Accumulator reg | Absolute with X offset");
                    break;

                case 0xDE:
                    //txtDebug.Text += "DEC - Decrement Memory | Absolute with X offset \n";
                    PC += 3;
                    break;

                case 0xE0:
                    //txtDebug.Text += "CPX - Compare X reg | Immediate \n";
                    PC += 2;
                    break;

                case 0xE4:
                    //txtDebug.Text += "CPX - Compare X reg | Zero Page \n";
                    PC += 2;
                    break;

                case 0xE6:
                    //txtDebug.Text += "INC - Incremental mem | Zero Page \n";
                    PC += 2;
                    break;

                case 0xE8:
                    LogExternal("INX - Increment X | Implied ");
                    X++;

                    // Set Zero flag
                    if (X == 0)
                        Z = true;

                    // Set Negative flag
                    if (X >> 7 == 1)
                        N = true;

                    PC += 1;
                    break;

                case 0xEA:
                    //txtDebug.Text += "NOP - No Operation (sleep) \n";
                    PC += 1;
                    break;

                case 0xEC:
                    //txtDebug.Text += "CPX - Compare X Register | Absolute \n";
                    PC += 3;
                    break;

                case 0xEE:
                    //txtDebug.Text += "INC - Increment Memory | Absolute \n";
                    PC += 3;
                    break;

                case 0xF0:
                    LogExternal("BEQ - Branch on Equal(==) | Implicit ");
                    PC += 1;
                    if (C)
                    {
                        PC = (ushort)(PC + mem[PC]);
                        LogExternal("Branching.");
                    }
                    PC += 1;
                    break;

                case 0xF6:
                    //txtDebug.Text += "INC - Incremental mem | Zero Page with X offset\n";
                    PC += 2;
                    break;

                case 0xF8:
                    //txtDebug.Text += "SED - Set decimal | Implied \n";
                    D = true;
                    PC += 1;
                    break;

                case 0xFE:
                    //txtDebug.Text += "INC - Incremental mem | Absolute with X offset \n";
                    PC += 3;
                    break;

                default:
                    LogExternal("WARNING ->> Unknown opcode: " + mem[PC]);
                    break;


            }

            if (CpuStats != null)
            {
                CpuStats(mem, PC, C, Z, I, D, V, N, X, Y, A);
            }
        }

        public void InitVM()
        {
            // Clear registers
            A = 0x00;
            X = 0x00;
            Y = 0x00;
            P = 0x34;
            PC = 0x08000;

            // Clear flags
            N = V = B = D = I = Z = C = false;

            // Clear RAM
            for (int i = 0x0000; i < 0x07FF; i++)
            {
                mem[i] = 0xFF;
            }
        }

        //Helper functions

        private void checkZN(byte value)
        {
            if (value == 0)
            {
                Z = true;
            }
            else
            {
                Z = false;
            }

            if ((value & 0b00000010) == 0b00000010)
            {
                N = true;
            }
            else
            {
                N = false;
            }
        }

        private ushort read16(ushort PC)
        {
            var val1 = mem[PC + 1];
            var val2 = mem[PC + 2];
            ushort value = (ushort)((val1 << 8) | val2);
            return value;

        }

        private void compare(byte var1, byte var2)
        {
            if (var1 >= var2)
            {
                C = true;
            }
            else
            {
                C = false;
            }

            if (var1 == var2)
            {
                Z = true;
            }
            else
            {
                Z = false;
            }

            if (((var1 - var2) & 0b00000010) == 0b00000010)
            {
                N = true;
            }
            else
            {
                N = false;
            }
        }

        private void push16(ushort value)
        {
            byte hi = (byte)(value >> 8);
            byte lo = (byte)(value & 0xFF);
            push(hi);
            push(lo);
        }

        private void push(byte value)
        {
            LogExternal("Pushed new value to stack.");
            mem[0x100 | S] = value;
            S--;
        }

        private byte pop()
        {
            S++;
            return mem[0x100 | S];
        }

        private ushort pop16()
        {
            byte lo = pop();
            byte hi = pop();
            return (ushort)(hi << 8 | lo);
        }

        private byte PackCPUToByte()
        {
            //Packs the CPU state to a byte, can be pushed to stack pile
            var n = 0;
            var v = 0;
            var b = 0;
            var d = 0;
            var i = 0;
            var z = 0;
            var c = 0;
            if (N)
            {
                n = 1;
            }
            if (V)
            {
                v = 1;
            }
            if (B)
            {
                b = 1;
            }
            if (D)
            {
                d = 1;
            }
            if (I)
            {
                i = 1;
            }
            if (Z)
            {
                z = 1;
            }
            if (C)
            {
                c = 1;
            }
            P = (byte)((n << 7) | (v << 6) | (0 << 5) | (b << 4) | (d << 3) | (i << 2) | (z << 1) | c);
            return (byte)((n << 7) | (v << 6) | (0 << 5) | (b << 4) | (d << 3) | (i << 2) | (z << 1) | c);
        }

        private void UnpackByteToCpu()
        {
            N = false;
            V = false;
            B = false;
            D = false;
            I = false;
            Z = false;
            C = false;
            var states = pop();
            if ((0b10000000 & states) == 0b10000000)
            {
                N = true;
            }
            if ((0b01000000 & states) == 0b01000000)
            {
                V = true;
            }
            if ((0b00010000 & states) == 0b00010000)
            {
                B = true;
            }
            if ((0b00001000 & states) == 0b00001000)
            {
                D = true;
            }
            if ((0b00000100 & states) == 0b00000100)
            {
                I = true;
            }
            if ((0b00000010 & states) == 0b00000010)
            {
                Z = true;
            }
            if ((0b00000001 & states) == 0b00000001)
            {
                C = true;
            }
        }
    }
}
