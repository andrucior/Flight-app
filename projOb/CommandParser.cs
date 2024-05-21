using DynamicData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace projOb
{
    public class CommandParser
    {
        public string[] Command { get; private set; }
        public string CommandName { get; private set; }
        public readonly static Dictionary<string, CommandParserGenerator> Parsers = new Dictionary<string, CommandParserGenerator>
        {
            { "display", new DisplayParserGenerator() },
            { "update", new UpdateParserGenerator() },
            { "delete", new DeleteParserGenerator() },
            { "add", new AddParserGenerator() }
        };

        public CommandParser(string line)
        {
            Command = line.Split(" ");
            CommandName = Command[0];
        }
        static public string GetRightHandSide(string input)
        {
            string[] splitted = input.Split('=');
            return splitted[2];
        }
        static public string GetLeftHandSide(string input)
        {
            string[] splitted = input.Split('=');
            return splitted[0];
        }

    }
    public class DisplayParser : CommandParser
    {
        public string[]? ObjectFields { get; private set; }
        public string[]? Conditions { get; private set; }
        public List<string> OR_AND;
        public string ClassName { get; private set; }
        public DisplayParser(string line) : base(line)
        {
            OR_AND = new List<string>();
            if (!Command[1].Contains('*'))
            {
                ObjectFields = Command[1].Split(",");
                ObjectFields = ObjectFields.Select(x => x.ToLower()).ToArray();
            }
            else
            {
                ObjectFields = null;
            }
            ClassName = Command[3];

            if (Command.Select(x => x.ToLower()).Contains("where"))
            {
                string tmp = string.Join(' ', Command[5..]);
                
                List<int> ors = new List<int>();
                List<int> ands = new List<int>();
                Conditions = Regex.Split(tmp, @"\s+and\s+|\s+or\s+", RegexOptions.IgnoreCase);
                Conditions = Conditions.Select(x => x.Trim()).ToArray();
                for (int j = 0; j < Command.Length; j++)
                {
                    if (Command[j] == "or")
                        OR_AND.Add("||");
                    if (Command[j] == "and")
                        OR_AND.Add("&&");
                } 
            }
        }
    }
    public class UpdateParser: CommandParser
    {
        public string ClassName { get; private set; }
        public string[]? Conditions { get; private set; }
        public string[] KeyValueList { get; private set; }
        public UpdateParser(string line) : base(line) 
        {
            ClassName = Command[1];
            string tmp = string.Join(' ', Command[3..]);
            int index = tmp.IndexOf("where", StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                KeyValueList = tmp.Substring(0, index).TrimEnd('(', ')').Split(',');
            }
            else
            {
                KeyValueList = tmp.TrimEnd('(', ')').Split(',');
            }

            if (index >= 0)
            {
                tmp = string.Join(' ', Command[--index..]);
                Conditions = Regex.Split(tmp, @"\s+and\s+|\s+or\s+", RegexOptions.IgnoreCase);
                Conditions = Conditions.Select(x => x.Trim()).ToArray();
            }
        }
    }
    public class DeleteParser: CommandParser
    {
        public string ClassName { get; private set; }
        public string[]? Conditions { get; private set; }    
        public DeleteParser(string line) : base(line) 
        {
            ClassName = Command[1];
            if (Command.Length > 3) 
            {
                string tmp = string.Join(' ', Command[3..]);
                Conditions = Regex.Split(tmp, @"\s+and\s+|\s+or\s+", RegexOptions.IgnoreCase);
                Conditions = Conditions.Select(x => x.Trim()).ToArray();
            }
        }
    }
    public class AddParser: CommandParser
    {
        public string? ClassName { get; private set; }
        public string[]? KeyValueList { get; private set; }
        public AddParser(string line) : base(line) 
        { 
            ClassName = Command[2];
            string tmp = Command[3];
            KeyValueList = tmp.TrimEnd('(',  ')').Split(',');
            KeyValueList = KeyValueList.Select(x => GetRightHandSide(x)).ToArray();
        }
    }

    public class ConditionMaker
    {
        public string FieldName;
        public string Operator;
        public MyObject Object;
        public string Condition;
        public string Value;

        public Dictionary<string, IComparable> Types = new Dictionary<string, IComparable>()
        {
            { "ID", UInt64.MaxValue },
            { "Name", string.Empty },
            { "Description", string.Empty },
            { "Origin", string.Empty },
            { "Target", string.Empty },
            { "TakeOff", string.Empty },
            { "WorldPosition", (float.MinValue, float.MinValue) },
            { "AMSL", float.MinValue },
            { "ISO", string.Empty },
            { "Serial", string.Empty },
            { "Model", string.Empty },
            { "FirstClassSize", UInt64.MaxValue },
            { "BusinessClassSize", UInt64.MaxValue },
            { "EconomyClassSize", UInt64.MaxValue },
            { "MaxLoad", string.Empty },
            { "Weight", float.MinValue },
            { "Code", string.Empty },
            { "Phone", string.Empty },
            { "Email", string.Empty },
            { "Miles", UInt64.MaxValue },
            { "Age", UInt64.MaxValue },
            { "Practise", UInt64.MaxValue },
            { "Role", string.Empty },
            { "Class", string.Empty }
        };
        public Dictionary<string, Func<IComparable, IComparable, bool>> Lambdas = new Dictionary<string, Func<IComparable, IComparable, bool>>()
        {
            { "<", (x,y) =>  x.CompareTo(y) < 0},
            { "<=", (x,y) => x.CompareTo(y) <= 0 },
            { "==", (x,y) => x.CompareTo(y) == 0},
            { ">=", (x,y) => x.CompareTo(y) >= 0},
            { ">", (x,y) => x.CompareTo(y) > 0},
            { "!=", (x,y) => x.CompareTo(y) != 0}
        };
        public ConditionMaker(string condition, MyObject obj)
        {
            Condition = condition;
            string[] tmp = Condition.Split(' ');
            
            if (tmp[0].Contains("worldposition.", StringComparison.CurrentCultureIgnoreCase))
                FieldName = tmp[0][..14];
            else if (tmp[0].Contains("plane.", StringComparison.CurrentCultureIgnoreCase))
                FieldName = tmp[0][..5];
            else
                FieldName = tmp[0];
            
            Operator = tmp[1];
            Value = tmp[2];
            Object = obj;
        }
        public bool CheckPredicate()
        {
            // do poprawy
            bool res1, res2 = false;
            IComparable check, result3;
                

            
            res1 = UInt64.TryParse(Value, out var result);
            float result2 = 0;
            if (!res1)
                res2 = float.TryParse([2], out result2);
            

            Types.TryGetValue(FieldName, out var field);
            Lambdas.TryGetValue(Operator, out var lambda);
            Object.CreateFieldStrings();

            string val = Object.FieldStrings[FieldName.ToLower()];

            if (res1)
            {
                check = Convert.ToUInt64(val);

                return lambda.Invoke(check, result);
            }
            else if (res2)
            {
                check = Convert.ToSingle(val);
                return lambda.Invoke(check, result2);
            }
            else if (!Value.Contains(':'))
            {
                check = val;
                result3 = Value;
            }
            else
            {
                check = DateTime.ParseExact(val, "HH:mm", CultureInfo.InvariantCulture);
                result3 = DateTime.ParseExact(Value, "HH:mm", CultureInfo.InvariantCulture);
            }
            return lambda.Invoke(check, result3);
        }
        
    }
    
}
