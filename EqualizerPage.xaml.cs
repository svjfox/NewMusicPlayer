using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Linq;

namespace MusicPlayerVS
{
    public partial class EqualizerPage : ContentPage
    {
        private readonly double[] _equalizerSettings = new double[10];
        private bool _isEqualizerEnabled;
        private readonly string[] _frequencyLabels =
        {
            "60Hz", "150Hz", "400Hz", "1kHz", "2.5kHz",
            "6kHz", "15kHz", "4kHz", "10kHz", "16kHz"
        };

        public EqualizerPage()
        {
            InitializeComponent();
            InitializeSliders();
            LoadSettings();
            UpdateToggleState();
        }

        private void InitializeSliders()
        {
            if (EqualizerGrid?.Children == null) return;

            foreach (var child in EqualizerGrid.Children)
            {
                if (child is Slider slider)
                {
                    slider.ValueChanged += OnSliderValueChanged;
                }
            }
        }

        private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (sender is Slider slider && int.TryParse(slider.AutomationId, out int index))
            {
                _equalizerSettings[index] = e.NewValue;
                SaveCurrentSettings();
            }
        }

        private void OnPresetClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                var preset = button.CommandParameter?.ToString() ?? string.Empty;

                var presetValues = preset switch
                {
                    "Rock" => new double[] { 6, 4, 2, 0, -2, -4, -2, 0, 2, 4 },
                    "Pop" => new double[] { -2, 0, 2, 4, 6, 4, 2, 0, -2, -4 },
                    "Jazz" => new double[] { 2, 4, 6, 4, 2, 0, -2, -4, -2, 0 },
                    _ => new double[10]
                };

                Array.Copy(presetValues, _equalizerSettings, presetValues.Length);
                ApplySettingsToSliders();
                SaveCurrentSettings();
            }
        }

        private void OnResetClicked(object sender, EventArgs e)
        {
            Array.Fill(_equalizerSettings, 0);
            ApplySettingsToSliders();
            SaveCurrentSettings();
        }

        private void OnSaveClicked(object sender, EventArgs e)
        {
            SaveCurrentSettings();
            DisplayAlert("Сохранено", "Настройки эквалайзера сохранены", "OK");
        }

        private void OnToggleClicked(object sender, ToggledEventArgs e)
        {
            _isEqualizerEnabled = e.Value;
            Preferences.Set("EqualizerEnabled", _isEqualizerEnabled);
            UpdateToggleState();
        }

        private void UpdateToggleState()
        {
            if (EqualizerSwitch != null)
            {
                EqualizerSwitch.IsToggled = _isEqualizerEnabled;
            }
        }

        private void LoadSettings()
        {
            _isEqualizerEnabled = Preferences.Get("EqualizerEnabled", false);

            if (Preferences.ContainsKey("EqualizerSettings"))
            {
                var savedSettings = Preferences.Get("EqualizerSettings", "");
                if (!string.IsNullOrEmpty(savedSettings))
                {
                    var values = savedSettings.Split(',').Select(double.Parse).ToArray();
                    Array.Copy(values, _equalizerSettings, Math.Min(values.Length, _equalizerSettings.Length));
                    ApplySettingsToSliders();
                }
            }
        }

        private void SaveCurrentSettings()
        {
            Preferences.Set("EqualizerSettings", string.Join(",", _equalizerSettings));
        }

        private void ApplySettingsToSliders()
        {
            if (EqualizerGrid?.Children == null) return;

            foreach (var child in EqualizerGrid.Children)
            {
                if (child is Slider slider &&
                    int.TryParse(slider.AutomationId, out int index) &&
                    index < _equalizerSettings.Length)
                {
                    slider.ValueChanged -= OnSliderValueChanged;
                    slider.Value = _equalizerSettings[index];
                    slider.ValueChanged += OnSliderValueChanged;
                }
            }
        }
    }
}