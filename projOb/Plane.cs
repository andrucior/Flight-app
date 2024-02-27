using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace projOb
{
    [Serializable]
    public abstract class Plane: MyObject
    {
        public string? Serial { get; set; }
        public string? ISO { get; set; }
        public string? Model { get; set; }
        
        public Plane(): base()
        {
            Serial = null; 
            ISO = null; 
            Model = null;
        }
        public Plane(string[] values): base(values)
        {
            if (values.Length < 4) throw new InvalidNumberOfArgsException();
            
            Serial = values[1];
            ISO = values[2];
            Model = values[3];
        }
        public override string JsonSerialize()
        {
            return JsonSerializer.Serialize(this);
        }

    }
    [Serializable]
    public class PassengerPlane: Plane
    {
        public UInt16 FirstClassSize {  get; set; }
        public UInt16 BusinessClassSize { get; set; }
        public UInt16 EconomyClassSize {  get; set; }

        public PassengerPlane(): base()
        {
            FirstClassSize = 0;
            BusinessClassSize = 0;
            EconomyClassSize = 0;
        }
        public PassengerPlane(string[] values) : base(values) 
        {
            if (values.Length < 7) throw new InvalidNumberOfArgsException();

            FirstClassSize = Convert.ToUInt16(values[4]);
            BusinessClassSize = Convert.ToUInt16(values[5]);
            EconomyClassSize = Convert.ToUInt16(values[6]);
        }
        public override string JsonSerialize()
        {
            return JsonSerializer.Serialize(this);
        }
    }
    [Serializable]
    public class CargoPlane: Plane 
    {
        public Single MaxLoad {  get; set; }
        public CargoPlane(): base()
        {
            MaxLoad = 0;
        }
        public CargoPlane(string[] values): base(values) 
        {
            if (values.Length < 5) throw new InvalidNumberOfArgsException();

            MaxLoad = Convert.ToSingle(values[4]);
        }
        public override string JsonSerialize()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
