using projOb;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Runtime.CompilerServices;
using System.Text;
namespace NetworkSourceSimulator;

// todo:
// Referencje do obiektu 
class Project
{
    volatile static bool running = true;
    static List<MyObject> objects = new List<MyObject>();
    static void Main(string[] args)
    {
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "test.ftr");
        Dictionary<string, Generator> generators = CreateDictionary();        
        List<MyObject> objects = new List<MyObject>();
        ReadAndSerialize(filePath, generators, objects);
        CreateThreads(filePath);
    }
    static Dictionary<string, Generator> CreateDictionary()
    {
        Dictionary<string, Generator> generators = new Dictionary<string, Generator>();
        generators.Add("C", new CrewGenerator());
        generators.Add("P", new PassengerGenerator());
        generators.Add("CA", new CargoGenerator());
        generators.Add("CP", new CargoPlaneGenerator());
        generators.Add("PP", new PassengerPlaneGenerator());
        generators.Add("AI", new AirportGenerator());
        generators.Add("FL", new FlightGenerator());
        return generators;
    }
    static Dictionary<string, Generator> CreateDictionaryNSS()
    {
        Dictionary<string, Generator> generators = new Dictionary<string, Generator>();
        generators.Add("NCR", new CrewGenerator());
        generators.Add("NPA", new PassengerGenerator());
        generators.Add("NCA", new CargoGenerator());
        generators.Add("NCP", new CargoPlaneGenerator());
        generators.Add("NPP", new PassengerPlaneGenerator());
        generators.Add("NAI", new AirportGenerator());
        generators.Add("NFL", new FlightGenerator());
        return generators;
    }

    static void ReadAndSerialize(string filePath, Dictionary<string, Generator> generators, List<MyObject> objects)
    {
        string? line;
        int lineNr = 1;
        string json;
        string jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "backup.json");
        using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using StreamReader sr = new StreamReader(fileStream);
        using StreamWriter jsStream = new StreamWriter(jsonPath);

        while ((line = sr.ReadLine()) != null)
        {
            string[] parms = line.Split(",");
            string name = parms[0];

            if (!generators.TryGetValue(name, out var generator)) throw new ArgumentException();

            try
            {
                var obj = generator.Create(parms[1..]);
                objects.Add(obj);
                json = obj.JsonSerialize();
                jsStream.Write(json);
            }
            catch (InvalidNumberOfArgsException ex)
            {
                throw new Exception($"Invalid number of arguments in line {lineNr}. Make sure it is correct. Object was omitted", ex);
            }
            catch (ArgumentException ex)
            {
                throw new Exception($"Invalid name in line {lineNr}. Make sure it is correct", ex);
            }
            lineNr++;
        }
    }
    static void CreateThreads(string filePath)
    {
        string? message;
        NetworkSourceSimulator nss = new NetworkSourceSimulator(filePath, 0, 1);
        Thread server = new Thread(new ThreadStart(nss.Run));
        Thread console = new Thread(() =>
        {
            while (Project.running)
            {
                message = Console.ReadLine();
                if (message == "exit")
                {
                    if (server.IsAlive)
                        server.IsBackground = true;
                    Project.running = false;
                }
                if (message == "print")
                {
                    DateTime now = DateTime.Now;
                    string jsonPath = Path.Combine(Directory.GetCurrentDirectory(), $"snapshot_{now.Hour}_{now.Minute}_{now.Second}.json");
                    using StreamWriter streamWriter = new StreamWriter(jsonPath);
                    foreach (var obj in Project.objects)
                    {
                        string jSon = obj.JsonSerialize();
                        streamWriter.Write(jSon);
                    }
                }
            }
            return;
        });
        nss.OnNewDataReady += ReadData;
        console.Start();
        server.Start();    
    }
    static void ReadData(object sender, NewDataReadyArgs args) 
    {
        Dictionary<string, Generator> generators = CreateDictionaryNSS();
        NetworkSourceSimulator nss =  (NetworkSourceSimulator) sender;
        Message msg = nss.GetMessageAt(args.MessageIndex);
        string result = Encoding.ASCII.GetString(msg.MessageBytes[0..3]);
        generators.TryGetValue(result, out var generator);
        var obj = generator.CreateByte(msg.MessageBytes);
        Project.objects.Add(obj);
    }
    
}