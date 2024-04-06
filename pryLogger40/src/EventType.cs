using System;

namespace pryLogger.src
{
    /// <summary>
    /// Enumeration representing possible event types.
    /// </summary>
    public enum EventType
    {
        /// <summary>
        /// Event related to the database.
        /// </summary>
        Db,

        /// <summary>
        /// Event related to log recording.
        /// </summary>
        Log,

        /// <summary>
        /// Event related to REST requests.
        /// </summary>
        Rest,

        /// <summary>
        /// No specific event type.
        /// </summary>
        None,

        /// <summary>
        /// All event types.
        /// </summary>
        All,
    }
}
