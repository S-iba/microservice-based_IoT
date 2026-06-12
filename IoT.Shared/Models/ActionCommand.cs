using System;
using System.Collections.Generic;
using System.Text;

namespace IoT.Shared.Models
{
    public class ActionCommand
    {
        public int Id { get; set; } // Your preferred int ID for efficiency
        public string ActionType { get; set; } // e.g., "FLASH_LED"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
