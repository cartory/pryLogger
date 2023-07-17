using System;

namespace pryLogger.src.Attributes 
{ 
    [AttributeUsage(AttributeTargets.Parameter)]
    public class LogRename : Attribute
    {
        public string Name { get; set; }

        private LogRename() : base() { }
        public LogRename(string name) : base() => Name = name;
    }
}
