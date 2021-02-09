using System;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Forms.Views;
using MvvmCross.Presenters.Attributes;
using MvvmCross.ViewModels;
using WeavyTelerikChat.Core.Models;
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

        //private void autoComplete_ValueChanged(object sender, Syncfusion.SfAutoComplete.XForms.ValueChangedEventArgs e)
        //{
        //    try
        //    {
        //        Device.BeginInvokeOnMainThread(async () =>
        //        {
        //            await ViewModel.LoadResultsAsync(e.Value);
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}
    }
}
