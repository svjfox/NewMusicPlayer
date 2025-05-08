using MusicPlayerVS.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Maui.Controls;

namespace MusicPlayerVS
{
    public partial class PlaylistDetailPage : ContentPage
    {
        private readonly Playlist _playlist;

        public PlaylistDetailPage(Playlist playlist)
        {
            InitializeComponent();
            _playlist = playlist;
            BindingContext = _playlist;
            SongsCollectionView.ItemsSource = _playlist.Songs;
        }

        private async void PlayAllClicked(object sender, EventArgs e)
        {
            if (_playlist.Songs == null || _playlist.Songs.Count == 0)
            {
                await DisplayAlert("Info", "Playlist is empty!", "OK");
                return;
            }

            // Устанавливаем текущий плейлист
            MusicDataService.CurrentPlaylist = _playlist;
            MusicDataService.CurrentSongIndex = 0;

            // Находим MainPage через Navigation
            var mainPage = GetMainPage();
            if (mainPage != null)
            {
                mainPage.PlayPlaylist(_playlist);
                await Navigation.PopToRootAsync();

                var toast = Toast.Make($"Playing '{_playlist.Name}' playlist",
                                    CommunityToolkit.Maui.Core.ToastDuration.Short);
                await toast.Show();
            }
        }

        private MainPage GetMainPage()
        {
            foreach (var page in Navigation.NavigationStack)
            {
                if (page is MainPage mainPage)
                {
                    return mainPage;
                }
            }
            return null;
        }

        private async void SongSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Song selectedSong)
            {
                await PlaySong(selectedSong);
                ((CollectionView)sender).SelectedItem = null;
            }
        }

        private async Task PlaySong(Song song)
        {
            MusicDataService.CurrentPlaylist = _playlist;
            MusicDataService.CurrentSongIndex = _playlist.Songs.IndexOf(song);

            var mainPage = GetMainPage();
            if (mainPage != null)
            {
                mainPage.PlayCurrentSongFromPlaylist();
                await Navigation.PopToRootAsync();
            }
        }

        private async void PlaySongClicked(object sender, EventArgs e)
        {
            if (((ImageButton)sender).CommandParameter is Song song)
            {
                await PlaySong(song);
            }
        }
    }
}