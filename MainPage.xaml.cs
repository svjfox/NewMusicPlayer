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
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui;


namespace MusicPlayerVS
{
    public partial class MainPage : ContentPage
    {
        private bool isFirstButtonClick = true;
        private bool isDarkTheme = false;
        private bool isRepeatEnabled = false;
        private bool isFavorite = false;

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

            // Устанавливаем начальную тему (например, темную)
            isDarkTheme = true; // Или false для светлой темы
            ApplyTheme();

            // Инициализация обработчиков событий
            PlayButton.Clicked += PlayPauseButton_Clicked;
            PauseButton.Clicked += PlayPauseButton_Clicked;
            PrevButton.Clicked += NextPrevButton_Clicked;
            NextButton.Clicked += NextPrevButton_Clicked;
            RepeatButton.Clicked += RepeatButton_Clicked;
            FavoriteButton.Clicked += FavoriteButton_Clicked;
            PositionSlider.ValueChanged += PositionSlider_ValueChanged;
            VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;

            InitializePlaylist();

            // Настройка таймера
            positionTimer.Elapsed += (s, e) => UpdatePlayerUI();
            positionTimer.Start();
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
            Song currentSong = playlist[currentSongIndex];
            SongTitleLabel.Text = currentSong.Title;
            ArtistLabel.Text = currentSong.Artist;

            ChangeLabel($"Now playing {currentSong.Title}", Colors.White);

            if (MediaPlayer.Source == null)
            {
                MediaPlayer.Source = MediaSource.FromResource(currentSong.FilePath);
            }

            MediaPlayer.Play(); // Убрали await

            PlayButton.IsVisible = false;
            PauseButton.IsVisible = true;
        }

        private async Task PauseMusic()
        {
            ChangeLabel("Paused...", Colors.Orange);
            MediaPlayer.Pause(); // Убрали await

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

            // Если текущий трек играл, запускаем следующий/предыдущий трек
            if (wasPlaying)
            {
                MediaPlayer.Play();
            }
        }

        private async Task LoadAndPlayCurrentSong()
        {
            Song currentSong = playlist[currentSongIndex];
            MediaPlayer.Source = MediaSource.FromResource(currentSong.FilePath);

            SongTitleLabel.Text = currentSong.Title;
            ArtistLabel.Text = currentSong.Artist;

            ChangeLabel($"Now playing {currentSong.Title}", Colors.LightSkyBlue);

            PrevButton.IsEnabled = currentSongIndex != 0;
            NextButton.IsEnabled = currentSongIndex != playlist.Count - 1;
        }

        private void ChangeLabel(string text, Color textColor)
        {
            SongTitleLabel.Text = text;
            SongTitleLabel.TextColor = textColor;
        }

        private async void MediaPlayer_MediaFailed(object sender, MediaFailedEventArgs e)
        {
            await DisplayAlert("Error", $"Failed to play {playlist[currentSongIndex].Title}: {e.ErrorMessage}", "OK");
            currentSongIndex = (currentSongIndex + 1) % playlist.Count;
            await LoadAndPlayCurrentSong();
        }

        private async void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            // Если режим повтора включен, воспроизводим текущий трек
            if (isRepeatEnabled)
            {
                await LoadAndPlayCurrentSong();
            }
            else
            {
                // Переходим к следующему треку
                currentSongIndex = (currentSongIndex + 1) % playlist.Count;
                await LoadAndPlayCurrentSong();
            }

            // Запускаем воспроизведение
            MediaPlayer.Play();
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

        private void ToggleTheme_Clicked(object sender, EventArgs e)
        {
            isDarkTheme = !isDarkTheme; // Переключаем значение
            ApplyTheme();              // Применяем тему
        }

        private void ApplyTheme()
        {
            Debug.WriteLine($"Applying theme: {(isDarkTheme ? "Dark" : "Light")}");

            ResourceDictionary newTheme = isDarkTheme
                ? (ResourceDictionary)Resources["DarkTheme"]
                : (ResourceDictionary)Resources["LightTheme"];

            foreach (var key in newTheme.Keys)
            {
                if (Resources.ContainsKey(key))
                {
                    Resources[key] = newTheme[key];
                }
                else
                {
                    Resources.Add(key, newTheme[key]);
                }
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