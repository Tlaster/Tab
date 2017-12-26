using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Tab.Sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<SampleViewModel> _items;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
            var random = new Random();
            _items = new ObservableCollection<SampleViewModel>(new[]
            {
                new SampleViewModel(random.Next().ToString(), random.Next().ToString()),
                new SampleViewModel(random.Next().ToString(), random.Next().ToString()),
                new SampleViewModel(random.Next().ToString(), random.Next().ToString()),
                new SampleViewModel(random.Next().ToString(), random.Next().ToString()),
                new SampleViewModel(random.Next().ToString(), random.Next().ToString()),
            });
            TabControl.ItemsSource = _items;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void TabControl_OnAddRequest(object sender, EventArgs e)
        {
            _items.Add(new SampleViewModel("aaa", "bbbb"));
        }
    }

    public class SampleViewModel
    {
        public SampleViewModel(string title, string text)
        {
            Title = title;
            Text = text;
        }

        public string Title { get; }
        public string Text { get; }
    }
}
