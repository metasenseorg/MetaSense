using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BackendAPI.Data
{
    public interface ISettingsData
    {
        string Get(string key);
        //TableQuery<Read> Readings();
        Task ExportReads(StreamWriter stream);
        int Delete(string key);
        int Set(string key, string value);
        void AddLog(string type, string tag, string message);
        int ClearLogs();
        int AddRead(Read read);
        int ClearReads();
        IEnumerable<Tuple<DateTime,double>> LastHours(int hours, Func<Read,double> selector);
    }
}
