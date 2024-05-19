using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace projOb
{
    [Serializable]
    public class Plane: MyObject
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
        [JsonConstructor]
        public Plane(string[] values): base(values)
        {
            if (values.Length < 4) throw new InvalidNumberOfArgsException();
            
            Serial = values[2];
            ISO = values[3];
            Model = values[4];
        }
        public Plane(byte[] values) : base(values)
        {
            Serial = Encoding.ASCII.GetString(values, 15, 10);
            Serial = Serial.TrimEnd('\0');
            ISO = Encoding.ASCII.GetString(values, 25, 3);
            UInt16 ML = BitConverter.ToUInt16(values, 28);
            Model = Encoding.ASCII.GetString(values, 30, ML);
        }
        public override string JsonSerialize()
        {
            return JsonSerializer.Serialize(this);
        }

    }
    [Serializable]
    public class PassengerPlane: Plane, IReportable
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
        [JsonConstructor]
        public PassengerPlane(string[] values) : base(values) 
        {
            if (values.Length < 7) throw new InvalidNumberOfArgsException();

            FirstClassSize = Convert.ToUInt16(values[5]);
            BusinessClassSize = Convert.ToUInt16(values[6]);
            EconomyClassSize = Convert.ToUInt16(values[7]);
        }
        public PassengerPlane(byte[] values): base(values)
        {
            FirstClassSize = BitConverter.ToUInt16(values, 30 + Model.Length);
            BusinessClassSize = BitConverter.ToUInt16(values, 32 + Model.Length);
            EconomyClassSize = BitConverter.ToUInt16(values, 34 + Model.Length);
        }
        public override string JsonSerialize()
        {
            return JsonSerializer.Serialize(this);
        }
        public string Accept(Media media)
        {
            return media.CreateMessage(this);
        }
    }
    [Serializable]
    public class CargoPlane: Plane, IReportable
    {
        public Single MaxLoad {  get; set; }
        public CargoPlane(): base()
        {
            MaxLoad = 0;
        }
        [JsonConstructor]
        public CargoPlane(string[] values): base(values) 
        {
            if (values.Length < 5) throw new InvalidNumberOfArgsException();

            MaxLoad = Convert.ToSingle(values[5]);
        }
        public CargoPlane(byte[] values): base(values)
        {
            MaxLoad = BitConverter.ToSingle(values, 30 + Model.Length);
        }
        public override string JsonSerialize()
        {
            return JsonSerializer.Serialize(this);
        }
        public string Accept(Media media)
        {
            return media.CreateMessage(this);
        }
        
    }
}
