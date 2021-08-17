using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VirtualClassroom.NET.Model;

namespace VirtualClassroom.NET.Services
{
    public class MeetingService
    {
        private readonly string basicZoomApiUrl = Properties.Settings.Default.ZoomApiUrl;
        private readonly string zoomApiKey = Properties.Settings.Default.ZoomApiKey;
        private string dataServerUrl = Properties.Settings.Default.DataServerUrl;
        private string dataServerAuthKey = Properties.Settings.Default.VCAuth;

        private DataService _dataService;


        public MeetingService()
        {
            _dataService = new DataService();

        }

        public async Task<MeetingDetails> CreateMeeting(string name, DateTime startTime, int duration, string timezone)
        {
            var data = new
            {
                start_time = startTime,
                duration,
                password = "0000000",
                timezone
                //TODO: add settings object: recording, video, audio
            };
            var json = JsonConvert.SerializeObject(data);
            

            //var res = await ZoomApiRequest("/users/me/meetings", json);

            // TODO: process result of Meeting Creation request

            return new MeetingDetails
            {
                MeetingId = 5670052168,
                LoginName = "Temp Login Name",
                PassCode = "0000000"
            };
        }


        public async Task<ClassSessionList> GetCurrentSessions()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("VC-AUTH", dataServerAuthKey);

                var result = await client.GetAsync(dataServerUrl + "/test/class-sessions-today");
                if (result.StatusCode != HttpStatusCode.OK)
                {
                    _dataService.AddLog($"Error when request class sessions list: {result.StatusCode}", LogType.Error);

                    return null;
                }
                else
                {
                    var json = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ClassSessionList>(json);
                }
            }
        }

        public async Task SendMeetingToDataserver(MeetingDetails meeting, ClassSession session)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(dataServerUrl);

                var data = new
                {
                    from_time = session.FromTime,
                    to_time = session.ToTime,
                    timezone = session.Timezone,
                    //"faculty_name"
                    //"faculty_email"
                    //"meeting_id"
                    //"join_url"
                    //"start_url"
                    //"recording"
                    //"class_type"
                };

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json);

                HttpResponseMessage response = await client.PutAsync("/zoom/register-class-session", content);
                
                //if (response.IsSuccessStatusCode)
                //{
                //    return response.Content.ReadAsStringAsync().Result;
                //}
                //else
                //{
                //    return null;
                //}

            }
        }

        private async Task<string> ZoomApiRequest(string query, string body)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(basicZoomApiUrl);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + zoomApiKey);

                var content = new StringContent(body);

                HttpResponseMessage response = await client.PostAsync(query, content);
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    return null;
                }
            }

        }
    }
}
