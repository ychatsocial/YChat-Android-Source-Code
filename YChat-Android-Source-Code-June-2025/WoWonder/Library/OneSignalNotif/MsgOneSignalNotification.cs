﻿using System;
using System.Linq;
using Android.App;
using Android.Content;
using Com.Onesignal;
using Com.Onesignal.Debug;
using Com.Onesignal.InAppMessages;
using Com.Onesignal.Notifications;
using Com.Onesignal.User.Subscriptions;
using Java.Util.Functions;
using Newtonsoft.Json;
using WoWonder.Activities.Chat.MsgTabbes;
using WoWonder.Helpers.MediaPlayerController;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.OneSignalNotif.Models;
using WoWonder.SQLite;
using WoWonderClient;
using Object = Java.Lang.Object;

namespace WoWonder.Library.OneSignalNotif
{
    public class MsgOneSignalNotification : Object, IConsumer, INotificationLifecycleListener, INotificationClickListener, IPushSubscriptionObserver, IInAppMessageLifecycleListener
    {
        //Force your app to Register Notification directly without loading it from server (For Best Result)

        private OsObject.OsNotificationObject DataNotification;

        private static volatile MsgOneSignalNotification InstanceRenamed;
        public static MsgOneSignalNotification Instance
        {
            get
            {
                MsgOneSignalNotification localInstance = InstanceRenamed;
                if (localInstance == null)
                {
                    lock (typeof(MsgOneSignalNotification))
                    {
                        localInstance = InstanceRenamed;
                        if (localInstance == null)
                        {
                            InstanceRenamed = localInstance = new MsgOneSignalNotification();
                        }
                    }
                }
                return localInstance;

            }
        }

