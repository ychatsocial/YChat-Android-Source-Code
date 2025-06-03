﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Icu.Text;
using Android.Media;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using Com.Aghajari.Emojiview.View;
using Google.Android.Material.Dialog;
using Java.Lang;
using Java.Util;
using Laerdal.FFmpeg.Android;
using Newtonsoft.Json;
using WoWonder.Activities.AddPost.Service;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.Video;
using WoWonder.StickersView;
using WoWonderClient.JobWorker;
using Console = System.Console;
using Exception = System.Exception;
using File = Java.IO.File;
using Math = System.Math;
using Signal = Laerdal.FFmpeg.Android.Signal;
using Thread = System.Threading.Thread;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.Videos
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class VideoEditorActivity : BaseActivity, ILogCallback, IStatisticsCallback, IExecuteCallback, IDialogListCallBack, VideoSeekBarView.ISeekBarDelegate, VideoTimelineView.IVideoTimelineViewListener, MediaPlayer.IOnPreparedListener, MediaPlayer.IOnCompletionListener
    {
        #region Variables Basic

        private VideoTimelineView VideoTimelineView;

        private LinearLayout StartEndTime;
        private TextView TxtStartTime, TxtEndTime, TxtEditedSize;
        private TextView IconAudio, IconRotation, IconSpeed;

        //private FrameLayout VideoContainer;
        private VideoView VideoView;
        private VideoSeekBarView VideoSeekBarView;

        private ImageView BtnPlay;

        private LinearLayout CommentLayout;
        private ImageView EmojisView;
        private AXEmojiEditText EmojisIconEditText;
        private ImageView BtnSend;


        private string VideoPath, Type, IdVideo;
        private File MTranscodeOutputFile;
        private float LastProgress;
        private bool NeedSeek;

        private string Speed;
        private int Rotation;
        private bool KeepVideoAudio;
        private bool RemoveAudio;
        private string AudioReplacementUri;

        private int VideoHeight, VideoWidth;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.VideoEditorLayout);

                VideoPath = Intent?.GetStringExtra("Uri") ?? "";
                Type = Intent?.GetStringExtra("Type") ?? "";
                IdVideo = Intent?.GetStringExtra("IdVideo") ?? "";

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                InitializerView();

                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Methods.Path.Chack_MyFolder();
                }
                else
                {
                    if (PermissionsController.CheckPermissionStorage(this))
                    {
                        Methods.Path.Chack_MyFolder();
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(100);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);

                if (VideoView is { IsPlaying: true })
                {
                    VideoView.Pause();
                    BtnPlay.SetImageResource(Resource.Drawable.icon_play_vector);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
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
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                VideoTimelineView?.Destroy();

                VideoView?.StopPlayback();
                VideoView = null;

                //FfMpeg?.Dispose();

                DestroyBasic();

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Menu 

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                VideoTimelineView = FindViewById<VideoTimelineView>(Resource.Id.video_timeline_view);
                StartEndTime = FindViewById<LinearLayout>(Resource.Id.start_end_time);
                TxtStartTime = FindViewById<TextView>(Resource.Id.start_time);
                TxtEndTime = FindViewById<TextView>(Resource.Id.end_time);
                TxtEditedSize = FindViewById<TextView>(Resource.Id.edited_size);

                IconAudio = FindViewById<TextView>(Resource.Id.icon_audio);
                IconRotation = FindViewById<TextView>(Resource.Id.icon_rotation);
                IconSpeed = FindViewById<TextView>(Resource.Id.icon_speed);

                VideoView = FindViewById<VideoView>(Resource.Id.video_view);
                VideoSeekBarView = FindViewById<VideoSeekBarView>(Resource.Id.video_seekbar);

                CommentLayout = FindViewById<LinearLayout>(Resource.Id.commentLayout);
                BtnPlay = FindViewById<ImageView>(Resource.Id.play_button);
                EmojisView = FindViewById<ImageView>(Resource.Id.emojiicon);
                EmojisIconEditText = FindViewById<AXEmojiEditText>(Resource.Id.EmojiconEditText5);
                BtnSend = FindViewById<ImageView>(Resource.Id.sendButton);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconAudio, IonIconsFonts.VolumeHigh); //VolumeOff
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconRotation, IonIconsFonts.Sync);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconSpeed, IonIconsFonts.IosSpeedometer);

                InitEmojisView();

                IconRotation.Visibility = ViewStates.Gone;
                IconSpeed.Visibility = ViewStates.Gone;

                if (Type is "Post" or "Messages")
                {
                    EmojisView.Visibility = ViewStates.Invisible;
                    EmojisIconEditText.Visibility = ViewStates.Invisible;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitEmojisView()
        {
            Methods.SetColorEditText(EmojisIconEditText, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (WoWonderTools.IsTabDark())
                        EmojisViewTools.LoadDarkTheme();
                    else
                        EmojisViewTools.LoadTheme(AppSettings.MainColor);

                    EmojisViewTools.MStickerView = false;
                    EmojisViewTools.LoadView(this, EmojisIconEditText, "", EmojisView);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            });
        }

        private void InitToolbar()
        {
            try
            {
                var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = " ";
                    toolBar.SetTitleTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(toolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    var icon = AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
                    icon?.SetTint(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SupportActionBar.SetHomeAsUpIndicator(icon);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                switch (addEvent)
                {
                    // true +=  // false -=
                    case true:
                        BtnPlay.Click += BtnPlayOnClick;
                        BtnSend.Click += BtnSendOnClick;
                        IconAudio.Click += IconAudioOnClick;
                        IconRotation.Click += IconRotationOnClick;
                        IconSpeed.Click += IconSpeedOnClick;
                        break;
                    default:
                        BtnPlay.Click -= BtnPlayOnClick;
                        BtnSend.Click -= BtnSendOnClick;
                        IconAudio.Click -= IconAudioOnClick;
                        IconRotation.Click -= IconRotationOnClick;
                        IconSpeed.Click -= IconSpeedOnClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void DestroyBasic()
        {
            try
            {
                VideoTimelineView = null!;
                StartEndTime = null!;
                TxtStartTime = null!;
                TxtEndTime = null!;
                TxtEditedSize = null!;
                IconAudio = null!;
                IconRotation = null!;
                IconSpeed = null!;
                VideoView = null!;
                VideoSeekBarView = null!;
                BtnPlay = null!;
                CommentLayout = null!;
                EmojisView = null!;
                EmojisIconEditText = null!;
                BtnSend = null!;
                VideoPath = null!;
                Type = null!;
                IdVideo = null!;
                MTranscodeOutputFile = null!;
                LastProgress = 0;
                NeedSeek = false;
                Speed = null!;
                Rotation = 0;
                KeepVideoAudio = false;
                AudioReplacementUri = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void IconSpeedOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                arrayAdapter.Add(GetText(Resource.String.Lbl_Normal));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Faster));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Slower));

                dialogList.SetTitle(Resource.String.Lbl_PlaybackSpeed);
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void IconRotationOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                arrayAdapter.Add(GetText(Resource.String.Lbl_AsInput));
                arrayAdapter.Add("90°");
                arrayAdapter.Add("180°");
                arrayAdapter.Add("270°");

                dialogList.SetTitle(Resource.String.Lbl_VideoRotation);
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void IconAudioOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                arrayAdapter.Add(GetText(Resource.String.Lbl_No));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Remove));
                arrayAdapter.Add(GetText(Resource.String.Lbl_ChooseSource));

                dialogList.SetTitle(Resource.String.Lbl_ReplaceAudio);
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnPlayOnClick(object sender, EventArgs e)
        {
            try
            {
                Play();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnSendOnClick(object sender, EventArgs e)
        {
            try
            {
                // AppHelper.ShowProgressDialog(this, Resources.getString(R.@string.loading));
                if (!NeedSeek)
                {
                    try
                    {
                        switch (Type)
                        {
                            case "Story":
                                {
                                    var item = new FileModel
                                    {
                                        StoryFileType = "video",
                                        StoryFilePath = VideoPath,
                                        StoryDescription = EmojisIconEditText.Text,
                                        StoryTitle = EmojisIconEditText.Text,
                                        StoryThumbnail = VideoPath,
                                    };

                                    Intent intent = new Intent(this, typeof(PostService));
                                    intent.SetAction(PostService.ActionStory);
                                    intent.PutExtra("DataPost", JsonConvert.SerializeObject(item));
                                    StartService(intent);
                                    break;
                                }
                            case "Messages":
                            case "Post":
                                {
                                    // put the String to pass back into an Intent and close this activity
                                    var resultIntent = new Intent();
                                    resultIntent.PutExtra("VideoPath", VideoPath);
                                    resultIntent.PutExtra("VideoId", IdVideo);
                                    SetResult(Result.Ok, resultIntent);

                                    break;
                                }
                        }

                        Finish();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }
                else
                    StartConvert();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private void InitializerView()
        {
            try
            {
                VideoTimelineView.SetVideoPath(VideoPath);
                VideoTimelineView.SetVideoTimelineViewListener(this);

                VideoSeekBarView.BarDelegate = this;

                VideoView.SetVideoURI(Uri.Parse(VideoPath));
                VideoView.SetOnPreparedListener(this);
                VideoView.SetOnCompletionListener(this);

                Runtime.GetRuntime()?.RunFinalization();
                Runtime.GetRuntime()?.Gc();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region MediaPlayer

        public void OnPrepared(MediaPlayer mp)
        {
            try
            {
                //UpdateVideoOriginalInfo();
                VideoView.SeekTo(1000);
                UpdateVideoEditedInfo(true);

                VideoHeight = mp.VideoHeight;
                VideoWidth = mp.VideoWidth;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnCompletion(MediaPlayer mp)
        {
            try
            {
                OnPlayComplete();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Play()
        {
            try
            {
                if (VideoView.IsPlaying)
                {
                    VideoView.Pause();
                    BtnPlay.SetImageResource(Resource.Drawable.icon_play_vector);
                }
                else
                {
                    BtnPlay.SetImageResource(0);
                    LastProgress = 0;
                    if (NeedSeek)
                    {
                        float prog = VideoTimelineView.GetLeftProgress() + (VideoTimelineView.GetRightProgress() - VideoTimelineView.Left) * VideoSeekBarView.GetProgress();
                        VideoView.SeekTo((int)(VideoView.Duration * prog));
                        NeedSeek = false;
                    }


                    VideoView.Start();

                    new Thread(() =>
                    {
                        try
                        {
                            if (VideoView == null)
                                return;

                            while (VideoView.IsPlaying)
                            {
                                RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        if (VideoView.IsPlaying)
                                        {
                                            float startTime = VideoTimelineView.GetLeftProgress() * VideoView.Duration;
                                            float endTime = VideoTimelineView.GetRightProgress() * VideoView.Duration;
                                            if (startTime == endTime)
                                            {
                                                startTime = endTime - 0.01f;
                                            }
                                            float progress = (VideoView.CurrentPosition - startTime) / (endTime - startTime);
                                            if (progress > LastProgress)
                                            {
                                                VideoSeekBarView.SetProgress(progress);
                                                LastProgress = progress;
                                            }
                                            if (VideoView.CurrentPosition >= endTime)
                                            {
                                                try
                                                {
                                                    VideoView.Pause();
                                                    OnPlayComplete();
                                                }
                                                catch (Exception e)
                                                {
                                                    Methods.DisplayReportResultTrack(e);
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });

                                Thread.Sleep(50);
                            }
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    }).Start();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void OnPlayComplete()
        {
            BtnPlay.SetImageResource(Resource.Drawable.icon_play_vector);
            VideoSeekBarView.SetProgress(0);
            try
            {
                VideoView.SeekTo((int)(VideoTimelineView.GetLeftProgress() * VideoView.Duration));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        #endregion

        #region VideoTimeline

        public void OnLeftProgressChanged(float progress)
        {
            try
            {
                if (VideoView.IsPlaying)
                {
                    VideoView.Pause();
                    BtnPlay.SetImageResource(Resource.Drawable.icon_play_vector);
                }

                VideoView.SeekTo((int)(VideoView.Duration * progress));

                NeedSeek = true;
                VideoSeekBarView.SetProgress(0);
                UpdateVideoEditedInfo();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnRightProgressChanged(float progress)
        {
            try
            {
                if (VideoView.IsPlaying)
                {
                    VideoView.Pause();
                    BtnPlay.SetImageResource(Resource.Drawable.icon_play_vector);
                }

                VideoView.SeekTo((int)(VideoView.Duration * progress));

                NeedSeek = true;
                VideoSeekBarView.SetProgress(0);
                UpdateVideoEditedInfo();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void DidStartDragging()
        {
            try
            {

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void DidStopDragging()
        {
            try
            {

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region VideoSeekBar

        public void OnSeekBarDrag(float progress)
        {
            try
            {
                if (VideoView.IsPlaying)
                {
                    float prog = VideoTimelineView.GetLeftProgress() + (VideoTimelineView.GetRightProgress() - VideoTimelineView.Left) * progress;
                    VideoView.SeekTo((int)(VideoView.Duration * prog));
                    LastProgress = progress;
                }
                else
                {
                    LastProgress = progress;
                    NeedSeek = true;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Video Fun 

        private void UpdateVideoOriginalInfo()
        {
            try
            {
                File file = new File(VideoPath);
                string videoDimension = VideoView.Width + "x" + VideoView.Height;
                long duration = (long)Math.Ceiling((double)VideoView.Duration);

                int minutes = (int)(duration / 1000 / 60);
                int seconds = (int)Math.Ceiling((double)duration / 1000) - minutes * 60;
                string videoTimeSize = minutes + ":" + seconds + ", " + Methods.FunString.Format_byte_size(file.Path);
                Console.WriteLine("OriginalVideo : " + videoDimension + " , " + videoTimeSize);

                //TxtOriginalSize.Text = "OriginalVideo : " + videoDimension + " , " + videoTimeSize;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void UpdateVideoEditedInfo(bool first = false)
        {
            try
            {
                long esimatedDuration = (long)Math.Ceiling((VideoTimelineView.GetRightProgress() - VideoTimelineView.GetLeftProgress()) * VideoView.Duration);

                File file = new File(VideoPath);
                double size = new FileInfo(file.Path).Length;
                float videoWidth = VideoView.Width;
                float videoHeight = VideoView.Height;
                if (videoWidth > 640 || videoHeight > 640)
                {
                    float scale = videoWidth > videoHeight ? 640.0f / videoWidth : 640.0f / videoHeight;
                    videoWidth *= scale;
                    videoHeight *= scale;
                    size *= scale * scale;
                }

                string videoDimension = (int)videoWidth + "x" + (int)videoHeight;
                Console.WriteLine(videoDimension);
                int minutes = (int)(esimatedDuration / 1000 / 60);
                int seconds = (int)Math.Ceiling((double)esimatedDuration / 1000) - minutes * 60;
                int estimatedSize = (int)(size * ((float)esimatedDuration / VideoView.Duration));

                string videoTimeSize = minutes + ":" + seconds + " • " + Methods.FunString.Format_byte_size(estimatedSize);

                TxtEditedSize.Visibility = ViewStates.Visible;
                TxtEditedSize.Text = videoTimeSize;

                long startTime = (long)Math.Ceiling(VideoTimelineView.GetLeftProgress() * VideoView.Duration);
                int startTimeMinutes = (int)(startTime / 1000 / 60);
                int startTimeSeconds = (int)Math.Ceiling((double)startTime / 1000) - startTimeMinutes * 60;
                TxtStartTime.Text = startTimeMinutes + ":" + startTimeSeconds;

                long endTime = (long)Math.Ceiling(VideoTimelineView.GetRightProgress() * VideoView.Duration);
                int endTimeMinutes = (int)(endTime / 1000 / 60);
                int endTimeSeconds = (int)Math.Ceiling((double)endTime / 1000) - endTimeMinutes * 60;
                TxtEndTime.Text = endTimeMinutes + ":" + endTimeSeconds;

                //var sss = (int)Methods.Time.ConvertMillisecondsToSeconds(esimatedDuration);
                if (!IsAllow(seconds) && first)
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_TheSubmittedVideoCut) + " " + AppSettings.StoryVideoDuration + " " + GetText(Resource.String.Lbl_CutSeconds), ToastLength.Long);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Start Convert and create 

        private void StartConvert()
        {
            try
            {
                long esimatedDuration = (long)Math.Ceiling((VideoTimelineView.GetRightProgress() - VideoTimelineView.GetLeftProgress()) * VideoView.Duration);
                int minutes = (int)(esimatedDuration / 1000 / 60);
                int seconds = (int)Math.Ceiling((double)esimatedDuration / 1000) - minutes * 60;

                if (IsAllow(seconds))
                {
                    Console.WriteLine("Min duration is reached");

                    string timeStamp = new SimpleDateFormat("yyyyMMdd_HHmmss").Format(new Date());
                    string videoFileName = "Video_" + timeStamp + "_";
                    string storageDir = Methods.Path.FolderDcimVideo;

                    MTranscodeOutputFile = new File(storageDir + "/" + videoFileName + ".mp4");

                    long startTime = (long)(VideoTimelineView.GetLeftProgress() * VideoView.Duration);
                    long endTime = (long)(VideoTimelineView.GetRightProgress() * VideoView.Duration);

                    if (startTime < 0)
                        startTime = 0;

                    if (endTime < 0)
                        endTime = 0;

                    FastMotionVideoCommand(startTime, endTime);
                }
                else
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_MaxDurationIsReachedPlease), ToastLength.Short);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private bool IsAllow(int seconds)
        {
            if (Type is "Post" or "Messages")
            {
                return true;
            }

            if (AppSettings.ShowFullVideo)
                return true;
            if (seconds <= AppSettings.StoryVideoDuration)
                return true;

            return false;
        }


        private void FastMotionVideoCommand(long startMs, long endMs)
        {
            try
            {
                var filePath = MTranscodeOutputFile.AbsolutePath;

                List<string> complexCommand = new List<string>
                {
                    "-ss", "" + startMs / 1000, //seeks to position(time from where we want to start cutting video)
                    "-y",
                };

                //path file Video
                complexCommand.Add("-i");
                complexCommand.Add(VideoPath);

                if (!string.IsNullOrEmpty(AudioReplacementUri))
                {
                    if (!RemoveAudio)
                    {
                        //path file Audio
                        complexCommand.Add("-i");
                        complexCommand.Add(AudioReplacementUri);
                    }
                }

                //$shell = shell_exec("$ffmpeg_b -y -i $video_file_full_path -vcodec libx264 -preset ".$wo['config']['convert_speed']." -filter:v scale=426:-2 -crf 26 $video_output_full_path_240 2>&1");

                //limit the duration of data read from the input file(duration from cutting start position time upto which we want to cut video)
                complexCommand.Add("-t");
                complexCommand.Add("" + (endMs - startMs) / 1000);

                //Set the video codec
                complexCommand.Add("-vcodec");
                complexCommand.Add("libx264");

                //Set the video bitrate
                complexCommand.Add("-b:v");
                complexCommand.Add("150k");

                //Set the audio bitrate
                complexCommand.Add("-b:a");
                complexCommand.Add("48000");

                //Set the number of audio channels
                complexCommand.Add("-ac");
                complexCommand.Add("2");

                //sets the sampling rate for audio streams if encoded
                complexCommand.Add("-ar");
                complexCommand.Add("22050");

                complexCommand.Add("-filter:v");
                complexCommand.Add("scale=426:-2");

                complexCommand.Add("-crf");
                complexCommand.Add("26");

                if (!string.IsNullOrEmpty(AudioReplacementUri))
                {
                    if (KeepVideoAudio)
                    {
                        complexCommand.Add("-filter_complex");
                        complexCommand.Add("[0:a][1:a]amerge,pan=stereo|c0<c0+c2|c1<c1+c3[out]");
                        complexCommand.Add("-map");
                        complexCommand.Add("1:v");
                        complexCommand.Add("-map");
                        complexCommand.Add("[out]");
                    }
                    else
                    {
                        //merge with new song without video audio 
                        complexCommand.Add("-map");
                        complexCommand.Add("0:0");
                        complexCommand.Add("-map");
                        complexCommand.Add("1:0");
                    }

                    complexCommand.Add("-c:v");
                    complexCommand.Add("copy");
                    complexCommand.Add("-c:a");
                    complexCommand.Add("aac");
                }
                else
                {
                    if (RemoveAudio)
                    {
                        //Disable audio recording
                        complexCommand.Add("-an");
                    }
                }

                if (!string.IsNullOrEmpty(Speed) && Speed != "Normal")
                {
                    complexCommand.Add("-filter_complex");
                    switch (Speed)
                    {
                        case "Faster":
                            complexCommand.Add("[0:v]setpts=0.5*PTS[v];[0:a]atempo=2.0[a]");
                            break;
                        case "Slower":
                            complexCommand.Add("[0:v]setpts=2.0*PTS[v];[0:a]atempo=0.5[a]");
                            break;
                    }

                    if (string.IsNullOrEmpty(AudioReplacementUri))
                    {
                        complexCommand.Add("-map");
                        complexCommand.Add("[v]");
                        complexCommand.Add("-map");
                        complexCommand.Add("[a]");
                    }
                }

                if (AppSettings.EnableVideoCompress)
                {
                    //Compress
                    complexCommand.Add("-s");
                    //complexCommand.Add("720x576");
                    complexCommand.Add(VideoHeight + "x" + VideoWidth);
                }

                if (AppSettings.EnableVideoCompress || !string.IsNullOrEmpty(Speed))
                {
                    complexCommand.Add("-r");
                    complexCommand.Add("50");
                }

                complexCommand.Add("-preset");
                complexCommand.Add("ultrafast");

                complexCommand.Add(filePath);

                //string[] complexCommand = new string[]
                //{
                //    "-ss", "" + startMs / 1000, //seeks to position(time from where we want to start cutting video)
                //    "-y",

                //    "-i", AudioReplacementUri, //path file  
                //    "-i", VideoPath, //path file   

                //    "-t", "" + (endMs - startMs) / 1000, //limit the duration of data read from the input file(duration from cutting start position time upto which we want to cut video)
                //    "-vcodec", "mpeg4", //Set the video codec
                //    "-b:v", "2097k", //Set the video bitrate
                //    "-b:a", "48000", //Set the audio bitrate
                //    "-ac", "2", //Set the number of audio channels
                //    "-ar", "22050", //sets the sampling rate for audio streams if encoded

                //    //Keep video audio
                //    "-filter_complex", "[0:a][1:a]amerge,pan=stereo|c0<c0+c2|c1<c1+c3[out]",
                //    "-map", "1:v",
                //    "-map", "[out]",

                //    //merge with new song without video audio
                //    //"-map", "0:v:0",
                //    //"-map", "1:a:0",

                //    "-c:v", "copy",
                //    "-c:a", "aac",

                //    "-shortest",

                //    //"-c", "copy", //copies the video, audio and bit stream from the input to the output file without re-encoding them.

                //    // "-an", //Disable audio recording

                //    // "-filter_complex", "[0:v]setpts=0.5*PTS[v];[0:a]atempo=2.0[a]", //fast
                //    //"-filter_complex", "[0:v]setpts=2.0*PTS[v];[0:a]atempo=0.5[a]", //slow
                //    //"-map", "[v]",
                //    //"-map", "[a]",
                //    //"-r", "50", 

                //    "-preset", "ultrafast", //ultrafast,superfast, veryfast, faster, fast, medium, slow, slower, veryslow

                //    filePath
                //};

                ExecFFmpegBinary(complexCommand.ToArray());
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ExecFFmpegBinary(string[] command)
        {
            try
            {
                var list = Config.ExternalLibraries;

                Config.IgnoreSignal(Signal.Sigxcpu);
                Config.LogLevel = Level.AvLogFatal;
                Config.EnableStatisticsCallback(this);
                Config.EnableLogCallback(this);
                Config.EnableRedirection();

                long executionId = FFmpeg.ExecuteAsync(command, this);

                Console.WriteLine(executionId);
                Console.WriteLine("Started command : ffmpeg");

                //Show a progress
                AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void Apply(long executionId, int returnCode)
        {
            try
            {
                Console.WriteLine("Finished command : ffmpeg");
                AndHUD.Shared.Dismiss();

                Console.WriteLine(returnCode);

                var output = FFmpeg.ListExecutions()?.FirstOrDefault(a => a.ExecutionId == executionId);
                Console.WriteLine(output);

                var tt = Config.LastReturnCode;

                Console.WriteLine(tt);

                if (returnCode == Config.ReturnCodeSuccess) //Async command execution completed successfully
                {
                    OnSuccess(executionId);
                }
                else if (returnCode == Config.ReturnCodeCancel) //Async command execution cancelled by user
                {
                    AndHUD.Shared.Dismiss();
                }
                else
                {
                    Console.WriteLine("FAILED with Async command execution failed with returnCode : " + returnCode);

                    AndHUD.Shared.Dismiss();

                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_ErrorCreatingVideo), ToastLength.Short);
                }
            }
            catch (Exception e)
            {
                AndHUD.Shared.Dismiss();
                Console.WriteLine(e);
            }
        }

        private void OnSuccess(long p0)
        {
            try
            {
                Console.WriteLine("SUCCESS with output : " + p0);
                AndHUD.Shared.Dismiss();

                var fileName = MTranscodeOutputFile.AbsolutePath.Split('/').Last();
                var fileNameWithoutExtension = fileName.Split('.').First();
                var pathWithoutFilename = Methods.Path.FolderDcimImage;
                var fullPathFile = new File(Methods.Path.FolderDcimImage, fileNameWithoutExtension + ".png");

                var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(pathWithoutFilename, fileNameWithoutExtension + ".png");
                if (videoPlaceHolderImage == "File Dont Exists")
                {
                    var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, MTranscodeOutputFile.AbsolutePath);
                    Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, pathWithoutFilename);
                }

                RunOnUiThread(() =>
                {
                    try
                    {
                        switch (Type)
                        {
                            case "Story":
                                {
                                    var item = new FileModel
                                    {
                                        StoryFileType = "video",
                                        StoryFilePath = MTranscodeOutputFile.AbsolutePath,
                                        StoryDescription = EmojisIconEditText.Text,
                                        StoryTitle = EmojisIconEditText.Text,
                                        StoryThumbnail = fullPathFile.Path,
                                    };

                                    Intent intent = new Intent(this, typeof(PostService));
                                    intent.SetAction(PostService.ActionStory);
                                    intent.PutExtra("DataPost", JsonConvert.SerializeObject(item));
                                    StartService(intent);
                                    break;
                                }
                            case "Messages":
                            case "Post":
                                {
                                    // put the String to pass back into an Intent and close this activity
                                    var resultIntent = new Intent();
                                    resultIntent.PutExtra("VideoPath", MTranscodeOutputFile.AbsolutePath);
                                    resultIntent.PutExtra("VideoId", IdVideo);
                                    SetResult(Result.Ok, resultIntent);

                                    break;
                                }
                        }

                        Finish();
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                if (itemString == GetText(Resource.String.Lbl_Normal))
                {
                    Speed = "Normal";
                }
                else if (itemString == GetText(Resource.String.Lbl_Faster))
                {
                    Speed = "Faster";
                }
                else if (itemString == GetText(Resource.String.Lbl_Slower))
                {
                    Speed = "Slower";
                }
                else if (itemString == GetText(Resource.String.Lbl_AsInput))
                {
                    Rotation = 0;
                }
                else if (itemString == "90°")
                {
                    Rotation = 90;
                }
                else if (itemString == "180°")
                {
                    Rotation = 180;
                }
                else if (itemString == "270°")
                {
                    Rotation = 270;
                }
                else if (itemString == GetText(Resource.String.Lbl_No))
                {
                    AudioReplacementUri = "";
                    RemoveAudio = false;
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconAudio, IonIconsFonts.VolumeHigh);
                }
                else if (itemString == GetText(Resource.String.Lbl_Remove))
                {
                    AudioReplacementUri = "";
                    RemoveAudio = true;
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconAudio, IonIconsFonts.VolumeOff);
                }
                else if (itemString == GetText(Resource.String.Lbl_ChooseSource))
                {
                    RemoveAudio = false;
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconAudio, IonIconsFonts.VolumeHigh);
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 505 =>  
                        new IntentController(this).OpenIntentAudio();
                    }
                    else
                    {
                        if (PermissionsController.CheckPermissionStorage(this))
                        {
                            //requestCode >> 505 =>  
                            new IntentController(this).OpenIntentAudio();
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(100);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                switch (requestCode)
                {

                    // Music
                    case 505 when resultCode == Result.Ok:
                        {
                            Uri uri = data.Data;
                            var path = Methods.AttachmentFiles.GetActualPathFromFile(this, uri);
                            var type = Methods.AttachmentFiles.Check_FileExtension(path);
                            switch (type)
                            {
                                case "Audio":
                                    {
                                        AudioReplacementUri = path;
                                        break;
                                    }
                                default:
                                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short);
                                    break;
                            }
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                switch (requestCode)
                {
                    case 100 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        Methods.Path.Chack_MyFolder();
                        break;
                    case 100:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion


        public void Apply(Statistics p0)
        {
            Console.WriteLine(p0);
        }

        public void Apply(LogMessage p0)
        {
            Console.WriteLine(p0);
        }
    }
}