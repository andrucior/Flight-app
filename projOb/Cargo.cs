using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace projOb
{
    [Serializable]
    public class Cargo: MyObject
    {
        public Single Weight { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public Cargo(): base()
        {
            Weight = 0;
            Code = null;
            Description = null;
        }
        [JsonConstructor]
        public Cargo(string[] values): base(values)
        {
            if (values.Length < 3) throw new InvalidNumberOfArgsException();

            Weight = Convert.ToSingle(values[1], CultureInfo.InvariantCulture);
            Code = values[2];
            Description = values[3];
        }
        public Cargo(byte[] values) : base(values)
        {
            Weight = BitConverter.ToSingle(values, 15);
            Code = Encoding.ASCII.GetString(values, 19, 6);
            UInt16 DL = BitConverter.ToUInt16(values, 25);
            Description = Encoding.ASCII.GetString(values, 27, DL);
        }
        public override string JsonSerialize()
        {
            return JsonSerializer.Serialize(this);
        }
        public override void Delete()
        {
            base.Delete();
            Generator.List.CargoList.Remove(this);
        }
        public override void CreateFieldStrings()
        {
            base.CreateFieldStrings();
            FieldStrings.Add("weight", Weight.ToString());
            FieldStrings.Add("code", Code);
            FieldStrings.Add("description", Description);
        }
    }
}
