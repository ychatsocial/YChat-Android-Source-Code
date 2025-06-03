using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Widget;
using Java.Lang;
using WoWonder.Activities.Chat.ChatWindow;
using WoWonder.Activities.Chat.MsgTabbes;
using WoWonder.Helpers.Chat.Jobs;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Services;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Message;
using WoWonderClient.JobWorker;
using WoWonderClient.Requests;
using Exception = System.Exception;
using MessageData = WoWonderClient.Classes.Message.MessageData;

namespace WoWonder.Helpers.Chat
{
    public static class MessageController
    {
        //############# DON'T  MODIFY HERE ############# 

        private static ChatWindowActivity WindowActivity;

        private static ChatTabbedMainActivity GlobalContext;
        //========================= Functions =========================
        public static async Task SendMessageTask(ChatWindowActivity windowActivity, string userId, string chatId, string messageHashId, string messageType, string text = "", string contact = "", string filePath = "", string imageUrl = "", string stickerId = "", string gifUrl = "", string productId = "", string lat = "", string lng = "", string storyId = "", string replyId = "")
        {
            try
            {
                WindowActivity = windowActivity;

                GlobalContext = ChatTabbedMainActivity.GetInstance();

                if (!string.IsNullOrEmpty(filePath))
                {
                    new UploadSingleFileToServerWorker(windowActivity, "ChatWindowActivity").UploadFileToServer(windowActivity, new FileModel
                    {
                        MessageHashId = messageHashId,
                        ChatId = chatId,
                        UserId = userId,
                        FilePath = filePath,
                        ReplyId = replyId,
                        StoryId = storyId,
                    });
                }
                else
                {
                    StartApiService(userId, messageHashId, messageType, text, contact, filePath, imageUrl, stickerId, gifUrl, productId, lat, lng, storyId, replyId);
                }
            }
            catch (Exception ex)
            {
                await Task.CompletedTask;
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private static void StartApiService(string userId, string messageHashId, string messageType, string text = "", string contact = "", string filePath = "", string imageUrl = "", string stickerId = "", string gifUrl = "", string productId = "", string lat = "", string lng = "", string storyId = "", string replyId = "")
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(WindowActivity, WindowActivity?.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => SendMessage(userId, messageHashId, messageType, text, contact, filePath, imageUrl, stickerId, gifUrl, productId, lat, lng, storyId, replyId) });
        }

        private static async Task SendMessage(string userId, string messageHashId, string messageType, string text = "", string contact = "", string filePath = "", string imageUrl = "", string stickerId = "", string gifUrl = "", string productId = "", string lat = "", string lng = "", string storyId = "", string replyId = "")
        {
            var (apiStatus, respond) = await RequestsAsync.Message.SendMessageAsync(userId, messageHashId, messageType, text, contact, filePath, imageUrl, stickerId, gifUrl, productId, lat, lng, storyId, replyId);
            if (apiStatus == 200)
            {
                if (respond is SendMessageObject result)
                {
                    UpdateLastIdMessage(result);
                }
            }
            else Methods.DisplayReportResult(WindowActivity, respond);
        }

