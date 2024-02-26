using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projOb
{
    [Serializable]
    public class Cargo: MyObject
    {
        public Single Weight { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }

        public Cargo(string[] values): base(values)
        {
            if (values.Length < 4) throw new InvalidNumberOfArgsException();

            Weight = Convert.ToSingle(values[1], CultureInfo.InvariantCulture);
            Code = values[2];
            Description = values[3];
        }
    }
}
