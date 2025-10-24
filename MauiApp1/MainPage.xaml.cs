using Microsoft.Maui.Controls;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnCreateNewTableClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TablePage(createNew: true));
        }

        private async void OnOpenTableClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TablePage(createNew: false));
        }


        private async void OnHelpClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HelpPage());
        }
    }
}
