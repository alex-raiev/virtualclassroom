using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
        private string scheduleFor = Properties.Settings.Default.BR;

        private DataService _dataService;

        public MeetingService()
        {
            _dataService = new DataService();

        }

        public async Task<MeetingDetails> CreateMeeting(ClassSession session)
        {
            var data = new
            {
                start_time = session.FromTime,
                type = 1,
                schedule_for = scheduleFor,
                duration = (session.ToTime - session.FromTime).TotalMinutes,
                timezone = session.Timezone,
                topic = session.FacultyName,
                agenda= session.CourseSection
            };

            var json = JsonConvert.SerializeObject(data);

            var res = await ZoomApiRequest("/users/me/meetings", json);

            var meeting = JsonConvert.DeserializeObject<MeetingDetails>(res);

            if (meeting != null)
            {
                meeting.SessionId = session.Id;
            }

            return meeting;
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
                client.DefaultRequestHeaders
                    .Add("VC-AUTH", dataServerAuthKey);

                var data = new
                {
                    class_session_id = session.Id,
                    from_time = $"{session.FromTime:yyyy-MM-ddThh:mm}",
                    to_time = $"{session.ToTime:yyyy-MM-ddThh:mm}",
                    timezone = session.Timezone,
                    faculty_name = session.FacultyName,
                    faculty_email = session.FacultyEmail,
                    meeting_id = meeting.MeetingId,
                    join_url = meeting.JoinUrl,
                    start_url = meeting.StartUrl,
                    recording = session.Recording,
                    class_mode = session.ClassMode,
                    course_code = session.CourseCode,
                    course_term = session.CourseTerm,
                    course_section = session.CourseSection,
                    venue = session.Venue
                };

                var json = JsonConvert.SerializeObject(data);
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                HttpResponseMessage response = await client.PostAsync(dataServerUrl + "/zoom/register-class-session", content);

                if (!response.IsSuccessStatusCode)
                {
                    _dataService.AddLog("Error when sending meeting details: " + response.ReasonPhrase, LogType.Error);
                }
            }
        }

        private async Task<string> ZoomApiRequest(string query, string body)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(basicZoomApiUrl);
                
                client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders
                    .Add("Authorization", "Bearer " + zoomApiKey);

                var content = new StringContent(body, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(basicZoomApiUrl + query, content);
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
