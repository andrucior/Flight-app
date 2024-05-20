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
        public Dictionary<string, string>? FieldStrings;
        public virtual void CreateFieldStrings()
        {
            FieldStrings = new Dictionary<string, string>()
            {
                { "id", ID.ToString() }
            };
        }
        public UInt64 ID { get; set; }
        public MyObject() { ID = 0; }
        [JsonConstructor]
        public MyObject(string[] values) { ID = Convert.ToUInt64(values[0]); }
        public MyObject(byte[] values) { ID = BitConverter.ToUInt64(values, 7); }
        public abstract string JsonSerialize();
        public virtual void Delete() { Project.MyObjects.Remove(this); }
        public string[] GetFields(string[] fields)
        {
            List<string> values = new List<string>();
            
            fields ??= FieldStrings.Keys.ToArray();

            foreach (var field in fields)
            {
                if (FieldStrings.ContainsKey(field))
                    values.Add(FieldStrings[field]);
            }
            
            return values.ToArray();
        }

    }
}
