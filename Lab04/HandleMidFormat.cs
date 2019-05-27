using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab04
{
    class HandleMidFormat
    {
        public byte[] mid = new byte[3];

        public String MidFormat
        {
            get { return String.Format("MidiFormate:0x{0:X2}-0x{1:X2}-0x{2:X2},RealData:0x{3:X4}", (int)mid[0], (int)mid[1], (int)mid[2],this.getData()); }
        }
        public HandleMidFormat()
        {
            mid[0] = 0;
            mid[1] = 0;
            mid[2] = 0;
        }
        public int getData()
        {
            return ((int)mid[2] << 7) + mid[1];
        }
        //
        public void CreateMidiPWM(int k,int PWMPin)
        {
            mid[0] = (byte)((0xD << 4) + PWMPin);
            mid[1] = (byte)(k & 0x7f);
            mid[2] = (byte)((k >> 7) & 0x7f);
        }
        public double getTempertaure(int adcData)
        {
            double Vcc = 5, R5 = 10000, Vref = 5, T0 = 25 + 273.15, B = 3435, R0 = 10000;
            double Vtemp = adcData * (Vref / 1024);
            double R9 = Vcc * (R5 / Vtemp) - R5;
            double Tx = 1 / (Math.Log(R9 / R0) / B + 1 / T0);
            return (Tx - 273.15);
        }
    }
}

