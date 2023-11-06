using System;

namespace pryLogger.src.Log.Attributes
{
    /// <summary>
    /// An attribute that allows renaming a parameter when logging.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class LogRename : Attribute
    {
        /// <summary>
        /// Gets or sets the new name for the parameter when it is logged.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogRename"/> class with the specified name.
        /// </summary>
        /// <param name="name">The new name for the parameter when logged.</param>
        public LogRename(string name) : base()
        {
            this.Name = name;
        }
    }
}
