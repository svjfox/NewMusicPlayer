using System;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System.Collections.Generic;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Primitives;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Threading;
using System.Timers;
using System.Windows.Input;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Maui;
using MusicPlayerVS.Models;

namespace MusicPlayerVS
{
    public partial class MainPage : ContentPage
    {
        private bool isFirstButtonClick = true;
        private bool isDarkTheme = true; // Установлено false для светлой темы по умолчанию
        private bool isRepeatEnabled = false;
        private bool isFavorite = false;
        private ObservableCollection<Playlist> _playlists = new ObservableCollection<Playlist>();
        private Playlist _currentPlaylist
        {
            get => MusicDataService.CurrentPlaylist;
            set => MusicDataService.CurrentPlaylist = value;
        }

        private List<Song> playlist = new List<Song>
        {
            new Song("Ghost", "Confetti", "Ghost.mp3"),
            new Song("No Guts No Glory", "Cyberpunk Dreams", "No Guts No Glory.mp3"),
            new Song("Zero to Hero", "Electric Pulse", "Zero to Hero.mp3")
        };

        private int currentSongIndex = 0;
        private readonly System.Timers.Timer positionTimer = new(1000);

        public MainPage()
        {
            InitializeComponent();

            // Устанавливаем начальную тему
            isDarkTheme = Preferences.Get("IsDarkTheme", false); // Установлено false для светлой темы по умолчанию

            UpdateThemeIcon();

            // Инициализация обработчиков событий
            PlayButton.Clicked += PlayPauseButton_Clicked;
            PauseButton.Clicked += PlayPauseButton_Clicked;
            PrevButton.Clicked += NextPrevButton_Clicked;
            NextButton.Clicked += NextPrevButton_Clicked;
            RepeatButton.Clicked += RepeatButton_Clicked;
            FavoriteButton.Clicked += FavoriteButton_Clicked;
            PositionSlider.ValueChanged += PositionSlider_ValueChanged;
            VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;
            PlaylistsButton.Clicked += OpenPlaylists_Clicked;
            ThemeToggleButton.Clicked += ToggleTheme_Clicked;

            // Обработчики событий медиаплеера
            MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            MediaPlayer.MediaFailed += MediaPlayer_MediaFailed;

            InitializePlaylist();

            // Настройка таймера
            positionTimer.Elapsed += (s, e) => UpdatePlayerUI();
            positionTimer.Start();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateCurrentPlaylistInfo();
            ForceThemeUpdate(); // Применяем текущую тему
        }

