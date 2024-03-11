using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace projOb
{
    [Serializable]
    public abstract class Person: MyObject
    {
        public string? Name { get; set; }
        public UInt64 Age { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public Person(): base()
        {
            Name = null;
            Age = 0;
            Phone = null;
            Email = null;
        }
        public Person(string[] values): base(values)
        {
            if (values.Length < 5) throw new InvalidNumberOfArgsException();

            Name = values[1];
            Age = Convert.ToUInt64(values[2]);
            Phone = values[3];
            Email = values[4];
        }
        public Person(byte[] values): base(values)
        {
            UInt16 NameL = BitConverter.ToUInt16(values, 15); 
            Name = Encoding.ASCII.GetString(values, 17, NameL);
            Age = BitConverter.ToUInt16(values, 17 + NameL);
            Phone = Encoding.ASCII.GetString(values, 19 +  NameL, 12);
            UInt16 EmailL = BitConverter.ToUInt16(values, 31 + NameL);
            Email = Encoding.ASCII.GetString(values, 33 + NameL, EmailL);
        }
        public override string JsonSerialize()
        {
            return JsonSerializer.Serialize(this);
        }
    }
    [Serializable]
    public class Crew: Person
    {
        public UInt16 Practice { get; set; }
        public string? Role { get; set; }
        public Crew(): base()
        {
            Practice = 0;
            Role = null;
        }
        public Crew(string[] values) : base(values)
        {
            if (values.Length < 7) throw new InvalidNumberOfArgsException();

            Practice = Convert.ToUInt16(values[5]);
            Role = values[6];
        }
        public Crew(byte[] values) : base(values)
        {
            Practice = BitConverter.ToUInt16(values, 33 + Name.Length + Email.Length);
            Role = Encoding.ASCII.GetString(values, 35 + Name.Length + Email.Length, 1);
        }
        public override string JsonSerialize()
        {
            return JsonSerializer.Serialize(this);
        }
    }
    [Serializable]
    public class Passenger: Person
    {
        public string? Class {  get; set; }
        public UInt64 Miles { get; set; }
        public Passenger(): base()
        {
            Class = null;
            Miles = 0;
        }
        public Passenger(string[] values) : base(values)
        {
            if (values.Length < 7) throw new InvalidNumberOfArgsException();

            Class = values[5];
            Miles = Convert.ToUInt64(values[6]);
        }
        public Passenger(byte[] values): base(values)
        {
            Class = Encoding.ASCII.GetString(values, 33 + Name.Length + Email.Length, sizeof(char));
            Miles = BitConverter.ToUInt64(values, 34 + Name.Length + Email.Length);
        }
        public override string JsonSerialize()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
