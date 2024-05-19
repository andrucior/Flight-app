using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata;
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
            ClassName= Command[1];
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
        }
    }
    public class ConditionMaker
    {
        public string FieldName;
        public string Operator;
        public object Value;
        public MyObject Object;
        public Func<object, bool> Predicate;
        public Dictionary<string, object> Types = new Dictionary<string, object>()
        {
            { "ID", UInt64.MaxValue },
            { "Name", string.Empty },
            { "Description", string.Empty },
            { "Origin", string.Empty },
            { "Target", string.Empty },
            { "TakeOff", string.Empty },
            { "WorldPosition", (float.MinValue, float.MinValue) },
            { "AMSL", float.MinValue },
            { "Plane",  new Plane() },
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
            { "Role", string.Empty }
        };
        public ConditionMaker(string condition, MyObject obj)
        {
            string[] tmp = condition.Split(' ');
            FieldName = tmp[0];
            Operator = tmp[1];
            Object = obj;
            Types.TryGetValue(FieldName, out var field);
            Value = Convert.ChangeType(tmp[2], field.GetType());

            ParameterExpression parameter = Expression.Parameter(field.GetType(), FieldName);
            MemberExpression property = Expression.Property(Expression.Constant(obj), FieldName);
            ConstantExpression constant = Expression.Constant(Value, field.GetType());
            BinaryExpression comparison;

            switch(Operator) 
            {
                case "<":
                    comparison = Expression.LessThan(property, constant); 
                    break;
                case "<=":
                    comparison = Expression.LessThanOrEqual(property, constant);
                    break;
                case ">":
                    comparison = Expression.GreaterThan(property, constant);
                    break;
                case ">=":
                    comparison = Expression.GreaterThanOrEqual(property, constant); 
                    break;
                case "==":
                    comparison = Expression.Equal(property, constant);
                    break;
                default:
                    throw new Exception("invalid operator");
            }

            Predicate = Expression.Lambda<Func<object, bool>>(comparison, parameter).Compile();
        }
        public bool CheckPredicate()
        {
            return Predicate.Invoke(Value);
        }
    }
    
}
