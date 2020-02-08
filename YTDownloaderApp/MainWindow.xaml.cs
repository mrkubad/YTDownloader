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
using YTDownloaderApp.Controls;
using YTDownloaderCore.Regexes;
using YTDownloaderCore.Downloader;
using YTDownloaderCore.Models;
using System.Collections.ObjectModel;

namespace YTDownloaderApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<VideoModel> Models { get; set; }
        public ICommand WindowMouseMove { get; set; }
        public ICommand ListViewDoubleClick { get; set; }
        public ICommand ListViewSelectionChanged { get; set; }
        private Downloader Downloader;

        private string PreviousValidUrl;
        public MainWindow()
        {
            InitializeComponent();
            Models = new ObservableCollection<VideoModel>();
            DataContext = this;
            // WebClient first download is slow, so download small site to improve speed of download next things
            // It is async to not block window creation
            Downloader = new Downloader();
            Downloader.DownloadStringTaskAsync("http://www.nbp.pl/kursy/xml/");

            WindowMouseMove = new RelayCommand(o =>
            {
                if(Clipboard.ContainsText())
                {
                    string clipboardContent = Clipboard.GetText();
                    if(RegexLibrary.YoutubeUrl.IsMatch(clipboardContent))
                    {
                        if (string.Compare(PreviousValidUrl, clipboardContent) != 0)
                        {
                            PreviousValidUrl = clipboardContent;
                            var pruba = new VideoModel(RegexLibrary.YoutubeUrl.Match(clipboardContent).Groups["VideoId"].ToString());
                            pruba.ParseEnd += VideoIsReady;
                            pruba.ParseVideoInformationAsync();
                            // TODO: We need to create object here representing item on LV
                        }
                    }
                }
            });
            ListViewDoubleClick = new RelayCommand(o => {
                Debug.WriteLine(o);
                Debug.WriteLine("Działa z komanda!!");
            
            });
            ListViewSelectionChanged = new RelayCommand(o =>
            {
                Debug.WriteLine(o);
                Debug.WriteLine("Command SelectionChanged");
            });
            
            //lv.ItemsSource = new List<string> { "dupa", "sraka", "3", "4", "5", "6", "7" };
        }
        private void VideoIsReady(object sender, EventArgs e)
        {
            Models.Add((VideoModel)sender);
        }
    }
}
