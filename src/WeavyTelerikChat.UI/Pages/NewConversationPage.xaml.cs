using System;
using MvvmCross.Forms.Views;
using WeavyTelerikChat.Core.ViewModels.Conversation;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WeavyTelerikChat.UI.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewConversationPage : MvxContentPage<NewConversationViewModel>
    {
        public NewConversationPage()
        {
            InitializeComponent();

        }

        private void autoCompleteView_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await ViewModel.LoadResultsAsync(e.NewTextValue ?? "");
                });
            }
            catch (Exception ex)
            {
            }
        }

    }
}
