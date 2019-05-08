using System;

namespace BackendAPI
{
    public class BinaryUtils
    {
        public static byte GetSubByte(byte[] buf, int pos, int bitPos, int bitNum)
        {
            var bitMask = (byte)(0xFF >> (8 - bitNum));
            var b = GetByte(buf, pos);
            b >>= bitPos;
            b = (byte)(b & bitMask);
            return b;
        }

        public static byte GetByte(byte[] buf, int pos)
        {
            return buf[pos];
        }

        public static UInt16 GetUInt16(byte[] buf, int pos)
        {
            byte[] tmpbuf = new byte[2];
            if (BitConverter.IsLittleEndian)
            {
                tmpbuf[0] = buf[pos];
                tmpbuf[1] = buf[pos + 1];
            }
            else
            {
                tmpbuf[1] = buf[pos];
                tmpbuf[0] = buf[pos + 1];
            }
            return BitConverter.ToUInt16(tmpbuf, 0);
        }

        public static UInt32 GetUInt32(byte[] buf, int pos)
        {
            byte[] tmpbuf = new byte[4];
            if (BitConverter.IsLittleEndian)
            {
                tmpbuf[0] = buf[pos];
                tmpbuf[1] = buf[pos + 1];
                tmpbuf[2] = buf[pos + 2];
                tmpbuf[3] = buf[pos + 3];
            }
            else
            {
                tmpbuf[3] = buf[pos];
                tmpbuf[2] = buf[pos + 1];
                tmpbuf[1] = buf[pos + 2];
                tmpbuf[0] = buf[pos + 3];
            }
            return BitConverter.ToUInt32(tmpbuf, 0);
        }
    }
}