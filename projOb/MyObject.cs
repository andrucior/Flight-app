using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace projOb
{
    [Serializable]
    public abstract class MyObject
    {
        public UInt64 ID { get; set; }
        public MyObject() { ID = 0; }
        public MyObject(string[] values) { ID = Convert.ToUInt64(values[0]); }
        public MyObject(byte[] values) { ID = BitConverter.ToUInt64(values, 7); }
        public abstract string JsonSerialize();
    }
}
