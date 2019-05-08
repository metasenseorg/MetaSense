using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace NodeLibrary
{
    public class BluetoothUtils
    {
        public long ToLongAddr(string addr)
        {
            var addrS = addr;
            var b = new StringBuilder();
            foreach (var str in addrS.Split(':'))
                b.Append(str);
            return long.Parse(addr, NumberStyles.HexNumber);
        }
        public string ToStringAddr(long addr)
        {
            var hexAddr = $"{addr:X12}";
            var matches = Regex.Matches(hexAddr, ".{2}");
            var sb = new StringBuilder();
            var mIdx = 0;
            foreach (Match m in matches)
            {
                if (mIdx > 0)
                    sb.Append(":");
                sb.Append(m.Value);
                mIdx++;
            }
            return sb.ToString();
        }

    }
}
