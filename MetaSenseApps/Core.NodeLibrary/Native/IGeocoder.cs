using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeLibrary.Native
{
    public interface IGeocoder
    {
        /// <summary>
        /// Determine the location from latitude and longitude coordinates.
        /// </summary>
        /// <param name="locLat">The latitude of the location</param>
        /// <param name="locLong">The longitude of the location</param>
        /// <returns>A string representing the location</returns>
        string ReverseGeocode(double locLat, double locLong);
    }
}
