// MusicDataService.cs
using System.Collections.ObjectModel;
using MusicPlayerVS.Models;

namespace MusicPlayerVS
{
    public static class MusicDataService
    {

        public static ObservableCollection<Playlist> Playlists { get; } = new ObservableCollection<Playlist>();
        public static Playlist CurrentPlaylist { get; set; }

        static MusicDataService()
        {
            // Инициализация тестовыми данными
            var favorites = new Playlist { Name = "Favorites", CoverImage = "favorites_cover.png" };
            var workout = new Playlist { Name = "Workout", CoverImage = "workout_cover.png" };

            Playlists.Add(favorites);
            Playlists.Add(workout);
        }
    }
}