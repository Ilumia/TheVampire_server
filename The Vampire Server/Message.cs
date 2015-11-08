using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace The_Vampire_Server
{
    public class Message
    {
        private byte[] data;
        private int length;

        public byte Type { get { return data[0]; } }
        public byte[] Data
        {
            get
            {
                byte[] temp = new byte[0];
                temp = new byte[Length - 1];
                for (int i = 0; i < Length - 1; i++) { temp[i] = data[i + 1]; }
                return temp;
            }
            set { this.data = value; }
        }
        public int Length { get { return length; } }
        public void InitSendPacket(byte type, byte[] _data)
        {
            length = _data.Length + sizeof(byte);
            data = new byte[Length];
            data[0] = type;
            for (int i = 0; i < _data.Length; i++) { data[i + 1] = _data[i]; }
        }
        public void InitRecvPacket(byte[] _data)
        {
            if (_data.Length < 4)
                return;
            length = BitConverter.ToInt32(_data, 0);
            data = new byte[Length + 3];
        }
        public byte[] DataBuffer { get { return data; } }
        public byte[] GetBuffer()
        {
            return new byte[4];
        }
    }
}
