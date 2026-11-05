using System;

namespace Net48MvcSmoke.Models
{
    public class SmokeModel
    {
        public string Name { get; set; } = string.Empty;
        public bool ShowDetails { get; set; }
    }

    public class TrainingSession
    {
        public DateTimeOffset StartsAt { get; set; }
    }
}
