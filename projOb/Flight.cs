using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projOb
{
    [Serializable]
    public class Flight: MyObject
    {
        public UInt64 OriginID { get; set; }
        public UInt64 TargetID { get; set; }
        public string TakeOff { get; set; }
        public string Landing {  get; set; }
        public Single Longitude { get; set; }
        public Single Latitude { get; set; }
        public Single AMSL { get; set; }
        public UInt64 PlaneID {  get; set; }  
        public UInt64[] CrewID { get; set; }
        public UInt64[] LoadID { get; set; }

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
    }
}
