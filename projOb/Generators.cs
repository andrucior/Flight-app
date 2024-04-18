using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projOb
{
    public abstract class Generator
    {
        static public Lists List = new Lists();
        public abstract MyObject Create(string[] values);
        public abstract MyObject CreateByte(byte[] values);
    }

    public abstract class PersonGenerator: Generator
    {
        public abstract override Person Create(string[] values);
        public abstract override Person CreateByte(byte[] values);
    }

    public class CrewGenerator: PersonGenerator
    {
        public override Crew Create(string[] values) 
        { 
            Crew crew =  new Crew(values);
            lock (List.CrewList)
                List.CrewList.Add(crew);
            return crew;
        }
        public override Crew CreateByte(byte[] values) 
        {
            Crew crew = new Crew(values);
            lock (List.CrewList)
                List.CrewList.Add(crew);
            return crew;
        }
    }
    public class PassengerGenerator: PersonGenerator
    {
        public override Person Create(string[] values) 
        { 
            Passenger passenger = new Passenger(values);
            lock (List.PassengerList)
                List.PassengerList.Add(passenger);
            return passenger; 
        }
        public override Person CreateByte(byte[] values) 
        {
            Passenger passenger = new Passenger(values);
            lock (List.PassengerList)
                List.PassengerList.Add(passenger);
            return passenger;
        }
    }
    public class CargoGenerator: Generator
    {
        public override Cargo Create(string[] values) 
        { 
            Cargo cargo = new Cargo(values);
            lock (List.CargoList)
                List.CargoList.Add(cargo);
            return cargo;
        }
        public override Cargo CreateByte(byte[] values) 
        {
            Cargo cargo = new Cargo(values);
            lock(List.CargoList)
                List.CargoList.Add(cargo);
            return cargo;
        }
    }

    public class AirportGenerator: Generator
    {
        public override Airport Create(string[] values) 
        { 
            Airport airport = new Airport(values);
            lock (List.Airports)
                List.Airports.Add(airport);
            return airport;
        }
        public override MyObject CreateByte(byte[] values)
        {
            Airport airport = new Airport(values);
            lock(List.Airports)
                List.Airports.Add(airport);
            return airport;
        }
    }
    public class FlightGenerator : Generator
    {
        public override Flight Create(string[] values)
        {
            Flight flight = new Flight(values);
            lock (List.Flights)
                List.Flights.Add(flight);
            return flight;
        }
        public override Flight CreateByte(byte[] values)
        {
            Flight flight = new Flight(values);
            lock (List.Flights)
                List.Flights.Add(flight);
            return flight;

        }
    }
    public abstract class PlaneGenerator: Generator
    {
        public abstract override Plane Create(string[] values);
        public abstract override Plane CreateByte(byte[] values);
    }
    public class CargoPlaneGenerator: PlaneGenerator
    {
        public override CargoPlane Create(string[] values)
        {
            CargoPlane cargoPlane = new CargoPlane(values);
            lock (List.CargoPlanes)
                List.CargoPlanes.Add(cargoPlane);
            return cargoPlane;
        }
        public override CargoPlane CreateByte(byte[] values)
        {
            CargoPlane cargoPlane = new CargoPlane(values);
            lock (List.CargoPlanes)
                List.CargoPlanes.Add(cargoPlane);
            return cargoPlane;
        }
    }
    public class PassengerPlaneGenerator: PlaneGenerator
    {
        public override PassengerPlane Create(string[] values) 
        { 
            PassengerPlane passengerPlane = new PassengerPlane(values);
            lock (List.PassengerPlaneList)
                List.PassengerPlaneList.Add(passengerPlane);
            return passengerPlane; 
        }
        public override PassengerPlane CreateByte(byte[] values)
        {
            PassengerPlane passengerPlane = new PassengerPlane(values);
            lock (List.PassengerPlaneList)
                List.PassengerPlaneList.Add(passengerPlane);
            return passengerPlane;
        }

    }
    public class DataReaderGenerator
    {
        public DataReader Create(NetworkSourceSimulator.NetworkSourceSimulator nss, Dictionary<string, Generator> generators, List<MyObject> myObjects) { return new DataReader(nss, ref generators, ref myObjects); }
    }
    public class FlightAdapterGenerator
    {
        public FlightAdapter? Create(Flight? flight, List<Airport> airports,DateTime dateTime) 
        {
            if (flight == null) return null;
            return new FlightAdapter(flight, airports, dateTime); 
        }
    }
    public class Lists
    {
        public List<Crew> CrewList = new List<Crew>();
        public List<PassengerPlane> PassengerPlaneList = new List<PassengerPlane>();
        public List<Passenger> PassengerList = new List<Passenger>();
        public List<Cargo> CargoList = new List<Cargo>();
        public List<Airport> Airports = new List<Airport>();
        public List<CargoPlane> CargoPlanes = new List<CargoPlane>();
        public List<Flight> Flights = new List<Flight>();
    }
    public class RadioGenerator
    {
        public Radio Create(string name) { return new Radio(name); }
    }
    public class TVGenerator
    {
        public Television Create(string name) { return new Television(name);}
    }
    public class NewspaperGenerator
    {
        public Newspaper Create(string name) { return new Newspaper(name); }
    }
}
