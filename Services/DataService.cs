
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

                //TODO: commented for DEBUG 
                //var time1 = DateTime.Now.AddMinutes(-5);
                //var time2 = DateTime.Now.AddMinutes(5);
                var time1 = new DateTime(2021, 8, 17, 9, 25, 0);
                var time2 = new DateTime(2021, 8, 17, 9, 35, 0);

                // TODO: uncomment when data-server will be able provide normal schedule
                var result = data.Query()
                    .Where(i => i.FromTime > time1 && i.FromTime < time2)
                    .SingleOrDefault();

                return result;
                //return data.Query().FirstOrDefault();
            }
        }

        public ClassSession GetEndingSession()
        {
            using (var db = new LiteDatabase(dbFileName))
            {
                var data = db.GetCollection<ClassSession>(sessions);

                //TODO: commented for DEBUG 
                //var time1 = DateTime.Now.AddMinutes(-5);
                //var time2 = DateTime.Now.AddMinutes(5);
                var time1 = new DateTime(2021, 8, 17, 9, 50, 0);
                var time2 = new DateTime(2021, 8, 17, 10, 5, 0);

                // TODO: uncomment when data-server will be able provide normal schedule
                var result = data.Query()
                    .Where(i => i.ToTime > time1 && i.ToTime < time2)
                    .SingleOrDefault();

                return result;
                //return data.Query().FirstOrDefault();
            }

        }

        public void AddSession(ClassSession item)
        {
            using (var db = new LiteDatabase(dbFileName))
            {

                var collection = db.GetCollection<ClassSession>(sessions);

                //var sessionDb = collection.Query().Where(i => i.Id == item.Id);
                //if(sessionDb!= null)
                //    return;
                
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
