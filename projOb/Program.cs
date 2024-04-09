﻿using projOb;
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
namespace FlightTrackerGUI;

class Project
{
    volatile private static bool running = true;
    private static FlightsGUIData flightsGUIData = new FlightsGUIData();
    private static List<FlightGUI> flightsList = new List<FlightGUI>();
    private static DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
    private static DateTime endDate = startDate.AddDays(1);
    private static Dictionary<string, Generator> generators = new Dictionary<string, Generator>();
    private static List<Flight> flights = new List<Flight>();
    private static List<Airport> airports = new List<Airport>();
    private static List<MyObject> myObjects = new List<MyObject>();

    // Wzorce:
    // - visitor
    // - iterator

    static void Main(string[] args)
    {
        string filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "test.ftr");
        string jsonPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "backup.json");

        // Etap 1
        generators = CreateDictionary();
        Read(filePath);
        Serialize(jsonPath);

        // Etap 2
        // generators = CreateDictionary2ndStage();
        // CreateThreads2ndStage(filePath);
        // Thread.Sleep(1000);

        // Etap 3
        // flights = GetFlightList();
        airports = GetAirportList();
        // CreateThreads3rdStage();

        // Etap 4
        Program4thStage();

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

        while ((line = sr.ReadLine()) != null)
        {
            string[] parms = line.Split(",");
            string name = parms[0];

            if (!generators.TryGetValue(name, out var tuple)) throw new ArgumentException();

            try
            {
                var obj = tuple.Create(parms[0..]);
                lock (myObjects)
                {
                    myObjects.Add(obj);
                }
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
        lock (myObjects)
        {
            foreach (var obj in myObjects)
            {
                json = obj.JsonSerialize();
                jsStream.WriteLine(json);
            }
        }
    }
    static void CreateThreads2ndStage(string filePath)
    {
        string? message;
        NetworkSourceSimulator.NetworkSourceSimulator nss = new NetworkSourceSimulator.NetworkSourceSimulator(filePath, 0, 0);
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
                    SnapShot();
                }
                if (message == "report")
                {
                    /*
                    report wypisuje na konsolę przegląd wiadomości wygenerowany na podstawie danych
                    wczytanych z pliku FTR
                    */
                }
            }
            return;
        });

        DataReader dr = new DataReaderGenerator().Create(nss, generators, myObjects);
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
    static void CreateThreads3rdStage()
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

                if (DateTime.Compare(takeOff, landing) < 0)
                {
                    if (DateTime.Compare(takeOff, startDate) <= 0 && DateTime.Compare(landing, startDate) >= 0)
                    {
                        FlightAdapterGenerator flightAdapterGenerator = new FlightAdapterGenerator();
                        FlightGUI flightGUI = flightAdapterGenerator.Create(flight, airports, startDate);
                        lock (flightsList)
                            flightsList.Add(flightGUI);
                    }
                }
            }

            lock (flightsGUIData)
                flightsGUIData.UpdateFlights(flightsList);
            startDate = startDate.AddMinutes(10);
            Thread.Sleep(1000);
        }

        return;
    }
    static void Program4thStage()
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

        NewsGenerator newsGenerator = new NewsGenerator(medias, GetReportableList());
        while (newsGenerator.HasMore())
            Console.WriteLine(newsGenerator.GenerateTextNews());
        while (true)
        {
            if (Console.ReadLine() == "report")
            {
                if (newsGenerator.GenerateTextNews() == null)
                    Console.WriteLine("No more news to be generated");
            }
        }
    }
}