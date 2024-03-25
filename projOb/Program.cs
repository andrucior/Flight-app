using projOb;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Runtime.CompilerServices;
using System.Text;
using Mapsui.Projections;
using Avalonia.Rendering;
using System.Drawing;
using System.Globalization;
// namespace NetworkSourceSimulator;
namespace FlightTrackerGUI;

class Project
{
    volatile private static bool running = true;
    static void Main(string[] args)
    {
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "test.ftr");
        Dictionary<string, (Generator, List<MyObject>)> generators = CreateDictionary();
        Read(filePath, generators);
        // string jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "backup.json");
        // Serialize(generators, jsonPath);
        
        // Dictionary<string, (Generator, List<MyObject>)> generators = CreateDictionary2ndStage();
        // CreateThreads(filePath, generators2);
        Thread.Sleep(1000);
        List<Flight> flights = GetFlightList(generators, "FL");
        List<Airport> airports = GetAirportList(generators, "AI");
        NewThreads(flights, airports);

    }
    static List<Flight> GetFlightList(Dictionary<string, (Generator generator, List<MyObject> list)> generators, string key)
    {
        (Generator, List <MyObject>) ans;
        
        generators.TryGetValue(key, out ans);
        List<Flight> flights = new List<Flight>();
        foreach (Flight flight in ans.Item2) 
        { 
            flights.Add(flight);    
        }
        return flights;
    }
    static List<Airport> GetAirportList(Dictionary<string, (Generator generator, List<MyObject> list)> generators, string key)
    {
        (Generator, List<MyObject>) ans;

        generators.TryGetValue(key, out ans);
        List<Airport> airports = new List<Airport>();
        foreach (Airport airport in ans.Item2)
        {
            airports.Add(airport);
        }
        return airports;
    }
    static Dictionary<string, (Generator generator, List<MyObject> list)> CreateDictionary()
    {
        Dictionary<string, (Generator generator, List<MyObject> list)> generators = new Dictionary<string, (Generator, List<MyObject>)>();
        generators.Add("C", (new CrewGenerator(), new List<MyObject>()));
        generators.Add("P", (new PassengerGenerator(), new List<MyObject>()));
        generators.Add("CA", (new CargoGenerator(), new List<MyObject>()));
        generators.Add("CP", (new CargoPlaneGenerator(), new List<MyObject>()));
        generators.Add("PP", (new PassengerPlaneGenerator(), new List<MyObject>()));
        generators.Add("AI", (new AirportGenerator(), new List<MyObject>()));
        generators.Add("FL", (new FlightGenerator(), new List<MyObject>()));
        return generators;
    }
    static Dictionary<string, (Generator, List<MyObject>)> CreateDictionary2ndStage()
    {
        Dictionary<string, (Generator generator, List<MyObject> list)> generators = new Dictionary<string, (Generator generator, List<MyObject> list)>();
        generators.Add("NCR", (new CrewGenerator(), new List<MyObject>()));
        generators.Add("NPA", (new PassengerGenerator(), new List<MyObject>()));
        generators.Add("NCA", (new CargoGenerator(), new List<MyObject>()));
        generators.Add("NCP", (new CargoPlaneGenerator(), new List<MyObject>()));
        generators.Add("NPP", (new PassengerPlaneGenerator(), new List<MyObject>()));
        generators.Add("NAI", (new AirportGenerator(), new List<MyObject>()));
        generators.Add("NFL", (new FlightGenerator(), new List<MyObject>()));
        return generators;
    }

    static void Read(string filePath, Dictionary<string, (Generator, List<MyObject>)> generators)
    {
        string? line;
        int lineNr = 1;
        
        
        using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using StreamReader sr = new StreamReader(fileStream);
       

        while ((line = sr.ReadLine()) != null)
        {
            string[] parms = line.Split(",");
            string name = parms[0];

            if (!generators.TryGetValue(name, out var tuple)) throw new ArgumentException();
            
            var generator = tuple.Item1;
            var list = tuple.Item2;

            try
            {
                var obj = generator.Create(parms[1..]);
                list.Add(obj);
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
    static void Serialize(Dictionary<string, (Generator, List<MyObject>)> generators, string path)
    {
        
        using StreamWriter jsStream = new StreamWriter(path);
        string json;

        foreach (var list in generators.Values)
        {
            lock (list.Item2)
            {
                foreach (var obj in list.Item2)
                {
                    json = obj.JsonSerialize();
                    jsStream.WriteLine(json);
                }
            }
        }
        
    }
    static void CreateThreads(string filePath, Dictionary<string, (Generator, List<MyObject>)> generators2)
    {
        string? message;
        NetworkSourceSimulator.NetworkSourceSimulator nss = new NetworkSourceSimulator.NetworkSourceSimulator(filePath, 0, 1);
        Thread server = new Thread(nss.Run);
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
                    SnapShot(generators2);
                }
            }
            return;
        });
       

        DataReader dr = new DataReaderGenerator().Create(nss, generators2);
        nss.OnNewDataReady += dr.ReadData;
        console.Start();
        server.Start();
       

    }
    
    static void SnapShot(Dictionary<string, (Generator, List<MyObject>)> generators)
    {
        DateTime now = DateTime.Now;
        string jsonPath = Path.Combine(Directory.GetCurrentDirectory(), $"snapshot_{now.Hour}_{now.Minute}_{now.Second}.json");
        Serialize(generators, jsonPath);
    }
    static void NewThreads(List<Flight> flights, List<Airport> airports)
    {
        DateTime start = DateTime.Now;
        FlightsGUIData flightsGUIData = new FlightsGUIData();
        List<FlightGUI> flightsList = new List<FlightGUI>();
        
        DateTime startDate = new DateTime(start.Year, start.Month, start.Day, 0, 0, 0);
        DateTime endDate = startDate.AddDays(1);
        
        Thread runner = new Thread(() =>
        {
            FlightTrackerGUI.Runner.Run();
        });
        
        Thread updateFlights = new Thread(() =>
        {
            while (startDate <= endDate)
            {

                //TimeSpan diff = star - start;

                lock (flightsList)
                {
                    flightsList.Clear();
                    flightsGUIData.UpdateFlights(flightsList);
                }

                foreach (var flight in flights) 
                {
                    string format = "dd:MM:yyyy HH:mm:ss";
                    CultureInfo provider = CultureInfo.InvariantCulture;

                    DateTime takeOff = Convert.ToDateTime(flight.TakeOff);
                    DateTime landing = Convert.ToDateTime(flight.Landing);

                    if (DateTime.Compare(takeOff, landing) >= 0) continue;

                    if (DateTime.Compare(takeOff, startDate) <= 0 && DateTime.Compare(landing, startDate) >= 0)
                    {
                        FlightAdapter flightGUI = new FlightAdapter(flight, airports, startDate);
                        lock (flightsList)
                            flightsList.Add(flightGUI);
                    }
                }
                
                lock (flightsGUIData)
                    flightsGUIData.UpdateFlights(flightsList);
                startDate = startDate.AddMinutes(30);
                Thread.Sleep(1000);
                
            }
            
            return;
        });
        // FlightTrackerGUI.Runner.UpdateGUI(flightsGUIData);
        Thread GUIdata = new Thread(() =>
        {
            while (startDate <= endDate)
            { 
                lock (flightsGUIData)
                    FlightTrackerGUI.Runner.UpdateGUI(flightsGUIData);
                Thread.Sleep(1000);
            }
            
        });
        runner.IsBackground = true;
        GUIdata.IsBackground = true;
        runner.Start();
        updateFlights.Start();
        GUIdata.Start();
        
        
    }
}