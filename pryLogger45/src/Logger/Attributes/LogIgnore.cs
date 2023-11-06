using System;

namespace pryLogger.src.Log.Attributes
{
    /// <summary>
    /// An attribute that indicates a parameter should be ignored when logging.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class LogIgnore : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogIgnore"/> class.
        /// </summary>
        public LogIgnore() : base() { }
    }
}
