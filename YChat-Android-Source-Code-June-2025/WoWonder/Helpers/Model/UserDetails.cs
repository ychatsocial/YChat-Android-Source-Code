﻿using System;
using Newtonsoft.Json;
using WoWonder.Helpers.Utils;
using WoWonder.SocketSystem;
using WoWonderClient;
using WoWonderClient.Classes.Posts;

namespace WoWonder.Helpers.Model
{
    public static class UserDetails
    {
        public static string AccessToken = string.Empty;
        public static string UserId = string.Empty;
        public static string Username = string.Empty;
        public static string FullName = string.Empty;
        public static string Password = string.Empty;
        public static string Email = string.Empty;
        public static string Cookie = string.Empty;
        public static string Status = string.Empty;

        public static string Avatar { get; set; } = WoWonderTools.GetDefaultAvatar();

        public static string Cover = string.Empty;
        public static string DeviceId = string.Empty;
        public static string DeviceMsgId = string.Empty;
        public static string LangName = string.Empty;
        public static string IsPro = string.Empty;
        public static string Url = string.Empty;
        public static string Lat = string.Empty;
        public static string Lng = string.Empty;
        public static string Country = string.Empty;
        public static string City = string.Empty;
        public static string VisionApiKey = string.Empty;

        public static int CountNotificationsStatic = 0;
        public static string OffsetLastChat = "0";

        public static bool NotificationPopup { get; set; } = true;
        public static bool SoundControl = true;
        public static bool OnlineUsers = true;

        public static long UnixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public static string Time = UnixTimestamp.ToString();

        public static string MarketDistanceCount = "";

        public static string NearbyShopsDistanceCount = "0";
        public static string NearbyBusinessDistanceCount = "";

        public static string NearByDistanceCount = "0";
        public static string NearByGender = "all";
        public static string NearByStatus = "2";
        public static string NearByRelationship = "5";

        public static string SearchGender = "all";
        public static string SearchCountry = "all";
        public static string SearchStatus = "all";
        public static string SearchVerified = "all";
        public static string SearchProfilePicture = "all";
        public static string SearchFilterByAge = "off";
        public static string SearchAgeFrom = "10";
        public static string SearchAgeTo = "70";

        public static string FilterJobType = "";
        public static string FilterJobLocation = "";
        public static string FilterJobCategories = "";

        public static PostDataObject DataLivePost = null;

        public static readonly string AndroidId = ""; //Android.Provider.Settings.Secure.GetString(Application.Context.ContentResolver, Android.Provider.Settings.Secure.AndroidId);
        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, PreserveReferencesHandling = PreserveReferencesHandling.Objects };

        public static bool OpenDialog;

        //Chat
        public static bool ChatHead = true;
        public static bool FingerprintLock;
        public static bool PhotoWifi = true;
        public static bool VideoWifi = true;
        public static bool AudioWifi = true;

        public static bool PhotoMobile = true;
        public static bool VideoMobile = true;
        public static bool AudioMobile = true;
        public static WoSocketHandler Socket { get; set; }

        public static void ClearAllValueUserDetails()
        {
            try
            {
                AccessToken = string.Empty;
                UserId = string.Empty;
                Username = string.Empty;
                FullName = string.Empty;
                Password = string.Empty;
                Email = string.Empty;
                Cookie = string.Empty;
                Status = string.Empty;
                Avatar = string.Empty;
                Cover = string.Empty;
                DeviceId = string.Empty;
                DeviceMsgId = string.Empty;
                LangName = string.Empty;
                Lat = string.Empty;
                Lng = string.Empty;

                Current.AccessToken = string.Empty;
                Socket = null;

                NearByDistanceCount = "0";
                NearByGender = "all";
                NearByStatus = "2";
                NearByRelationship = "5";

                SearchGender = "all";
                SearchCountry = "all";
                SearchStatus = "all";
                SearchVerified = "all";
                SearchProfilePicture = "all";
                SearchFilterByAge = "off";
                SearchAgeFrom = "10";
                SearchAgeTo = "70";

                SoundControl = true;
                OnlineUsers = false;
                ChatHead = true;
                FingerprintLock = false;

                PhotoWifi = true;
                VideoWifi = true;
                AudioWifi = true;

                PhotoMobile = true;
                VideoMobile = true;
                AudioMobile = true;

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}