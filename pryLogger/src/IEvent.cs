using System;
using System.Text;
using System.Collections.Generic;

namespace pryLogger.src
{
    public interface IEvent
    {
        DateTimeOffset Start { get; set; }
        double ElapsedTime { get; set; }

        void Finish();
    }
}
