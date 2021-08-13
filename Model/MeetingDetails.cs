using System;

namespace VirtualClassroom.NET.Model
{
    public class MeetingDetails
    {
        public int SessionId { get; set; }
        public ulong MeetingId { get; set; }
        public string PassCode { get; set; }
        public string LoginName { get; set; }
        public DateTime StartTime { get; set; }
        public string Url { get; set; }
    }
}
