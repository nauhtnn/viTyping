using System;
using System.Collections.Generic;
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

namespace viTyping
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        public Page1()
        {
            InitializeComponent();
        }

        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            {
                TextRange r = new TextRange(tbxF1.Document.ContentStart,
                        tbxF1.Document.ContentEnd);
                //r.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.White);
                r.ApplyPropertyValue(RichTextBox.ForegroundProperty, Brushes.Black);
            }
            string txt = new TextRange(tbxF1.Document.ContentStart, tbxF1.Document.ContentEnd).Text;
            int HARD_IDX = 2;//hardcode
            int len = Math.Min(txt.Length, txtF0.Text.Length) - HARD_IDX;
            for (int i = 0; i < len; ++i)
                if (txt[i] != txtF0.Text[i])
                {
                    TextRange r = new TextRange(tbxF1.Document.ContentStart.GetPositionAtOffset(i + HARD_IDX),
                        tbxF1.Document.ContentStart.GetPositionAtOffset(i + HARD_IDX + 1));
                    string x = r.Text;
                    if (r.Text.CompareTo(" ") == 0 || r.Text.Length == 0)
                        r.Text = "#";
                    //r.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Red);
                    r.ApplyPropertyValue(RichTextBox.ForegroundProperty, Brushes.Red);
                    return;
                }
            if (txt.Length < txtF0.Text.Length)
            {
                TextRange r = new TextRange(tbxF1.Document.ContentEnd,
                        tbxF1.Document.ContentEnd);
                r.Text = "#";
                //r.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Red);
                r.ApplyPropertyValue(RichTextBox.ForegroundProperty, Brushes.Red);
            }
            else if(txt.Length < txtF0.Text.Length + 3)//hardcode
            {
                MessageBox.Show("Bạn đã nhập văn bản đúng!", "Xin chúc mừng!");
                btnExit.IsEnabled = true;
            }
            else
            {
                TextRange r = new TextRange(tbxF1.Document.ContentStart.GetPositionAtOffset(len + HARD_IDX),
                        tbxF1.Document.ContentEnd);
                string x = r.Text;
                if (r.Text.CompareTo(" ") == 0 || r.Text.Length == 0)
                    r.Text = "#";
                //r.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Red);
                r.ApplyPropertyValue(RichTextBox.ForegroundProperty, Brushes.Red);
            }
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(this);
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.ResizeMode = ResizeMode.NoResize;

            if(System.IO.File.Exists("conf.txt"))
            {
                string[] conf = System.IO.File.ReadAllLines("conf.txt");
                if(0 < conf.Length)
                    tbxF1.FontSize = txtF0.FontSize = int.Parse(conf[0]);
                if (1 < conf.Length && System.IO.File.Exists(conf[1]))
                {
                    BitmapImage src = new BitmapImage();
                    src.BeginInit();
                    src.UriSource = new Uri(conf[1], UriKind.Relative);
                    src.CacheOption = BitmapCacheOption.OnLoad;
                    src.EndInit();

                    img.Source = src;
                }
            }

            if (System.IO.File.Exists("f0.txt"))
                txtF0.Text = System.IO.File.ReadAllText("f0.txt");

            if (System.IO.File.Exists("f1.txt"))
            {
                TextRange r = new TextRange(tbxF1.Document.ContentEnd,
                        tbxF1.Document.ContentEnd);
                r.Text = System.IO.File.ReadAllText("f1.txt");
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            System.IO.File.WriteAllText("f1.txt", new TextRange(tbxF1.Document.ContentStart, tbxF1.Document.ContentEnd).Text);
        }
    }
}
