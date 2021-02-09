using MvvmCross.Forms.Views;
using WeavyTelerikChat.Core.Models;
using WeavyTelerikChat.Core.ViewModels.Conversation;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WeavyTelerikChat.UI.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConversationPage : MvxContentPage<ConversationViewModel>
    {
        public ConversationPage()
        {
            InitializeComponent();
        }        
    }
}
