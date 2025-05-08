using System;

namespace MusicPlayerVS.Models
{
    public class Song
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string FilePath { get; set; }
        public string CoverImage { get; set; } = "music_cover.png";
        public bool IsFavorite { get; set; }
        public bool IsSelected { get; set; }

        public Song(string title, string artist, string filePath)
        {
            Title = title;
            Artist = artist;
            FilePath = filePath;
        }

        public Song(string title, string artist, string filePath, string coverImage) : this(title, artist, filePath)
        {
            CoverImage = coverImage;
        }
    }
}