using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace projOb
{
    [Serializable]
    public abstract class Person: MyObject
    {
        public string Name { get; set; }
        public UInt64 Age { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public Person(string[] values): base(values)
        {
            if (values.Length < 5) throw new InvalidNumberOfArgsException();

            Name = values[1];
            Age = Convert.ToUInt64(values[2]);
            Phone = values[3];
            Email = values[4];
        }
    }
    [Serializable]
    public class Crew: Person
    {
        public UInt16 Practice { get; set; }
        public string Role { get; set; }

        public Crew(string[] values) : base(values)
        {
            if (values.Length < 7) throw new InvalidNumberOfArgsException();

            Practice = Convert.ToUInt16(values[5]);
            Role = values[6];
        }
    }
    [Serializable]
    public class Passenger: Person
    {
        public string Class {  get; set; }
        public UInt64 Miles { get; set; }

        public Passenger(string[] values) : base(values)
        {
            if (values.Length < 7) throw new InvalidNumberOfArgsException();

            Class = values[5];
            Miles = Convert.ToUInt64(values[6]);
        }
    }



}
