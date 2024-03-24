using NetworkSourceSimulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace projOb
{
    public class DataReader
    {
        private NetworkSourceSimulator.NetworkSourceSimulator Nss { get; set; }
        private Dictionary<string, (Generator, List<MyObject>)> Generators;
        
        public DataReader(NetworkSourceSimulator.NetworkSourceSimulator Nss, ref Dictionary<string, (Generator, List<MyObject>)> Generators)
        {
            this.Nss = Nss;
            this.Generators = Generators;
        }

        public void ReadData(object sender, NewDataReadyArgs args)
        {
            Message msg = Nss.GetMessageAt(args.MessageIndex);
            string result = Encoding.ASCII.GetString(msg.MessageBytes[0..3]);
            Generators.TryGetValue(result, out var generator);
            var obj = generator.Item1.CreateByte(msg.MessageBytes);
            lock (generator.Item2)
                generator.Item2.Add(obj);
        }
    }
}
