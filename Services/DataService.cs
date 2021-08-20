
using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using VirtualClassroom.NET.Model;

namespace VirtualClassroom.NET.Services
{
    public class DataService
    {
        private int m_preStartThreshold = Properties.Settings.Default.PreStartThreshold;
        private int m_reconnectThreshold = Properties.Settings.Default.ReconnectThreshold;
        private string m_dbFileName = "vcr.data";
        private string m_sessions = "m_sessions";
        private string m_meetings = "m_meetings";
        private string m_logs = "log";
        
        public ClassSession Get(int id)
        {
            using (var db = new LiteDatabase(m_dbFileName))
            {
                var data = db.GetCollection<ClassSession>(m_sessions);

                return data.Query().Where(i => i.Id == id).SingleOrDefault();
            }
        }

        public ClassSession GetUpcomingSession()
        {
            using (var db = new LiteDatabase(m_dbFileName))
            {
                var data = db.GetCollection<ClassSession>(m_sessions);
                
                var time1 = DateTime.Now.AddMinutes(-m_preStartThreshold);
                var time2 = DateTime.Now.AddMinutes(m_reconnectThreshold);
                
                var result = data.Query()
                    .Where(i => i.FromTime > time1 && i.FromTime < time2)
                    .SingleOrDefault();

                return result;
            }
        }

        public ClassSession GetEndingSession()
        {
            using (var db = new LiteDatabase(m_dbFileName))
            {
                var data = db.GetCollection<ClassSession>(m_sessions);
                
                var time1 = DateTime.Now.AddMinutes(-m_preStartThreshold);
                var time2 = DateTime.Now.AddMinutes(m_reconnectThreshold);
                
                var result = data.Query()
                    .Where(i => i.ToTime > time1 && i.ToTime < time2)
                    .SingleOrDefault();

                return result;
            }

        }

        public void AddSession(ClassSession item)
        {
            using (var db = new LiteDatabase(m_dbFileName))
            {

                var collection = db.GetCollection<ClassSession>(m_sessions);

                collection.Insert(item);
            }
        }

        public void UpdateSession(ClassSession item)
        {
            using (var db = new LiteDatabase(m_dbFileName))
            {
                var collection = db.GetCollection<ClassSession>(m_sessions);
                collection.Update(item);
            }
        }

        public void AddMeeting(MeetingDetails item)
        {
            using (var db = new LiteDatabase(m_dbFileName))
            {
                var collection = db.GetCollection<MeetingDetails>(m_meetings);

                var meetingDb = collection.Query().Where(i => i.SessionId == item.SessionId).FirstOrDefault();

                if (meetingDb != null)
                {
                    collection.Update(item);
                }
                else
                {
                    collection.Insert(item);
                }
            }
        }
        public MeetingDetails GetMeeting(int sessionId)
        {
            using (var db = new LiteDatabase(m_dbFileName))
            {
                var data = db.GetCollection<MeetingDetails>(m_meetings);

                return data.Query().Where(i => i.SessionId == sessionId).SingleOrDefault();
            }
        }

        public List<string> GetLog()
        {
            using (var db = new LiteDatabase(m_dbFileName))
            {
                var log = db.GetCollection<LogItem>(m_logs)
                    .Query()
                    .OrderByDescending(i => i.Time)
                    .ToList();

                return log.Select(i => $"[{i.Time}] ({i.Type}) {i.Message}")
                    .Take(30)
                    .ToList();
            }
        }

        public void AddLog(string message, LogType type)
        {
            using (var db = new LiteDatabase(m_dbFileName))
            {
                var log = db.GetCollection<LogItem>(m_logs);
                log.Insert(new LogItem
                {
                    Message = message,
                    Time = DateTime.Now,
                    Type = type
                });
            }
        }
    }
}
