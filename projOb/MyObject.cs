using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projOb
{
    [Serializable]
    public abstract class MyObject
    {
        public UInt64 ID { get; set; }
        public MyObject(string[] values)
        {
            ID = Convert.ToUInt64(values[0]);
        }
    }
}
