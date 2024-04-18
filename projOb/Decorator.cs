using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkSourceSimulator;

namespace projOb
{
    public class Decorator
    {
        public NetworkSourceSimulator.NetworkSourceSimulator DataSource { get; set; }
        private List<MyObject> Objects;
        private List<Flight> Flights;
        private List<Airport> Airports;
        private List<Plane> Planes;
        private List<Crew> Crew;
        private DateTime StartDate;
        private string JsonPath;
        public Decorator(NetworkSourceSimulator.NetworkSourceSimulator dataSource, ref List<MyObject> objects, 
            ref List<Flight> flights, ref List<Airport> airports, ref List<Plane> planes, ref List<Crew> crew, DateTime date, string path) 
        { 
            DataSource = dataSource;
            Objects = objects;
            Flights = flights;
            Airports = airports;
            Planes = planes;
            Crew = crew;
            StartDate = date;
            JsonPath = path;
            DataSource.OnIDUpdate += IDUpdate;
            DataSource.OnPositionUpdate += PositionUpdate;
            DataSource.OnContactInfoUpdate += ContactInfoUpdate;
        }
        private MyObject? FindID(UInt64 id)
        {
            MyObject? o = Objects.Find((obj) => obj.ID == id);
            return o;
        }
        private Airport? FindAirport(UInt64 id)
        {
            Airport? airport = Airports.Find((air) => air.ID == id);
            return airport;
        }
        private Plane? FindPlane(UInt64 id)
        {
            Plane? plane = Planes.Find((pl) => pl.ID == id);
            return plane;
        }
        private Flight? FindFlight(UInt64 id)
        {
            Flight? fl = Flights.Find((flight)=> flight.ID == id);
            return fl;
        }
        private Crew? FindCrew(UInt64 id)
        {
            Crew? crew = Crew.Find((cr) => cr.ID == id);
            return crew;
        }
        private void IDUpdate(object sender, IDUpdateArgs args) 
        {
            MyObject? o = FindID(args.ObjectID);
            if (o != null) 
            { 
                Serialize(o, "ID update");
                o.ID = args.NewObjectID;
                Serialize(o, "ID update");
            }
        }
        private void PositionUpdate(object sender, PositionUpdateArgs args) 
        {
            Plane? plane = FindPlane(args.ObjectID);
            Flight? flight = FindFlight(args.ObjectID);
            Airport? airport = FindAirport(args.ObjectID);

            FlightAdapter? flightGUI = new FlightAdapterGenerator().Create(flight, Airports, StartDate); 

            
            
        }
        private void ContactInfoUpdate(object sender, ContactInfoUpdateArgs args)
        {
            Crew? crew = FindCrew(args.ObjectID);
            if (crew != null) 
            {
                Serialize(crew, "Contact info update");
                crew.Phone = args.PhoneNumber;
                crew.Email = args.EmailAddress;
                Serialize(crew, "Contact info update");
            }
        }
        public void Serialize(MyObject myObject, string message)
        {
            using StreamWriter sw = new StreamWriter(JsonPath, true);
            string line;
            line = $"Log{DateTime.Now} " + message;
            sw.WriteLine(line);
            line = myObject.JsonSerialize();
            sw.WriteLine(line);
        }


    }
}
