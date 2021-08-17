using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using VirtualClassroom.NET.Model;
using VirtualClassroom.NET.Services;

using ZOOM_SDK_DOTNET_WRAP;

namespace VirtualClassroom.NET
{
    public partial class MainForm : Form
    {
        private readonly DataService _dataService;
        private readonly MeetingService _meetingService;

        private ClassSessionList _classSessionList;
     
        private string appKey = Properties.Settings.Default.AppKey;
        private string appSecret = Properties.Settings.Default.AppSecret;

        private bool _isOnMeetingNow;
        private MeetingDetails _currentMeeting;

        public MainForm()
        {
            _dataService = new DataService();
            _meetingService = new MeetingService();

            InitializeComponent();

            InitZoomSDK();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pollingTimer.Enabled = true;
        }

        private void InitZoomSDK()
        {
            CZoomSDKeDotNetWrap.Instance.GetAuthServiceWrap().Add_CB_onAuthenticationReturn(onAuthenticationReturn);
            CZoomSDKeDotNetWrap.Instance.GetAuthServiceWrap().Add_CB_onLoginRet(onLoginRet);
            CZoomSDKeDotNetWrap.Instance.GetAuthServiceWrap().Add_CB_onLogout(onLogout);

            var param = new AuthParam
            {
                appKey = appKey,
                appSecret = appSecret
            };

            var err = CZoomSDKeDotNetWrap.Instance.GetAuthServiceWrap().SDKAuth(param);

            if (SDKError.SDKERR_SUCCESS != err)
                MessageBox.Show($"Auth SDK failed. {err}", "VirtualClassroom", MessageBoxButtons.OK);

            RegisterCallBack();
        }

        private void JoinMeeting(ulong meetingId, string passw, string userName)
        {
            var m2eetingParam = new StartParam
            {
                userType = SDKUserType.SDK_UT_WITHOUT_LOGIN
            };

            var withoutLoginParam = new JoinParam4WithoutLogin
            {
                userName = userName,
                meetingNumber = meetingId,
                psw = passw,
                isAudioOff = false,
                isVideoOff = false,
                isDirectShareDesktop = false
            };

            var meetingParam = new JoinParam
            {
                userType = SDKUserType.SDK_UT_WITHOUT_LOGIN,
                withoutloginJoin = withoutLoginParam
            };

            var err = CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().Join(meetingParam);

            if (SDKError.SDKERR_SUCCESS == err)
            {
                _isOnMeetingNow = true;
            }
        }

        private void LeaveMeeting()
        {
            if (_isOnMeetingNow)
            {
                var err = CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().Leave(LeaveMeetingCmd.LEAVE_MEETING);
                if (SDKError.SDKERR_SUCCESS != err)
                {
                    AddToLog($"Error when ending meeting. {err}", LogType.Error);
                }
            }
        }

        private void btnManualStart_Click(object sender, EventArgs e)
        {
            JoinMeeting(5670052168, "00000000", "Test manual meeting join");   
        }

        public void onAuthenticationReturn(AuthResult ret)
        {
            if (AuthResult.AUTHRET_SUCCESS == ret)
            {
                //start_meeting_wnd.Show();
            }
        }

        public void onLoginRet(LOGINSTATUS ret, IAccountInfo pAccountInfo)
        {
            //todo
        }

        public void onLogout()
        {
            //todo
        }


        public void onMeetingStatusChanged(MeetingStatus status, int iResult)
        {
            switch (status)
            {
                case MeetingStatus.MEETING_STATUS_CONNECTING:
                    AddToLog("Meeting is started", LogType.Info);
                    _isOnMeetingNow = true;
                    Hide();
                    break;
                case MeetingStatus.MEETING_STATUS_ENDED:
                case MeetingStatus.MEETING_STATUS_FAILED:
                    AddToLog("Meeting is finished.", LogType.Info);
                    _isOnMeetingNow = false;
                    Show();
                    break;
            }
        }

        public void onUserJoin(Array lstUserID)
        {
            if (null == lstUserID)
                return;

            for (var i = lstUserID.GetLowerBound(0); i <= lstUserID.GetUpperBound(0); i++)
            {
                var userid = (uint) lstUserID.GetValue(i);
                var user = CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingParticipantsController()
                    .GetUserByUserID(userid);
                if (null != user)
                {
                    var name = user.GetUserNameW();
                    Console.Write(name);
                }
            }
        }

