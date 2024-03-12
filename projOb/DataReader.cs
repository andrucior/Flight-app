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
        private List<MyObject> Objects { get; set; }
        private Dictionary<string, Generator> Generators;

        public DataReader(NetworkSourceSimulator.NetworkSourceSimulator Nss, ref List<MyObject> objects)
        {
            this.Nss = Nss;
            this.Objects = objects;
            Generators = new Dictionary<string, Generator>();
            Generators.Add("NCR", new CrewGenerator());
            Generators.Add("NPA", new PassengerGenerator());
            Generators.Add("NCA", new CargoGenerator());
            Generators.Add("NCP", new CargoPlaneGenerator());
            Generators.Add("NPP", new PassengerPlaneGenerator());
            Generators.Add("NAI", new AirportGenerator());
            Generators.Add("NFL", new FlightGenerator());
        }

        public void ReadData(object sender, NewDataReadyArgs args)
        {
            Message msg = Nss.GetMessageAt(args.MessageIndex);
            string result = Encoding.ASCII.GetString(msg.MessageBytes[0..3]);
            Generators.TryGetValue(result, out var generator);
            var obj = generator.CreateByte(msg.MessageBytes);
            Objects.Add(obj);
        }
    }
}
