using System;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using Microsoft.Maui;
using CommunityToolkit.Maui.Core.Primitives;

namespace MusicPlayerVS
{
    public partial class MainPage : ContentPage
    {
        private bool isFirstButtonClick = true;
        private bool isDarkTheme = true;
        private bool isRepeatEnabled = false;
        private bool isFavorite = false;
        private SynchronizationContext syncContext;

        private List<Song> playlist = new List<Song>
        {
            new Song("Ghost", "Confetti", "MusicPlayerVS.Resources.Audio.Ghost.mp3"),
            new Song("No Guts No Glory", "Cyberpunk Dreams", "MusicPlayerVS.Resources.Audio.No Guts No Glory.mp3"),
            new Song("Zero to Hero", "Electric Pulse", "MusicPlayerVS.Resources.Audio.Zero to Hero.mp3")
        };

        private int currentSongIndex = 0;
        private System.Timers.Timer positionTimer;

        public MainPage()
        {
            InitializeComponent();
            syncContext = SynchronizationContext.Current;

            ApplyTheme(isDarkTheme);
            SetupMediaPlayer();
            InitializeTimer();
            SetupEventHandlers();
        }

        private void SetupMediaPlayer()
        {
            MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            MediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
            MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;

            LoadCurrentSong();
        }

        private void InitializeTimer()
        {
            positionTimer = new System.Timers.Timer(1000); // не чаще 1 сек
            positionTimer.Elapsed += (s, e) =>
            {
                syncContext.Post(_ => UpdatePlayerUI(), null);
            };
            positionTimer.Start();
        }

        private void SetupEventHandlers()
        {
            PlayButton.Clicked += PlayPauseButton_Clicked;
            PauseButton.Clicked += PlayPauseButton_Clicked;
            PrevButton.Clicked += NextPrevButton_Clicked;
            NextButton.Clicked += NextPrevButton_Clicked;
            RepeatButton.Clicked += RepeatButton_Clicked;
            FavoriteButton.Clicked += FavoriteButton_Clicked;
            PositionSlider.ValueChanged += PositionSlider_ValueChanged;
            VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;
        }

        private void LoadCurrentSong()
        {
            var currentSong = playlist[currentSongIndex];

            try
            {
                // Исправлено: уточнение пути в embedded ресурсах
                var mediaSource = MediaSource.FromResource(currentSong.FilePath);
                MediaPlayer.Source = mediaSource;

                SongTitleLabel.Text = currentSong.Title;
                ArtistLabel.Text = currentSong.Artist;

                PositionSlider.Value = 0;
                CurrentTimeLabel.Text = "0:00";
                TotalTimeLabel.Text = "0:00";

                // Удалено: MediaPlayer.Speed = 0.7;
                MediaPlayer.Speed = 1.0; // нормальная скорость
            }
            catch (Exception ex)
            {
                DisplayAlert("Ошибка", $"Ошибка при загрузке файла: {ex.Message}", "OK");
            }
        }

        private void UpdatePlayerUI()
        {
            if (MediaPlayer?.CurrentState == MediaElementState.Playing)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    PositionSlider.Value = MediaPlayer.Position.TotalSeconds;
                    CurrentTimeLabel.Text = FormatTime(MediaPlayer.Position);

                    if (MediaPlayer.Duration.TotalSeconds > 0)
                    {
                        TotalTimeLabel.Text = FormatTime(MediaPlayer.Duration);
                    }
                });
            }
        }

        private string FormatTime(TimeSpan time)
        {
            return $"{(int)time.TotalMinutes}:{time.Seconds:00}";
        }

        private void PlayPauseButton_Clicked(object sender, EventArgs e)
        {
            if (isFirstButtonClick)
            {
                NextButton.IsEnabled = true;
                PrevButton.IsEnabled = true;
                isFirstButtonClick = false;
            }

            if (MediaPlayer.CurrentState == MediaElementState.Playing)
            {
                PauseMusic();
            }
            else
            {
                PlayMusic();
            }
        }

        private void PlayMusic()
        {
            Song currentSong = playlist[currentSongIndex];
            ChangeLabel($"Сейчас играет {currentSong.Title}", Colors.White);

            if (MediaPlayer.Source == null)
            {
                LoadCurrentSong();
            }

            MediaPlayer.Play();

            PlayButton.IsVisible = false;
            PauseButton.IsVisible = true;
        }

        private void PauseMusic()
        {
            ChangeLabel("Пауза...", Colors.Orange);
            MediaPlayer.Pause();

            PlayButton.IsVisible = true;
            PauseButton.IsVisible = false;
        }

        private void NextPrevButton_Clicked(object sender, EventArgs e)
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

            LoadCurrentSong();

            if (wasPlaying)
            {
                MediaPlayer.Play();
            }
        }

        private void ChangeLabel(string text, Color textColor)
        {
            SongTitleLabel.Text = text;
            SongTitleLabel.TextColor = textColor;
        }

        private void MediaPlayer_MediaFailed(object sender, MediaFailedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Ошибка", $"Не удалось воспроизвести {playlist[currentSongIndex].Title}: {e.ErrorMessage}", "OK");
                currentSongIndex = (currentSongIndex + 1) % playlist.Count;
                LoadCurrentSong();
            });
        }

        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            if (isRepeatEnabled)
            {
                MediaPlayer.SeekTo(TimeSpan.Zero);
                MediaPlayer.Play();
            }
            else
            {
                currentSongIndex = (currentSongIndex + 1) % playlist.Count;
                LoadCurrentSong();
                MediaPlayer.Play();
            }
        }

        private void MediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (MediaPlayer.Duration.TotalSeconds > 0)
                {
                    PositionSlider.Maximum = MediaPlayer.Duration.TotalSeconds;
                    TotalTimeLabel.Text = FormatTime(MediaPlayer.Duration);
                }
            });
        }

        private void VolumeSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            MediaPlayer.Volume = e.NewValue;
        }

        private void PositionSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (MediaPlayer?.CurrentState != null &&
                Math.Abs(e.NewValue - MediaPlayer.Position.TotalSeconds) > 1)
            {
                MediaPlayer.SeekTo(TimeSpan.FromSeconds(e.NewValue));
            }
        }

        private void ToggleTheme_Clicked(object sender, EventArgs e)
        {
            isDarkTheme = !isDarkTheme;
            ApplyTheme(isDarkTheme);
        }

        private void ApplyTheme(bool isDarkTheme)
        {
            Application.Current.Resources.MergedDictionaries.Clear();
            if (isDarkTheme)
            {
                Application.Current.Resources.MergedDictionaries.Add(new MusicPlayerVS.Resources.Themes.DarkTheme());
            }
            else
            {
                Application.Current.Resources.MergedDictionaries.Add(new MusicPlayerVS.Resources.Themes.LightTheme());
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
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            positionTimer?.Stop();
            positionTimer?.Dispose();
            MediaPlayer.Handler?.DisconnectHandler();
        }
    }

    public class Song
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string FilePath { get; set; }

        public Song(string title, string artist, string filePath)
        {
            Title = title;
            Artist = artist;
            FilePath = filePath;
        }
    }
}
