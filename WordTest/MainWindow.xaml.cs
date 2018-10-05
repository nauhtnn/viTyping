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
using Microsoft.Office.Interop.Word;


namespace WordTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        Microsoft.Office.Interop.Word.Application mWordApp;
        Document mDoc;
        Dictionary<int, string> vReq1;
        Dictionary<int, string> vReq2;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                mWordApp.Quit();
            }
            catch (System.Runtime.InteropServices.COMException ex) { }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(mDoc == null)
				return;
            int penalty = 0;
            int i = 0;
            foreach (Microsoft.Office.Interop.Word.Paragraph p in mDoc.Paragraphs)
            {
                string req;
                if (vReq1.TryGetValue(i, out req))
                {
                    if(!MatchFontRequirement(p.Range, req))
                        ++penalty;
                }
                else if(!MatchNoFormatFont(p.Range))
                    ++penalty;
                if (vReq2.TryGetValue(i, out req))
                {
                    if (!MatchAlignRequirement(p, req))
                        ++penalty;
                }
                else if(!MatchNoFormatAlign(p))
                    ++penalty;
                ////output.Text += "b " + p.Range.Font.Bold.ToString() +
                //    "i " + p.Range.Font.Italic.ToString() +
                //    "u " + p.Range.Font.Underline.ToString() + "\n";
                //if(vReq2)
                //  ++penalty;
                ++i;
            }
            txtPenalty.Text = Math.Round(10 - (float) penalty / i * 10, 1).ToString() + " điểm";
        }

        private bool MatchAlignRequirement(Microsoft.Office.Interop.Word.Paragraph p, string req)
        {
            if ((req.Contains("l") && p.Alignment != WdParagraphAlignment.wdAlignParagraphLeft) ||
                    !req.Contains("l") && p.Alignment == WdParagraphAlignment.wdAlignParagraphLeft)
            {
                //output.Text += "error para_" + p.Range.Text + "_";
                return false;
            }
            if ((req.Contains("r") && p.Alignment != WdParagraphAlignment.wdAlignParagraphRight) ||
                    !req.Contains("r") && p.Alignment == WdParagraphAlignment.wdAlignParagraphRight)
            {
                //output.Text += "error para_" + p.Range.Text + "_";
                return false;
            }
            if ((req.Contains("c") && p.Alignment != WdParagraphAlignment.wdAlignParagraphCenter) ||
                    !req.Contains("c") && p.Alignment == WdParagraphAlignment.wdAlignParagraphCenter)
            {
                //output.Text += "error para_" + p.Range.Text + "_";
                return false;
            }
            return true;
        }

        private bool MatchFontRequirement(Range r, string req)
        {
            if ((req.Contains("b") && r.Font.Bold == 0) ||
                    !req.Contains("b") && r.Font.Bold != 0)
            {
                //output.Text += "error range_" + r.Text + "_";
                return false;
            }
            if ((req.Contains("i") && r.Font.Italic == 0) ||
                    !req.Contains("i") && r.Font.Italic != 0)
            {
                //output.Text += "error range_" + r.Text + "_";
                return false;
            }
            if ((req.Contains("u") && r.Font.Underline == WdUnderline.wdUnderlineNone) ||
                    !req.Contains("u") && r.Font.Underline != WdUnderline.wdUnderlineNone)
            {
                //output.Text += "error range_" + r.Text + "_";
                return false;
            }
            return true;
        }

        private bool MatchNoFormatAlign(Microsoft.Office.Interop.Word.Paragraph p)
        {
            if (p.Alignment != WdParagraphAlignment.wdAlignParagraphLeft)
                return false;
            return true;
        }

        private bool MatchNoFormatFont(Range r)
        {
            if (r.Font.Bold != 0 || r.Font.Italic != 0 || r.Font.Underline != WdUnderline.wdUnderlineNone)
                return false;
            return true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                mDoc.Close();
            }
            catch(NullReferenceException ex) { }
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
			var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
			this.Left = 0;
			this.Top = desktopWorkingArea.Bottom - this.Height;
			this.Width = desktopWorkingArea.Width;
			
            // Open a doc file.
            mWordApp = new Microsoft.Office.Interop.Word.Application();
            mWordApp.Visible = true;

			string cur_dir = System.IO.Directory.GetCurrentDirectory();
            fName.Text = cur_dir + "/wordtest.docx";
            fReq.Text = cur_dir + "/wordtest.txt";
			
			if(System.IO.File.Exists(cur_dir + "/sam.txt"))
			{
				string sam_file = System.IO.File.ReadAllText(cur_dir + "/sam.txt");
				output.Text += sam_file;
			}

            vReq1 = new Dictionary<int, string>();
            vReq2 = new Dictionary<int, string>();
            LoadRequirement(fReq.Text);

            GetWindow(this).Closing += MainWindow_Closing;
			
			try
            {
                mDoc = mWordApp.Documents.Open(fName.Text, ReadOnly: false);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                MessageBox.Show("Cannot open document!" + ex.ToString());
                mDoc = null;
            }
			
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            foreach (Microsoft.Office.Interop.Word.Paragraph p in mDoc.Paragraphs)
			{
				int i = p.Range.Text.IndexOf(substr.Text);
				if(-1 < i)
				{
					Range r = p.Range;
					r.Start = p.Range.Start + i;
					r.End = r.Start + substr.Text.Length;
					rsubstr.Text = r.Text;
					break;
				}
			}
        }

        private void LoadRequirement(string fname)
        {
            if (!System.IO.File.Exists(fname))
            {
                MessageBox.Show("File not found: " + fname);
                return;
            }
            string[] lines = System.IO.File.ReadAllLines(fname);
            foreach(string s in lines)
            {
                string[] p = s.Split('\t');
                if(2 == p.Length)
                {
                    int i;
                    if (IsValidFontReq(p[1]) && int.TryParse(p[0], out i))
                    {
                        vReq1.Add(i, p[1]);
                    }
                    if (IsValidAlignReq(p[1]) && int.TryParse(p[0], out i))
                    {
                        vReq2.Add(i, p[1]);
                    }
                }
            }
        }

        private bool IsValidFontReq(string s)
        {
            foreach (char c in s)
                if ((c != 'b') && (c != 'i') && (c != 'u'))
                    return false;
            return true;
        }

        private bool IsValidAlignReq(string s)
        {
            foreach (char c in s)
                if ((c != 'l') && (c != 'r') && (c != 'c'))
                    return false;
            return true;
        }
    }
}
