using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.AspNet.SignalR.Client;
using MvvmCross.Navigation;
using Newtonsoft.Json;
using Telerik.XamarinForms.ConversationalUI;
using WeavyTelerikChat.Core.Models;
using WeavyTelerikChat.Core.Services;
using WeavyTelerikChat.Core.ViewModels.Conversation;
using Xamarin.Forms;

[assembly: MvxNavigation(typeof(ConversationViewModel), @"mvx://conversation/\?id=(?<id>[A-Z0-9]{32})$")]
namespace WeavyTelerikChat.Core.ViewModels.Conversation
{
    public class ConversationViewModel : BaseViewModel<ConversationItem>
    {
        private ConversationItem _currentConversation;
        private readonly IConversationService _conversationService;
        private readonly IHubService _hubService;
        private int _skip = 0;
        private bool _canLoadMore = false;
        private bool _isVisible;

        public ConversationViewModel(IConversationService conversationService, IHubService hubService)
        {
            _conversationService = conversationService;
            _hubService = hubService;
            Messages = new ObservableCollection<object>();
            TypingParticipants = new ObservableCollection<Author>();

            LoadMoreCommand = new Command<object>(execute: (o) =>
            {
                Task.Run(async () => await LoadMessages(clear: false, skip: _skip)).Wait();
            },
            canExecute: (o) =>
            {
                return _canLoadMore;
            });

            SendMessageCommand = new Command(execute: async (args) => {
                var message = (string)args;                
                await SendMessage(message);
            },
            canExecute: (args) =>
            {
                return true;
            });            
        }

        private async Task SendMessage(string message)
        {            
            await _conversationService.SendMessage(Id, message);
        }

        public override void Prepare(ConversationItem conversation)
        {
            _currentConversation = conversation;
            _currentUser = new Author { Name = Constants.Me.Profile.Name, Avatar = Constants.Me.ThumbUrlFull };
            _skip = 0;

            Id = _currentConversation.Id;
            Title = _currentConversation.ConversationTitle;

            Task.Run(async () => await LoadMessages(clear: true, skip: _skip)).Wait();
            Task.Run(async () => await _conversationService.MarkAsRead(Id)).Wait();
        }

        private List<TextMessage> CreateTelerikMessages(Models.Message message)
        {

            var author = (message.CreatedBy?.Id == Constants.Me.Id) ? _currentUser : new Author { Name = message.CreatedBy?.Name, Avatar = message.CreatedBy.ThumbUrlFull };
            var messages = new List<TextMessage>();

            messages.Add(new TextMessage
            {
                Text = message.Text,               
                Author = author
            });
            return messages;
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

            _isVisible = true;

            _proxyEvent = _hubService.Proxy.On<string, string>("eventReceived", (type, data) =>
            {
                switch (type)
                {
                    case "typing.weavy":
                        var typing = JsonConvert.DeserializeObject<SignalrTyping>(data);
                        if (typing.Conversation.Equals(Id) && typing.User.Id != Constants.Me.Id)
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                TypingParticipants.Add(new Author() { Name = typing.User.Name });
                            });

                            Task.Factory.StartNew(() =>
                        {
                            Thread.Sleep(4000);
                            TypingParticipants.Clear();
                        });
                        }
                        break;
                    case "message-inserted.weavy":
                        var message = JsonConvert.DeserializeObject<SignalrMessage>(data);
                        if (message.Conversation.Equals(Id) && message.CreatedBy.Id != Constants.Me.Id)
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                TypingParticipants.Clear();
                                var addedMessages = CreateTelerikMessages(message);
                                addedMessages.Reverse();
                                foreach (var addedMessage in addedMessages)
                                {
                                    Messages.Add(addedMessage);
                                }

                                if (_isVisible)
                                {
                                    Task.Run(async () => await _conversationService.MarkAsRead(Id)).Wait();
                                }

                            });

                        }
                        break;
                    default:
                        break;
                }
            });
        }

        public override void ViewDisappeared()
        {
            base.ViewDisappeared();

            _isVisible = false;
            _proxyEvent.Dispose();
        }


        #region Binding Props
        private ObservableCollection<object> _messages;
        public ObservableCollection<object> Messages
        {
            get => _messages;
            set => SetProperty(ref _messages, value);
        }

        private Author _currentUser;
        public Author CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _title;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private ObservableCollection<Author> _typingParticipants;
        public ObservableCollection<Author> TypingParticipants
        {
            get => _typingParticipants;
            set => SetProperty(ref _typingParticipants, value);
        }

        private ICommand _sendMessageCommand;
        public ICommand SendMessageCommand
        {
            get => _sendMessageCommand;
            set => SetProperty(ref _sendMessageCommand, value);
        }

        private ICommand _loadMoreCommand;
        private IDisposable _proxyEvent;

        public ICommand LoadMoreCommand
        {
            get => _loadMoreCommand;
            set => SetProperty(ref _loadMoreCommand, value);
        }
        #endregion

        private async Task LoadMessages(bool clear, bool addBefore = false, int top = 10, int skip = 0)
        {

            try
            {
                IsBusy = true;

                if (clear)
                {
                    Messages.Clear();
                }

                var messages = await _conversationService.GetMessages(Id, top, skip);

                if (messages != null && messages.Data != null)
                {
                    foreach (var message in messages.Data)
                    {
                        var isByMe = message.CreatedBy?.Id == Constants.Me.Id;
                        var newMessages = CreateTelerikMessages(message);

                        foreach (var addedMessage in newMessages)
                        {
                            if (addBefore)
                            {
                                Messages.Insert(0, addedMessage);
                            }
                            else
                            {
                                Messages.Add(addedMessage);
                            }
                        }
                    }

                    _skip += top;

                }

                // check if there are more messages
                _canLoadMore = messages.Next != null;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

}
