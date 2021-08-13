using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VirtualClassroom.NET.Model;

namespace VirtualClassroom.NET.Services
{
    public class MeetingService
    {
        private readonly string basicZoomUrl = "";
        public async Task<MeetingDetails> CreateMeeting(string name, DateTime startTime, int duration, string timezone)
        {
            using (var client = new HttpClient())
            {
                var data = new
                {
                    start_time = startTime,
                    duration,
                    password = "00000000",
                    timezone
                    //TODO: add settings object: recording, video, audio
                };
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json);

                var result = await client.PostAsync(basicZoomUrl + "/users/me/meetings", content);

                // TODO: process result of Meeting Creation request

                return new MeetingDetails
                {
                    MeetingId = 5670052168,
                    LoginName = "Temp Login Name",
                    PassCode = "00000000"
                };
            }
        }
    }
}
