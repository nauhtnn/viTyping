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
                        if(isText0)
                            text0.Append(tokens[0] + "\r\n");
                        else
                            text1.Append(tokens[0] + "\r\n");
                    }
                }
                char[] trimChars = { '\r', '\n', ' ', '\t' };
                testConfigs.Add(CFG.TEXT0.ToString(), text0.ToString().Trim(trimChars));
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
            throw new NotImplementedException();
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(this);
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.ResizeMode = ResizeMode.NoResize;
        }
    }
}
