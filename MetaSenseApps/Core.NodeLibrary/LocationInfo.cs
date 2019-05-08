using System;

namespace NodeLibrary
{
    public class LocationInfo
    {
        public double DistanceInMetersFrom(LocationInfo location)
        {
            return DistanceInMeters(Latitude, Longitude, location.Latitude, location.Longitude);
        }
        public double DistanceInMetersFrom(double latitude, double longitude)
        {
            return DistanceInMeters(Latitude, Longitude, latitude, longitude);
        }
        public static double DistanceInMeters(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            var theta = longitude1 - longitude2;
            var dist = Math.Sin(Deg2Rad(latitude1)) * Math.Sin(Deg2Rad(latitude2)) + Math.Cos(Deg2Rad(latitude1)) * Math.Cos(Deg2Rad(latitude2)) * Math.Cos(Deg2Rad(theta));
            dist = Math.Acos(dist);
            dist = Rad2Deg(dist);
            dist = dist * 60 * 1853.159616;
            return (dist);
        }
        private static double Deg2Rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }
        private static double Rad2Deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }
        public LocationInfo(double latitude, double longitude, double? radius, double? altitude, double? speed, double? direction, DateTime timestamp)
        {
            Latitude = latitude;
            Longitude = longitude;
            Radius = radius;
            Altitude = altitude;
            Speed = speed;
            Direction = direction;
            TimeStamp = timestamp;
        }
        public double Latitude { get; protected set; }
        public double Longitude { get; protected set; }
        public double? Radius { get; protected set; } // meters
        public double? Altitude { get; protected set; } // Feets
        public double? Speed { get; protected set; }  // Miles/hour
        public double? Direction { get; protected set; } // Degrees
        public DateTime TimeStamp { get; protected set; } // Degrees
    }
}