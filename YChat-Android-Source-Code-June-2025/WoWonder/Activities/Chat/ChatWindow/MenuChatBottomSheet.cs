﻿using System;
using System.Collections.ObjectModel;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.BottomSheet;
using WoWonder.Activities.Chat.Broadcast;
using WoWonder.Activities.Chat.GroupChat;
using WoWonder.Activities.Chat.PageChat;
using WoWonder.Adapters;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using Exception = System.Exception;

namespace WoWonder.Activities.Chat.ChatWindow
{
    public class MenuChatBottomSheet : BottomSheetDialogFragment
    {
        #region Variables Basic

        private TextView TitleText;
        private ImageView IconClose;

        private ChatWindowActivity ChatWindowContext;
        private GroupChatWindowActivity GroupChatWindowContext;
        private PageChatWindowActivity PageChatWindowContext;
        private BroadcastChatWindowActivity BroadcastChatWindowContext;

        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ItemOptionAdapter MAdapter;

        private string Page;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = WoWonderTools.IsTabDark() ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.BottomSheetDefaultLayout, container, false);
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
                SetRecyclerViewAdapters(view);

                LoadDataChat();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                IconClose = view.FindViewById<ImageView>(Resource.Id.iconClose);
                IconClose.Click += IconCloseOnClick;

