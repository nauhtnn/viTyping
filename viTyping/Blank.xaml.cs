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
using ProfileLibrary;

namespace viTyping
{
    enum PLAIN_PROBLEM_FORMAT
    {
        PROBLEM_DESC,
        DURATION_MINUTE,
        DURATION_SECOND,
        FONT_SIZE,
        PICTURE,
        TEXT0,
        TEXT1
    };

    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1: Page
    {
		DateTime StartTime;
		TimeSpan TestDuration;
        TimeSpan RemainingTime;
		System.Timers.Timer mTimer;
		bool bRunning = true;
        static TextRange HighlightRange = null;
        
        public Profile profile = new Profile();

        public Page1()
        {
            InitializeComponent();
        }
        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            if(!HighlightPlainTextDiff(UserText.Document, TargetText.Text.Replace("\n", "").ToCharArray(), UserText))
            {
                bRunning = false;
                //btnExit.IsEnabled = true;
                UserText.IsEnabled = false;
                MessageBox.Show("Xin chúc mừng!", "Bạn đã nhập văn bản đúng!");
                //btnCheck.Content = "10 điểm";
                AppendGrade("10 điểm");
                //System.IO.File.WriteAllText("f1.txt", UserText.Text);
                profile.NextProblem();
                profile.Save();
                ParseProblem(LoadProblem());
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

        private void InitTimer()
        {
            StartTime = DateTime.Now;
            mTimer = new System.Timers.Timer(1000);
            mTimer.Elapsed += UpdateSrvrMsg;
            mTimer.AutoReset = true;
            mTimer.Enabled = true;
        }

        private static int SearchUnmatchingWord(char[] text, int i)
        {
            if(text[i] == ' ' || text[i] == '\r' ||
                text[i] == '\n')
            {
                while (i < text.Length && (text[i] == ' ' || text[i] == '\r' ||
                    text[i] == '\n'))
                    ++i;
            }
            else while (i < text.Length && text[i] != ' ' && text[i] != '\r' &&
                    text[i] != '\n')
                    ++i;

            return i;
        }

        private static bool HighlightPlainTextDiff(FlowDocument document, char[] s, RichTextBox rtb)
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
                            int unmatching_word = SearchUnmatchingWord(textRun, i);
                            rtb.Selection.Select(pointer.GetPositionAtOffset(i),
                                pointer.GetPositionAtOffset(unmatching_word));
                            rtb.Focus();
                            return true;
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
                    HighlightRange = new TextRange(pointer.Paragraph.ElementStart,
                                pointer.Paragraph.ElementEnd);
                    HighlightRange.ApplyPropertyValue(TextElement.BackgroundProperty,
                        Brushes.Gray); //new SolidColorBrush(SysColor));
                    //rtb.Selection.Select(HighlightRange.Start, HighlightRange.End);
                    //rtb.Focus();
                    return true;
                }

                pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
            }

