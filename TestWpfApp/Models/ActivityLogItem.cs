using System;

namespace TestWpfApp.Models
{
    public class ActivityLogItem
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }

        public ActivityLogItem()
        {
            CreatedAt = DateTime.Now;
        }

        public ActivityLogItem(string type, string message)
            : this()
        {
            Type = type;
            Message = message;
        }
    }
}
