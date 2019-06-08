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
using System.IO;

namespace viTyping
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
		DateTime StartTime;
		TimeSpan TestDuration;
        TimeSpan RemainingTime;
		System.Timers.Timer mTimer;
		bool bRunning = true;
        const string TEST_FOLDER = "test/";
        const string PROGRESS_SAVE_FILE = "sav.txt";

        int CurrentTest = -1;
		
        public Page1()
        {
            InitializeComponent();
        }

        /*private void btnCheck_Click(object sender, RoutedEventArgs e)
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
				btnCheck.Content = "10 điểm";
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
        }*/
        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            if(Grading().CompareTo("10") == 0)
            {
                bRunning = false;
                //btnExit.IsEnabled = true;
                tbxF1.IsEnabled = false;
                MessageBox.Show("Xin chúc mừng!", "Bạn đã nhập văn bản đúng!");
                //btnCheck.Content = "10 điểm";
                AppendGrade("10 điểm");
                //System.IO.File.WriteAllText("f1.txt", tbxF1.Text);
                UpdateCurrentTestID();
                SaveCurrentTestID();
                LoadTest();
            }
            else
            {
                MessageBox.Show("Bạn chưa nhập văn bản đúng theo mẫu!", "Thông báo!");
            }
        }

        private void AppendGrade(string txt)
        {
            // This text is always added, making the file longer over time
            // if it is not deleted.
            using (StreamWriter sw = File.AppendText("f2.txt"))
            {
                sw.WriteLine(txt + " at " + DateTime.Now.ToShortDateString() +
                    " " + DateTime.Now.Hour +
                    ":" + DateTime.Now.Minute);
            }
        }

        //private void LoadConfig()
        //{
        //    int min = 15,
        //        sec = 0;
        //    if (File.Exists("conf.txt"))
        //    {
        //        string[] conf = File.ReadAllLines("conf.txt");
        //        if (0 < conf.Length)
        //            tbxF1.FontSize = txtF0.FontSize = int.Parse(conf[0]);
        //        if (1 < conf.Length && File.Exists(conf[1]))
        //        {
                    
        //        }
        //        if (2 < conf.Length)
        //            min = int.Parse(conf[2]);
        //        if (3 < conf.Length)
        //            sec = int.Parse(conf[3]);
        //    }
        //}

        private void InitTimer()
        {
            StartTime = DateTime.Now;
            mTimer = new System.Timers.Timer(1000);
            mTimer.Elapsed += UpdateSrvrMsg;
            mTimer.AutoReset = true;
            mTimer.Enabled = true;
        }

        private void SaveCurrentTestID()
        {
            if (CurrentTest < 0)
                CurrentTest = 0;
            File.WriteAllText(PROGRESS_SAVE_FILE, CurrentTest.ToString());
        }

        private void UpdateCurrentTestID()
        {
            if (CurrentTest < 0)
            {
                if (File.Exists(PROGRESS_SAVE_FILE))
                    CurrentTest = int.Parse(File.ReadAllText(PROGRESS_SAVE_FILE));
                else
                    CurrentTest = 0;
            }
            else
                ++CurrentTest;
        }

        private SortedDictionary<string, string> LoadTestConfigs()
        {
            if(File.Exists(PROGRESS_SAVE_FILE))
                CurrentTest = int.Parse(File.ReadAllText(PROGRESS_SAVE_FILE));

            if (CurrentTest < 0)
                CurrentTest = 0;

            string testPath = TEST_FOLDER + CurrentTest + ".txt";

            SortedDictionary<string, string> testConfigs = new SortedDictionary<string, string>();

            if (File.Exists(testPath))
            {
                StringBuilder text = new StringBuilder();
                foreach (string line in File.ReadAllLines(testPath))
                {
                    string[] tokens = line.Split('\t');
                    if (tokens.Length == 2)
                        testConfigs.Add(tokens[0], tokens[1]);
                    else if(tokens.Length == 1)
                    {
                        text.Append(tokens[0] + "\r\n");
                    }
                }
                char[] trimChars = { '\r', '\n', ' ', '\t' };
                testConfigs.Add(CFG.TEXT.ToString(), text.ToString().Trim(trimChars));
            }
            if(!testConfigs.ContainsKey(CFG.DURATION_MINUTE.ToString()))
                testConfigs.Add(CFG.DURATION_MINUTE.ToString(), "10");
            if (!testConfigs.ContainsKey(CFG.DURATION_SECOND.ToString()))
                testConfigs.Add(CFG.DURATION_SECOND.ToString(), "0");
            if (!testConfigs.ContainsKey(CFG.PICTURE.ToString()))
                testConfigs.Add(CFG.PICTURE.ToString(), "default_picture.png");
            if (!testConfigs.ContainsKey(CFG.FONT_SIZE.ToString()))
                testConfigs.Add(CFG.FONT_SIZE.ToString(), "14");
            if (!testConfigs.ContainsKey(CFG.TEXT.ToString()))
                testConfigs.Add(CFG.TEXT.ToString(), "asdfghjkl;");
            return testConfigs;
        }

        private void LoadTest()
        {
            SortedDictionary<string, string> testConfigs = LoadTestConfigs();

            int minute = int.Parse(testConfigs[CFG.DURATION_MINUTE.ToString()]);
            int second = int.Parse(testConfigs[CFG.DURATION_SECOND.ToString()]);
            TestDuration = new TimeSpan(0, minute, second);
            RemainingTime = new TimeSpan(0, minute, second);
            txtRTime.Text = "" + RemainingTime.Minutes + " : " + RemainingTime.Seconds;

            txtF0.Text = testConfigs[CFG.TEXT.ToString()];
            txtF0.FontSize = int.Parse(testConfigs[CFG.FONT_SIZE.ToString()]);
            tbxF1.FontSize = txtF0.FontSize;

            if(File.Exists(TEST_FOLDER + testConfigs[CFG.PICTURE.ToString()]))
            {
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(TEST_FOLDER + testConfigs[CFG.PICTURE.ToString()], UriKind.Relative);
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();
                CenterPicture.Source = src;
            }

            txtTestID.Text = "Bài " + (CurrentTest + 1);
            
            tbxF1.Text = "";
            tbxF1.IsEnabled = true;
            bRunning = true;

            //if (File.Exists("f1.txt"))
            //{
            //    //TextRange r = new TextRange(tbxF1.Document.ContentEnd,
            //    //        tbxF1.Document.ContentEnd);
            //    //r.Text = System.IO.File.ReadAllText("f1.txt");
            //    tbxF1.Text = File.ReadAllText("f1.txt");
            //}
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(this);
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.ResizeMode = ResizeMode.NoResize;

            LoadTest();

            InitTimer();
        }

        //private void btnExit_Click(object sender, RoutedEventArgs e)
        //{
        //    Window.GetWindow(this).Close();
        //}

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //System.IO.File.WriteAllText("f1.txt", new TextRange(tbxF1.Document.ContentStart, tbxF1.Document.ContentEnd).Text);
            System.IO.File.WriteAllText("f1.txt", tbxF1.Text);
        }
		
		private void UpdateSrvrMsg(object source, System.Timers.ElapsedEventArgs e)
        {
            if (bRunning)
            {
                if (0 < RemainingTime.Ticks)
                {
                    RemainingTime = TestDuration - (DateTime.Now - StartTime);
                    Dispatcher.Invoke(() =>
                    {
                        txtRTime.Text = RemainingTime.Minutes.ToString() + " : " + RemainingTime.Seconds;
                    });
                }
                else
                {
                    RemainingTime = new TimeSpan(0, 0, 0);
					bRunning = false;
					//mTimer.Elapsed -= UpdateSrvrMsg;
					//mTimer.Enabled = false;
					Dispatcher.Invoke(() =>
                    {
                        txtRTime.Text = RemainingTime.Minutes.ToString() + " : " + RemainingTime.Seconds;
						tbxF1.IsEnabled = false;
						MessageBox.Show("Bạn chưa nhập văn bản đúng theo mẫu!", "Hết giờ!");
                        //string txt = Grading() + " điểm";
                        //btnCheck.Content = txt;
						//btnExit.IsEnabled = true;
                        //AppendGrade(txt);
                        //System.IO.File.WriteAllText("f1.txt", tbxF1.Text);
                    });
                }
            }
        }
		
		private string Grading()
		{
            //string txt = new TextRange(tbxF1.Document.ContentStart, tbxF1.Document.ContentEnd).Text;
            char[] trimChars = { '\r', '\n', ' ', '\t' };
            string txt = tbxF1.Text.Trim(trimChars);
			int l = Levenshtein(txt, txtF0.Text);
            return Math.Round(10.0f - 10.0f * l / txtF0.Text.Length, 1).ToString();
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
