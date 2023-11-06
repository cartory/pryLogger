using System;
using System.Text;
using System.Collections.Generic;

namespace pryLogger.src
{
    public interface IEvent
    {
        DateTimeOffset Starts { get; set; }
        double ElapsedTime { get; set; }

        void Start();
        void Finish();
    }
}
