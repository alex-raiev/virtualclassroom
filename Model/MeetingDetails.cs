using System;
using Newtonsoft.Json;

namespace VirtualClassroom.NET.Model
{
    public class MeetingDetails
    {
        public int SessionId { get; set; }
        [JsonProperty("id")]
        public ulong MeetingId { get; set; }
        [JsonProperty("password")]
        public string PassCode { get; set; }
        public string LoginName { get; set; }
        public string Timezone { get; set; }
        public DateTime StartTime { get; set; }
        [JsonProperty("join_url")]
        public string JoinUrl { get; set; }
        [JsonProperty("start_url")]
        public string StartUrl { get; set; }
    }
}
