using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace projOb
{
    [Serializable]
    public abstract class MyObject
    {
        public string Abr { get; set; }
        public UInt64 ID { get; set; }
        public MyObject() { ID = 0; Abr = string.Empty; }
        [JsonConstructor]
        public MyObject(string[] values) 
        { 
            Abr = values[0];
            ID = Convert.ToUInt64(values[1]); 
        }
        public MyObject(byte[] values) { ID = BitConverter.ToUInt64(values, 7); }
        public abstract string JsonSerialize();
    }
}
