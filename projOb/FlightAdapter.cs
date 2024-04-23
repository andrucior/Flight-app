using Mapsui.Projections;
using Mapsui.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace projOb
{
    public class FlightAdapter : FlightGUI
    {
        public Flight Flight;
        private static List<Airport> Airports = Generator.List.Airports;
        private WorldPosition Origin;
        private WorldPosition Target;
        public FlightAdapter(Flight flight, DateTime startDate, WorldPosition start, WorldPosition target)
        { 
            Flight = flight;
            ID = flight.ID;
            Origin = new WorldPosition(start.Latitude, start.Longitude);
            Target = new WorldPosition(target.Latitude, target.Longitude);

            MapCoordRotation = CalculateRotation();
            double ratio = CalculateRatio(startDate);
            WorldPosition = CalculatePosition(ratio);
        }
        public double CalculateRotation()
        {
            (double x_start, double y_start) = SphericalMercator.FromLonLat(Origin.Longitude, Origin.Latitude);
            (double x_end, double y_end) = SphericalMercator.FromLonLat(Target.Longitude, Target.Latitude);
            
            double dx = x_end - x_start;
            double dy = y_end - y_start;
            
            return Math.Atan2(dx, dy);
        }
        
        public WorldPosition CalculatePosition(double ratio)
        {
            if (ratio > 1)
                return new WorldPosition(Target.Latitude, Target.Longitude);


            (double x_start, double y_start) = SphericalMercator.FromLonLat(Origin.Longitude, Origin.Latitude);
            (double x_end, double y_end) = SphericalMercator.FromLonLat(Target.Longitude, Target.Latitude);

            double x = x_start + ratio * (x_end - x_start);
            double y = (y_end - y_start) / (x_end - x_start) * x + (y_start - (y_start - y_end) / (x_start - x_end) * x_start);

            (double lon, double lat) = SphericalMercator.ToLonLat(x, y);

            return new WorldPosition(lat, lon);
        }
        public double CalculateRatio(DateTime startDate)
        {
            DateTime timeOfTakeOff = Convert.ToDateTime(Flight.TakeOff);
            TimeSpan diff = startDate - timeOfTakeOff;
            TimeSpan travelTime = Convert.ToDateTime(Flight.Landing) - Convert.ToDateTime(Flight.TakeOff);
            double ratio = diff.TotalSeconds / travelTime.TotalSeconds;
            
            return ratio;
        }
        public static (Airport origin, Airport destination) FindAirports(Flight flight)
        {
            Airport? origin = Airports.Find((Airport airp) => (airp.ID == flight.OriginID));
            Airport? target = Airports.Find((Airport airp) => (airp.ID == flight.TargetID));

            if (origin == null || target == null) throw new Exception("Aiport not found");

            return (origin, target);
        }
    }
}
