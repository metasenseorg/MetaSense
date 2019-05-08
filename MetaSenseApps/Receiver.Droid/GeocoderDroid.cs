using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Receiver.Droid;
using Xamarin.Forms;
using NodeLibrary.Native;
using Android.Locations;

[assembly: Dependency(typeof(GeocoderDroid))]
namespace Receiver.Droid
{
    internal sealed class GeocoderDroid : IGeocoder
    {
        private Geocoder _geocoder;
        private const int NUM_ADDRESSES = 1;

        /// <summary>
        /// Initialize the geocoder.
        /// </summary>
        public GeocoderDroid()
        {
            _geocoder = new Geocoder(Forms.Context);
        }

        /// <summary>
        /// Determine the location from latitude and longitude coordinates.
        /// </summary>
        /// <param name="locLat">The latitude of the location</param>
        /// <param name="locLong">The longitude of the location</param>
        /// <returns>A string representing the location</returns>
        public string ReverseGeocode(double locLat, double locLong)
        {
            var addresses = _geocoder.GetFromLocation(locLat, locLong, 1).ToArray();
            var subLocality = addresses[0].SubLocality;
            var locality = addresses[0].Locality;
            var subAdminArea = addresses[0].SubAdminArea;

            if (subLocality == null)
            {
                if (locality == null)
                    return subAdminArea;
                else
                    return locality;
            }
            else
                return subLocality;
        }
    }
}