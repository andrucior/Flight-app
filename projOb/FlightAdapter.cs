using Mapsui.Projections;
using Mapsui.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace projOb
{
    public class FlightAdapter : FlightGUI
    {
        private Flight Fl;
        private List<Airport> Airports;


        public FlightAdapter(Flight fl, List<Airport> airports, DateTime startDate) 
        {
            if (fl == null) throw new Exception("Flight cannot be null");

            Airports = airports;
            ID = fl.ID; 
            
            Airport? origin = Airports.Find((Airport airp) => (airp.ID == fl.OriginID));
            Airport? target = Airports.Find((Airport airp) => (airp.ID == fl.TargetID));
            if (origin == null || target == null) throw new Exception("Aiport not found"); 
            
            Fl = fl;
            MapCoordRotation = CalculateRotation(origin, target);
            
            DateTime timeOfTakeOff = Convert.ToDateTime(fl.TakeOff);
            TimeSpan diff = startDate - timeOfTakeOff;
            TimeSpan travelTime = Convert.ToDateTime(fl.Landing) - Convert.ToDateTime(fl.TakeOff);

            double ratio = diff.TotalSeconds / travelTime.TotalSeconds;
            WorldPosition = CalculatePosition(origin, target, ratio);
            
        }
        public static double CalculateRotation (Airport origin, Airport target)
        {
            (double x_start, double y_start) = SphericalMercator.FromLonLat(origin.Longitude, origin.Latitude);
            (double x_end, double y_end) = SphericalMercator.FromLonLat(target.Longitude, target.Latitude);
            double dx = x_end - x_start;
            double dy = y_end - y_start;
            return Math.Atan2(dx, dy);
        }
        public static WorldPosition CalculatePosition (Airport origin, Airport target, double ratio)
        {
            (double x_start, double y_start) = SphericalMercator.FromLonLat(origin.Longitude, origin.Latitude);
            (double x_end, double y_end) = SphericalMercator.FromLonLat(target.Longitude, target.Latitude);

            double x = x_start + ratio * (x_end - x_start);
            double y = (y_end - y_start) / (x_end - x_start) * x + (y_start - (y_start - y_end) / (x_start - x_end) * x_start);

            (double lon, double lat) = SphericalMercator.ToLonLat(x, y);

            return new WorldPosition(lat, lon);
        }
    }
}
