
using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using VirtualClassroom.NET.Model;

namespace VirtualClassroom.NET.Services
{
    public class DataService
    {
        private string dbFileName = "vcr.data";
        private string sessions = "sessions";
        private string meetings = "meetings";
        private string logs = "log";
        
        public ClassSession Get(int id)
        {
            using (var db = new LiteDatabase(dbFileName))
            {
                var data = db.GetCollection<ClassSession>(sessions);

                return data.Query().Where(i => i.Id == id).SingleOrDefault();
            }
        }

        public ClassSession GetUpcomingSession()
        {
            using (var db = new LiteDatabase(dbFileName))
            {
                var data = db.GetCollection<ClassSession>(sessions);

                // TODO: uncomment when data-server will be able provide normal schedule
                //return data.Query()
                //    .Where(i => (DateTime.Now - i.FromTime).TotalMinutes < 10 )
                //    .SingleOrDefault();

                return data.Query().FirstOrDefault();
            }
        }

        public void AddSession(ClassSession item)
        {
            using (var db = new LiteDatabase(dbFileName))
            {
                var collection = db.GetCollection<ClassSession>(sessions);
                collection.Insert(item);
            }
        }

        public void UpdateSession(ClassSession item)
        {
            using (var db = new LiteDatabase(dbFileName))
            {
                var collection = db.GetCollection<ClassSession>(sessions);
                collection.Update(item);
            }
        }

        public void AddMeeting(MeetingDetails item)
        {
            using (var db = new LiteDatabase(dbFileName))
            {
                var collection = db.GetCollection<MeetingDetails>(meetings);
                collection.Update(item);
            }
        }
        public MeetingDetails GetMeeting(int sessionId)
        {
            using (var db = new LiteDatabase(dbFileName))
            {
                var data = db.GetCollection<MeetingDetails>(meetings);

                return data.Query().Where(i => i.SessionId == sessionId).SingleOrDefault();
            }
        }

        public List<string> GetLog()
        {
            using (var db = new LiteDatabase(dbFileName))
            {
                var log = db.GetCollection<LogItem>(logs)
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
            using (var db = new LiteDatabase(dbFileName))
            {
                var log = db.GetCollection<LogItem>(logs);
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
