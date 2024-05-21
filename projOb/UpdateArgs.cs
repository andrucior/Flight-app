using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projOb
{
    public class UpdateArgs: EventArgs
    {
        public UInt64 ID { get; private set; }
        public string? Name { get; private set; }
        public string? Description { get; private set; }
        public string? Origin { get; private set; }
        public string? Target { get; private set; }
        public string? TakeOff { get; private set; }
        public float Latitude { get; private set; }
        public float Longitude { get; private set; }
        public float AMSL { get; private set; }
        public string? ISO { get; private set; }
        public string? Model { get; private set; }
        public UInt64 FirstClassSize { get; private set; }
        public UInt64 EconomyClassSize { get; private set; }
        public string? MaxLoad { get; private set; }
        public float Weight { get; private set; }
        public string? Code { get; private set; }
        public string? Phone { get; private set; }
        public string? Email { get; private set; }
        public UInt64 Miles { get; private set; }
        public UInt64 Age { get; private set; }
        public UInt64 Practice { get; private set; }
        public string? Role { get; private set; }
        public string? Class { get; private set; }
        public UpdateArgs(string[] fields, string[] values)
        {

        }
        public UpdateArgs() { }


    }
}