            if(s_i < s.Length)
            {
                HighlightRange = new TextRange(document.ContentEnd,
                                document.ContentEnd);
                HighlightRange.Text = "     ";
                HighlightRange.ApplyPropertyValue(TextElement.BackgroundProperty,
                    Brushes.Gray);//new SolidColorBrush(SysColor));
                //rtb.Selection.Select(HighlightRange.Start, HighlightRange.End);
                //rtb.Focus();
                return true;
            }
            return false;
        }

        public SortedDictionary<string, string> LoadProblem()
        {
            SortedDictionary<string, string> data = new SortedDictionary<string, string>();

            if (File.Exists(profile.CurrentProblemPath()))
            {
                StringBuilder text0 = new StringBuilder();
                StringBuilder text1 = new StringBuilder();
                bool isText0 = true;
                foreach (string line in File.ReadAllLines(profile.CurrentProblemPath()))
                {
                    string[] tokens = line.Split('\t');
                    if (tokens.Length == 2)
                        data.Add(tokens[0], tokens[1]);
                    else if (tokens.Length == 1)
                    {
                        if (tokens[0].Contains("<<<<>>>>"))
                            isText0 = false;
                        else if(isText0)
                            text0.Append(tokens[0] + "\n");
                        else
                            text1.Append(tokens[0] + "\n");
                    }
                }
                char[] trimChars = { '\r', '\n', ' ', '\t' };
                data.Add(PLAIN_PROBLEM_FORMAT.TEXT0.ToString(), text0.ToString().Trim(trimChars));
                data.Add(PLAIN_PROBLEM_FORMAT.TEXT1.ToString(), text1.ToString().Trim(trimChars));
            }
            if (!data.ContainsKey(PLAIN_PROBLEM_FORMAT.DURATION_MINUTE.ToString()))
                data.Add(PLAIN_PROBLEM_FORMAT.DURATION_MINUTE.ToString(), "10");
            if (!data.ContainsKey(PLAIN_PROBLEM_FORMAT.DURATION_SECOND.ToString()))
                data.Add(PLAIN_PROBLEM_FORMAT.DURATION_SECOND.ToString(), "0");
            if (!data.ContainsKey(PLAIN_PROBLEM_FORMAT.PICTURE.ToString()))
                data.Add(PLAIN_PROBLEM_FORMAT.PICTURE.ToString(), "default_picture.png");
            if (!data.ContainsKey(PLAIN_PROBLEM_FORMAT.FONT_SIZE.ToString()))
                data.Add(PLAIN_PROBLEM_FORMAT.FONT_SIZE.ToString(), "14");
            if (!data.ContainsKey(PLAIN_PROBLEM_FORMAT.TEXT0.ToString()))
                data.Add(PLAIN_PROBLEM_FORMAT.TEXT0.ToString(), "asdfghjkl;");
            if (!data.ContainsKey(PLAIN_PROBLEM_FORMAT.TEXT1.ToString()))
                data.Add(PLAIN_PROBLEM_FORMAT.TEXT1.ToString(), "");
            return data;
        }

        public void ParseProblem(SortedDictionary<string, string> data)
        {
             data = LoadProblem();

            int minute = int.Parse(data[PLAIN_PROBLEM_FORMAT.DURATION_MINUTE.ToString()]);
            int second = int.Parse(data[PLAIN_PROBLEM_FORMAT.DURATION_SECOND.ToString()]);
            TestDuration = new TimeSpan(0, minute, second);
            RemainingTime = new TimeSpan(0, minute, second);
            txtRTime.Text = "" + RemainingTime.Minutes + " : " + RemainingTime.Seconds;

            TestDescription.Text = "\tBài " + (profile.CurrentProblemID + 1) + ": " +
                data[PLAIN_PROBLEM_FORMAT.PROBLEM_DESC.ToString()];

            UserText.Document.Blocks.Clear();
            UserText.Document.ContentStart.InsertTextInRun(data[PLAIN_PROBLEM_FORMAT.TEXT1.ToString()]);
            UserText.IsEnabled = true;

            TargetText.Text = data[PLAIN_PROBLEM_FORMAT.TEXT0.ToString()];
            LineIdx0.FontSize = LineIdx1.FontSize = UserText.FontSize =
                TargetText.FontSize = int.Parse(data[PLAIN_PROBLEM_FORMAT.FONT_SIZE.ToString()]);

            //count line
            int n_lines = TargetText.Text.Count((char c) => { return c == '\n'; }) + 1;
            StringBuilder line_idx = new StringBuilder();
            for (int i = 1; i <= n_lines; ++i)
                line_idx.Append(i.ToString() + "\n");
            LineIdx0.Text = line_idx.ToString();
            LineIdx1.Text = line_idx.ToString();

            if (File.Exists(profile.FolderPath + data[PLAIN_PROBLEM_FORMAT.PICTURE.ToString()]))
            {
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(profile.FolderPath + data[PLAIN_PROBLEM_FORMAT.PICTURE.ToString()], UriKind.Relative);
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();
                TestPicture.Source = src;
            }

            bRunning = true;
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(this);
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.ResizeMode = ResizeMode.NoResize;

            if (profile.FolderPath == null)
                profile.SetPath(Directory.GetCurrentDirectory() + "\\" + "plain\\");

            ParseProblem(LoadProblem());

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
            if(HighlightRange != null)
                HighlightRange.Text = "";
        }
    }
}
