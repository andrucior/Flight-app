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
            
            double ratio = diff / travelTime;

            double t = (double)diff.Hours + (double)diff.Minutes / 60;
            
            double x = origin.Latitude + ratio * (target.Latitude - origin.Latitude);
            double y = origin.Longitude + ratio * (target.Longitude - origin.Latitude);
            WorldPosition = new WorldPosition(x, y);
        }
        public double CalculateVelocity(Airport start, Airport dest)
        {
            DateTime now = DateTime.Now;
            DateTime landing = Convert.ToDateTime(Fl.Landing);
            DateTime takeOff = Convert.ToDateTime(Fl.TakeOff);
            

            TimeSpan timeSpan = landing - takeOff;

            double R = 6371e3; // metres
            double φ1 = start.Latitude * Math.PI / 180; // φ, λ in radians
            double φ2 = dest.Latitude * Math.PI / 180;
            double Δφ = (dest.Latitude - start.Latitude) * Math.PI / 180;
            double Δλ = (dest.Longitude - start.Longitude) * Math.PI / 180;

            double a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                      Math.Cos(φ1) * Math.Cos(φ2) *
                      Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double s = R * c / 1000; // in km
            double t = timeSpan.Hours + timeSpan.Minutes / 60;
            double v = s / t;
            return v;
        }
        public static double CalculateRotation (Airport origin, Airport target)
        {
            (double x_start, double y_start) = SphericalMercator.FromLonLat(origin.Longitude, origin.Latitude);
            (double x_end, double y_end) = SphericalMercator.FromLonLat(target.Longitude, target.Latitude);

            double dx = x_end - x_start;
            double dy = y_end - y_start;
            
            return Math.Atan2(dx, dy);
        }


    }
}
