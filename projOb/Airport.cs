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
    public class Airport: MyObject
    {
        public string? Name { get; set; } 
        public string? Code { get; set; }
        public Single Longitude { get; set; }
        public Single Latitude { get; set; }
        public Single AMSL { get; set; }
        public string? ISO { get; set; }
        public Airport(): base() 
        {
            Name = null;
            Code = null;
            Longitude = 0;
            Latitude = 0;
            AMSL = 0;
            ISO = null;
        }
        public Airport(string[] values): base(values)
        {
            if (values.Length < 7) throw new InvalidNumberOfArgsException();

            Name = Convert.ToString(values[1]);
            Code = Convert.ToString(values[2]);
            Longitude = Convert.ToSingle(values[3], CultureInfo.InvariantCulture);
            Latitude = Convert.ToSingle(values[4], CultureInfo.InvariantCulture);
            AMSL = Convert.ToSingle(values[5], CultureInfo.InvariantCulture);
            ISO = values[6];
        }
        public Airport(byte[] values) : base(values)
        {
            UInt16 NL = BitConverter.ToUInt16(values, 15);
            Name = Encoding.ASCII.GetString(values, 17, NL);
            Code = Encoding.ASCII.GetString(values, 17 + NL, 3);
            Longitude = BitConverter.ToSingle(values, 20 + NL);
            Latitude = BitConverter.ToSingle(values, 24 + NL);
            AMSL = BitConverter.ToSingle(values, 28 + NL);
            ISO = Encoding.ASCII.GetString(values, 32 + NL, 3);
        }
        public override string JsonSerialize()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
