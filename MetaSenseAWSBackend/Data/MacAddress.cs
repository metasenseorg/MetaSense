using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BackendAPI.Data
{
    public class MacAddress
    {
        private string _mac;
        private MacAddress(string mac)
        {
            if (!IsMac(mac)) throw new ArgumentException($"{mac} is not a MAC Address.");
            _mac = mac;
        }
        public string MAC => _mac;
        private static Regex _macRegEx = new Regex("^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})|([0-9A-Fa-f]{12})$");
        public static bool IsMac(string m)
        {
            return _macRegEx.IsMatch(m);
        }

        public static MacAddress parse(string mac)
        {
            try
            {
                return new MacAddress(mac);
            }
            catch (Exception)
            {
                return null;
            }

        }
        public override bool Equals(object obj)
        {
            var address = obj as MacAddress;
            return address != null &&
                   _mac == address._mac;
        }

        public override int GetHashCode()
        {
            return -308041191 + EqualityComparer<string>.Default.GetHashCode(_mac);
        }

        public static bool operator ==(MacAddress address1, MacAddress address2)
        {
            return EqualityComparer<MacAddress>.Default.Equals(address1, address2);
        }

        public static bool operator !=(MacAddress address1, MacAddress address2)
        {
            return !(address1 == address2);
        }
    }
}
