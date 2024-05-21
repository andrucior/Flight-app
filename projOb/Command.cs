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
            { "flight", Generator.List.Flights },
            { "crew", Generator.List.CrewList },
            { "passenger", Generator.List.PassengerList },
            { "cargo",  Generator.List.CargoList },
            { "cargoplane", Generator.List.CargoPlanes },
            { "passengerplane", Generator.List.PassengerPlaneList },
            { "airport", Generator.List.Airports }
        };
        static public Dictionary<string, CommandGenerator> CommandGenerators = new Dictionary<string, CommandGenerator>
        {
            { "display", new DisplayGenerator() },
            { "add", new AddGenerator() },
            { "delete", new DeleteGenerator() },
            { "update", new UpdateGenerator() }
        };
        public Dictionary<string, Generator> Generators = new Dictionary<string, Generator>()
        {
            { "flight", new FlightGenerator() },
            { "crew", new CrewGenerator() },
            { "passenger", new PassengerGenerator() },
            { "cargo", new CargoGenerator() },
            { "cargoplane", new CargoPlaneGenerator() },
            { "paseengerplane", new PassengerPlaneGenerator() },
            { "airport", new AirportGenerator() }
        };

        public CommandParser Parser { get; private set; }
        public Command(string line) { Parser = new CommandParser(line); }
        public abstract void Execute();
    }
    public class Display: Command
    {
        public new DisplayParser Parser { get; private set; }
        public Display(string line): base(line) 
        {
            Parser = new DisplayParser(line);
        }
        public override void Execute()
        {
            Objects.TryGetValue(Parser.ClassName, out var objects);

            if (objects == null) throw new Exception();
            
            List<MyObject> toDisplay = new List<MyObject>();
           
            lock (objects)
            {
                
                foreach (var obj in objects)
                {
                    bool toAdd = true;
                    if (Parser.Conditions != null)
                        toAdd = CheckConditions(obj);
                    if (toAdd)
                        toDisplay.Add(obj);
                }
            }
            DisplayOnConsole(toDisplay, Parser.ObjectFields);
        }
        public bool CheckConditions(MyObject obj)
        {
            bool toAdd = false;
            int condCounter = 0;
            foreach (var condition in Parser.Conditions)
            {
                var cond = new ConditionMaker(condition, obj);
                if (condCounter == 0)
                    toAdd = cond.CheckPredicate();
                else if (Parser.OR_AND[condCounter - 1] == "&&")
                    toAdd = toAdd && cond.CheckPredicate();
                else
                    toAdd = toAdd || cond.CheckPredicate();
                condCounter++;
            }
            return toAdd;
        }
        public void DisplayOnConsole(List<MyObject> objects, string[] objectFields)
        {
            if (objectFields == null && objects.Count > 0)
            {
                objects[0].CreateFieldStrings();
                objectFields = objects[0].FieldStrings.Keys.ToArray();
            }
            if (objects.Count == 0) 
                return;
            
            string[,] fieldValues = new string[objects.Count + 1, objectFields.Length];
            for (int k = 0; k < objectFields.Length; k++)
                fieldValues[0, k] = objectFields[k].ToUpper();
            int i = 1;
            foreach(var obj in objects)
            {
                obj.CreateFieldStrings();
                var tmp = obj.GetFields(objectFields);
                for(int j = 0; j < tmp.Length; j++)
                    fieldValues[i, j] = tmp[j];
                i++;
            }
            DisplayMatrixAsTable(fieldValues);

        }
        static void DisplayMatrixAsTable(string[,] matrix)
        {
            // Get the dimensions of the matrix
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            // Find the maximum length of elements in each column
            int[] maxLengths = new int[cols];
            for (int col = 0; col < cols; col++)
            {
                int maxLength = 0;
                for (int row = 0; row < rows; row++)
                {
                    int length = matrix[row, col].Length;
                    if (length > maxLength)
                    {
                        maxLength = length;
                    }
                }
                maxLengths[col] = maxLength;
            }

            // Display the matrix as a table
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    Console.Write(matrix[row, col].PadRight(maxLengths[col] + 2)); // Add padding for alignment
                    Console.Write("|".PadRight(maxLengths[col]));
                }
                Console.WriteLine();
                Console.WriteLine(new string('-', (cols - 1) * maxLengths[cols - 1])); // Move to the next row
            }
        }
    }
    public class Delete: Command 
    {
        public new DeleteParser Parser { get; private set; }
        public Delete(string line): base(line) { Parser = new DeleteParser(line); }
        public override void Execute() 
        {
            Objects.TryGetValue(Parser.ClassName, out var objects);
            
            if (objects == null) throw new Exception();

            lock (objects)
            {
                var list = objects.ToList();
                for(int i = 0; i < list.Count; i++)
                {
                    var obj = list[i];
                    bool toDelete = true; 
                    if (Parser.Conditions != null)
                        toDelete = CheckConditions(obj);
                    if (toDelete) 
                        obj.Delete();
                }
            }
        }
        public bool CheckConditions(MyObject obj)
        {
            bool toAdd = false;
            int condCounter = 0;
            foreach (var condition in Parser.Conditions)
            {
                var cond = new ConditionMaker(condition, obj);
                if (condCounter == 0)
                    toAdd = cond.CheckPredicate();
                else if (Parser.OR_AND[condCounter - 1] == "&&")
                    toAdd = toAdd && cond.CheckPredicate();
                else
                    toAdd = toAdd || cond.CheckPredicate();
                condCounter++;
            }
            return toAdd;
        }
    }
    public class Add: Command
    {
        public new AddParser Parser { get; private set; }
        public Add(string line): base(line) { Parser = new AddParser(line); }
        public override void Execute() 
        {
            string key = Parser.ClassName.ToLower();
            Generators[key].Create(Parser.KeyValueList);
        }
    }
    public class Update : Command
    {
        public new UpdateParser Parser { get; private set; }
        public Update(string line): base(line) { Parser = new UpdateParser(line); }
        public event EventHandler Changed;
        public override void Execute()
        {
            Objects.TryGetValue(Parser.ClassName, out var objects);
            
            if (objects == null) throw new Exception();

            string[] fields = Parser.KeyValueList.Select(CommandParser.GetLeftHandSide).ToArray();
            fields = fields.Select(x => x.Trim()).ToArray();
            string[] values = Parser.KeyValueList.Select(CommandParser.GetRightHandSide).ToArray();
            values = values.Select(x => x.Trim()).ToArray();
            FieldStringsSubscriber fieldStringsSubscriber = new FieldStringsSubscriber();
             
            foreach (var obj in objects)
            {
                bool toUpdate = true;
                int i = 0;
                foreach (var field in fields)
                {
                    
                    if (Parser.Conditions != null)
                    {
                        foreach(var condition in Parser.Conditions)
                        {
                            ConditionMaker cond = new ConditionMaker(condition, obj);
                            
                            if (!cond.CheckPredicate())
                            {
                                toUpdate = false;
                                break;
                            }
                        }
                    }
                    if (toUpdate)
                    {
                        Changed += obj.OnUpdate;
                        obj.FieldStrings[$"{field}"] = values[i++];
                        this.Changed?.Invoke(this, new EventArgs());
                        Changed -= obj.OnUpdate;
                    }
                }
                
            }
        }
    }

}
