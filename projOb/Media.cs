using NetworkSourceSimulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projOb
{
    public interface IReportable
    {
        public string Accept(Media media);
    }
    public abstract class Media
    {
        public string? Name;
        public Media(string name)
        {
            Name = name;
        }
        public abstract string CreateMessage(Airport airport);
        public abstract string CreateMessage(CargoPlane cargoPlane);
        public abstract string CreateMessage(PassengerPlane passengerPlane);
    }
    public class Radio : Media
    {
        public Radio(string name) : base(name) { }
        public override string CreateMessage(Airport airport) 
        { 
            return $"Reporting for {Name}, Ladies and gentlemen, we are at the {airport.Name} airport"; 
        }
        public override string CreateMessage(CargoPlane cargoPlane)
        {
            return $"Reporting for {Name}, Ladies and gentlemen, we seeing the {cargoPlane.Serial} aircraft fly above us";
        }
        public override string CreateMessage(PassengerPlane passengerPlane)
        {
            return $"Reporting for {Name}, Ladies and gentlemen, we've just witnessed {passengerPlane.Serial} take off";
        }
    }
    public class Television : Media
    {
        public Television(string name) : base(name) { }
        public override string CreateMessage(Airport airport) 
        {
            return $"<An image of {airport.Name} airport>";
        }
        public override string CreateMessage(CargoPlane cargoPlane)
        {
            return $"<An image of {cargoPlane.Serial} cargo plane>";
        }
        public override string CreateMessage(PassengerPlane passengerPlane)
        {
            return $"<An image of {passengerPlane.Serial} passenger plane>";
        }
    }

    public class Newspaper : Media
    {
        public Newspaper(string name) : base(name) { }
        public override string CreateMessage(Airport airport) 
        { 
            return $"{Name} - A report from the {airport.Name} airport, {airport.ISO}"; 
        }
        public override string CreateMessage(CargoPlane cargoPlane)
        {
            return $"{Name} - An interview with the crew of {cargoPlane.Serial}";
        }
        public override string CreateMessage(PassengerPlane passengerPlane)
        {
            return $"{Name} - Breaking news! {passengerPlane.Model} aircraft loses EASA " +
                $"fails certification after inspection of {passengerPlane.Serial}";
        }
    }
}
