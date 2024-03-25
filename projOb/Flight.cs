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
    public class Flight: MyObject
    {
        public UInt64 OriginID { get; set; }
        public UInt64 TargetID { get; set; }
        public string? TakeOff { get; set; }
        public string? Landing { get; set; }
        public Single Longitude { get; set; }
        public Single Latitude { get; set; }
        public Single AMSL { get; set; }
        public UInt64 PlaneID {  get; set; }  
        public UInt64[]? CrewID { get; set; }
        public UInt64[]? LoadID { get; set; }
        public Flight(): base()
        {
            OriginID = 0;
            TargetID = 0;
            TakeOff = null;
            Landing = null;
            Longitude = 0;
            Latitude = 0;
            AMSL = 0;
            PlaneID = 0;
            CrewID = null;
            LoadID = null;
        }
        public Flight(string[] values): base(values)
        {
            if (values.Length < 11) throw new InvalidNumberOfArgsException();

            OriginID = Convert.ToUInt64(values[1]);
            TargetID = Convert.ToUInt64(values[2]);
            TakeOff = values[3];
            Landing = values[4];
            Longitude = Convert.ToSingle(values[5], CultureInfo.InvariantCulture);
            Latitude = Convert.ToSingle(values[6], CultureInfo.InvariantCulture);
            AMSL = Convert.ToSingle(values[7], CultureInfo.InvariantCulture);
            PlaneID = Convert.ToUInt64(values[8]);
            
            string[] tmp = (values[9])
                .Trim('[', ']')
                .Split(';');
            CrewID = new UInt64[tmp.Length];
            CrewID = tmp.Select(UInt64.Parse).ToArray();

            string[] tmp2 = (values[10])
                .Trim('[', ']')
                .Split(';');   
            LoadID = new UInt64[tmp2.Length];
            LoadID = tmp2.Select(UInt64.Parse).ToArray();
        }
        public Flight(byte[] values) : base(values)
        {
            OriginID = BitConverter.ToUInt64(values, 15);
            TargetID = BitConverter.ToUInt64(values, 23);
            UInt64 TO = BitConverter.ToUInt64(values, 31);
            DateTimeOffset date = DateTimeOffset.FromUnixTimeMilliseconds((long)TO);
            TakeOff = date.ToString("dd.MM.yyyy HH:mm:ss");
            UInt64 LT = BitConverter.ToUInt64(values, 39);
            DateTimeOffset lt = DateTimeOffset.FromUnixTimeMilliseconds((long)LT);
            Landing = lt.ToString("dd.MM.yyyy HH:mm:ss");
            PlaneID = BitConverter.ToUInt64(values, 47);
            UInt16 CC = BitConverter.ToUInt16(values, 55); 
            CrewID = new UInt64[CC];
            for (int i = 0; i < CC; i++)
                CrewID[i] = BitConverter.ToUInt64(values, (57 + 8 * i));
            UInt16 PCC = BitConverter.ToUInt16(values, 57 + 8 * CC);
            LoadID = new UInt64[PCC];
            for (int i = 0; (i < PCC); i++)
                LoadID[i] = BitConverter.ToUInt64(values, 59 + 8 * CC + 8 * i);
        }
        public override string JsonSerialize()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
