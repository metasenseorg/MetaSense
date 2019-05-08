using System;
using System.Collections.Generic;

namespace NodeLibrary
{
    public sealed class LocationPath
    {
        public LocationPath()
        {
            Members = new List<PathElement>();
        }
        public abstract class PathElement
        {
            public DateTime Enter { get; }
            public DateTime Exit { get; }
            protected PathElement(DateTime enter, DateTime exit)
            {
                Enter = enter;
                Exit = exit;
            }
        }
        public sealed class PathSegment: PathElement
        {
            public LocationInfo Start { get; }
            public LocationInfo End { get; }
            public PathSegment(LocationInfo start, LocationInfo end, DateTime enter, DateTime exit) : base(enter,exit)
            {
                Start = start;
                End = end;
            }
        }
        public sealed class PathArea : PathElement
        {
            public LocationInfo Center { get; }
            public double Radius { get; }
            public PathArea(LocationInfo center, double radius, DateTime enter, DateTime exit) : base(enter, exit)
            {
                Center = center;
                Radius = radius;
            }
        }
        public List<PathElement> Members { get; }

        
        public void Add(LocationInfo loc)
        {
            Members.Add(new PathArea(loc, loc.Radius ?? 1, loc.TimeStamp, loc.TimeStamp));
        }
        
    }
}