        public static void UpdateLastIdMessage(SendMessageObject chatMessages)
        {
            try
            {
                MessageData messageInfo = chatMessages?.MessageData?.FirstOrDefault();
                if (messageInfo != null)
                {
                    var typeModel = ChatTools.GetTypeModel(messageInfo);
                    if (typeModel == MessageModelType.None)
                        return;

                    if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                        UserDetails.Socket?.EmitAsync_SendMessageFile(messageInfo.ToId, UserDetails.AccessToken, UserDetails.Username, messageInfo.Id, messageInfo.ReplyId, messageInfo.StoryId);

                    AdapterModelsClassMessage checker = WindowActivity?.MAdapter?.DifferList?.FirstOrDefault(a => a.MesData?.Id == messageInfo.MessageHashId);
                    if (checker != null)
                    {
                        var message = ChatTools.MessageFilter(messageInfo.ToId, messageInfo, typeModel, true);
                        message.ModelType = typeModel;
                        message.ErrorSendMessage = false;
                        message.Seen ??= "0";
                        message.BtnDownload = true;

                        checker.MesData = message;
                        checker.Id = Long.ParseLong(message.Id);
                        checker.TypeView = typeModel;

                        //Update All data users to database
                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.Insert_Or_Update_To_one_MessagesTable(checker.MesData);

                        WindowActivity?.RunOnUiThread(() =>
                        {
                            try
                            {
                                //Update data RecyclerView Messages.
                                //if (message.ModelType == MessageModelType.RightSticker || message.ModelType == MessageModelType.RightImage || message.ModelType == MessageModelType.RightMap || message.ModelType == MessageModelType.RightVideo)
                                WindowActivity?.UpdateOneMessage(checker.MesData);

                                if (UserDetails.SoundControl)
                                    Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Popup_SendMesseges.mp3");
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });

                        #region LastChat

                        var updaterUser = GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.UserId == message.ToId && a.LastChat?.ChatType == "user");
                        if (updaterUser?.LastChat != null)
                        {
                            updaterUser.LastChat.ChatTime = message.Time;
                            updaterUser.LastChat.LastMessage = new LastMessageUnion
                            {
                                LastMessageClass = message
                            };

                            var index = GlobalContext.ChatTab.LastChatTab.MAdapter.LastChatsList.IndexOf(updaterUser);
                            if (index > -1)
                            {
                                updaterUser.LastChat.LastMessage.LastMessageClass.Text = typeModel switch
                                {
                                    MessageModelType.RightGif => WindowActivity?.GetText(Resource.String.Lbl_SendGifFile),
                                    MessageModelType.RightText => !string.IsNullOrEmpty(message.Text) ? Methods.FunString.DecodeString(message.Text) : WindowActivity?.GetText(Resource.String.Lbl_SendMessage),
                                    MessageModelType.RightSticker => WindowActivity?.GetText(Resource.String.Lbl_SendStickerFile),
                                    MessageModelType.RightContact => WindowActivity?.GetText(Resource.String.Lbl_SendContactnumber),
                                    MessageModelType.RightFile => WindowActivity?.GetText(Resource.String.Lbl_SendFile),
                                    MessageModelType.RightVideo => WindowActivity?.GetText(Resource.String.Lbl_SendVideoFile),
                                    MessageModelType.RightImage => WindowActivity?.GetText(Resource.String.Lbl_SendImageFile),
                                    MessageModelType.RightAudio => WindowActivity?.GetText(Resource.String.Lbl_SendAudioFile),
                                    MessageModelType.RightMap => WindowActivity?.GetText(Resource.String.Lbl_SendLocationFile),
                                    _ => updaterUser.LastChat?.LastMessage.LastMessageClass.Text
                                };

                                GlobalContext?.RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        if (updaterUser.LastChat.Mute?.Pin == "no")
                                        {
                                            var checkPin = GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.LastOrDefault(o => o.LastChat != null && o.LastChat.Mute?.Pin == "yes");
                                            if (checkPin != null)
                                            {
                                                var toIndex = GlobalContext.ChatTab.LastChatTab.MAdapter.LastChatsList.IndexOf(checkPin) + 1;

                                                if (index != toIndex)
                                                {
                                                    GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.Move(index, toIndex);
                                                    GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.NotifyItemMoved(index, toIndex);
                                                }

                                                GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.NotifyItemChanged(toIndex, "WithoutBlobText");
                                            }
                                            else
                                            {
                                                if (ListUtils.FriendRequestsList.Count > 0)
                                                {
                                                    if (index != 1)
                                                    {
                                                        GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.Move(index, 1);
                                                        GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.NotifyItemMoved(index, 1);
                                                    }

                                                    GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.NotifyItemChanged(1, "WithoutBlobText");
                                                }
                                                else
                                                {
                                                    if (index != 0)
                                                    {
                                                        GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.Move(index, 0);
                                                        GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.NotifyItemMoved(index, 0);
                                                    }

                                                    GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.NotifyItemChanged(0, "WithoutBlobText");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.NotifyItemChanged(index, "WithoutBlobText");
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });

                                SqLiteDatabase dbSqLite = new SqLiteDatabase();
                                //Update All data users to database
                                dbSqLite.Insert_Or_Update_one_LastUsersChat(updaterUser?.LastChat);

                            }
                        }
                        else
                        {
                            //insert new user  
                            if (Methods.CheckConnectivity())
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => AppUpdaterHelper.LoadChatAsync() });
                        }

                        #endregion

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