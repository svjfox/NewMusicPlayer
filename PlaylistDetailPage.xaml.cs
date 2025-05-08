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

        private async void SongSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Song selectedSong)
            {
                await PlaySong(selectedSong);
                ((CollectionView)sender).SelectedItem = null;
            }
        }

        private async void PlaySongClicked(object sender, EventArgs e)
        {
            if (((ImageButton)sender).CommandParameter is Song song)
            {
                await PlaySong(song);
            }
        }

        private async Task PlaySong(Song song)
        {
            MusicDataService.CurrentPlaylist = _playlist;
            MusicDataService.CurrentSongIndex = _playlist.Songs.IndexOf(song);

            await Navigation.PopAsync();

            if (Navigation.NavigationStack.LastOrDefault() is MainPage mainPage)
            {
                mainPage.PlayCurrentSongFromPlaylist();
            }
        }

        private async void PlayAllClicked(object sender, EventArgs e)
        {
            if (_playlist.Songs == null || _playlist.Songs.Count == 0)
            {
                await DisplayAlert("Info", "Playlist is empty!", "OK");
                return;
            }

            MusicDataService.CurrentPlaylist = _playlist;
            MusicDataService.CurrentSongIndex = 0;

            await Navigation.PopAsync();

            if (Navigation.NavigationStack.LastOrDefault() is MainPage mainPage)
            {
                mainPage.PlayPlaylist(_playlist);

                var toast = Toast.Make($"Playing '{_playlist.Name}' playlist",
                                      CommunityToolkit.Maui.Core.ToastDuration.Short);
                await toast.Show();
            }
        }
    }
}