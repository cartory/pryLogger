using System;

namespace pryLogger.src.Attributes 
{ 
    [AttributeUsage(AttributeTargets.Parameter)]
    public class LogIgnore : Attribute
    {
        public LogIgnore() : base() { }
    }
}
