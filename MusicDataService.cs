using System.Collections.ObjectModel;
using MusicPlayerVS.Models;

namespace MusicPlayerVS
{
    public static class MusicDataService
    {
        public static ObservableCollection<Playlist> Playlists { get; } = new();
        public static Playlist CurrentPlaylist { get; set; }
        public static int CurrentSongIndex { get; set; }

        static MusicDataService()
        {
            // Инициализация тестовыми данными
            var favorites = new Playlist
            {
                Name = "Favorites",
                CoverImage = "favorites_cover.png",
                Songs =
                {
                    new Song("Ghost", "Confetti", "Ghost.mp3"),
                    new Song("No Guts No Glory", "Cyberpunk Dreams", "No Guts No Glory.mp3")
                }
            };

            var workout = new Playlist
            {
                Name = "Workout",
                CoverImage = "workout_cover.png",
                Songs =
                {
                    new Song("Zero to Hero", "Electric Pulse", "Zero to Hero.mp3")
                }
            };

            Playlists.Add(favorites);
            Playlists.Add(workout);
        }
    }
}