using System;
using System.Collections.Generic;
using System.Data;
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
    }
    public class DisplayParser: CommandParser
    {
        public string[]? ObjectFields { get; private set; }
        public string[]? Conditions { get; private set; }
        public string ClassName {  get; private set; }
        public DisplayParser(string line) : base(line)
        {
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
            
            if (Command.Length > 4)
            {
                string tmp = String.Join(" ", Command[5..]);
                Conditions = Regex.Split(tmp, @"\s+and\s+|\s+or\s+", RegexOptions.IgnoreCase);
                Conditions = Conditions.Select(x => x.Trim()).ToArray();
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
            string tmp = Convert.ToString(Command[3..]);
            int index = tmp.IndexOf("where", StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                KeyValueList = tmp.Substring(0, index).TrimEnd('(', ')').Split(',');
            }
            else
            {
                KeyValueList = tmp.TrimEnd('(', ')').Split(',');
            }

            if (Command.Length > 4)
            {
                tmp = Convert.ToString(Command[5..]);
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
                string tmp = Convert.ToString(Command[3..]);
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
            ClassName = Command[1];
            string tmp = Convert.ToString(Command[3..]);
            KeyValueList = tmp.TrimEnd('(',  ')').Split(',');
            KeyValueList = KeyValueList.Select(x => GetRightHandSide(x)).ToArray();
        }
        static private string GetRightHandSide(string input)
        {
            string[] splitted = input.Split('=');
            return splitted[1];
        }
    }

    public class ConditionMaker
    {
        public string? FieldName;
        public string? Operator;
        public IComparable? Value;
        public MyObject? Object;
        public string? Condition;
        public ParameterExpression Parameter;
        MemberExpression Property;
        ConstantExpression Constant;
        BinaryExpression Comparison;
        LambdaExpression Predicate;

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
        public ConditionMaker(string condition)
        {
            Condition = condition; 
        }
        public bool CheckPredicate(Airport airport)
        {
            string[] tmp = Condition.Split(' ');
            FieldName = tmp[0];
            Operator = tmp[1];
            Object = airport;
            Types.TryGetValue(FieldName, out var field);
            Value = (IComparable?)tmp[2];
            MemberExpression property = Expression.Property(Expression.Constant(airport), FieldName);
            return false;
        }
        public ConditionMaker(string condition, MyObject obj)
        {
            string[] tmp = condition.Split(' ');
            FieldName = tmp[0];
            Operator = tmp[1];
            Object = obj;
            if (FieldName.StartsWith("Plane."))
                FieldName = FieldName[6..];
            
            Types.TryGetValue(FieldName, out IComparable? field);
            Value = (IComparable?)Convert.ChangeType(tmp[2], field.GetType());

            Parameter = Expression.Parameter(field.GetType(), FieldName);
            Property = Expression.Property(Expression.Constant(obj), FieldName);
            Constant = Expression.Constant(Value, field.GetType());
            

            switch(Operator) 
            {
                case "<":
                    Comparison = Expression.LessThan(Property, Constant); 
                    break;
                case "<=":
                    Comparison = Expression.LessThanOrEqual(Property, Constant);
                    break;
                case ">":
                    Comparison = Expression.GreaterThan(Property, Constant);
                    break;
                case ">=":
                    Comparison = Expression.GreaterThanOrEqual(Property, Constant); 
                    break;
                case "==":
                    Comparison = Expression.Equal(Property, Constant);
                    break;
                case "!=":
                    Comparison = Expression.NotEqual(Property, Constant);   
                    break;
                default:
                    throw new Exception("invalid operator");
            }

             Predicate = Expression.Lambda(Comparison, Parameter);
        }
        
        public bool CheckPredicate()
        {
            var kurwa =  (Func<MyObject, bool>)Predicate.Compile();
            return kurwa(Object);
        }
     
    }
    
}
