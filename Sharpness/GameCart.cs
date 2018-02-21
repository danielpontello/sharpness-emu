using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharpness
{
    class GameCart
    {
        //iNES parser
        private string nesCheck;
        private string fileName;
        private byte formatIdentifier;
        private byte numberOfPRGBanks;
        private byte numberOfCHRBanks;
        private bool verticalScrolling = false;
        private bool hasBattery = false;
        private bool hasTrainer = false;
        private bool use4Screen = false;         //Indicates if 4 screen mirroring should be used
        private int mapperNumber = 00;
        private byte numberOfRAMBanks = 1;       //Has a catch

        // Attributes
        public string NesCheck { get => nesCheck; set => nesCheck = value; }
        public byte FormatIdentifier { get => formatIdentifier; set => formatIdentifier = value; }
        public byte NumberOfPRGBanks { get => numberOfPRGBanks; set => numberOfPRGBanks = value; }
        public byte NumberOfCHRBanks { get => numberOfCHRBanks; set => numberOfCHRBanks = value; }
        public bool VerticalScrolling { get => verticalScrolling; set => verticalScrolling = value; }
        public bool HasBattery { get => hasBattery; set => hasBattery = value; }
        public bool HasTrainer { get => hasTrainer; set => hasTrainer = value; }
        public bool Use4Screen { get => use4Screen; set => use4Screen = value; }
        public int MapperNumber { get => mapperNumber; set => mapperNumber = value; }
        public byte NumberOfRAMBanks { get => numberOfRAMBanks; set => numberOfRAMBanks = value; }
        public string FileName { get => fileName; set => fileName = value; }

        //Read game data
        public byte[] data;

        public GameCart(BinaryReader reader, string filename)
        {
            byte[] header = reader.ReadBytes(16);
            
            //Parse values from header received
            this.NesCheck = (char)header[0] + (char)header[1] + (char)header[2] + "";
            this.FormatIdentifier = header[3];
            this.NumberOfPRGBanks = header[4];
            this.NumberOfCHRBanks = header[5];

            int j = 0;
            string controlByte1 = Convert.ToString(header[6], 2);
            int[] arr1 = new int[8];
            string controlByte2 = Convert.ToString(header[7], 2);
            int[] arr2 = new int[8];
            for (int i = 0; i < 8; i++)
            {
                arr1[i] = 0;
                arr2[i] = 0;
            }

            j = 8 - controlByte1.Length;
            foreach (var ch1 in controlByte1)
            {
                arr1[j++] = Convert.ToInt32(ch1.ToString());
            }

            j = 8 - controlByte2.Length;
            foreach (var ch2 in controlByte2)
            {
                arr2[j++] = Convert.ToInt32(ch2.ToString());
            }

            this.VerticalScrolling = Convert.ToBoolean(arr1[0]);
            this.HasBattery = Convert.ToBoolean(arr1[1]);
            this.HasTrainer = Convert.ToBoolean(arr1[2]);
            this.Use4Screen = Convert.ToBoolean(arr1[3]);
            //Bitwise magic (untested)
            this.MapperNumber = (((arr1[4] + arr1[5] + arr1[6] + arr1[7]) & 0b00001111) << 4) | ((arr2[4] + arr2[5] + arr2[6] + arr2[7]) & 0b00001111);

            //RAM bank check
            if (header[8] == 0)
            {
                this.NumberOfRAMBanks = 1;
            }
            else
            {
                this.NumberOfRAMBanks = header[8];
            }

            this.FileName = filename;

            //Setup game data
            data = reader.ReadBytes(16384 * this.NumberOfPRGBanks);
        }
            

        public string GetGameInformation()
        {
            string data = "";
            data += "Name: " + this.fileName + "\n";
            data += "Header: " + this.NesCheck + "\n";
            data += "Format identifier: " + this.FormatIdentifier.ToString("X2") + "\n";
            data += "N° of PRG Banks: " + this.NumberOfPRGBanks + "\n";
            data += "N° of CHR Banks: " + this.NumberOfCHRBanks + "\n";
            data += "Has battery: " + this.HasBattery + "\n";
            data += "Has trainer: " + this.HasTrainer + "\n"; 
            data += "Mapper number: " + this.MapperNumber  +"\n";
            data+= "RAM banks: " + this.NumberOfRAMBanks + "\n";
                return data;
        }
    }
}
