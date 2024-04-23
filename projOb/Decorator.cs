using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkSourceSimulator;

namespace projOb
{
    public class Decorator
    {
        public NetworkSourceSimulator.NetworkSourceSimulator DataSource { get; set; }
        private List<FlightGUI> flightsList = new List<FlightGUI>();
        private List<MyObject> Objects;
        private List<Flight> Flights;
        private List<Airport> Airports;
        private List<Plane> Planes;
        private List<Crew> Crew;
        private DateTime StartDate;
        private string Path;
        public Decorator(NetworkSourceSimulator.NetworkSourceSimulator dataSource, ref List<MyObject> objects, 
            ref List<Flight> flights, ref List<Airport> airports, ref List<Plane> planes, ref List<Crew> crew, DateTime date) 
        { 
            DataSource = dataSource;
            Objects = objects;
            Flights = flights;
            Airports = airports;
            Planes = planes;
            Crew = crew;
            StartDate = date;
            string now = DateTime.Now.ToString("MM.dd");
            string format = ".txt";
            int i = 0;
            do
            {
                Path = now + $" ({i++})" + format;
            } while (File.Exists(Path));
               
            
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
        private Flight? FindFlightFromPlaneID(UInt64 id)
        {
            Plane? plane = FindPlane(id);
            Flight? flight = Flights.Find((flight) => flight.PlaneID == id);
            return flight;
        }
        private void IDUpdate(object sender, IDUpdateArgs args) 
        {
            MyObject? o = FindID(args.ObjectID);
            if (o != null) 
            {
                if (o.ID != args.ObjectID)
                {
                    Serialize(o, "ID update");
                    o.ID = args.NewObjectID;
                    Serialize(o, "ID update");
                }
                else
                {
                    using StreamWriter sw = new StreamWriter(Path, true);
                    sw.WriteLine("Couldn't update ID: invalid arguments");
                }
            }
            
        }
        private void PositionUpdate(object sender, PositionUpdateArgs args) 
        {
            Plane? plane = FindPlane(args.ObjectID);
            Flight? flight = FindFlight(args.ObjectID);
            Airport? airport = FindAirport(args.ObjectID);

            if (flight != null)
            {
                Serialize(flight, "Position update");
                flightsList.Remove(new FlightAdapterGenerator().Create(flight, Airports, StartDate));
                FlightAdapter? flightGUI = new FlightGUIDecorator(ref flight, args, Airports, StartDate).Update();
                flightsList.Add(flightGUI);
                Serialize(flight, "Position update");
                
            }
            if (airport != null)
            {
                Serialize(airport, "Position update");
                airport.ID = args.ObjectID;
                airport.Longitude = args.Longitude;
                airport.AMSL = args.AMSL;
                airport.Latitude = args.Latitude;
                Serialize(airport, "Position update");

            }
            if (plane != null)
            {
                Serialize(plane, "Position update");
                plane.ID = args.ObjectID;
                // Do poprawy pozycja - dekorator??
                Flight? fl = FindFlightFromPlaneID(plane.ID);
                flightsList.Remove(new FlightAdapterGenerator().Create(fl, Airports, StartDate));
                FlightAdapter? flightGUI = new FlightGUIDecorator(ref fl, args, Airports, StartDate).Update();
                flightsList.Add(flightGUI);
                Serialize(plane, "Posiotion update");
                
            }            
            
        }
        private void ContactInfoUpdate(object sender, ContactInfoUpdateArgs args)
        {
            Crew? crew = FindCrew(args.ObjectID);
            if (crew != null) 
            {
                if (crew.Phone != args.PhoneNumber || crew.Email != args.EmailAddress)
                {
                    Serialize(crew, "Contact info update");
                    crew.Phone = args.PhoneNumber;
                    crew.Email = args.EmailAddress;
                    Serialize(crew, "Contact info update");
                }
                else 
                {
                    using StreamWriter sw = new StreamWriter(Path, true);
                    sw.WriteLine("Couldn't update contact info: invalid arguments");
                }
            }
        }
        public void Serialize(MyObject myObject, string message)
        {
            using StreamWriter sw = new StreamWriter(Path, true);
            string line;
            line = $"Log {DateTime.Now} " + message;
            sw.WriteLine(line);
            line = myObject.JsonSerialize();
            sw.WriteLine(line);
        }
    }

    public class FlightGUIDecorator
    {
        private Flight Flight;
        private PositionUpdateArgs PositionUpdateArgs;
        private DateTime StartDate;
        private List<Airport> Airports; 
        public FlightGUIDecorator(ref Flight flight, PositionUpdateArgs positionUpdateArgs, List<Airport> airports, DateTime date) 
        { 
            Flight = flight;
            PositionUpdateArgs = positionUpdateArgs;
            StartDate = date;
            Airports = airports;
        }
        public FlightAdapter Update()
        {
            Flight.Latitude = PositionUpdateArgs.Latitude;
            Flight.Longitude = PositionUpdateArgs.Longitude;
            Flight.AMSL = PositionUpdateArgs.AMSL;

            return new FlightAdapterGenerator().Create(Flight, Airports, StartDate);
        }

    }
}
