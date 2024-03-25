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
using System.Net.WebSockets;
using Avalonia.Controls.Shapes;
// namespace NetworkSourceSimulator;
namespace FlightTrackerGUI;

class Project
{
    volatile private static bool running = true;
    private static DateTime start = DateTime.Now;
    private static FlightsGUIData flightsGUIData = new FlightsGUIData();
    private static List<FlightGUI> flightsList = new List<FlightGUI>();
    private static DateTime startDate = new DateTime(start.Year, start.Month, start.Day, 0, 0, 0);
    private static DateTime endDate = startDate.AddDays(1);
    private static Dictionary<string, (Generator, List<MyObject>)> generators = new Dictionary<string, (Generator, List<MyObject>)>();

    static void Main(string[] args)
    {
        string filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "test.ftr");

        // Etap 1
        generators = CreateDictionary();
        Read(filePath);
        // string jsonPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "backup.json");
        // Serialize(jsonPath);

        // Etap 2
        // generators = CreateDictionary2ndStage();
        // CreateThreads(filePath);
        // Read(jsonPath);

        // Etap 3
        List<Flight> flights = GetFlightList("FL");
        List<Airport> airports = GetAirportList("AI");
        NewThreads(flights, airports);

    }
    static List<Flight> GetFlightList(string key)
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
    static List<Airport> GetAirportList(string key)
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
        generators = new Dictionary<string, (Generator, List<MyObject>)>();
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
        generators = new Dictionary<string, (Generator generator, List<MyObject> list)>();
        generators.Add("NCR", (new CrewGenerator(), new List<MyObject>()));
        generators.Add("NPA", (new PassengerGenerator(), new List<MyObject>()));
        generators.Add("NCA", (new CargoGenerator(), new List<MyObject>()));
        generators.Add("NCP", (new CargoPlaneGenerator(), new List<MyObject>()));
        generators.Add("NPP", (new PassengerPlaneGenerator(), new List<MyObject>()));
        generators.Add("NAI", (new AirportGenerator(), new List<MyObject>()));
        generators.Add("NFL", (new FlightGenerator(), new List<MyObject>()));
        return generators;
    }

    static void Read(string filePath)
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
                var obj = generator.Create(parms[0..]);
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
    static void Serialize(string path)
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
        string jsonPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), $"snapshot_{now.Hour}_{now.Minute}_{now.Second}.json");
        Serialize(jsonPath);
    }
    static void NewThreads(List<Flight> flights, List<Airport> airports)
    {

        Thread runner = new Thread(FlightTrackerGUI.Runner.Run);
        Thread updateFlights = new Thread(updateFlightsFun);
        Thread GUIdata = new Thread(GUIDataFun);
        runner.IsBackground = true;
        GUIdata.IsBackground = true;

        runner.Start();
        updateFlights.Start();
        GUIdata.Start();
    }
    static void GUIDataFun()
    {
        while (startDate <= endDate)
        {
            lock (flightsGUIData)
                FlightTrackerGUI.Runner.UpdateGUI(flightsGUIData);
            Thread.Sleep(1000);
        }
    }
    static void updateFlightsFun()
    {

        var flights = GetFlightList("FL");
        var airports = GetAirportList("AI");

        while (startDate <= endDate)
        {
            lock (flightsList)
            {
                flightsList.Clear();
                flightsGUIData.UpdateFlights(flightsList);
            }

            foreach (var flight in flights)
            {
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
            startDate = startDate.AddMinutes(10);
            Thread.Sleep(1000);
        }
        return;
    }
}