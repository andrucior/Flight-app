using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projOb
{
    public abstract class Generator
    {
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
        public override Crew Create(string[] values) { return new Crew(values); }
        public override Crew CreateByte(byte[] values) { return new Crew(values); }
    }
    public class PassengerGenerator: PersonGenerator
    {
        public override Person Create(string[] values) { return new Passenger(values); }
        public override Person CreateByte(byte[] values) { return new Passenger(values); }
    }
    public class CargoGenerator: Generator
    {
        public override Cargo Create(string[] values) { return new Cargo(values); }
        public override Cargo CreateByte(byte[] values) { return new Cargo(values); }
    }

    public class AirportGenerator: Generator
    {
        public override Airport Create(string[] values) { return new Airport(values); }
        public override MyObject CreateByte(byte[] values) { return new Airport(values); }
    }
    public class FlightGenerator: Generator
    {
        public override Flight Create(string[] values) { return new Flight(values); }
        public override Flight CreateByte(byte[] values) { return new Flight(values); }
    }
    public abstract class PlaneGenerator: Generator
    {
        public abstract override Plane Create(string[] values);
        public abstract override Plane CreateByte(byte[] values);
    }
    public class CargoPlaneGenerator: PlaneGenerator
    {
        public override CargoPlane Create(string[] values) { return new CargoPlane(values); }
        public override CargoPlane CreateByte(byte[] values) {  return new CargoPlane(values); }
    }
    public class PassengerPlaneGenerator: PlaneGenerator
    {
        public override PassengerPlane Create(string[] values) { return new PassengerPlane(values); }
        public override PassengerPlane CreateByte(byte[] values) { return new PassengerPlane(values); }

    }
}
