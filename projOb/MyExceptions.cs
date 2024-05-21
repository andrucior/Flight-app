using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projOb
{
    public class InvalidNumberOfArgsException: Exception
    {
        public InvalidNumberOfArgsException() : base() { }
        public InvalidNumberOfArgsException(string message) : base(message) { }
        public InvalidNumberOfArgsException(string message, Exception exception): base(message, exception) { }
    }
    public class AirportException: Exception
    {
        public AirportException() : base() { }
        public AirportException(string message) : base(message) { }
        public AirportException(string message, Exception exception): base(message, exception) { }
    }
    public class UsageException : Exception
    {
        public UsageException() : base() { }
        public UsageException(string message) : base(message) 
        { 
            switch(message)
            {
                case "update":
                    Console.WriteLine($"USAGE: update {{object_class}} set {{key_value_list}} [where conditions]");
                    Console.WriteLine("{} - obligatory, [] - optional, case insensitive");
                    break;
                case "display":
                    Console.WriteLine("USAGE: display {object_fields or *} from {object_class} [where conditions]");
                    Console.WriteLine("{} - obligatory, [] - optional, case insensitive");
                    break;
                case "delete":
                    Console.WriteLine("USAGE: delete {object_class} [where conditions]");
                    Console.WriteLine("{} - obligatory, [] - optional, case insensitive");
                    break;
                case "add":
                    Console.WriteLine("USAGE: add {object_class} new {key_value_list}");
                    Console.WriteLine("{} - obligatory, [] - optional, case insensitive");
                    break;
            }
        }
        public UsageException(string message, Exception exception) : base(message, exception) { }
    }
}
