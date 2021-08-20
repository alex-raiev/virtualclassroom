using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VirtualClassroom.NET.Model
{
    public class ClassSession
    {
        [JsonProperty("class_session_id")]
        public int Id { get; set; }
        [JsonProperty("course_code")]
        public string CourseCode { get; set; }
        [JsonProperty("course_term")]
        public string CourseTerm { get; set; }
        [JsonProperty("course_section")]
        public string CourseSection { get; set; }
        public string Venue { get; set; }
        public string Timezone { get; set; }
        [JsonProperty("faculty_name")]
        public string FacultyName { get; set; }
        [JsonProperty("faculty_email")]
        public string FacultyEmail { get; set; }
        [JsonProperty("from_time")]
        public DateTime FromTime { get; set; }
        [JsonProperty("to_time")]
        public DateTime ToTime { get; set; }
        public string Recording { get; set; }
        [JsonProperty("class_mode")]
        public string ClassMode { get; set; }
    }

    public class ClassSessionList
    {
        public List<ClassSession> ClassSessions { get; set; }
    }
}
