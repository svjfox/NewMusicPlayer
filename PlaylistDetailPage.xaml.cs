using MusicPlayerVS.Models;
using System.Collections.ObjectModel;

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

            // Находим индекс песни в плейлисте
            var index = _playlist.Songs.IndexOf(song);
            if (index >= 0)
            {
                MusicDataService.CurrentSongIndex = index;

                // Возвращаемся на главную страницу и запускаем воспроизведение
                await Navigation.PopAsync();

                if (Navigation.NavigationStack.LastOrDefault() is MainPage mainPage)
                {
                    mainPage.PlayCurrentSongFromPlaylist();
                }
            }
        }
        private async void PlayAllClicked(object sender, EventArgs e)
        {
            MusicDataService.CurrentPlaylist = _playlist;
            MusicDataService.CurrentSongIndex = 0;
            await Navigation.PopAsync();

            if (Navigation.NavigationStack.LastOrDefault() is MainPage mainPage)
            {
                mainPage.PlayCurrentSongFromPlaylist();
            }
        }
    }
}