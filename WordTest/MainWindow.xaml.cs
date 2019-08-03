using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using ProfileLibrary;

namespace WordTest
{
    enum WORD_FMT
    {
        PROBLEM_DESC,
        WORKING_FILE,
        MODEL_FILE,
        PLAIN_TEXT,
        ALIGNMENT,
        FONT,
        TABLE
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        Microsoft.Office.Interop.Word.Application workingApp;
        Microsoft.Office.Interop.Word.Application modelApp;
        Document workingDoc;
        Document modelDoc;

        Problem problem;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            QuitAllWordApps();
        }

        private void QuitWordApp(Microsoft.Office.Interop.Word.Application app)
        {
            if (app == null)
                return;
            try
            {
                foreach (Document d in app.Documents)
                    CloseWordDocument(d);
                app.Quit();
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                MessageBox.Show("Quiting Word app exception!");
            }
        }

        private void QuitAllWordApps()
        {
            QuitWordApp(workingApp);
            QuitWordApp(modelApp);
        }

        private bool MatchPlainText()
        {
            if (workingDoc.Content.Text == modelDoc.Content.Text)
                return true;
            Microsoft.Office.Interop.Word.Paragraph p = workingDoc.Paragraphs.First,
                q = modelDoc.Paragraphs.First;
            while (p != null && q != null)
            {
                Range[] r = p.Range.Characters.Cast<Range>().ToArray(),
                    s = q.Range.Characters.Cast<Range>().ToArray();
                int i, j;
                for(i = 0, j = 0; i < r.Length && j < s.Length; ++i, ++j)
                {
                    if (r[i].Text != s[i].Text)
                    {
                        r[i].Select();
                        s[i].Select();
                        return false;
                    }
                }
                if (i < r.Length)
                {
                    r[i].Select();
                    return false;
                }
                if (j < s.Length)
                {
                    s[j].Select();
                    return false;
                }
                p = p.Next();
                q = q.Next();
            }
            if (p != null)
            {
                p.Range.Select();
                return false;
            }
            if (q != null)
            {
                q.Range.Select();
                return false;
            }
            return true;
        }

        private bool IsValid()
        {
            try
            {
                bool dummy = workingApp.Visible;
                dummy = workingDoc.ReadOnly;
                dummy = modelApp.Visible;
                dummy = modelDoc.ReadOnly;
                return true;
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                MessageBox.Show("Văn bản chưa được mở.\n" +
                    "Hướng dẫn:\n- Đóng tất cả cửa sổ MS Word.\n- Nhấn nút \"Khôi phục lại\".");
                return false;
            }
        }

        private void SetSpin(bool spin)
        {
            if(spin)
            {
                CheckBtn.Visibility = Visibility.Collapsed;
                SpinningIcon.Visibility = Visibility.Visible;
            }
            else
            {
                CheckBtn.Visibility = Visibility.Visible;
                SpinningIcon.Visibility = Visibility.Collapsed;
            }
        }
		
