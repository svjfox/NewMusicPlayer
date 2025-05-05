using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using MusicPlayerVS.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace MusicPlayerVS
{
    public partial class AddSongsToPlaylistPage : ContentPage
    {
        private Playlist _playlist;
        private ObservableCollection<Song> _allSongs = new ObservableCollection<Song>();

        public AddSongsToPlaylistPage(Playlist playlist)
        {
            InitializeComponent();
            _playlist = playlist;
            LoadAllSongs();
            AllSongsCollectionView.ItemsSource = _allSongs;
        }

        private void LoadAllSongs()
        {
            _allSongs.Clear();

            // Загружаем все песни
            _allSongs.Add(new Song("Ghost", "Confetti", "Ghost.mp3"));
            _allSongs.Add(new Song("No Guts No Glory", "Cyberpunk Dreams", "No Guts No Glory.mp3"));
            _allSongs.Add(new Song("Zero to Hero", "Electric Pulse", "Zero to Hero.mp3"));

            // Помечаем выбранные песни
            foreach (var song in _allSongs)
            {
                song.IsSelected = _playlist.Songs.Any(s => s.FilePath == song.FilePath);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Сохраняем изменения при закрытии страницы
            _playlist.Songs.Clear();
            foreach (var song in _allSongs.Where(s => s.IsSelected))
            {
                _playlist.Songs.Add(song);
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                AllSongsCollectionView.ItemsSource = _allSongs;
            }
            else
            {
                AllSongsCollectionView.ItemsSource = _allSongs.Where(s =>
                    s.Title.Contains(e.NewTextValue, StringComparison.OrdinalIgnoreCase) ||
                    s.Artist.Contains(e.NewTextValue, StringComparison.OrdinalIgnoreCase));
            }
        }

        private void SaveClicked(object sender, EventArgs e)
        {
            _playlist.Songs.Clear();
            foreach (var song in _allSongs.Where(s => s.IsSelected))
            {
                _playlist.Songs.Add(song);
            }

            Navigation.PopAsync();
        }
    }
}