        public void onUserLeft(Array lstUserID)
        {
            //todo
        }

        public void onHostChangeNotification(uint userId)
        {
            //todo
        }

        public void onLowOrRaiseHandStatusChanged(bool bLow, uint userid)
        {
            //todo
        }

        public void onUserNameChanged(uint userId, string userName)
        {
            //todo
        }

        private void RegisterCallBack()
        {
            CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().Add_CB_onMeetingStatusChanged(onMeetingStatusChanged);
            CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingParticipantsController()
                .Add_CB_onHostChangeNotification(onHostChangeNotification);
            CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingParticipantsController()
                .Add_CB_onLowOrRaiseHandStatusChanged(onLowOrRaiseHandStatusChanged);
            CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingParticipantsController()
                .Add_CB_onUserJoin(onUserJoin);
            CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingParticipantsController()
                .Add_CB_onUserLeft(onUserLeft);
            CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingParticipantsController()
                .Add_CB_onUserNameChanged(onUserNameChanged);
        }

        private async void pollingTimer_Tick(object sender, EventArgs e)
        {
            if (_isOnMeetingNow)
            {
                StopMeeting();

                return;
            }

            _classSessionList = await _meetingService.GetCurrentSessions();

            if (_classSessionList.ClassSessions.Any())
            {
                AddToLog("Polling class session info...", LogType.Info);

                UpdateClassSessions(_classSessionList.ClassSessions);
            }
            else
            {
                AddToLog("No schedule at the moment.", LogType.Info);
            }

            StartMeeting();
        }

        //bool IsComing(DateTime value1, DateTime value2)
        //{
        //    var timeSpan = (value1 - value2).TotalMinutes;

        //    return timeSpan < 10;
        //}

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void UpdateClassSessions(List<ClassSession> newItems)
        {
            foreach (var item in newItems)
            {
                var dbItem = _dataService.Get(item.Id);
                if (dbItem == null)
                {
                    _dataService.AddSession(item);

                    AddToLog($"Added session with id: {item.Id}.", LogType.Info);

                    try
                    {
                        var meetingDetails = _meetingService.CreateMeeting(item.CourseSection, item.FromTime, 60, item.Timezone).Result;

                        // Link sessions to meetings
                        meetingDetails.SessionId = item.Id;

                        _dataService.AddMeeting(meetingDetails);

                        AddToLog($"Meeting created and saved: {meetingDetails.MeetingId}.", LogType.Info);
                    }
                    catch (Exception e)
                    {
                        AddToLog(e.Message, LogType.Exception);
                    }
                }
                else
                {
                    _dataService.UpdateSession(item);

                    var meetingDb = _dataService.GetMeeting(item.Id);
                    if (meetingDb == null)
                    {

                        var meetingDetails = _meetingService.CreateMeeting(item.CourseSection, item.FromTime, 60, item.Timezone).Result;

                        // Link sessions to meetings
                        meetingDetails.SessionId = item.Id;

                        _dataService.AddMeeting(meetingDetails);

                        AddToLog($"Meeting created and saved: {meetingDetails.MeetingId}.", LogType.Info);
                    }

                    AddToLog($"Updated session with id: {item.Id}", LogType.Info);
                }
            }
        }

        private void StartMeeting()
        {
            var upcomingSession = _dataService.GetUpcomingSession();
            if (upcomingSession != null)
            {
                var meetingDetails = _dataService.GetMeeting(upcomingSession.Id);
                if (meetingDetails != null)
                {
                    AddToLog("Found upcoming meeting. Joining...", LogType.Info);

                    JoinMeeting(meetingDetails.MeetingId, meetingDetails.PassCode, meetingDetails.LoginName);
                }
            }
        }

        private void StopMeeting()
        {
            var endingSession = _dataService.GetEndingSession();
            if (endingSession != null)
            {
                AddToLog("Leaving meeting", LogType.Info);

                // TODO: for DEBUG purposes only
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(1000);
                }

                LeaveMeeting();
            }
        }

        private void AddToLog(string message, LogType type)
        {
            _dataService.AddLog(message, type);
        }

        private void logTimer_Tick(object sender, EventArgs e)
        {
            logBox.Lines = _dataService.GetLog().ToArray();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            pollingTimer.Enabled = !pollingTimer.Enabled;

            if (pollingTimer.Enabled)
            {
                btnPause.Text = "Pause";
            }
            else
            {
                btnPause.Text = "Resume";
            }
        }
    }
}