		private void MatchAll()
		{
            bool isMatched = true;
            if (!MatchPlainText())
                isMatched = false;
            if (isMatched && problem.Desc.ContainsKey(WORD_FMT.ALIGNMENT.ToString()) &&
                problem.Desc[WORD_FMT.ALIGNMENT.ToString()] == "1" &&
                !MatchAlignment())
                isMatched = false;
            if (isMatched && problem.Desc.ContainsKey(WORD_FMT.FONT.ToString()) &&
                problem.Desc[WORD_FMT.FONT.ToString()] == "1" &&
                !MatchFont())
                isMatched = false;
            if (isMatched && problem.Desc.ContainsKey(WORD_FMT.TABLE.ToString()) &&
                problem.Desc[WORD_FMT.TABLE.ToString()] == "1" &&
                !MatchTable())
                isMatched = false;
            if (isMatched)
            {
                Dispatcher.Invoke(() => {
                    SetSpin(false);
                    MessageBox.Show("Xin chúc mừng!");
                    CloseAllDocuments();
                    NextProblem();
                });
                return;
            }
            Dispatcher.Invoke(() => {
                SetSpin(false);
                MessageBox.Show("Hai văn bản không khớp.\n" +
                "Hướng dẫn:\nLần lượt chọn từng văn bản.\nKiểm tra vị trí không khớp được đánh dấu.");
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(!IsValid())
				return;
            SetSpin(true);
            workingApp.Selection.Collapse();
            modelApp.Selection.Collapse();
			System.Threading.Thread th = new System.Threading.Thread(MatchAll);
			th.Start();
        }

        //Document.Tables, Table.Cell starts from 1, not 0
        private bool MatchTable()
        {
            //for (int i = 1, j = 1; i <= workingDoc.Tables.Count && j <= modelDoc.Tables.Count; ++i, ++j)
            //{
            //    Microsoft.Office.Interop.Word.Table wt = workingDoc.Tables[i],
            //    mt = modelDoc.Tables[j];
            //    for (int u = wt.Rows.Count; u > 0; --u)
            //    {
            //        for (int v = wt.Columns.Count; v > 0; --v)
            //        {
            //            if (wt.Cell(u, v).Range.Bold != mt.Cell(u, v).Range.Bold ||
            //                wt.Cell(u, v).Range.Italic != mt.Cell(u, v).Range.Italic)
            //            {
            //                wt.Cell(u, v).Select();
            //                mt.Cell(u, v).Select();
            //                return false;
            //            }
            //        }
            //    }
            //}
            return true;
        }

        //MatchPlainText already checked 2 documents have the same text
        private bool MatchAlignment()
        {
            Microsoft.Office.Interop.Word.Paragraph p = workingDoc.Paragraphs.First,
                q = modelDoc.Paragraphs.First;
            while(p != null)
            {
                if(p.Alignment != q.Alignment)
                {
                    p.Range.Select();
                    q.Range.Select();
                    return false;
                }
                p = p.Next();
                q = q.Next();
            }
            return true;
        }

        private void MatchFont2()
        {
            string s = System.IO.Directory.GetCurrentDirectory();
            workingDoc.SaveAs("C:/Users/minad/AppData/Local/WordTestData/tmp.docx");
            DocumentFormat.OpenXml.Packaging.WordprocessingDocument doc = null;
            try
            {
                doc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(
                    "C:/Users/minad/AppData/Local/WordTestData/con_Rong_chau_Tien.docx", false);
            }
            catch (DocumentFormat.OpenXml.Packaging.OpenXmlPackageException)
            {
                return;
            }
            catch (System.IO.IOException)
            {
                return;
            }
            DocumentFormat.OpenXml.Wordprocessing.Body body = doc.MainDocumentPart.Document.Body;
            //int idx = -1;
            foreach (DocumentFormat.OpenXml.Wordprocessing.Paragraph p in body.ChildElements.OfType<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
            {
                DocumentFormat.OpenXml.Wordprocessing.Run r = p.ChildElements.First<DocumentFormat.OpenXml.Wordprocessing.Run>();
                while(r != null)
                {
                    Console.WriteLine(r.InnerText);
                    r = r.NextSibling<DocumentFormat.OpenXml.Wordprocessing.Run>();
                }
            }
            doc.Close();
        }

        //MatchPlainText already checked 2 documents have the same text
        private bool MatchFont()
        {
            Range[] wr = workingDoc.Words.Cast<Range>().ToArray(),
                mr = modelDoc.Words.Cast<Range>().ToArray();
            for (int m = 0, n = 0; m < wr.Length; ++m, ++n)
            {
                if (wr[m].Text[0] < '0' ||
                    ('9' < wr[m].Text[0] && wr[m].Text[0] < 'A') ||
                    ('Z' < wr[m].Text[0] && wr[m].Text[0] < 'a') ||
                    'z' < wr[m].Text[0])
                    continue;
                if (wr[m].Font.Bold != mr[n].Font.Bold ||
                    wr[m].Font.Italic != mr[n].Font.Italic ||
                    wr[m].Font.Size != mr[n].Font.Size ||
                    wr[m].Font.Name != mr[n].Font.Name ||
                    wr[m].Font.Color != mr[n].Font.Color)
                {
                    wr[m].Select();
                    mr[n].Select();
                    return false;
                }
            }

            return true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private Microsoft.Office.Interop.Word.Application OpenWordApp(bool isModel)
        {
            Microsoft.Office.Interop.Word.Application app;
            try
            {
                app = new Microsoft.Office.Interop.Word.Application();
                app.Visible = true;
                app.WindowState = WdWindowState.wdWindowStateNormal;
                int w = app.UsableWidth;
                app.Top = app.UsableHeight / 9;
                app.Height = app.UsableHeight * 8 / 9;
                if(isModel)
                {
                    app.Width = app.UsableWidth / 3;
                    app.Left = app.Width * 2;
                }
                else
                {
                    app.Width = app.UsableWidth * 2 / 3;
                    app.Left = 0;
                }
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                MessageBox.Show("Xảy ra lỗi khi mở ứng dụng MS Word\n" +
                    "Hướng dẫn: Thử mở một cửa sổ MS Word.");
                app = null;
            }
            return app;
        }

        private Document OpenWordDocument(string path, Microsoft.Office.Interop.Word.Application app)
        {
            Document doc;

            try
            {
                doc = app.Documents.Open(path, ReadOnly: app == modelApp);
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                MessageBox.Show("Opening Word document exception!");
                doc = null;
            }
            return doc;
        }

        private void CloseWordDocument(Document doc)
        {
            if (doc == null)
                return;
            try
            {
                doc.Saved = true;
                doc.Close();
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                MessageBox.Show("Closing Word document exception!");
            }
        }

        private void CloseAllDocuments()
        {
            CloseWordDocument(workingDoc);
            workingDoc = null;
            CloseWordDocument(modelDoc);
            modelDoc = null;
        }

        private void ScaleGUI()
        {
            var area = SystemParameters.WorkArea;
            double scale = area.Width / MainApp.Width;
            MainApp.Width = area.Width;
            ProblemDesc.FontSize = ProblemDesc.FontSize * scale;
            ProblemDesc.Width = ProblemDesc.Width * scale;
            CheckBtn.FontSize = CheckBtn.FontSize * scale;
            CheckBtn.Width = CheckBtn.Width * scale;
            ExitBtn.FontSize = ExitBtn.FontSize * scale;
            ExitBtn.Width = ExitBtn.Width * scale;

            System.Windows.Window.GetWindow(this).Width = area.Width;
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
			Left = 0;
            Top = 0;

            ScaleGUI();

            GetWindow(this).Closing += MainWindow_Closing;

            
            modelApp = OpenWordApp(true);
            workingApp = OpenWordApp(false);

            problem = new Problem("WordTest");
            problem.ReadMap();
            problem.LoadID();

            NextProblem();
        }

        private void NextProblem()
        {
            problem.Next();

            ProblemDesc.Text = problem.Desc[WORD_FMT.PROBLEM_DESC.ToString()];

            // Open documents
            modelDoc = OpenWordDocument(
                problem.LookupFullPath(problem.Desc[WORD_FMT.MODEL_FILE.ToString()]), modelApp);
            workingDoc = OpenWordDocument(
                problem.LookupFullPath(problem.Desc[WORD_FMT.WORKING_FILE.ToString()]), workingApp);
        }

        private void MainApp_Initialized(object sender, EventArgs e)
		{
			ScaleGUI();
		}
    }
}
