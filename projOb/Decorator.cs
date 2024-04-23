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
        public NetworkSourceSimulator.NetworkSourceSimulator DataSource;
        private List<MyObject> Objects;
        public DateTime StartDate;
        private readonly string Path;
        static private int i = 0;
        public Subscriber(NetworkSourceSimulator.NetworkSourceSimulator dataSource, ref List<MyObject> objects,
            DateTime date) 
        { 
            DataSource = dataSource;
            StartDate = date;
            Objects = objects;
            string now = DateTime.Now.ToString("MM.dd");
            string format = ".txt";
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
            List<projOb.Plane> planes = [.. Generator.List.CargoPlanes, .. Generator.List.PassengerPlaneList];
            Plane? plane = planes.Find((pl) => pl.ID == id);
            return plane;
        }
        private Flight? FindFlight(UInt64 id)
        {
            Flight? fl = Generator.List.Flights.Find((flight)=> flight.ID == id);
            return fl;
        }
        private Crew? FindCrew(UInt64 id)
        {
            Crew? crew = Generator.List.CrewList.Find((cr) => cr.ID == id);
            return crew;
        }
        private Flight? FindFlightFromPlaneID(UInt64 id)
        {
            Flight? flight = Generator.List.Flights.Find((flight) => flight.PlaneID == id);
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

                if (flight != null)
                {
                    Serialize(flight, "Position update. State before:");
                    Generator.List.FlightGUIs.Add(new FlightGUIDecorator(ref flight, args, StartDate).Update());
                    Serialize(flight, "Position update. State after:");
                }
            }
            else WriteErrorLog(error);
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
        public FlightGUIDecorator(ref Flight flight, PositionUpdateArgs positionUpdateArgs, DateTime date) 
        { 
            Flight = flight;
            PositionUpdateArgs = positionUpdateArgs;
            StartDate = date;
        }
        public FlightAdapter Update()
        {
            Flight.Latitude = PositionUpdateArgs.Latitude;
            Flight.Longitude = PositionUpdateArgs.Longitude;
            Flight.AMSL = PositionUpdateArgs.AMSL;

            Airport target = FlightAdapter.FindAirports(Flight).destination;
            FlightAdapter flightAdapter =  new FlightAdapterGenerator().Create(Flight, StartDate, 
                new WorldPosition(Flight.Latitude, Flight.Longitude), new WorldPosition(target.Latitude, target.Longitude));
            return flightAdapter;
        }

    }
}
