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

namespace Levenshtein
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            double max_grade;
            try
            {
                max_grade = ParseMaxGrade();
            }
            catch(ArgumentException)
            {
                Grade.Text = "-";
                return;
            }
            int round;
            try
            {
                round = ParseRound();
            }
            catch (ArgumentException)
            {
                Grade.Text = "-";
                return;
            }
            char[] trimChars = { '\r', '\n', ' ', '\t' };
            string txt = userText.Text.Trim(trimChars);
            int l = Levenshtein(txt, sourceText.Text);
            Grade.Text = Math.Round(max_grade - max_grade * l / sourceText.Text.Length, round).ToString();
        }

        private double ParseMaxGrade()
        {
            double max_grade;
            if (!double.TryParse(MaxGrade.Text, out max_grade))
            {
                System.Windows.MessageBox.Show("Error in max grade parsing!");
                throw new ArgumentException();
            }
            if(max_grade <= 0.1)
            {
                System.Windows.MessageBox.Show("Max grade <= 0.1 !");
                throw new ArgumentException();
            }
            return max_grade;
        }

        private int ParseRound()
        {
            int round;
            if (!int.TryParse(Round.Text, out round))
            {
                System.Windows.MessageBox.Show("Error in round parsing!");
                throw new ArgumentException();
            }
            if (round < 0)
            {
                System.Windows.MessageBox.Show("Round < 0 !");
                throw new ArgumentException();
            }
            return round;
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
