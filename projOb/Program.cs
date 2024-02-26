using projOb;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;

class Project
{
    static void Main(string[] args)
    {
        string filePath = @"D:\andrz\Documents\test.ftr.txt";
        
        Dictionary<string, Func<string[], MyObject>> generators = new Dictionary<string, Func<string[], MyObject>>();
        
        generators.Add("C", arg => { return new Crew(arg); });
        generators.Add("P", arg => { return new Passenger(arg); });
        generators.Add("CA", arg => { return new Cargo(arg); });
        generators.Add("CP", arg => { return new CargoPlane(arg); });
        generators.Add("PP", arg => { return new PassengerPlane(arg); });
        generators.Add("AI", arg => { return new Airport(arg); });
        generators.Add("FL", arg => { return new Flight(arg); });
        
        List<MyObject> objects = new List<MyObject>();

        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            using (StreamReader sr = new StreamReader(fileStream)) 
            {
                string? line;
                int lineNr = 1;

                while ((line = sr.ReadLine()) != null)
                {
                    string[] parms = line.Split(",");
                    string name = parms[0];
                    if (!generators.TryGetValue(name, out var func)) throw new ArgumentException();
                    
                    try
                    {
                        var obj = func(parms[1..]);
                        objects.Add(obj);
                    }
                    catch (InvalidNumberOfArgsException ex)
                    {
                        throw new Exception($"Invalid number of arguments in line {lineNr}. Make sure it is correct. Object was omitted", ex);
                    }
                    catch(ArgumentException ex) 
                    {
                        throw new Exception($"Invalid name in line {lineNr}. Make sure it is correct", ex);
                    }

                    lineNr++;   
                }
            }   
        }

        string json = JsonSerializer.Serialize(objects);
        string jsonPath = @"D:\andrz\Documents\backup.json";
        using (StreamWriter fileStream = new StreamWriter(jsonPath))
        {
            fileStream.Write(json);
        }

    }
}