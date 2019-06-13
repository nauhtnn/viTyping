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
using System.Text.RegularExpressions;

namespace viTyping
{
    /// <summary>
    /// Interaction logic for Modification.xaml
    /// </summary>
    public partial class Modification : Page, ProblemLoader
    {
        public Modification()
        {
            InitializeComponent();
        }

        const string TEST_FOLDER = "modification/";
        const string PROGRESS_SAVE_FILE = TEST_FOLDER + "modification.txt";

        int CurrentTest = -1;
        string x0;

        public SortedDictionary<string, string> LoadData()
        {
            if (File.Exists(PROGRESS_SAVE_FILE))
                CurrentTest = int.Parse(File.ReadAllText(PROGRESS_SAVE_FILE));

            if (CurrentTest < 0)
                CurrentTest = 0;

            string testPath = TEST_FOLDER + CurrentTest + ".txt";

            SortedDictionary<string, string> testConfigs = new SortedDictionary<string, string>();

            if (File.Exists(testPath))
            {
                StringBuilder text0 = new StringBuilder();
                StringBuilder text1 = new StringBuilder();
                bool isText0 = true;
                foreach (string line in File.ReadAllLines(testPath))
                {
                    string[] tokens = line.Split('\t');
                    if (tokens.Length == 2)
                        testConfigs.Add(tokens[0], tokens[1]);
                    else if (tokens.Length == 1)
                    {
                        if (tokens[0].Contains("<<<<>>>>"))
                            isText0 = false;
                        else if(isText0)
                            text0.Append(tokens[0] + "\r\n");
                        else
                            text1.Append(tokens[0] + "\r\n");
                    }
                }
                char[] trimChars = { '\r', '\n', ' ', '\t' };
                x0 = text0.ToString().Trim(trimChars);
                testConfigs.Add(CFG.TEXT0.ToString(), x0);
                testConfigs.Add(CFG.TEXT1.ToString(), text1.ToString().Trim(trimChars));
            }
            if (!testConfigs.ContainsKey(CFG.DURATION_MINUTE.ToString()))
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
            SortedDictionary<string, string> configs = LoadData();
            //UserText.Document.Blocks.Add(new Paragraph(new Run(configs[CFG.TEXT0.ToString()])));
            string text1 = configs[CFG.TEXT1.ToString()];
            Paragraph p = new Paragraph();
            foreach (string i in text1.Split('\n'))
                p.Inlines.Add(i);
            //bool color = false;
            //foreach(string s in text1.Split('_'))
            //{
            //    if (color)
            //    {
            //        Run r = new Run(s);
            //        r.Background = new SolidColorBrush(Colors.Blue);
            //        p.Inlines.Add(r);
            //    }
            //    else
            //    {
            //        p.Inlines.Add(s);
            //    }
            //    color = !color;
            //}            
            UserText.Document.Blocks.Add(p);
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(this);
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.ResizeMode = ResizeMode.NoResize;

            ParseData(0, 0);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HighlightPlainTextDiff(UserText.Document, x0.Replace("\n", "").ToCharArray());
            //string[] x = x0.Split(' ');
            ////x0.
            //IEnumerable<TextRange> wordRanges = GetAllWordRanges(UserText.Document);
            //StringBuilder sb = new StringBuilder();
            //foreach (TextRange wordRange in wordRanges)
            //{
            //    //if (wordRange.Text == x[i++])
            //    //{
            //    //    wordRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
            //    //}
            //    sb.Append(wordRange.Text + "__");
            //}
            //TestDescription.Text = sb.ToString();
        }

        public static void HighlightPlainTextDiff(FlowDocument document, char[] s)
        {
            int s_i = 0;
            TextPointer pointer = document.ContentStart;
            while (pointer != null)
            {
                Console.WriteLine(pointer.GetPointerContext(LogicalDirection.Forward));
                //Console.WriteLine(pointer.GetTextInRun(LogicalDirection.Forward));
                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    char[] textRun = pointer.GetTextInRun(LogicalDirection.Forward).ToCharArray();
                    int i = 0;
                    for(; i < textRun.Length && s_i < s.Length; ++i, ++s_i)
                    {
                        if (textRun[i] != s[s_i])
                        {
                            TextRange range = new TextRange(pointer.GetPositionAtOffset(i),
                                pointer.GetPositionAtOffset(textRun.Length));
                            range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Blue);
                            return;
                        }
                    }
                }

                pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
            }
        }

        public static IEnumerable<TextRange> GetAllWordRanges(FlowDocument document)
        {
            //string pattern = @"[^\W\d](\w|[-']{1,2}(?=\w))*";
            TextPointer pointer = document.ContentStart;
            while (pointer != null)
            {
                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = pointer.GetTextInRun(LogicalDirection.Forward);
                    yield return new TextRange(pointer.GetPositionAtOffset(0),
                        pointer.GetPositionAtOffset(textRun.Length));
                    //MatchCollection matches = Regex.Matches(textRun, pattern);
                    //foreach (Match match in matches)
                    //{
                    //    int startIndex = match.Index;
                    //    int length = match.Length;
                    //    TextPointer start = pointer.GetPositionAtOffset(startIndex);
                    //    TextPointer end = start.GetPositionAtOffset(length);
                    //    yield return new TextRange(start, end);
                    //}
                }

                pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
            }
        }
    }
}
