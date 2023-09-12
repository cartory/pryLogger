using System;

namespace pryLogger.src
{
    /// <summary>
    /// Represents an event with a start time and elapsed time.
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Gets or sets the start time of the event.
        /// </summary>
        DateTimeOffset Starts { get; set; }

        /// <summary>
        /// Gets or sets the elapsed time of the event.
        /// </summary>
        double ElapsedTime { get; set; }

        /// <summary>
        /// Starts the event, recording the start time.
        /// </summary>
        void Start();

        /// <summary>
        /// Finishes the event, calculating and setting the elapsed time.
        /// </summary>
        void Finish();
    }
}
