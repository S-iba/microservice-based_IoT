using System;
using System.Collections.Generic;
using System.Text;

namespace IoT.Shared.Events
{
    public class CommandCompletedEvent
    {
        public string ActionType { get; set; }
        public string Status { get; set; } // e.g., "Success" or "Failure"
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }
}
