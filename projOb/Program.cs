using projOb;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;

class Project
{
    static void Main(string[] args)
    {
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "test.ftr");
        string jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "backup.json");
        
        Dictionary<string, Generator> generators = CreateDictionary();
        
        List<MyObject> objects = new List<MyObject>();
        using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using StreamReader sr = new StreamReader(fileStream);
        using StreamWriter jsStream = new StreamWriter(jsonPath);

        string? line;
        int lineNr = 1;
        string json;

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
}