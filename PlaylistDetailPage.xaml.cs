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
            SongsCollectionView.ItemsSource = _playlist.Songs; // Отображаем песни в CollectionView
        }

        private async void SongSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Song selectedSong)
            {
                await PlaySong(selectedSong); // Воспроизведение выбранной песни
                ((CollectionView)sender).SelectedItem = null; // Снимаем выделение
            }
        }

        private async void PlaySongClicked(object sender, EventArgs e)
        {
            if (((ImageButton)sender).CommandParameter is Song song)
            {
                await PlaySong(song); // Воспроизведение выбранной песни
            }
        }

        private async Task PlaySong(Song song)
        {
            MusicDataService.CurrentPlaylist = _playlist; // Устанавливаем текущий плейлист
            MusicDataService.CurrentSongIndex = _playlist.Songs.IndexOf(song); // Индекс текущей песни

            // Возвращаемся на главную страницу
            await Navigation.PopAsync();

            if (Navigation.NavigationStack.LastOrDefault() is MainPage mainPage)
            {
                mainPage.PlayCurrentSongFromPlaylist(); // Воспроизведение песни в главном плеере
            }
        }

        private async void PlayAllClicked(object sender, EventArgs e)
        {
            MusicDataService.CurrentPlaylist = _playlist;
            MusicDataService.CurrentSongIndex = 0; // Начинаем воспроизведение с первой песни
            await Navigation.PopAsync();

            if (Navigation.NavigationStack.LastOrDefault() is MainPage mainPage)
            {
                mainPage.PlayCurrentSongFromPlaylist(); // Воспроизведение всех песен
            }
        }
    }
}
