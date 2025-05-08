using System.Collections.ObjectModel;

namespace MusicPlayerVS.Models
{
    public class Playlist
    {
        public string Name { get; set; }
        public string CoverImage { get; set; }
        public ObservableCollection<Song> Songs { get; set; } = new ObservableCollection<Song>();
    }
}