using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FFME_multiple
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string BaseDirectory = @"C:\Users\UnoSp\OneDrive\ffme-testsuite\";
        private readonly string[] Urls = new string[]
               {
            $"{BaseDirectory}video-elephants-loq-01.mp4",
            $"{BaseDirectory}audio-sync-test-01.mp4",
            $"{BaseDirectory}audio-sync-test-02.mp4",
            $"{BaseDirectory}audio-sync-test-03.mp4",
            $"{BaseDirectory}video-hd-01.mp4",
            $"{BaseDirectory}video-standard-01.mp4",
            $"{BaseDirectory}video-standard-02.mp4",
            $"{BaseDirectory}youtube-export-01.mp4",
            $"{BaseDirectory}video-subtitles-01.mkv",
            $"{BaseDirectory}video-subtitles-03.mkv",
            $"{BaseDirectory}youtube-export-02.mp4",
            $"{BaseDirectory}youtube-export-03.mp4",
               };

        MainViewModel VM;
        public MainWindow()
        {
            Unosquare.FFME.MediaElement.FFmpegDirectory = @"c:\ffmpeg"; InitializeComponent();
            Loaded += MainWindow_Loaded;
            VM = DataContext as MainViewModel;
            VM.SliderChanged += VM_SliderChanged;
            
        }
  
        private void VM_SliderChanged(object? sender, EventArgs e)
        {
            string msg = $"{DateTime.Now} Slider changed {VM.TimelineSliderValue} of {VM.DurationMs}";
            Debug.WriteLine(msg);
            Dispatcher.Invoke(() => Messages.Items.Add(msg));

            try
            {
                Seek(TimeSpan.FromSeconds(VM.TimelineSliderValue));
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "Failed to seek");
            }
        }

        private async void mePlayer_MediaEnded(object sender, EventArgs e)
        {
            Unosquare.FFME.MediaElement player = sender as Unosquare.FFME.MediaElement;
            if (player != null)
            {
                string msg = $"{DateTime.Now} Ended: Restarting play for {player.Source}";
                Debug.WriteLine(msg);
                Dispatcher.Invoke(() => Messages.Items.Add(msg)); 
                //  await player.Open(player.Source);
                await player.Seek(new TimeSpan());
                await player.Play();
            }
        }
        private async void mePlayer_MediaClosed(object sender, EventArgs e)
        {
            Unosquare.FFME.MediaElement player = sender as Unosquare.FFME.MediaElement;
            if (player != null)
            {
                string msg = $"{DateTime.Now} Closed: (NOT)Restarting play for {player.Source}";
                Debug.WriteLine(msg);
                Dispatcher.Invoke(() => Messages.Items.Add(msg));
             //   await player.Open(player.Source);
              //  await player.Play();
            }
        }
        Dictionary<Unosquare.FFME.MediaElement, int> fileIndices = new();
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            const int desiredInstences = 12;
            var maxInstances = Math.Min(desiredInstences, LayoutGrid.ColumnDefinitions.Count * LayoutGrid.RowDefinitions.Count);
            var instanceCount = 0;
            TimeSpan? max = TimeSpan.MinValue;
            for (var rowIndex = 0; rowIndex < LayoutGrid.RowDefinitions.Count; rowIndex++)
            {
                for (var colIndex = 0; colIndex < LayoutGrid.ColumnDefinitions.Count; colIndex++)
                {
                    if (instanceCount >= maxInstances)
                        continue;

                    var url = Urls[instanceCount];
                    if (!System.IO.File.Exists(url))
                    {
                        Debug.WriteLine($"{url} doesn't exist");
                    }
                    var player = new Unosquare.FFME.MediaElement
                    {
                        LoadedBehavior = Unosquare.FFME.Common.MediaPlaybackState.Play,

                        ScrubbingEnabled = true
                    };
                    player.MediaEnded += mePlayer_MediaEnded;
                    player.MediaClosed += mePlayer_MediaClosed;
                    fileIndices.Add(player, instanceCount);
                    await player.Open(new Uri(url));
                    if (player.PlaybackEndTime.Value > max)
                    {
                        max = player.PlaybackEndTime;
                    }
                    player.MediaOpening += (snd, ev) =>
                    {
                        ev.Options.IsAudioDisabled = true;

                        if (ev.Options.VideoStream == null)
                            return;

                        // Scaling
                        var maxHeight = 1080 / LayoutGrid.RowDefinitions.Count;
                        if (ev.Options.VideoStream.PixelHeight > maxHeight)
                            ev.Options.VideoFilter = $"scale={maxHeight}:-1";
                    };

                    player.SetValue(Grid.ColumnProperty, colIndex);
                    player.SetValue(Grid.RowProperty, rowIndex);

                    LayoutGrid.Children.Add(player);
                    instanceCount++;
                    //if (instanceCount > 3)
                    //    break;
                }
                VM.DurationMs = max.Value.TotalSeconds;
            }
        }

        private async void Pause_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => Messages.Items.Add($"{DateTime.Now} Pause Clicked"));
            int count = 0;
            foreach (var child in LayoutGrid.Children)
            {
                Unosquare.FFME.MediaElement player = child as Unosquare.FFME.MediaElement;
                if (player == null)
                    continue;
               
                await player.Pause();
                count++;
            }
            Dispatcher.Invoke(() => Messages.Items.Add($"{DateTime.Now} Paused {count} instances"));
        }

        private async void Play_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => Messages.Items.Add($"{DateTime.Now} Play Clicked"));
            int count = 0;
            foreach (var child in LayoutGrid.Children)
            {
                Unosquare.FFME.MediaElement player = child as Unosquare.FFME.MediaElement;
                if (player == null)
                    continue;
                await player.Play();
                count++;
            }
            Dispatcher.Invoke(() => Messages.Items.Add($"{DateTime.Now} Played {count} instances"));

        }
        Dictionary<Unosquare.FFME.MediaElement, int> resetCounts = new();
        private async void Seek(TimeSpan relativeTime)
        {
            int count = 0;

            Dispatcher.Invoke(() => Messages.Items.Add($"{DateTime.Now} Seek Clicked"));
            foreach (var child in LayoutGrid.Children)
            {
                Unosquare.FFME.MediaElement player = child as Unosquare.FFME.MediaElement;
                if (player == null)
                    continue;
                TimeSpan seekTime = player.PlaybackEndTime.Value / 2;
                if (relativeTime < player.PlaybackEndTime.Value)
                    seekTime = relativeTime;
                if (player.IsSeeking)
                {
                    Dispatcher.Invoke(() => Messages.Items.Add($"{DateTime.Now} Seek in progress {player.Source}"));
                    if (resetCounts.ContainsKey(player))
                    {
                        resetCounts[player]++;
                    }
                    else
                    {
                        resetCounts.Add(player, 0);
                    }
                    if (resetCounts[player] > 3)
                    {
                        await player.Open(new Uri(Urls[fileIndices[player]]));
                        Dispatcher.Invoke(() => Messages.Items.Add($"{DateTime.Now} Reopened {player.Source}"));

                    }
                    continue;
                }
                else if (resetCounts.ContainsKey(player))
                {
                    resetCounts[player] = 0;
                }
                await player.Seek(seekTime);
                count++;
            }
            Dispatcher.Invoke(() => Messages.Items.Add($"{DateTime.Now} Seek on {count} instances"));

        }
        private async void Seek_Click(object sender, RoutedEventArgs e)
        {
            Seek(TimeSpan.FromHours(99));
 
        }
    }
}
