using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projOb
{
    public abstract class Command
    {
        public Dictionary<string, IEnumerable<MyObject>> Objects = new Dictionary<string, IEnumerable<MyObject>>
        {
            { "Flight", Generator.List.Flights },
            { "Crew", Generator.List.CrewList },
            { "Passenger", Generator.List.PassengerList },
            { "Cargo",  Generator.List.CargoList },
            { "CargoPlane", Generator.List.CargoPlanes },
            { "PassengerPlane", Generator.List.PassengerPlaneList },
            { "Airport", Generator.List.Airports }
        };

        public CommandParser? Parser { get; private set; }
        public Command(string line) 
        {
            Parser = new CommandParser(line);
        }
        public abstract void Execute();
    }
    public class Display: Command
    {
        public new DisplayParser? Parser { get; private set; }
        public Display(string line): base(line) 
        {
            Parser = new DisplayParser(line);
            Objects.TryGetValue(Parser.ClassName, out var objects);
            foreach (var condition in Parser.Conditions)
            {
                foreach (var obj in objects)
                {
                    var cond = new ConditionMaker(condition, obj);
                    if (cond.CheckPredicate())
                        Console.WriteLine(obj.JsonSerialize());
                }
            }

        }
        public override void Execute()
        {
                    
        }
    }
    public class Delete: Command 
    {
        public new DeleteParser? Parser { get; private set; }
        public Delete(string line): base(line) { Parser = new DeleteParser(line); }
        public override void Execute() { }
    }
    public class Add: Command
    {
        public new AddParser Parser { get; private set; }
        public Add(string line): base(line) { Parser = new AddParser(line); }
        public override void Execute() { }
    }

}
