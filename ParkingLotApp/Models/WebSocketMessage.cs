using System;
using System.Text.Json.Serialization;

namespace ParkingLotApp.Models
{
    public class WebSocketMessage
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public object? Data { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        public WebSocketMessage()
        {
            Timestamp = DateTime.Now;
        }

        public WebSocketMessage(string type, string message, object? data = null)
        {
            Type = type;
            Data = data;
            Timestamp = DateTime.Now;
        }
    }
} 