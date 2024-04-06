using System;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using System.Collections.Generic;

namespace pryLogger.src.Logger.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    public sealed class LogParamIgnoreAttribute : Attribute
    {
        public LogParamIgnoreAttribute() : base() { }
    }

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    public sealed class LogParamAttribute : Attribute
    {
        public string Name { get; set; }

        public LogParamAttribute(string name) : base() => this.Name = name;
    }
}
