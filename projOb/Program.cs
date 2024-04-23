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
using ExCSS;
using FlightTrackerGUI;

class Project
{
    volatile private static bool running = true;
    private static FlightsGUIData FlightsGUIData = new FlightsGUIData();
    private static List<FlightGUI> FlightsList = new List<FlightGUI>();
    private static DateTime StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
    private static DateTime EndDate = StartDate.AddDays(1);
    private static Dictionary<string, Generator> generators = new Dictionary<string, Generator>();
    private static List<MyObject> MyObjects = new List<MyObject>();
    private static Subscriber? Subscriber;
    static void Main(string[] args)
    {
        string filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "test.ftr");
        string jsonPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "backup.json");
        string networkFilePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "example.ftre");

        // Etap 1
        generators = CreateDictionary();
        Read(filePath);
        Serialize(jsonPath);

        // Etap 2 i 4
        CreateThreadsConsoleServer(networkFilePath);
        generators = CreateDictionary2ndStage();
        CreateThreadsGUI();       

    }
    static List<Flight> GetFlightList()
    {
        return Generator.List.Flights;
    }
    static List<Airport> GetAirportList()
    {
        return Generator.List.Airports;
    }
    static List<IReportable> GetReportableList()
    {
        List<IReportable> reportables = new List<IReportable>();
        reportables.AddRange(Generator.List.Airports);
        reportables.AddRange(Generator.List.CargoPlanes);
        reportables.AddRange(Generator.List.PassengerPlaneList);
        return reportables;
    }


    static Dictionary<string, Generator> CreateDictionary()
    {
        generators = new Dictionary<string, Generator>();
        generators.Add("C", new CrewGenerator());
        generators.Add("P", new PassengerGenerator());
        generators.Add("CA", new CargoGenerator());
        generators.Add("CP", new CargoPlaneGenerator());
        generators.Add("PP", new PassengerPlaneGenerator());
        generators.Add("AI", new AirportGenerator());
        generators.Add("FL", new FlightGenerator());
        return generators;
    }
    static Dictionary<string, Generator> CreateDictionary2ndStage()
    {
        generators = new Dictionary<string, Generator>();
        generators.Add("NCR", new CrewGenerator());
        generators.Add("NPA", new PassengerGenerator());
        generators.Add("NCA", new CargoGenerator());
        generators.Add("NCP", new CargoPlaneGenerator());
        generators.Add("NPP", new PassengerPlaneGenerator());
        generators.Add("NAI", new AirportGenerator());
        generators.Add("NFL", new FlightGenerator());
        return generators;
    }

    static void Read(string filePath)
    {
        string? line;
        int lineNr = 1;
        
        using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using StreamReader sr = new StreamReader(fileStream);
        string errorPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "errors.txt");

        while ((line = sr.ReadLine()) != null)
        {
            string[] parms = line.Split(",");
            string name = parms[0];

            if (!generators.TryGetValue(name, out var tuple)) throw new ArgumentException();

            try
            {
                var obj = tuple.Create(parms[0..]);
                lock (MyObjects)
                {
                    MyObjects.Add(obj);
                }
            }
            catch (InvalidNumberOfArgsException ex)
            {
                throw new Exception($"Invalid number of arguments in line {lineNr}. Make sure it is correct. Object was omitted", ex);
            }
            catch (ArgumentException ex)
            {
                using StreamWriter sw = new StreamWriter(errorPath);
                sw.WriteLine($"Invalid arguments in line {lineNr}. Make sure it is correct", ex);
            }
            
            lineNr++;
        }
    }
    static void Serialize(string path)
    {
        using StreamWriter jsStream = new StreamWriter(path);
        string json;
        lock (MyObjects)
        {
            foreach (var obj in MyObjects)
            {
                json = obj.JsonSerialize();
                jsStream.WriteLine(json);
            }
        }
    }
    static void CreateThreadsConsoleServer(string filePath)
    {
        string? message;
        List<Media> medias = CreateMediaList();

        NetworkSourceSimulator.NetworkSourceSimulator nss = new NetworkSourceSimulator.NetworkSourceSimulator(filePath, 0, 1000);
        List<projOb.Plane> planes = [.. Generator.List.CargoPlanes, .. Generator.List.PassengerPlaneList];
        string logPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "log.txt");

        Subscriber = new Subscriber(nss, ref MyObjects, StartDate);
        Thread server = new Thread(Subscriber.DataSource.Run);
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
                    SnapShot();
                }
                if (message == "report")
                {
                    NewsGenerator newsGenerator = new NewsGenerator(medias, GetReportableList());
                    string? output = newsGenerator.GenerateTextNews();

                    while (output != null)
                    {
                        Console.WriteLine(output);
                        output = newsGenerator.GenerateTextNews();
                    }
                }
            }
            return;
        });

        DataReader dr = new DataReaderGenerator().Create(nss, generators, MyObjects);
        nss.OnNewDataReady += dr.ReadData;
        console.Start();
        server.Start();
    }

    static void SnapShot()
    {
        DateTime now = DateTime.Now;
        string jsonPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), $"snapshot_{now.Hour}_{now.Minute}_{now.Second}.json");
        Serialize(jsonPath);
    }
    static void CreateThreadsGUI()
    {
        Thread runner = new Thread(FlightTrackerGUI.Runner.Run);
        Thread updateFlights = new Thread(UpdateFlightsFun);
        Thread GUIdata = new Thread(GUIDataFun);
        runner.IsBackground = true;
        GUIdata.IsBackground = true;
        updateFlights.IsBackground = true;
        runner.Start();
        updateFlights.Start();
        GUIdata.Start();
    }
    static void GUIDataFun()
    {
        while (Project.running)
        {
            lock (FlightsGUIData)
                FlightTrackerGUI.Runner.UpdateGUI(FlightsGUIData);
            Thread.Sleep(1000);
        }
    }
    static void UpdateFlightsFun()
    {
        List<FlightAdapter> toRemove = new List<FlightAdapter>();   
        while (Project.running)
        {
            lock (FlightsList)
            {
                FlightsList.Clear();
                FlightsGUIData.UpdateFlights(FlightsList);
            }

            foreach (var flight in Generator.List.Flights)
            {
                DateTime takeOff = Convert.ToDateTime(flight.TakeOff);
                DateTime landing = Convert.ToDateTime(flight.Landing);

                if (DateTime.Compare(takeOff, StartDate) <= 0)// && DateTime.Compare(landing, StartDate) >= 0)
                {
                    Airport target = FlightAdapter.FindAirports(flight).destination;
                    FlightAdapterGenerator flightAdapterGenerator = new FlightAdapterGenerator();
                    FlightGUI flightGUI = flightAdapterGenerator.Create(flight, StartDate, 
                        new WorldPosition(flight.Latitude, flight.Longitude), new WorldPosition(target.Latitude, target.Longitude));

                    lock (FlightsList)
                        FlightsList.Add(flightGUI);
                }

            }
            
            lock (FlightsGUIData)
                FlightsGUIData.UpdateFlights(FlightsList);
            StartDate = StartDate.AddMinutes(30);
            Subscriber.StartDate = StartDate;
            Thread.Sleep(1000);
        }

        return;
    }
    static List<Media> CreateMediaList()
    {
        string[] radioNames = { "Radio Kwantyfikator", "Radio Shmem" };
        string[] tvNames = { "Telewizja Abelowa", "Kanał TV-tensor" };
        string[] newspaperNames = { "Gazeta Kategoryczna", "Dziennik Politechniczny" };

        List<Media> medias = new List<Media>();
        RadioGenerator radioGenerator = new RadioGenerator();
        TVGenerator TVGenerator = new TVGenerator();
        NewspaperGenerator newspaperGenerator = new NewspaperGenerator();

        for (int i = 0; i < radioNames.Length; i++)
            medias.Add(radioGenerator.Create(radioNames[i]));
        for (int i = 0; i < tvNames.Length; i++)
            medias.Add(TVGenerator.Create(tvNames[i]));
        for (int i = 0; i < newspaperNames.Length; i++)
            medias.Add(newspaperGenerator.Create(newspaperNames[i]));
        
        return medias;
    }
    static void Program4thStage()
    {
        List<Media> medias = CreateMediaList();
        
        while (Project.running)
        {
            string? message = Console.ReadLine();

            if (message == "report")
            {
                NewsGenerator newsGenerator = new NewsGenerator(medias, GetReportableList());
                string? output = newsGenerator.GenerateTextNews();
                
                while (output != null)
                {
                    Console.WriteLine(output);
                    output = newsGenerator.GenerateTextNews();
                }
            }
            
            if (message == "exit")
                Project.running = false;
            
            if (message == null)
                throw new Exception("Error occured while reading from console");
        }
    }
}