        private void InitializePlaylist()
        {
            MediaPlayer.Source = MediaSource.FromResource(playlist[currentSongIndex].FilePath);

            MediaPlayer.MediaOpened += (s, e) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (MediaPlayer.Duration != null)
                    {
                        PositionSlider.Maximum = MediaPlayer.Duration.TotalSeconds;
                        TotalTimeLabel.Text = FormatTime(MediaPlayer.Duration);
                    }
                });
            };
        }

        private void ToggleTheme_Clicked(object sender, EventArgs e)
        {
            isDarkTheme = !isDarkTheme;
            if (isDarkTheme)
            {
                ((App)Application.Current).ApplyDarkTheme();
            }
            else
            {
                ((App)Application.Current).ApplyLightTheme();
            }
            UpdateThemeIcon();
        }


        private void UpdateThemeIcon()
        {
            // Обновляем иконку переключения темы
            ThemeToggleButton.Source = isDarkTheme ? "light_theme.png" : "dark_theme.png";
        }


        private void ForceThemeUpdate()
        {
            var textColor = (Color)Application.Current.Resources["TextColor"];
            var bgColor = (Color)Application.Current.Resources["BackgroundColor"];
            var cardColor = (Color)Application.Current.Resources["CardColor"];
            var primaryColor = (Color)Application.Current.Resources["PrimaryColor"];
            var secondaryColor = (Color)Application.Current.Resources["SecondaryColor"];

            // Обновляем элементы
            SongTitleLabel.TextColor = textColor;
            ArtistLabel.TextColor = secondaryColor;
            CurrentTimeLabel.TextColor = textColor;
            TotalTimeLabel.TextColor = textColor;
            CurrentPlaylistLabel.TextColor = secondaryColor;

            // Обновляем фон
            this.BackgroundColor = bgColor;

            // Обновляем Frame'ы
            MainContentFrame.BackgroundColor = cardColor;
            ControlsFrame.BackgroundColor = cardColor;
            PlayPauseFrame.BackgroundColor = primaryColor;
        }




        #region Playlist Management Methods

        private async void OpenPlaylists_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PlaylistsPage());
        }

        public void PlayPlaylist(Playlist playlist)
        {
            _currentPlaylist = playlist;
            this.playlist.Clear();

            foreach (var song in playlist.Songs)
            {
                this.playlist.Add(new Song(song.Title, song.Artist, song.FilePath)
                {
                    CoverImage = song.CoverImage,
                    IsFavorite = song.IsFavorite
                });
            }

            currentSongIndex = 0;
            UpdateCurrentPlaylistInfo();
            PlayMusic().ConfigureAwait(false);
        }

        public void PlaySong(Song song)
        {
            var index = _currentPlaylist?.Songs.IndexOf(song) ?? playlist.IndexOf(song);
            if (index >= 0)
            {
                currentSongIndex = index;
                PlayMusic().ConfigureAwait(false);
            }
            else
            {
                playlist.Clear();
                playlist.Add(new Song(song.Title, song.Artist, song.FilePath)
                {
                    CoverImage = song.CoverImage,
                    IsFavorite = song.IsFavorite
                });

                currentSongIndex = 0;
                PlayMusic().ConfigureAwait(false);
            }
        }

        public void PlayCurrentSongFromPlaylist()
        {
            if (MusicDataService.CurrentPlaylist != null &&
                MusicDataService.CurrentSongIndex < MusicDataService.CurrentPlaylist.Songs.Count)
            {
                var song = MusicDataService.CurrentPlaylist.Songs[MusicDataService.CurrentSongIndex];
                PlaySong(song);
            }
        }

        public void PlayNextSongFromPlaylist()
        {
            if (MusicDataService.CurrentPlaylist != null)
            {
                MusicDataService.CurrentSongIndex =
                    (MusicDataService.CurrentSongIndex + 1) % MusicDataService.CurrentPlaylist.Songs.Count;
                PlayCurrentSongFromPlaylist();
            }
        }

        private void UpdateCurrentPlaylistInfo()
        {
            if (_currentPlaylist != null)
            {
                CurrentPlaylistLabel.Text = $"{_currentPlaylist.Name} " +
                                         $"(song {currentSongIndex + 1}/{_currentPlaylist.Songs.Count})";
                CurrentPlaylistLabel.IsVisible = true;
            }
            else
            {
                CurrentPlaylistLabel.IsVisible = false;
            }
        }

        #endregion

        #region Player Control Methods

        private void UpdatePlayerUI()
        {
            if (MediaPlayer.CurrentState == MediaElementState.Playing)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (MediaPlayer.Position != null)
                    {
                        PositionSlider.Value = MediaPlayer.Position.TotalSeconds;
                        CurrentTimeLabel.Text = FormatTime(MediaPlayer.Position);
                    }
                });
            }
        }

        private string FormatTime(TimeSpan time)
        {
            return $"{(int)time.TotalMinutes}:{time.Seconds:00}";
        }

        private async void PlayPauseButton_Clicked(object sender, EventArgs e)
        {
            if (isFirstButtonClick)
            {
                NextButton.IsEnabled = true;
                PrevButton.IsEnabled = true;
                isFirstButtonClick = false;
            }

            if (MediaPlayer.CurrentState == MediaElementState.Playing)
            {
                await PauseMusic();
            }
            else
            {
                await PlayMusic();
            }
        }

        private async Task PlayMusic()
        {
            if (playlist.Count == 0) return;

            Song currentSong = playlist[currentSongIndex];
            SongTitleLabel.Text = currentSong.Title;
            ArtistLabel.Text = currentSong.Artist;
            AlbumCoverImage.Source = currentSong.CoverImage;
            isFavorite = currentSong.IsFavorite;
            FavoriteButton.Source = isFavorite ? "favorite_on_icon.png" : "favorite_icon.png";

            if (MediaPlayer.Source == null ||
                (MediaPlayer.Source is FileMediaSource fileSource && fileSource.Path != currentSong.FilePath))
            {
                MediaPlayer.Source = MediaSource.FromResource(currentSong.FilePath);
            }

            MediaPlayer.Play();

            PlayButton.IsVisible = false;
            PauseButton.IsVisible = true;
            UpdateCurrentPlaylistInfo();
        }

        private async Task PauseMusic()
        {
            MediaPlayer.Pause();
            PlayButton.IsVisible = true;
            PauseButton.IsVisible = false;
        }

        private async void NextPrevButton_Clicked(object sender, EventArgs e)
        {
            bool wasPlaying = MediaPlayer.CurrentState == MediaElementState.Playing;

            if (sender == PrevButton)
            {
                currentSongIndex = (currentSongIndex - 1 + playlist.Count) % playlist.Count;
            }
            else if (sender == NextButton)
            {
                currentSongIndex = (currentSongIndex + 1) % playlist.Count;
            }

            await LoadAndPlayCurrentSong();

            if (wasPlaying)
            {
                MediaPlayer.Play();
            }
        }

        private async Task LoadAndPlayCurrentSong()
        {
            if (playlist.Count == 0) return;

            Song currentSong = playlist[currentSongIndex];
            MediaPlayer.Source = MediaSource.FromResource(currentSong.FilePath);

            SongTitleLabel.Text = currentSong.Title;
            ArtistLabel.Text = currentSong.Artist;
            AlbumCoverImage.Source = currentSong.CoverImage;
            isFavorite = currentSong.IsFavorite;
            FavoriteButton.Source = isFavorite ? "favorite_on_icon.png" : "favorite_icon.png";

            PrevButton.IsEnabled = currentSongIndex != 0;
            NextButton.IsEnabled = currentSongIndex != playlist.Count - 1;
            UpdateCurrentPlaylistInfo();
        }

        #endregion

        #region Event Handlers

        private async void MediaPlayer_MediaFailed(object sender, MediaFailedEventArgs e)
        {
            await DisplayAlert("Error", $"Failed to play {playlist[currentSongIndex].Title}: {e.ErrorMessage}", "OK");
            currentSongIndex = (currentSongIndex + 1) % playlist.Count;
            await LoadAndPlayCurrentSong();
        }

        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            if (isRepeatEnabled)
            {
                PlayMusic().ConfigureAwait(false);
            }
            else if (_currentPlaylist != null)
            {
                currentSongIndex = (currentSongIndex + 1) % _currentPlaylist.Songs.Count;
                PlayCurrentSongFromPlaylist();
            }
            else
            {
                currentSongIndex = (currentSongIndex + 1) % playlist.Count;
                LoadAndPlayCurrentSong().ConfigureAwait(false);
            }
        }

        private void VolumeSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            MediaPlayer.Volume = e.NewValue;
        }

        private void PositionSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (Math.Abs(e.NewValue - MediaPlayer.Position.TotalSeconds) > 1 &&
                (MediaPlayer.CurrentState == MediaElementState.Playing ||
                 MediaPlayer.CurrentState == MediaElementState.Paused))
            {
                MediaPlayer.SeekTo(TimeSpan.FromSeconds(e.NewValue));
            }
        }

        private void RepeatButton_Clicked(object sender, EventArgs e)
        {
            isRepeatEnabled = !isRepeatEnabled;
            ((ImageButton)sender).Source = isRepeatEnabled ? "repeat_on_icon.png" : "repeat_icon.png";
        }

        private void FavoriteButton_Clicked(object sender, EventArgs e)
        {
            isFavorite = !isFavorite;
            ((ImageButton)sender).Source = isFavorite ? "favorite_on_icon.png" : "favorite_icon.png";

            if (currentSongIndex < playlist.Count)
            {
                playlist[currentSongIndex].IsFavorite = isFavorite;
            }

            if (_currentPlaylist != null && currentSongIndex < _currentPlaylist.Songs.Count)
            {
                _currentPlaylist.Songs[currentSongIndex].IsFavorite = isFavorite;
            }
        }

        private void MediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            if (MediaPlayer.Duration != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    PositionSlider.Maximum = MediaPlayer.Duration.TotalSeconds;
                    TotalTimeLabel.Text = FormatTime(MediaPlayer.Duration);
                });
            }
        }

        #endregion
    }
}