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
        private Flight Flight;
        private List<Airport> Airports;
        public FlightAdapter(Flight flight, List<Airport> airports, DateTime startDate) 
        {
            if (airports.Count == 0) throw new Exception("Airport list empty");

            Airports = airports;
            Flight = flight;
            ID = flight.ID;
            (Airport? origin, Airport? target) = FindAirports();
            MapCoordRotation = CalculateRotation(origin, target);
            double ratio = CalculateRatio(flight, startDate);
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
            if (Math.Abs(ratio - 1) < double.Epsilon * 10e300 * 10e20) 
                return new WorldPosition(target.Latitude, target.Longitude);

            (double x_start, double y_start) = SphericalMercator.FromLonLat(origin.Longitude, origin.Latitude);
            (double x_end, double y_end) = SphericalMercator.FromLonLat(target.Longitude, target.Latitude);

            double x = x_start + ratio * (x_end - x_start);
            double y = (y_end - y_start) / (x_end - x_start) * x + (y_start - (y_start - y_end) / (x_start - x_end) * x_start);

            (double lon, double lat) = SphericalMercator.ToLonLat(x, y);
            
            return new WorldPosition(lat, lon);
        }
        public static double CalculateRatio(Flight flight, DateTime startDate)
        {
            DateTime timeOfTakeOff = Convert.ToDateTime(flight.TakeOff);
            TimeSpan diff = startDate - timeOfTakeOff;
            TimeSpan travelTime = Convert.ToDateTime(flight.Landing) - Convert.ToDateTime(flight.TakeOff);
            double ratio = diff.TotalSeconds / travelTime.TotalSeconds;
            
            return ratio;
        }
        public (Airport origin, Airport destination) FindAirports()
        {
            Airport? origin = Airports.Find((Airport airp) => (airp.ID == Flight.OriginID));
            Airport? target = Airports.Find((Airport airp) => (airp.ID == Flight.TargetID));

            if (origin == null || target == null) throw new Exception("Aiport not found");

            return (origin, target);
        }
    }
}
