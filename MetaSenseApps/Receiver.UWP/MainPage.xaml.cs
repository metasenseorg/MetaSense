namespace Receiver.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();

            LoadApplication(new Receiver.App());
        }
    }
}