                TitleText = view.FindViewById<TextView>(Resource.Id.titleText);
                TitleText.Text = " ";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetRecyclerViewAdapters(View view)
        {
            try
            {
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);

                MAdapter = new ItemOptionAdapter(Activity)
                {
                    ItemOptionList = new ObservableCollection<Classes.ItemOptionObject>()
                };
                MAdapter.ItemClick += MAdapterOnItemClick;
                LayoutManager = new LinearLayoutManager(Context);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.SetAdapter(MAdapter);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(50);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                MRecycler.GetRecycledViewPool().Clear();
                MRecycler.SetAdapter(MAdapter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private void IconCloseOnClick(object sender, EventArgs e)
        {
            Dismiss();
        }

        private void MAdapterOnItemClick(object sender, ItemOptionAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position > -1)
                {
                    var item = MAdapter.GetItem(position);
                    if (Page == "ChatWindow")
                    {
                        if (item?.Id == "1") //View Profile
                        {
                            ChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_View_Profile));
                        }
                        else if (item?.Id == "2") //Block
                        {
                            ChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_Block));
                        }
                        else if (item?.Id == "3") //Change Chat Theme
                        {
                            ChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_ChangeChatTheme));
                        }
                        else if (item?.Id == "4") //Wallpaper
                        {
                            ChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_Wallpaper));
                        }
                        else if (item?.Id == "5") //Clear chat
                        {
                            ChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_Clear_chat));
                        }
                        else if (item?.Id == "6") //StartedMessages
                        {
                            ChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_StartedMessages));
                        }
                        else if (item?.Id == "7") //Media
                        {
                            ChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_Media));
                        }
                        else if (item?.Id == "8") //Search the conversation
                        {
                            ChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_Search));
                        }
                    }
                    else if (Page == "GroupChatWindow")
                    {
                        if (item?.Id == "1") //add Members
                        {
                            GroupChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_AddMembers));
                        }
                        else if (item?.Id == "2") //Group Info profile
                        {
                            GroupChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_GroupInfo));
                        }
                        else if (item?.Id == "3") //Delete Group
                        {
                            GroupChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_ExitGroup));
                        }
                        else if (item?.Id == "4") //Search the conversation
                        {
                            GroupChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_Search));
                        }
                    }
                    else if (Page == "PageChatWindow")
                    {
                        if (item?.Id == "1")
                        {

                        }
                    }
                    else if (Page == "BroadcastChatWindow")
                    {
                        if (item?.Id == "1") //add Recipients
                        {
                            BroadcastChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_AddRecipients));
                        }
                        else if (item?.Id == "2") //Broadcast Info profile
                        {
                            BroadcastChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_BroadcastInfo));
                        }
                        else if (item?.Id == "3") //Delete Broadcast 
                        {
                            BroadcastChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_DeleteBroadcast));
                        }
                    }

                    Dismiss();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private void LoadDataChat()
        {
            try
            {
                Page = Arguments?.GetString("Page") ?? ""; //ChatWindow ,GroupChatWindow,PageChatWindow
                switch (Page)
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
                    case "BroadcastChatWindow":
                        BroadcastChatWindowContext = BroadcastChatWindowActivity.GetInstance();
                        break;
                }

                if (Page == "ChatWindow")
                {
                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject
                    {
                        Id = "1",
                        Text = GetText(Resource.String.Lbl_View_Profile),
                        Icon = Resource.Drawable.icon_user_vector,
                    });

                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject
                    {
                        Id = "2",
                        Text = GetText(Resource.String.Lbl_Block),
                        Icon = Resource.Drawable.icon_block_vector,
                    });

                    if (AppSettings.ShowButtonColor)
                    {
                        MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject
                        {
                            Id = "3",
                            Text = GetText(Resource.String.Lbl_ChangeChatTheme),
                            Icon = Resource.Drawable.icon_color_vector,
                        });
                    }

                    if (AppSettings.ShowSettingsWallpaper)
                    {
                        MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject
                        {
                            Id = "4",
                            Text = GetText(Resource.String.Lbl_Wallpaper),
                            Icon = Resource.Drawable.icon_wallpaper_vector,
                        });
                    }

                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject
                    {
                        Id = "5",
                        Text = GetText(Resource.String.Lbl_Clear_chat),
                        Icon = Resource.Drawable.icon_clean_vector,
                    });

                    if (AppSettings.EnableFavoriteMessageSystem && ChatWindowContext.StartedMessageList?.Count > 0)
                    {
                        MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject
                        {
                            Id = "6",
                            Text = GetText(Resource.String.Lbl_StartedMessages),
                            Icon = Resource.Drawable.icon_star_vector,
                        });
                    }

                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject
                    {
                        Id = "7",
                        Text = GetText(Resource.String.Lbl_Media),
                        Icon = Resource.Drawable.icon_media_vector,
                    });

                    if (AppSettings.ShowSearchForMessage)
                    {
                        MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject
                        {
                            Id = "8",
                            Text = GetText(Resource.String.Lbl_Search_Conversation),
                            Icon = Resource.Drawable.icon_search_vector,
                        });
                    }
                }
                else if (Page == "GroupChatWindow")
                {
                    if (GroupChatWindowContext.GroupData.Owner != null && GroupChatWindowContext.GroupData.Owner.Value)
                    {
                        MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject
                        {
                            Id = "1",
                            Text = GetText(Resource.String.Lbl_AddMembers),
                            Icon = Resource.Drawable.icon_user_vector,
                        });
                    }

                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject
                    {
                        Id = "2",
                        Text = GetText(Resource.String.Lbl_GroupInfo),
                        Icon = Resource.Drawable.icon_info_vector,
                    });

                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject
                    {
                        Id = "3",
                        Text = GetText(Resource.String.Lbl_ExitGroup),
                        Icon = Resource.Drawable.icon_logout_vector,
                    });

                    if (AppSettings.ShowSearchForMessage)
                    {
                        MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject
                        {
                            Id = "7",
                            Text = GetText(Resource.String.Lbl_Search_Conversation),
                            Icon = Resource.Drawable.icon_search_vector,
                        });
                    }
                }
                else if (Page == "PageChatWindow")
                {

                }
                else if (Page == "BroadcastChatWindow")
                {
                    if (BroadcastChatWindowContext.BroadcastData.UserId == UserDetails.UserId)
                    {
                        MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject
                        {
                            Id = "1",
                            Text = GetText(Resource.String.Lbl_AddRecipients),
                            Icon = Resource.Drawable.icon_user_vector,
                        });
                    }

                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject
                    {
                        Id = "2",
                        Text = GetText(Resource.String.Lbl_BroadcastInfo),
                        Icon = Resource.Drawable.icon_info_vector,
                    });

                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject
                    {
                        Id = "3",
                        Text = GetText(Resource.String.Lbl_DeleteBroadcast),
                        Icon = Resource.Drawable.icon_logout_vector,
                    });
                }
                MAdapter.NotifyDataSetChanged();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}