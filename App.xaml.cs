namespace MusicPlayerVS
{
    public partial class App : Application
    {
        private readonly ResourceDictionary _lightTheme;
        private readonly ResourceDictionary _darkTheme;

        public App()
        {
            InitializeComponent();

            // Сохраняем ссылки на темы
            _lightTheme = new MusicPlayerVS.Resources.Themes.LightTheme();
            _darkTheme = new MusicPlayerVS.Resources.Themes.DarkTheme();

            // Устанавливаем тему по умолчанию
            ApplyLightTheme();

            MainPage = new MainPage();
        }

        public void ApplyLightTheme()
        {
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(_lightTheme);
            Current.UserAppTheme = AppTheme.Light;
        }

        public void ApplyDarkTheme()
        {
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(_darkTheme);
            Current.UserAppTheme = AppTheme.Dark;
        }
    }
}
