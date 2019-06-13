﻿using System;
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
    public partial class Page1: Page, ProblemLoader
    {
		DateTime StartTime;
		TimeSpan TestDuration;
        TimeSpan RemainingTime;
		System.Timers.Timer mTimer;
		bool bRunning = true;
        const string TEST_FOLDER = "blank/";
        const string PROGRESS_SAVE_FILE = TEST_FOLDER + "blank.txt";
        static List<TextRange> formattedRanges = new List<TextRange>();

        int CurrentTest = -1;
		
        public Page1()
        {
            InitializeComponent();
        }
        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            HighlightPlainTextDiff(UserText.Document, TargetText.Text.Replace("\n", "").ToCharArray());
            //if(Grading().CompareTo("10") == 0)
            //{
            //    bRunning = false;
            //    //btnExit.IsEnabled = true;
            //    UserText.IsEnabled = false;
            //    MessageBox.Show("Xin chúc mừng!", "Bạn đã nhập văn bản đúng!");
            //    //btnCheck.Content = "10 điểm";
            //    AppendGrade("10 điểm");
            //    //System.IO.File.WriteAllText("f1.txt", UserText.Text);
            //    UpdateCurrentTestID();
            //    SaveCurrentTestID();
            //    ParseData(0, 0);
            //}
            //else
            //{
            //    MessageBox.Show("Bạn chưa nhập văn bản đúng theo mẫu!", "Thông báo!");
            //}
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

        public static void HighlightPlainTextDiff(FlowDocument document, char[] s)
        {
            //Console.WriteLine("-----------------------");
            int s_i = 0;
            TextPointer pointer = document.ContentStart;
            bool IsBlankLine = false;
            while (pointer != null)
            {
                //Console.WriteLine(pointer.GetPointerContext(LogicalDirection.Forward));
                TextPointerContext context = pointer.GetPointerContext(LogicalDirection.Forward);
                if (context == TextPointerContext.Text)
                {
                    IsBlankLine = false;
                    char[] textRun = pointer.GetTextInRun(LogicalDirection.Forward).ToCharArray();
                    int i = 0;
                    for (; i < textRun.Length && s_i < s.Length; ++i, ++s_i)
                    {
                        if (textRun[i] != s[s_i])
                        {
                            TextRange range = new TextRange(pointer.GetPositionAtOffset(i),
                                pointer.GetPositionAtOffset(textRun.Length));
                            range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Blue);
                            formattedRanges.Add(range);
                            return;
                        }
                    }
                }
                else if(context == TextPointerContext.ElementStart)
                {
                    IsBlankLine = true;
                }
                else if(context == TextPointerContext.ElementEnd && IsBlankLine)
                {
                    pointer.InsertTextInRun("     ");
                    TextRange range = new TextRange(pointer.Paragraph.ElementStart,
                                pointer.Paragraph.ElementEnd);
                    range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Blue);
                    formattedRanges.Add(range);
                    return;
                }

                pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
            }
        }

        public SortedDictionary<string, string> LoadData()
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
                        text.Append(tokens[0] + "\n");
                    }
                }
                char[] trimChars = { '\r', '\n', ' ', '\t' };
                testConfigs.Add(CFG.TEXT0.ToString(), text.ToString().Trim(trimChars));
            }
            if(!testConfigs.ContainsKey(CFG.DURATION_MINUTE.ToString()))
                testConfigs.Add(CFG.DURATION_MINUTE.ToString(), "10");
            if (!testConfigs.ContainsKey(CFG.DURATION_SECOND.ToString()))
                testConfigs.Add(CFG.DURATION_SECOND.ToString(), "0");
            if (!testConfigs.ContainsKey(CFG.PICTURE.ToString()))
                testConfigs.Add(CFG.PICTURE.ToString(), "default_picture.png");
            if (!testConfigs.ContainsKey(CFG.FONT_SIZE.ToString()))
                testConfigs.Add(CFG.FONT_SIZE.ToString(), "14");
            if (!testConfigs.ContainsKey(CFG.TEXT0.ToString()))
                testConfigs.Add(CFG.TEXT0.ToString(), "asdfghjkl;");
            return testConfigs;
        }

        public void ParseData(int level, int subID)
        {
            SortedDictionary<string, string> testConfigs = LoadData();

            int minute = int.Parse(testConfigs[CFG.DURATION_MINUTE.ToString()]);
            int second = int.Parse(testConfigs[CFG.DURATION_SECOND.ToString()]);
            TestDuration = new TimeSpan(0, minute, second);
            RemainingTime = new TimeSpan(0, minute, second);
            txtRTime.Text = "" + RemainingTime.Minutes + " : " + RemainingTime.Seconds;

            TargetText.Text = testConfigs[CFG.TEXT0.ToString()];
            LineIdx0.FontSize = LineIdx1.FontSize = UserText.FontSize =
                TargetText.FontSize = int.Parse(testConfigs[CFG.FONT_SIZE.ToString()]);
            

            //count line
            int n_lines = TargetText.Text.Count((char c) => { return c == '\n'; }) + 1;
            StringBuilder line_idx = new StringBuilder();
            for (int i = 1; i <= n_lines; ++i)
                line_idx.Append(i.ToString() + "\n");
            LineIdx0.Text = line_idx.ToString();
            LineIdx1.Text = line_idx.ToString();

            if (File.Exists(TEST_FOLDER + testConfigs[CFG.PICTURE.ToString()]))
            {
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(TEST_FOLDER + testConfigs[CFG.PICTURE.ToString()], UriKind.Relative);
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();
                TestPicture.Source = src;
            }

            TestDescription.Text = "Bài " + (CurrentTest + 1);

            UserText.Document.Blocks.Clear();
            UserText.IsEnabled = true;
            bRunning = true;

            //if (File.Exists("f1.txt"))
            //{
            //    //TextRange r = new TextRange(UserText.Document.ContentEnd,
            //    //        UserText.Document.ContentEnd);
            //    //r.Text = System.IO.File.ReadAllText("f1.txt");
            //    UserText.Text = File.ReadAllText("f1.txt");
            //}
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(this);
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.ResizeMode = ResizeMode.NoResize;

            ParseData(0,0);

            InitTimer();
        }

        //private void btnExit_Click(object sender, RoutedEventArgs e)
        //{
        //    Window.GetWindow(this).Close();
        //}
		
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
						UserText.IsEnabled = false;
						MessageBox.Show("Bạn chưa nhập văn bản đúng theo mẫu!", "Hết giờ!");
                        //string txt = Grading() + " điểm";
                        //btnCheck.Content = txt;
						//btnExit.IsEnabled = true;
                        //AppendGrade(txt);
                        //System.IO.File.WriteAllText("f1.txt", UserText.Text);
                    });
                }
            }
        }
		
		//private string Grading()
		//{
  //          //string txt = new TextRange(UserText.Document.ContentStart, UserText.Document.ContentEnd).Text;
  //          char[] trimChars = { '\r', '\n', ' ', '\t' };
  //          string txt = UserText.Text.Trim(trimChars);
		//	int l = Levenshtein(txt, TargetText.Text);
  //          return Math.Round(10.0f - 10.0f * l / TargetText.Text.Length, 1).ToString();
		//}
		
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

        private void UserText_GotFocus(object sender, RoutedEventArgs e)
        {
            foreach(TextRange i in formattedRanges)
            {
                i.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.White);
                if (i.Text == "     ")
                    i.Text = "";
            }
            formattedRanges.Clear();
        }
    }
}
