using System;
using System.Timers;
using Android.Content.Res;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.Interpolator.View.Animation;
using AT.Markushi.UI;
using Java.IO;
using WoWonder.Activities.Chat.Broadcast;
using WoWonder.Activities.Chat.GroupChat;
using WoWonder.Activities.Chat.PageChat;
using WoWonder.Activities.Story;
using WoWonder.Helpers.Chat;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.Chat.ChatWindow.Fragment
{
    public class ChatRecordSoundFragment : AndroidX.Fragment.App.Fragment
    {

        private CircleButton RecordPlayButton;
        private CircleButton RecordCloseButton;
        private CircleButton SendRecordButton;
        private SeekBar VoiceSeekBar;
        private string RecordFilePath;
        private readonly string Type;
        private Methods.AudioRecorderAndPlayer AudioPlayerClass;

        private readonly ChatWindowActivity ChatWindowContext;
        private readonly GroupChatWindowActivity GroupChatWindowContext;
        private readonly PageChatWindowActivity PageChatWindowContext;
        private readonly BroadcastChatWindowActivity BroadcastChatWindowContext;

        private StoryReplyActivity MainStoryActivity;


        private Timer TimerSound;
        private MediaPlayer MediaPlayer;

        public ChatRecordSoundFragment(string type)
        {
            Type = type;
            switch (type)
            {
                case "ChatWindow":
                    ChatWindowContext = ChatWindowActivity.GetInstance();
                    break;
                case "GroupChatWindow":
                    GroupChatWindowContext = GroupChatWindowActivity.GetInstance();
                    break;
                case "PageChatWindow":
                    PageChatWindowContext = PageChatWindowActivity.GetInstance();
                    break;
                case "StoryReplyActivity":
                    MainStoryActivity = StoryReplyActivity.GetInstance();
                    break;
                case "BroadcastChatWindow":
                    BroadcastChatWindowContext = BroadcastChatWindowActivity.GetInstance();
                    break;
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.ChatRecourdSoundFragment, container, false);
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                RecordFilePath = Arguments?.GetString("FilePath");

                VoiceSeekBar = view.FindViewById<SeekBar>(Resource.Id.voiceseekbar);
                VoiceSeekBar.Max = 10000;
                VoiceSeekBar.Progress = 0;

                RecordPlayButton = view.FindViewById<CircleButton>(Resource.Id.playButton);
                RecordPlayButton.Click += RecordPlayButton_Click;

                RecordCloseButton = view.FindViewById<CircleButton>(Resource.Id.closeRecourdButton);
                RecordCloseButton.Click += RecordCloseButton_Click;

                SendRecordButton = view.FindViewById<CircleButton>(Resource.Id.sendRecourdButton);
                SendRecordButton.Click += SendRecordButtonOnClick;

                switch (Type)
                {
                    case "ChatWindow":
                        ChatWindowContext.SendButton.Tag = "Audio";

                        ChatWindowContext.RecordButton.Visibility = ViewStates.Gone;
                        ChatWindowContext.SendButton.Visibility = ViewStates.Visible;

                        AudioPlayerClass = new Methods.AudioRecorderAndPlayer(ChatWindowContext.UserId);
                        break;
                    case "GroupChatWindow":
                        GroupChatWindowContext.SendButton.Tag = "Audio";

                        GroupChatWindowContext.RecordButton.Visibility = ViewStates.Gone;
                        GroupChatWindowContext.SendButton.Visibility = ViewStates.Visible;

                        AudioPlayerClass = new Methods.AudioRecorderAndPlayer(GroupChatWindowContext.GroupId);
                        break;
                    case "PageChatWindow":
                        PageChatWindowContext.SendButton.Tag = "Audio";

                        PageChatWindowContext.RecordButton.Visibility = ViewStates.Gone;
                        PageChatWindowContext.SendButton.Visibility = ViewStates.Visible;

                        AudioPlayerClass = new Methods.AudioRecorderAndPlayer(PageChatWindowContext.PageId);
                        break;
                    case "BroadcastChatWindow":
                        BroadcastChatWindowContext.SendButton.Tag = "Audio";

                        BroadcastChatWindowContext.RecordButton.Visibility = ViewStates.Gone;
                        BroadcastChatWindowContext.SendButton.Visibility = ViewStates.Visible;

                        AudioPlayerClass = new Methods.AudioRecorderAndPlayer(BroadcastChatWindowContext.BroadcastId);
                        break;
                    case "StoryReplyActivity":

                        MainStoryActivity.SendButton.Tag = "Audio";

                        MainStoryActivity.RecordButton.Visibility = ViewStates.Gone;
                        MainStoryActivity.SendButton.Visibility = ViewStates.Visible;

                        AudioPlayerClass = new Methods.AudioRecorderAndPlayer(MainStoryActivity.UserId);
                        break;
                }
                TimerSound = new Timer();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SendRecordButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (Type)
                {
                    case "ChatWindow":
                        ChatWindowContext.SendRecordButton();
                        break;
                    case "GroupChatWindow":
                        GroupChatWindowContext.SendRecordButton();
                        break;
                    case "PageChatWindow":
                        PageChatWindowContext.SendRecordButton();
                        break;
                    case "StoryReplyActivity":
                        MainStoryActivity.SendRecordButton();
                        break;
                    case "BroadcastChatWindow":
                        BroadcastChatWindowContext.SendRecordButton();
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RecordCloseButton_Click(object sender, EventArgs e)
        {
            try
            {
                StopAudioPlay();

                switch (UserDetails.SoundControl)
                {
                    case true:
                        Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Error.mp3");
                        break;
                }

                AudioPlayerClass.Delete_Sound_Path(RecordFilePath);

                RecordFilePath = "";

                if (!string.IsNullOrEmpty(RecordFilePath))
                    Methods.AudioRecorderAndPlayer.StopAudioFromAsset();

                if (UserDetails.SoundControl)
                {
                    Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Error.mp3");
                }

                var fragmentHolder = Activity.FindViewById<FrameLayout>(Resource.Id.TopFragmentHolder);

                AudioPlayerClass.Delete_Sound_Path(RecordFilePath);
                var interplator = new FastOutSlowInInterpolator();
                fragmentHolder.Animate().SetInterpolator(interplator).TranslationY(1200).SetDuration(300);
                Activity.SupportFragmentManager.BeginTransaction().Remove(this)?.Commit();
                 
                switch (Type)
                {
                    case "ChatWindow":
                        ChatWindowContext.RecordButton.Tag = "Free";
                        ChatWindowContext.RecordButton.SetTheImageResource(Resource.Drawable.icon_mic_vector);
                        ChatWindowContext.RecordButton.SetListenForRecord(true);

                        ChatWindowContext.RecordButton.Visibility = ViewStates.Visible;
                        ChatWindowContext.SendButton.Visibility = ViewStates.Gone;
                        break;
                    case "GroupChatWindow":
                        GroupChatWindowContext.RecordButton.Tag = "Free";
                        GroupChatWindowContext.RecordButton.SetTheImageResource(Resource.Drawable.icon_mic_vector);
                        GroupChatWindowContext.RecordButton.SetListenForRecord(true);

                        GroupChatWindowContext.RecordButton.Visibility = ViewStates.Visible;
                        GroupChatWindowContext.SendButton.Visibility = ViewStates.Gone;
                        break;
                    case "PageChatWindow": 
                        PageChatWindowContext.RecordButton.Tag = "Free";
                        PageChatWindowContext.RecordButton.SetTheImageResource(Resource.Drawable.icon_mic_vector);
                        PageChatWindowContext.RecordButton.SetListenForRecord(true);

                        PageChatWindowContext.RecordButton.Visibility = ViewStates.Visible;
                        PageChatWindowContext.SendButton.Visibility = ViewStates.Gone;


                        break;
                    case "StoryReplyActivity": 
                        MainStoryActivity.RecordButton.Tag = "Free";
                        MainStoryActivity.RecordButton.SetTheImageResource(Resource.Drawable.icon_mic_vector);
                        MainStoryActivity.RecordButton.SetListenForRecord(true);

                        MainStoryActivity.RecordButton.Visibility = ViewStates.Visible;
                        MainStoryActivity.SendButton.Visibility = ViewStates.Gone;


                        break;
                    case "BroadcastChatWindow": 
                        BroadcastChatWindowContext.RecordButton.Tag = "Free";
                        BroadcastChatWindowContext.RecordButton.SetTheImageResource(Resource.Drawable.icon_mic_vector);
                        BroadcastChatWindowContext.RecordButton.SetListenForRecord(true);

                        BroadcastChatWindowContext.RecordButton.Visibility = ViewStates.Visible;
                        BroadcastChatWindowContext.SendButton.Visibility = ViewStates.Gone;

                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RecordPlayButton_Click(object sender, EventArgs e)
        {
            try
            {
                switch (MediaPlayer)
                {
                    case null:
                        {
                            MediaPlayer = new MediaPlayer();
                            MediaPlayer.SetAudioAttributes(new AudioAttributes.Builder()?.SetUsage(AudioUsageKind.Media)?.SetContentType(AudioContentType.Music)?.Build());

                            MediaPlayer.Completion += (sender, e) =>
                            {
                                try
                                {
                                    RecordPlayButton.Tag = "Play";
                                    RecordPlayButton.SetImageResource(Resource.Drawable.icon_play_vector);
                                    RecordPlayButton.ImageTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));

                                    MediaPlayer.Stop();
                                    MediaPlayer.Reset();
                                    MediaPlayer = null!;

                                    TimerSound.Enabled = false;
                                    TimerSound.Stop();
                                    TimerSound = null!;

                                    VoiceSeekBar.Progress = 0;
                                }
                                catch (Exception exception)
                                {
                                    Methods.DisplayReportResultTrack(exception);
                                }
                            };

                            MediaPlayer.Prepared += (s, ee) =>
                            {
                                try
                                {
                                    RecordPlayButton.Tag = "Pause";
                                    RecordPlayButton.SetImageResource(Resource.Drawable.icon_pause_vector);
                                    RecordPlayButton.ImageTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));

                                    TimerSound ??= new Timer { Interval = 1000 };

                                    MediaPlayer.Start();

                                    TimerSound.Elapsed += (sender, eventArgs) =>
                                    {
                                        Activity?.RunOnUiThread(() =>
                                        {
                                            try
                                            {
                                                if (TimerSound != null && TimerSound.Enabled)
                                                {
                                                    if (MediaPlayer != null)
                                                    {
                                                        int totalDuration = MediaPlayer.Duration;
                                                        int currentDuration = MediaPlayer.CurrentPosition;

                                                        // Updating progress bar
                                                        int progress = ChatTools.GetProgressSeekBar(currentDuration, totalDuration);

                                                        switch (Build.VERSION.SdkInt)
                                                        {
                                                            case >= BuildVersionCodes.N:
                                                                VoiceSeekBar.SetProgress(progress, true);
                                                                break;
                                                            default:
                                                                // For API < 24 
                                                                VoiceSeekBar.Progress = progress;
                                                                break;
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                Methods.DisplayReportResultTrack(e);
                                                RecordPlayButton.Tag = "Play";
                                            }
                                        });
                                    };
                                    TimerSound.Start();
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            };

                            if (RecordFilePath.Contains("http"))
                            {
                                MediaPlayer.SetDataSource(Activity, Uri.Parse(RecordFilePath));
                                MediaPlayer.PrepareAsync();
                            }
                            else
                            {
                                File file2 = new File(RecordFilePath);
                                var photoUri = FileProvider.GetUriForFile(Activity, Activity.PackageName + ".fileprovider", file2);

                                MediaPlayer.SetDataSource(Activity, photoUri);
                                MediaPlayer.PrepareAsync();
                            }

                            break;
                        }
                    default:
                        switch (RecordPlayButton?.Tag?.ToString())
                        {
                            case "Play":
                                {
                                    RecordPlayButton.Tag = "Pause";
                                    RecordPlayButton.SetImageResource(Resource.Drawable.icon_pause_vector);
                                    RecordPlayButton.ImageTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));

                                    MediaPlayer?.Start();

                                    if (TimerSound != null)
                                    {
                                        TimerSound.Enabled = true;
                                        TimerSound.Start();
                                    }

                                    break;
                                }
                            case "Pause":
                                {
                                    RecordPlayButton.Tag = "Play";
                                    RecordPlayButton.SetImageResource(Resource.Drawable.icon_play_vector);
                                    RecordPlayButton.ImageTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));

                                    MediaPlayer?.Pause();

                                    if (TimerSound != null)
                                    {
                                        TimerSound.Enabled = false;
                                        TimerSound.Stop();
                                    }

                                    break;
                                }
                        }

                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void StopAudioPlay()
        {
            try
            {
                RecordPlayButton.Tag = "Play";
                RecordPlayButton.SetColor(Color.White);
                RecordPlayButton.SetImageResource(Resource.Drawable.icon_play_vector);
                RecordPlayButton.ImageTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));

                if (MediaPlayer != null)
                {
                    MediaPlayer.Stop();
                    MediaPlayer.Reset();
                }
                MediaPlayer = null!;


                if (TimerSound != null)
                {
                    TimerSound.Enabled = false;
                    TimerSound.Stop();
                }

                TimerSound = null!;

                VoiceSeekBar.Progress = 0;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnDestroy()
        {
            try
            {
                StopAudioPlay();
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}