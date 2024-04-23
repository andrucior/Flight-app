using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data;
using NetworkSourceSimulator;

namespace projOb
{
    public class Subscriber
    {
        public NetworkSourceSimulator.NetworkSourceSimulator DataSource { get; set; }
        public List<FlightAdapter> NotYetList;
        private FlightsGUIData FlightsGUIData;
        private List<FlightGUI> FlightList;
        private List<MyObject> Objects;
        private List<Flight> Flights;
        private List<Airport> Airports;
        private List<Plane> Planes;
        private List<Crew> Crew;
        public DateTime StartDate;
        private string Path;
        private int i = 0;
        public Subscriber(NetworkSourceSimulator.NetworkSourceSimulator dataSource, ref List<MyObject> objects, 
            ref List<Flight> flights, ref List<Airport> airports, ref List<Plane> planes, ref List<Crew> crew, 
            DateTime date, ref List<FlightGUI> flightList, ref FlightsGUIData flightsGUIData) 
        { 
            DataSource = dataSource;
            Objects = objects;
            Flights = flights;
            Airports = airports;
            Planes = planes;
            Crew = crew;
            StartDate = date;
            FlightList = flightList;
            FlightsGUIData = flightsGUIData;
            NotYetList = new List<FlightAdapter>();
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
            Flight? flight = Flights.Find((flight) => flight.PlaneID == id);
            return flight;
        }
        private void IDUpdate(object sender, IDUpdateArgs args) 
        {
            MyObject? o = FindID(args.ObjectID);
            if (o != null) 
            {
                if (o.ID != args.NewObjectID)
                {
                    Serialize(o, "ID update. State before:");
                    o.ID = args.NewObjectID;
                    Serialize(o, "ID update. State after:");
                }
                else
                    WriteErrorLog("Couldn't update ID. ");
                
            }
            
        }
        private void PositionUpdate(object sender, PositionUpdateArgs args)
        {
            string error = "Couldn't update position. ";

            if (args.Latitude < -180 || args.Latitude > 180 || args.Longitude < -180 || args.Longitude > 180)
            {
                WriteErrorLog(error);
                return;
            }
            
            Plane? plane = FindPlane(args.ObjectID);
            Flight? flight = FindFlight(args.ObjectID);

            if (plane != null || flight != null)
            {
                if (plane != null)
                {
                    Serialize(plane, "Position update. State before:");
                    plane.ID = args.ObjectID;
                    flight = FindFlightFromPlaneID(plane.ID);
                    
                    if (flight == null)
                    {
                        WriteErrorLog(error);
                        return;
                    }

                    Serialize(plane, "Position update. State After:");
                }

                lock (FlightList)
                    FlightList.Remove(new FlightAdapterGenerator().Create(flight, Airports, StartDate));

                if (flight != null)
                {
                    Serialize(flight, "Position update. State before:");
                    flight.Latitude = args.Latitude;
                    flight.Longitude = args.Longitude;
                    flight.AMSL = args.AMSL;
                    Serialize(flight, "Position update. State after:");
                }

                lock (FlightList)
                    AddToFlightGUIList(flight, args);
            }
            else
                WriteErrorLog(error);
        }
        private void AddToFlightGUIList(Flight flight, PositionUpdateArgs args)
        {
            FlightAdapter? flightGUI = new FlightGUIDecorator(ref flight, args, Airports, StartDate).Update();
            DateTime takeOff = Convert.ToDateTime(flightGUI.Flight.TakeOff);
            DateTime landing = Convert.ToDateTime(flightGUI.Flight.Landing);

            if (DateTime.Compare(takeOff, StartDate) <= 0)
            {
                FlightList.Add(flightGUI);
                FlightsGUIData.UpdateFlights(FlightList);
            }
            else
                lock (NotYetList)
                    NotYetList.Add(flightGUI);
        }
        private void WriteErrorLog(string message)
        {
            using StreamWriter sw = new StreamWriter(Path, true);
            sw.WriteLine(message +"Invalid arguments");
            return;
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
                    WriteErrorLog("Couldn't update contact info");
            }
        }
        public void Serialize(MyObject myObject, string message)
        {
            using StreamWriter sw = new StreamWriter(Path, true);
            string line;
            line = $"Log {DateTime.Now} " + message;
            sw.WriteLine();
            sw.WriteLine(line);
            line = myObject.JsonSerialize();
            sw.WriteLine(line);
            sw.WriteLine();
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

            FlightAdapter flightAdapter =  new FlightAdapterGenerator().Create(Flight, Airports, StartDate);
            return flightAdapter;
        }

    }
}
