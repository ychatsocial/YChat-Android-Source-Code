﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using Androidx.Media3.UI;
using Anjo.Android.YouTubePlayerX.Player;
using Newtonsoft.Json;
using WoWonder.Activities.Gift;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.Tabbes;
using WoWonder.Activities.UserProfile;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonder.MediaPlayers;
using WoWonder.MediaPlayers.Exo;
using WoWonder.SQLite;
using Exception = System.Exception;
using Reaction = WoWonderClient.Classes.Posts.Reaction;
using String = Java.Lang.String;
using Uri = Android.Net.Uri;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;

namespace WoWonder.Activities.ReelsVideo
{
    public class ViewReelsVideoFragment : Fragment, IYouTubePlayerInitListener, StTools.IXAutoLinkOnClickListener
    {
        #region Variables Basic

        private ReelsVideoDetailsActivity GlobalContext;
        private static ViewReelsVideoFragment Instance;

        private StReadMoreOption ReadMoreOption;
        private PostDataObject DataVideos;
        private PostModelType PostFeedType;

        private ImageView IconBack;

        private View MainView;
        private FrameLayout Root;

        private PlayerView PlayerView;

        public YouTubePlayerView TubePlayerView;
        private IYouTubePlayer YoutubePlayer { get; set; }
        private YouTubePlayerEvents YouTubePlayerEvents;
        private string VideoIdYoutube;
        private PostClickListener ClickListener;

        public LinearLayout LikeLayout;
        private LinearLayout UserLayout, GiftLayout, CommentLayout, ShareLayout;
        public ImageView ImgLike;
        private ImageView UserImageView, ImgSendGift, ImgComment, ImgShare;
        public TextView TxtLikeCount;
        private TextView TxtUsername, TxtCommentCount, TxtShareCount;
        private ImageView FollowButton;

        private SuperTextView TxtDescription;

        private bool MIsVisibleToUser;
        public ExoController ExoController;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                GlobalContext = (ReelsVideoDetailsActivity)Activity;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                MainView = inflater.Inflate(Resource.Layout.ReelsVideoSwipeLayout, container, false);
                return MainView;
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
                MainView = view;

                //var position = Arguments?.GetInt("position", 0); 
                DataVideos = JsonConvert.DeserializeObject<PostDataObject>(Arguments?.GetString("DataItem") ?? "");

                Instance = this;
                InitComponent(view);
                InitPlayer();

                ClickListener = new PostClickListener(GlobalContext, NativeFeedType.Global);

                ReadMoreOption = new StReadMoreOption.Builder()
                    .TextLength(100, StReadMoreOption.TypeCharacter)
                    .MoreLabel(Activity.GetText(Resource.String.Lbl_ReadMore))
                    .LessLabel(Activity.GetText(Resource.String.Lbl_ReadLess))
                    .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LabelUnderLine(true)
                    .Build();

