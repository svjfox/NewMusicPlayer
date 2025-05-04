using MusicPlayerVS.Models;
using System.Collections.ObjectModel;

namespace MusicPlayerVS
{
    public partial class PlaylistDetailPage : ContentPage
    {
        private Playlist _playlist;

        public PlaylistDetailPage(Playlist playlist)
        {
            InitializeComponent();
            _playlist = playlist;
            BindingContext = _playlist;
            SongsCollectionView.ItemsSource = _playlist.Songs;
        }

        private void SongSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Song selectedSong)
            {
                // Здесь можно добавить логику воспроизведения выбранной песни
                // Например, передать в MainPage
                if (Navigation.NavigationStack.FirstOrDefault() is MainPage mainPage)
                {
                    mainPage.PlaySong(selectedSong);
                }

                ((CollectionView)sender).SelectedItem = null;
            }
        }

        private void PlayAllClicked(object sender, EventArgs e)
        {
            if (Navigation.NavigationStack.FirstOrDefault() is MainPage mainPage)
            {
                mainPage.PlayPlaylist(_playlist);
            }
        }

        private void ToggleFavoriteClicked(object sender, EventArgs e)
        {
            if (((ImageButton)sender).BindingContext is Song song)
            {
                song.IsFavorite = !song.IsFavorite;
                // Обновляем отображение
                SongsCollectionView.ItemsSource = null;
                SongsCollectionView.ItemsSource = _playlist.Songs;
            }
        }

        private async void EditPlaylistClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddSongsToPlaylistPage(_playlist));
        }
    }
}