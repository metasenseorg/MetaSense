using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeLibrary
{
    public class MetaSenseAQIMessage
    {
        public double AQI {
            get;
            protected set;
        }

        public string AQICategory
        {
            get;
            protected set;
        }

        public string Location
        {
            get;
            protected set;
        }

        /// <summary>
        /// Initializes the properties to the specified values.
        /// </summary>
        /// <param name="index">The instanteneous AQI</param>
        /// <param name="category">The AQI category of the index</param>
        /// <param name="location">The location of the read</param>
        public MetaSenseAQIMessage(double index, string category, string location)
        {
            AQI = index;
            AQICategory = category;
            Location = location;
        }

        /// <summary>
        /// Format the AQI information as a media post.
        /// </summary>
        /// <returns>A string representing the AQI information as a media post.</returns>
        public string ToMediaPost()
        {
            if (Location != null)
                return $"My current #AirQuality is {AQI} (Air Quality Index = {AQICategory}) in {Location}.";
            else
                return $"My current #AirQuality is {AQI} (Air Quality Index = {AQICategory}).";
        }
    }
}
