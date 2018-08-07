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
		DateTime kDtStart;
		TimeSpan kDtDur;
        TimeSpan dtRemn;
		System.Timers.Timer mTimer;
		bool bRunning = true;
		
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
				bRunning = false;
				btnExit.IsEnabled = true;
				tbxF1.IsEnabled = false;
                MessageBox.Show("Bạn đã nhập văn bản đúng!", "Xin chúc mừng!");
				btnCheck.Content = "10 điểm";            }
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

			int min = 15;
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
				if(2 < conf.Length)
					min = int.Parse(conf[2]);
            }

            if (System.IO.File.Exists("f0.txt"))
                txtF0.Text = System.IO.File.ReadAllText("f0.txt");

            if (System.IO.File.Exists("f1.txt"))
            {
                TextRange r = new TextRange(tbxF1.Document.ContentEnd,
                        tbxF1.Document.ContentEnd);
                r.Text = System.IO.File.ReadAllText("f1.txt");
            }
			
			kDtStart = DateTime.Now;
			mTimer = new System.Timers.Timer(1000);
            mTimer.Elapsed += UpdateSrvrMsg;
            mTimer.AutoReset = true;
            mTimer.Enabled = true;
			
			kDtDur = new TimeSpan(0, min, 0);
			dtRemn = new TimeSpan(0, min, 0);
			txtRTime.Text = "" + dtRemn.Minutes + " : " + dtRemn.Seconds;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            System.IO.File.WriteAllText("f1.txt", new TextRange(tbxF1.Document.ContentStart, tbxF1.Document.ContentEnd).Text);
        }
		
		private void UpdateSrvrMsg(object source, System.Timers.ElapsedEventArgs e)
        {
            if (bRunning)
            {
                if (0 < dtRemn.Ticks)
                {
                    dtRemn = kDtDur - (DateTime.Now - kDtStart);
                    Dispatcher.Invoke(() =>
                    {
                        txtRTime.Text = dtRemn.Minutes.ToString() + " : " + dtRemn.Seconds;
                    });
                }
                else
                {
                    dtRemn = new TimeSpan(0, 0, 0);
					bRunning = false;
					mTimer.Elapsed -= UpdateSrvrMsg;
					mTimer.Enabled = false;
					Dispatcher.Invoke(() =>
                    {
                        txtRTime.Text = dtRemn.Minutes.ToString() + " : " + dtRemn.Seconds;
						tbxF1.IsEnabled = false;
						MessageBox.Show("Bạn chưa nhập văn bản xong!", "Hết giờ!");
						btnCheck.Content = Grading() + " điểm";
						btnExit.IsEnabled = true;
                    });
                }
            }
        }
		
		private string Grading()
		{
			string txt = new TextRange(tbxF1.Document.ContentStart, tbxF1.Document.ContentEnd).Text;
			int l = Levenshtein(txt, txtF0.Text);
            return "" + l + "=" + Math.Round(10.0f - 10.0f * l / txtF0.Text.Length, 1).ToString();
		}
		
		public int Levenshtein(string s, string t)
		{
			int n = s.Length;
			int m = t.Length;
			int[,] d = new int[n + 1, m + 1];

			// Step 1
			if (n == 0)
			{
				return m;
			}

			if (m == 0)
			{
				return n;
			}

			// Step 2
			for (int i = 0; i <= n; d[i, 0] = i++)
			{
			}

			for (int j = 0; j <= m; d[0, j] = j++)
			{
			}

			// Step 3
			for (int i = 1; i <= n; i++)
			{
				//Step 4
				for (int j = 1; j <= m; j++)
				{
					// Step 5
					int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

					// Step 6
					d[i, j] = Math.Min(
						Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
						d[i - 1, j - 1] + cost);
				}
			}
			// Step 7
			return d[n, m];
		}
    }
}
