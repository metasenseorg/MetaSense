using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeLibrary.Native
{
    public interface ISharer
    {
        /// <summary>
        /// Shares the message.
        /// </summary>
        /// <param name="message">The AQI message containing information about the message to be
        ///     shared</param>
        void Share(MetaSenseAQIMessage message);
    }
}