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
using static SkiaSharp.HarfBuzz.SKShaper;

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
            return splitted[1];
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
        public List<string> OR_AND;
        public UpdateParser(string line) : base(line) 
        {
            OR_AND = new List<string>();
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
    public class DeleteParser: CommandParser
    {
        public string ClassName { get; private set; }
        public string[]? Conditions { get; private set; }
        public List<string> OR_AND;
        public DeleteParser(string line) : base(line) 
        {
            OR_AND = new List<string>();
            ClassName = Command[1];
            if (Command.Length > 3) 
            {
                string tmp = string.Join(' ', Command[3..]);
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
    public class AddParser: CommandParser
    {
        public string? ClassName { get; private set; }
        public string[]? KeyValueList { get; private set; }
        public string[]? Fields { get; private set; }
        public AddParser(string line) : base(line) 
        { 
            ClassName = Command[1];
            string tmp = string.Join(' ', Command[3..]);
            KeyValueList = tmp.TrimEnd('(', ')').Split(',');
            Fields = KeyValueList.Select(x => GetLeftHandSide(x)).ToArray();
            KeyValueList = KeyValueList.Select(x => GetRightHandSide(x)).ToArray();
        }
    }
    public class ConditionMaker
    {
        public string FieldName;
        public string Operator;
        public MyObject Object;
        public Plane? Plane;
        public string Condition;
        public string Value;
        public bool Flight;

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
                FieldName = tmp[0][15..];
            else if (tmp[0].Contains("plane.", StringComparison.CurrentCultureIgnoreCase))
            {
                FieldName = tmp[0][6..];
                Flight = true;
            }
            else
                FieldName = tmp[0];
            
            Operator = tmp[1];
            Value = tmp[2];
            Object = (Flight? obj.Plane : obj);
        }
        public bool CheckPredicate()
        {
            bool intg = false;
            IComparable check, result;

            if (Value.Contains('.'))
                result = float.Parse(Value);
            else if (Value.Contains(':'))
                result = DateTime.ParseExact(Value, "HH:mm", CultureInfo.InvariantCulture);
            else
            {
                try
                {
                    result = UInt64.Parse(Value);
                    intg = true;
                }
                catch(FormatException)
                {
                    result = Value;
                }
            }

            Lambdas.TryGetValue(Operator, out var lambda);
            string val = Object.FieldStrings[FieldName.ToLower()];

            if (Value.Contains('.'))
                check = Convert.ToSingle(val);
            else if (Value.Contains(':'))
                check = DateTime.ParseExact(val, "HH:mm", CultureInfo.InvariantCulture);
            else if (!intg)
                check = val;
            else
                check = Convert.ToUInt64(val);
            
            return lambda.Invoke(check, result);
        }
        
    }
    
}
