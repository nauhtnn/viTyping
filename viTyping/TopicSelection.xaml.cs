using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.IO;

namespace viTyping
{
    /// <summary>
    /// Interaction logic for TopicSelection.xaml
    /// </summary>
    public partial class TopicSelection : Page
    {
        public TopicSelection()
        {
            InitializeComponent();
        }

        void LoadTopics()
        {
            string[] topics = Directory.GetDirectories(Directory.GetCurrentDirectory());
            for (int i = 0; i < topics.Length; ++i)
                topics[i] = topics[i].Substring(Directory.GetCurrentDirectory().Length);
            for (int i = 0; i < topics.Length; ++i)
                topics[i] = topics[i].Replace("\\", "").Replace("/", "").ToUpper();

            if (File.Exists("topics.txt"))
            {
                Dictionary<string, string> dirs_2_topics = new Dictionary<string, string>();
                foreach (string s in File.ReadAllLines("topics.txt"))
                {
                    string[] key_val = s.Split('\t');
                    if (key_val.Length == 2)
                        dirs_2_topics.Add(key_val[0].ToUpper(), key_val[1].ToUpper());
                }

                for (int i = 0; i < topics.Length; ++i)
                    if (dirs_2_topics.ContainsKey(topics[i]))
                        topics[i] = dirs_2_topics[topics[i]];
            }

            foreach(string s in topics)
            {
                Button b = new Button();
                b.Content = s;
                b.FontSize = 24;
                b.Width = 192;
                b.Height = 64;
                b.Margin = new Thickness(0, 10, 0, 0);
                b.Background = Brushes.Coral;
                b.Click += Button_Click;
                MainGUI.Children.Add(b);
            }
        }

        private void Viewbox_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(this);
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.ResizeMode = ResizeMode.NoResize;

            LoadTopics();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            Page1 p = new Page1();
            p.profile.SetPath(Directory.GetCurrentDirectory() + "\\" + b.Content.ToString().ToLower() + "\\");
            NavigationService.Navigate(p, UriKind.Relative);
        }
    }
}
