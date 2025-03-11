namespace MAUI
{
    public partial class App : Application
    {
        public static string CurrentGroupName { get; set; }
        public static string CurrentPlayerName { get; set; }
        public static string CurrentGanador { get; set; }
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