        public void RegisterNotificationDevice(Context context)
        {
            try
            {
                if (AppSettings.ShowNotification && AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.RestApi)
                {
                    if (!string.IsNullOrEmpty(AppSettings.MsgOneSignalAppId) || !string.IsNullOrWhiteSpace(AppSettings.MsgOneSignalAppId))
                    {
                        //The following options are available with increasingly more information:
                        //NONE, FATAL, ERROR, WARN, INFO, DEBUG, VERBOSE
                        OneSignal.Debug.LogLevel = LogLevel.Verbose;
                        OneSignal.Debug.AlertLevel = LogLevel.None;

                        // OneSignal Initialization  
                        OneSignal.InitWithContext(context, AppSettings.MsgOneSignalAppId);

                        // OneSignal Methods
                        OneSignal.Notifications.RequestPermission(true, Continue.With(this));
                        OneSignal.Notifications.AddForegroundLifecycleListener(this);
                        OneSignal.Notifications.AddClickListener(this);
                        //OneSignal.Notifications.AddPermissionObserver(this);

                        OneSignal.InAppMessages.AddLifecycleListener(this);

                        OneSignal.Login(UserDetails.UserId);

                        OneSignal.ConsentRequired = true;
                        OneSignal.ConsentGiven = true;

                        OneSignal.InAppMessages.Paused = true;
                        OneSignal.Location.Shared = true;

                        OneSignal.User.PushSubscription.AddObserver(this);
                        IdsAvailable();
                    }
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public void UnRegisterNotificationDevice()
        {
            try
            {
                OneSignal.Notifications.ClearAllNotifications();

                OneSignal.Notifications.RemoveForegroundLifecycleListener(this);
                OneSignal.Notifications.RemoveClickListener(this);

                OneSignal.User.PushSubscription.RemoveObserver(this);

                OneSignal.Logout();

                //AppSettings.ShowNotification = false;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public void IdsAvailable()
        {
            try
            {
                var device = OneSignal.User.PushSubscription;

                if (device != null)
                {
                    //string email = device.EmailAddress;
                    //string emailId = device.EmailUserId;
                    string pushToken = device.Token;
                    string userId = device.Id;

                    //bool enabled = device.AreNotificationsEnabled();
                    bool subscribed = device.OptedIn;
                    //bool subscribedToOneSignal = device.IsEmailSubscribed;

                    if (subscribed && !string.IsNullOrEmpty(userId))
                        UserDetails.DeviceId = userId;
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        /// <summary>
        /// NotificationWillShowInForeground
        /// Adds a listener to run before whenever a notification lifecycle event occurs.
        /// </summary>
        /// <param name="result"></param>
        public void OnWillDisplay(INotificationWillDisplayEvent result)
        {
            try
            {
                var notification = result;

                string title = notification.Notification.Title;
                string message = notification.Notification.Body;
                var additionalData = notification.Notification.AdditionalData?.ToString();
                DataNotification = JsonConvert.DeserializeObject<OsObject.OsNotificationObject>(additionalData);

                string chatType = "", idChat = "";

                if (!string.IsNullOrEmpty(idChat))
                {
                    if (ListUtils.MuteList.Count == 0)
                    {
                        var sqLiteDatabase = new SqLiteDatabase();
                        ListUtils.MuteList = sqLiteDatabase.Get_MuteList();
                    }

                    var check = ListUtils.MuteList.FirstOrDefault(a => a.ChatId == idChat && a.ChatType == chatType);
                    if (check != null)
                    {
                        OneSignal.Notifications.RemoveNotification(result.Notification.AndroidNotificationId);
                    }
                }

                if (message.Contains("call") || message.Contains("calling") || title.Contains("call") || title.Contains("calling"))
                {
                    OneSignal.Notifications.RemoveNotification(result.Notification.AndroidNotificationId);

                    //LastChatFragment.ApiRun = false;
                    //if (Methods.CheckConnectivity())
                    //    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => AppUpdaterHelper.LoadChatAsync(true) });
                }

            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        /// <summary>
        /// NotificationOpened
        /// Adds a listener that will run whenever a notification is clicked on by the user.
        /// </summary>
        /// <param name="result"></param>
        public void OnClick(INotificationClickEvent result)
        {
            try
            {
                var notification = result;

                string title = notification.Notification.Title;
                string message = notification.Notification.Body;
                var additionalData = notification.Notification.AdditionalData?.ToString();
                DataNotification = JsonConvert.DeserializeObject<OsObject.OsNotificationObject>(additionalData);

                EventClickNotification();
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public void EventClickNotification()
        {
            try
            {
                Constant.IsOpenNotify = true;

                var type = DataNotification.Type;
                if (!string.IsNullOrEmpty(DataNotification.ChatType))
                {
                    type = DataNotification.ChatType;
                }
                else if (!string.IsNullOrEmpty(DataNotification.CallType))
                {
                    type = DataNotification.CallType;
                }

                Intent intent = new Intent(Application.Context, typeof(ChatTabbedMainActivity));
                intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                intent.AddFlags(ActivityFlags.SingleTop);
                intent.SetAction(Intent.ActionView);
                intent.PutExtra("userId", DataNotification.UserId);
                intent.PutExtra("PostId", DataNotification.PostId);
                intent.PutExtra("PageId", DataNotification.PageId);
                intent.PutExtra("GroupId", DataNotification.GroupId);
                intent.PutExtra("EventId", DataNotification.EventId);
                intent.PutExtra("type", type);
                intent.PutExtra("Notifier", "Chat");
                Application.Context.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        /// <summary>
        /// respond to permission state change
        /// </summary>
        /// <param name="state"></param>
        public void OnPushSubscriptionChange(PushSubscriptionChangedState state)
        {
            try
            {
                //wael check  
                if (state.Current.OptedIn && !string.IsNullOrEmpty(state.Current.Token))
                    UserDetails.DeviceId = state.Current.Token;

                IdsAvailable();
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public void OnDidDismiss(IInAppMessageDidDismissEvent e)
        {

        }

        public void OnDidDisplay(IInAppMessageDidDisplayEvent e)
        {

        }

        public void OnWillDismiss(IInAppMessageWillDismissEvent e)
        {

        }

        public void OnWillDisplay(IInAppMessageWillDisplayEvent e)
        {

        }

        public void Accept(Object t)
        {

        }
    }
}