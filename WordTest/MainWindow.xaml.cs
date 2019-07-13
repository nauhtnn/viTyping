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
using Microsoft.Office.Interop.Word;
//using ProfileLibrary;

namespace WordTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        Microsoft.Office.Interop.Word.Application workingApp;
        Microsoft.Office.Interop.Word.Application modelApp;
        Document workingDoc;
        Document modelDoc;
        Dictionary<int, string> vReq1;
        Dictionary<int, string> vReq2;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            QuitWordApps();
        }

        private void QuitWordApps()
        {
            try
            {
                workingDoc.Saved = true;
                workingDoc.Close();
                modelDoc.Saved = true;
                modelDoc.Close();
                workingApp.Quit();
                modelApp.Quit();
            }
            catch (NullReferenceException) { }
            catch (System.Runtime.InteropServices.COMException) { }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(workingDoc == null)
				return;
            int penalty = 0;
            int i = 0;
            foreach (Microsoft.Office.Interop.Word.Paragraph p in workingDoc.Paragraphs)
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
            QuitWordApps();
            Close();
        }

        private Microsoft.Office.Interop.Word.Application OpenApp(bool isModel)
        {
            Microsoft.Office.Interop.Word.Application app;
            try
            {
                app = new Microsoft.Office.Interop.Word.Application();
                app.Visible = true;
                app.WindowState = WdWindowState.wdWindowStateNormal;
                int w = app.UsableWidth;
                app.Top = 0;
                app.Height = app.UsableHeight / 2;
                app.Width = app.UsableWidth / 2;
                app.Left = (isModel) ? app.Width : 0;
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                MessageBox.Show("Cannot open app!" + ex.ToString());
                app = null;
            }
            return app;
        }

        private Document OpenDocument(string path, Microsoft.Office.Interop.Word.Application app)
        {
            Document doc;

            try
            {
                doc = app.Documents.Open(path, ReadOnly: app == modelApp);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                MessageBox.Show("Cannot open document!" + ex.ToString());
                doc = null;
            }
            return doc;
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
			var area = SystemParameters.WorkArea;
			Left = 0;
			Top = area.Bottom - Height;
			Width = area.Width;

            //vReq1 = new Dictionary<int, string>();
            //vReq2 = new Dictionary<int, string>();
            //LoadRequirement(fReq.Text);

            GetWindow(this).Closing += MainWindow_Closing;

            workingApp = OpenApp(false);
            modelApp = OpenApp(true);

            string curPath = System.IO.Directory.GetCurrentDirectory();

            // Open documents
            workingDoc = OpenDocument(curPath + "\\0.docx", workingApp);
            modelDoc = OpenDocument(curPath + "\\0_model.docx", modelApp);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            foreach (Microsoft.Office.Interop.Word.Paragraph p in workingDoc.Paragraphs)
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