                LoadData(DataVideos);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void SetMenuVisibility(bool menuVisible)
        {
            try
            {
                base.SetMenuVisibility(menuVisible);
                MIsVisibleToUser = menuVisible;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);

                if (IsResumed && MIsVisibleToUser)
                {
                    DataVideos ??= JsonConvert.DeserializeObject<PostDataObject>(Arguments?.GetString("DataItem") ?? "");
                    if (DataVideos != null)
                    {
                        StartVideo(DataVideos);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
                StopVideo();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnStop()
        {
            try
            {
                base.OnStop();

                if (MIsVisibleToUser)
                    StopVideo();
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

        public override void OnDestroyView()
        {
            try
            {
                ReleaseVideo();

                Instance = null;
                DestroyBasic();

                base.OnDestroyView();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnDestroy()
        {
            try
            {
                ReleaseVideo();

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                IconBack = view.FindViewById<ImageView>(Resource.Id.back);
                Root = view.FindViewById<FrameLayout>(Resource.Id.root);
                PlayerView = view.FindViewById<PlayerView>(Resource.Id.player_view);

                UserLayout = view.FindViewById<LinearLayout>(Resource.Id.userLayout);
                UserImageView = view.FindViewById<ImageView>(Resource.Id.imageAvatar);
                TxtUsername = view.FindViewById<TextView>(Resource.Id.username);
                FollowButton = view.FindViewById<ImageView>(Resource.Id.iconAdd);

                TxtDescription = view.FindViewById<SuperTextView>(Resource.Id.tv_descreption);

                GiftLayout = view.FindViewById<LinearLayout>(Resource.Id.GiftLayout);
                ImgSendGift = view.FindViewById<ImageView>(Resource.Id.img_sendGift);

                LikeLayout = view.FindViewById<LinearLayout>(Resource.Id.likeLayout);
                ImgLike = view.FindViewById<ImageView>(Resource.Id.img_like);
                TxtLikeCount = view.FindViewById<TextView>(Resource.Id.tv_likeCount);
                LikeLayout.Tag = "Like";

                CommentLayout = view.FindViewById<LinearLayout>(Resource.Id.commentLayout);
                ImgComment = view.FindViewById<ImageView>(Resource.Id.img_comment);
                TxtCommentCount = view.FindViewById<TextView>(Resource.Id.tv_comment_count);

                ShareLayout = view.FindViewById<LinearLayout>(Resource.Id.shareLayout);
                ImgShare = view.FindViewById<ImageView>(Resource.Id.img_share);
                TxtShareCount = view.FindViewById<TextView>(Resource.Id.tv_share_count);

                TubePlayerView = view.FindViewById<YouTubePlayerView>(Resource.Id.youtube_player_view);
                if (TubePlayerView != null)
                {
                    TubePlayerView.Visibility = ViewStates.Gone;

                    // The player will automatically release itself when the activity is destroyed.
                    // The player will automatically pause when the activity is paused
                    // If you don't add YouTubePlayerView as a lifecycle observer, you will have to release it manually.
                    Lifecycle.AddObserver(TubePlayerView);

                    TubePlayerView.PlayerUiController.ShowMenuButton(false);

                    TubePlayerView.PlayerUiController.ShowCustomActionLeft1(false);
                    TubePlayerView.PlayerUiController.ShowCustomActionLeft2(false);
                    TubePlayerView.PlayerUiController.ShowCustomActionRight1(false);
                    TubePlayerView.PlayerUiController.ShowCustomActionRight2(false);

                    TubePlayerView.PlayerUiController.ShowFullscreenButton(false);

                    //TubePlayerView.PlayerUiController.Menu.AddItem(new MenuItem("example", Resource.Drawable.icon_settings_vector, (view)->Toast.makeText(this, "item clicked", Toast.LENGTH_SHORT).show()));
                }

                if (!AppSettings.ShowGift)
                {
                    GiftLayout.Visibility = ViewStates.Gone;
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
                // true +=  // false -=
                if (addEvent)
                {
                    IconBack.Click += IconBackOnClick;
                    UserImageView.Click += UserLayoutOnClick;
                    FollowButton.Click += FollowButtonOnClick;
                    GiftLayout.Click += GiftLayoutOnClick;
                    LikeLayout.Click += LikeLayoutOnClick;
                    CommentLayout.Click += CommentLayoutOnClick;
                    ShareLayout.Click += ShareLayoutOnClick;
                }
                else
                {
                    IconBack.Click -= IconBackOnClick;
                    UserImageView.Click -= UserLayoutOnClick;
                    FollowButton.Click -= FollowButtonOnClick;
                    GiftLayout.Click -= GiftLayoutOnClick;
                    LikeLayout.Click -= LikeLayoutOnClick;
                    CommentLayout.Click -= CommentLayoutOnClick;
                    ShareLayout.Click -= ShareLayoutOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static ViewReelsVideoFragment GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        private void DestroyBasic()
        {
            try
            {
                IconBack = null;
                Root = null;
                PlayerView = null;

                UserLayout = null;
                UserImageView = null;
                TxtUsername = null;
                FollowButton = null;

                TxtDescription = null;

                GiftLayout = null;
                ImgSendGift = null;

                LikeLayout = null;
                ImgLike = null;
                TxtLikeCount = null;

                CommentLayout = null;
                ImgComment = null;
                TxtCommentCount = null;

                ShareLayout = null;
                ImgShare = null;
                TxtShareCount = null;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private void IconBackOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext.Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ShareLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                ClickListener.SharePostClick(new GlobalClickEventArgs { NewsFeedClass = DataVideos, }, PostFeedType);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CommentLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                ClickListener.CommentPostClick(new GlobalClickEventArgs
                {
                    NewsFeedClass = DataVideos,
                });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LikeLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (LikeLayout.Tag?.ToString() == "Liked")
                {
                    DataVideos.Reaction ??= new Reaction();

                    switch (AppSettings.PostButton)
                    {
                        case PostButtonSystem.Reaction:
                            {
                                if (DataVideos.Reaction != null)
                                {
                                    switch (DataVideos.Reaction.Count)
                                    {
                                        case > 0:
                                            DataVideos.Reaction.Count--;
                                            break;
                                        default:
                                            DataVideos.Reaction.Count = 0;
                                            break;
                                    }

                                    DataVideos.Reaction.Type = "";
                                    DataVideos.Reaction.IsReacted = false;
                                }
                                TxtLikeCount.Text = Methods.FunString.FormatPriceValue(DataVideos.Reaction.Count);
                                break;
                            }
                        default:
                            {
                                var x = Convert.ToInt32(DataVideos.PostLikes);
                                switch (x)
                                {
                                    case > 0:
                                        x--;
                                        break;
                                    default:
                                        x = 0;
                                        break;
                                }

                                DataVideos.IsLiked = false;
                                DataVideos.PostLikes = Convert.ToString(x, CultureInfo.InvariantCulture);
                                TxtLikeCount.Text = DataVideos.PostLikes;
                                break;
                            }
                    }

                    ImgLike.SetImageResource(Resource.Drawable.icon_heart_vector);
                    ImgLike.SetColorFilter(Color.White);
                    LikeLayout.Tag = "Like";

                    switch (AppSettings.PostButton)
                    {
                        case PostButtonSystem.Reaction:
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.PostActionsAsync(DataVideos.PostId, "reaction") });
                            break;
                        default:
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.PostActionsAsync(DataVideos.PostId, "like") });
                            break;
                    }
                }
                else
                {
                    switch (AppSettings.PostButton)
                    {
                        case PostButtonSystem.Reaction:
                            {
                                new ReactionReelsVideo(Activity , this)?.ClickDialog(new GlobalClickEventArgs
                                {
                                    NewsFeedClass = DataVideos, 
                                });
                                break;
                            }
                        default:
                            {
                                DataVideos.IsLiked = true;

                                var x = Convert.ToInt32(DataVideos.PostLikes);
                                x++;

                                DataVideos.PostLikes = Convert.ToString(x, CultureInfo.InvariantCulture);
                                TxtLikeCount.Text = DataVideos.PostLikes;

                                ImgLike.SetImageResource(Resource.Drawable.icon_heart_vector);
                                ImgLike.SetColorFilter(Color.White);
                                LikeLayout.Tag = "Liked";

                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.PostActionsAsync(DataVideos.PostId, "like") });

                                break;
                            }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void GiftLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("UserId", DataVideos.UserId);

                GiftDialogFragment mGiftFragment = new GiftDialogFragment
                {
                    Arguments = bundle
                };

                mGiftFragment.Show(ChildFragmentManager, mGiftFragment.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void UserLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                WoWonderTools.OpenProfile(Activity, DataVideos.UserId, DataVideos.Publisher);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void FollowButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    WoWonderTools.SetAddFriendReels(Activity, DataVideos.Publisher, FollowButton);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void AutoLinkTextClick(StTools.XAutoLinkMode p0, string p1, Dictionary<string, string> userData)
        {
            try
            {
                p1 = p1.Replace(" ", "").Replace("\n", "");
                var typeText = Methods.FunString.Check_Regex(p1);
                if (typeText == "Email")
                {
                    Methods.App.SendEmail(Activity, p1);
                }
                else if (typeText == "Website")
                {
                    string url = p1.Contains("http") switch
                    {
                        false => "http://" + p1,
                        _ => p1
                    };

                    //var intent = new Intent(MainContext, typeof(LocalWebViewActivity));
                    //intent.PutExtra("URL", url);
                    //intent.PutExtra("Type", url);
                    //MainContext.StartActivity(intent);
                    new IntentController(GlobalContext).OpenBrowserFromApp(url);
                }
                else if (typeText == "Hashtag")
                {
                    var intent = new Intent(Activity, typeof(HashTagPostsActivity));
                    intent.PutExtra("Id", p1);
                    intent.PutExtra("Tag", p1);
                    Activity.StartActivity(intent);
                }
                else if (typeText == "Mention")
                {
                    var dataUSer = ListUtils.MyProfileList?.FirstOrDefault();
                    string name = p1.Replace("@", "");

                    var sqlEntity = new SqLiteDatabase();
                    var user = sqlEntity.Get_DataOneUser(name);


                    if (user != null)
                    {
                        WoWonderTools.OpenProfile(Activity, user.UserId, user);
                    }
                    else if (userData?.Count > 0)
                    {
                        var data = userData.FirstOrDefault(a => a.Value == name);
                        if (data.Key != null && data.Key == UserDetails.UserId)
                        {
                            if (PostClickListener.OpenMyProfile)
                            {
                                return;
                            }

                            var intent = new Intent(Activity, typeof(MyProfileActivity));
                            Activity.StartActivity(intent);
                        }
                        else if (data.Key != null)
                        {
                            var intent = new Intent(Activity, typeof(UserProfileActivity));
                            //intent.PutExtra("UserObject", JsonConvert.SerializeObject(item));
                            intent.PutExtra("UserId", data.Key);
                            Activity.StartActivity(intent);
                        }
                        else
                        {
                            if (name == dataUSer?.Name || name == dataUSer?.Username)
                            {
                                if (PostClickListener.OpenMyProfile)
                                {
                                    return;
                                }

                                var intent = new Intent(Activity, typeof(MyProfileActivity));
                                Activity.StartActivity(intent);
                            }
                            else
                            {
                                var intent = new Intent(Activity, typeof(UserProfileActivity));
                                //intent.PutExtra("UserObject", JsonConvert.SerializeObject(item));
                                intent.PutExtra("name", name);
                                Activity.StartActivity(intent);
                            }
                        }
                    }
                    else
                    {
                        if (name == dataUSer?.Name || name == dataUSer?.Username)
                        {
                            if (PostClickListener.OpenMyProfile)
                            {
                                return;
                            }

                            var intent = new Intent(Activity, typeof(MyProfileActivity));
                            Activity.StartActivity(intent);
                        }
                        else
                        {
                            var intent = new Intent(Activity, typeof(UserProfileActivity));
                            //intent.PutExtra("UserObject", JsonConvert.SerializeObject(item));
                            intent.PutExtra("name", name);
                            Activity.StartActivity(intent);
                        }
                    }
                }
                else if (typeText == "Number")
                {
                    Methods.App.SaveContacts(Activity, p1, "", "2");
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region YouTube Player

        public void OnInitSuccess(IYouTubePlayer player)
        {
            try
            {
                YoutubePlayer = player;
                YouTubePlayerEvents = new YouTubePlayerEvents(player, VideoIdYoutube, "ReelsVideo");
                YoutubePlayer.AddListener(YouTubePlayerEvents);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Exo Player

        private void InitPlayer()
        {
            try
            {
                ExoController = new ExoController(Activity, "ReelsVideo");
                ExoController.SetPlayer(PlayerView);
                ExoController.SetPlayerControl(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void StopVideo()
        {
            try
            {
                if (PostFeedType == PostModelType.YoutubePost)
                {
                    if (YoutubePlayer != null && YouTubePlayerEvents.IsPlaying)
                        YoutubePlayer.Pause();
                }
                else
                {
                    ExoController?.StopVideo();
                }

                TabbedMainActivity.GetInstance()?.SetOffWakeLock();

                //GC Collect
                //GC.Collect();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void ReleaseVideo()
        {
            try
            {
                if (PostFeedType == PostModelType.YoutubePost)
                {
                    if (YoutubePlayer != null && YouTubePlayerEvents.IsPlaying)
                        YoutubePlayer.Pause();

                    TubePlayerView.Release();
                }
                else
                {
                    ExoController?.ReleaseVideo();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void StartVideo(PostDataObject dataObject)
        {
            try
            {
                ListUtils.VideoReelsViewsList ??= new ObservableCollection<PostDataObject>();
                ListUtils.VideoReelsViewsList.Add(dataObject);

                PostFeedType = PostFunctions.GetAdapterType(dataObject);

                if (PostFeedType == PostModelType.VideoPost)
                {
                    if (TubePlayerView != null)
                    {
                        TubePlayerView.Release();
                        TubePlayerView.Visibility = ViewStates.Gone;
                    }

                    // Uri
                    Uri uri = Uri.Parse(dataObject.PostFileFull);
                    ExoController?.FirstPlayVideo(uri);
                }
                else if (PostFeedType == PostModelType.YoutubePost)
                {
                    VideoIdYoutube = dataObject.PostYoutube;

                    if (TubePlayerView != null)
                    {
                        TubePlayerView.Visibility = ViewStates.Visible;
                        TubePlayerView.Initialize(this);
                    }

                    ExoController?.StopVideo();

                    if (PlayerView != null)
                        PlayerView.Visibility = ViewStates.Gone;
                }

                //Add new View for video
                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.GetPostDataAsync(dataObject?.PostId, "post_data", "1") });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadData(PostDataObject dataObject)
        {
            try
            {
                GlideImageLoader.LoadImage(Activity, dataObject.PostPrivacy == "4" ? "user_anonymous" : dataObject.Publisher.Avatar, UserImageView, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

                if (dataObject.PostPrivacy == "4")
                    TxtUsername.Text = Activity.GetText(Resource.String.Lbl_Anonymous);
                else
                {
                    if (AppSettings.ShowUsernameReels)
                    {
                        TxtUsername.Text = dataObject.Publisher.Username;
                    }
                    else
                    {
                        TxtUsername.Text = WoWonderTools.GetNameFinal(dataObject.Publisher);
                    }
                }

                if (dataObject.Publisher.UserId == UserDetails.UserId)
                    FollowButton.Visibility = ViewStates.Gone;
                else if (dataObject.Publisher.IsFollowing is "1" or "yes" or "Yes")
                    FollowButton.Visibility = ViewStates.Gone;
                else
                {
                    if (dataObject.Publisher.CanFollow == "0" && dataObject.Publisher.IsFollowing == "0" && dataObject.Publisher.UserId != UserDetails.UserId)
                    {
                        FollowButton.Visibility = ViewStates.Gone;
                    }

                    if (dataObject.Publisher.FollowPrivacy == "0") // Everyone
                    {
                        FollowButton.Visibility = ViewStates.Visible;
                    }
                    else if (dataObject.Publisher.FollowPrivacy == "1") // People i Follow
                    {
                        if (dataObject.Publisher.IsFollowingMe == "0")
                        {
                            FollowButton.Visibility = dataObject.Publisher.IsFollowing == "0" ? ViewStates.Gone : ViewStates.Visible;
                        }
                        else if (dataObject.Publisher.IsFollowingMe == "1")
                        {
                            FollowButton.Visibility = ViewStates.Visible;
                        }
                    }
                    else
                        FollowButton.Visibility = ViewStates.Visible;
                }

                TxtCommentCount.Text = dataObject.PostComments;

                if (AppSettings.ShowCountSharePost)
                {
                    TxtShareCount.Text = dataObject.DatumPostShare;
                }
                else
                {
                    TxtShareCount.Visibility = ViewStates.Gone;
                }

                if (dataObject.UserId == UserDetails.UserId)
                {
                    GiftLayout.Visibility = ViewStates.Gone;
                }

                if (string.IsNullOrEmpty(dataObject.Orginaltext) || string.IsNullOrWhiteSpace(dataObject.Orginaltext))
                {
                    TxtDescription.Visibility = ViewStates.Invisible;
                }
                else
                {
                    switch (dataObject.RegexFilterList != null & dataObject.RegexFilterList?.Count > 0)
                    {
                        case true:
                            TxtDescription.SetAutoLinkOnClickListener(this, dataObject.RegexFilterList);
                            break;
                        default:
                            TxtDescription.SetAutoLinkOnClickListener(this, new Dictionary<string, string>());
                            break;
                    }

                    ReadMoreOption.AddReadMoreTo(TxtDescription, new String(Methods.FunString.DecodeString(dataObject.Orginaltext)));
                }

                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.Reaction:
                        {
                            dataObject.Reaction ??= new Reaction();

                            TxtLikeCount.Text = Methods.FunString.FormatPriceValue(dataObject.Reaction.Count);

                            if (dataObject.Reaction.IsReacted != null && dataObject.Reaction.IsReacted.Value)
                            {
                                switch (string.IsNullOrEmpty(dataObject.Reaction.Type))
                                {
                                    case false:
                                        {
                                            var react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Id == dataObject.Reaction.Type).Value?.Id ?? "";
                                            switch (react)
                                            {
                                                case "1":
                                                    ImgLike.SetImageResource(Resource.Drawable.emoji_like);
                                                    break;
                                                case "2":
                                                    ImgLike.SetImageResource(Resource.Drawable.emoji_love);
                                                    break;
                                                case "3":
                                                    ImgLike.SetImageResource(Resource.Drawable.emoji_haha);
                                                    break;
                                                case "4":
                                                    ImgLike.SetImageResource(Resource.Drawable.emoji_wow);
                                                    break;
                                                case "5":
                                                    ImgLike.SetImageResource(Resource.Drawable.emoji_sad);
                                                    break;
                                                case "6":
                                                    ImgLike.SetImageResource(Resource.Drawable.emoji_angry);
                                                    break;
                                                default:
                                                    if (dataObject.Reaction.Count > 0)
                                                        ImgLike.SetImageResource(Resource.Drawable.emoji_like);
                                                    break;
                                            }
                                            LikeLayout.Tag = "Liked";
                                        }
                                        ImgLike.ClearColorFilter();
                                        break;
                                }
                            }
                            else
                            {
                                ImgLike.SetImageResource(Resource.Drawable.icon_heart_vector);
                                ImgLike.SetColorFilter(Color.White);
                                LikeLayout.Tag = "Like";
                            }
                        }
                        break;
                    default:
                        {
                            if (dataObject.Reaction.IsReacted != null && !dataObject.Reaction.IsReacted.Value)
                            {
                                ImgLike.SetImageResource(Resource.Drawable.icon_heart_vector);
                                ImgLike.SetColorFilter(Color.White);
                                LikeLayout.Tag = "Like";
                            }

                            if (dataObject.IsLiked != null && dataObject.IsLiked.Value)
                            {
                                ImgLike.SetImageResource(Resource.Drawable.emoji_like);
                                ImgLike.ClearColorFilter();
                                LikeLayout.Tag = "Liked";
                            }

                            TxtLikeCount.Text = dataObject.PostLikes;

                            break;
                        }
                }

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}