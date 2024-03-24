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
        public FlightAdapter(Flight fl, List<Airport> airports) 
        {
            if (fl == null) throw new Exception("Flight cannot be null");
            Fl = fl;
            Airports = airports;
            
        }
        public new WorldPosition WorldPosition
        {
            get { return WorldPosition; }
            init 
            {
                // draft
                new WorldPosition(Fl.Latitude, Fl.Longitude);
            } 
        }
        public new double MapCoordRotation
        {
            get { return 0; }
            init 
            {
                Airport? origin = Airports.Find((Airport airp) => (airp.ID == Fl.OriginID));
                Airport? target = Airports.Find((Airport airp) => (airp.ID == Fl.TargetID));
                if (origin == null || target == null) throw new Exception("Airport not found");
                
                value = CalculateRotation(origin, target);
            }
        }
        public double CalculateVelocity()
        {
            DateTime now = DateTime.Now;
            DateTime landing = Convert.ToDateTime(Fl.Landing);
            DateTime takeOff = Convert.ToDateTime(Fl.TakeOff);
            Airport start, dest;
            start = Airports.Find((Airport airp) => Fl.OriginID == airp.ID);
            dest = Airports.Find((Airport airp) => Fl.TargetID == airp.ID);

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
            double t = timeSpan.Hours;
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
