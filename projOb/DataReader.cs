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
        private Dictionary<string, Generator> Generators;
        private List<MyObject> MyObjects;
        public DataReader(NetworkSourceSimulator.NetworkSourceSimulator Nss, ref Dictionary<string, Generator> Generators, ref List<MyObject> myObjects)
        {
            this.Nss = Nss;
            this.Generators = Generators;
            this.MyObjects = myObjects;
        }
        public void ReadData(object sender, NewDataReadyArgs args)
        {
            Message msg = Nss.GetMessageAt(args.MessageIndex);
            string result = Encoding.ASCII.GetString(msg.MessageBytes[0..3]);
            Generators.TryGetValue(result, out var generator);
            
            if (generator == null) throw new Exception("Generator not found");
            
            var obj = generator.CreateByte(msg.MessageBytes);
            lock(MyObjects)
                MyObjects.Add(obj);   
        }
    }
}
