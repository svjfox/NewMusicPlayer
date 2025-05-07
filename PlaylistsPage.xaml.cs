using MusicPlayerVS.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Maui.Controls;

namespace MusicPlayerVS
{
    public partial class PlaylistsPage : ContentPage
    {
        public ObservableCollection<Playlist> Playlists => MusicDataService.Playlists;

        public PlaylistsPage()
        {
            InitializeComponent();
            BindingContext = this;
            PlaylistsCollectionView.ItemsSource = Playlists;
        }

        private async void PlaylistSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Playlist selectedPlaylist)
            {
                // Переход на страницу с детальной информацией о выбранном плейлисте
                await Navigation.PushAsync(new PlaylistDetailPage(selectedPlaylist));
                ((CollectionView)sender).SelectedItem = null;
            }
        }

        private async void AddPlaylistClicked(object sender, EventArgs e)
        {
            string result = await DisplayPromptAsync("New Playlist", "Enter playlist name:");

            if (!string.IsNullOrWhiteSpace(result))
            {
                // Создание нового плейлиста
                var newPlaylist = new Playlist { Name = result };
                Playlists.Add(newPlaylist);

                // Показать уведомление
                var toast = Toast.Make($"Playlist '{result}' created", CommunityToolkit.Maui.Core.ToastDuration.Short);
                await toast.Show();
            }
        }

        private async void PlaylistOptionsClicked(object sender, EventArgs e)
        {
            var button = (ImageButton)sender;
            var playlist = (Playlist)button.BindingContext;

            string action = await DisplayActionSheet(playlist.Name, "Cancel", null,
                "Rename", "Delete", "Add Songs");

            if (action == "Rename")
            {
                string newName = await DisplayPromptAsync("Rename Playlist", "Enter new name:", initialValue: playlist.Name);
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    playlist.Name = newName;
                }
            }
            else if (action == "Delete")
            {
                bool confirm = await DisplayAlert("Confirm", $"Delete playlist '{playlist.Name}'?", "Yes", "No");
                if (confirm)
                {
                    Playlists.Remove(playlist);
                }
            }
            else if (action == "Add Songs")
            {
                // Переход на страницу добавления песен
                await Navigation.PushAsync(new AddSongsToPlaylistPage(playlist));
            }
        }
    }
}
