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
        ALIGNMENT,
        FONT_SZ,
        FONT_NAME,
        FONT_COLOR,
        FONT_STYLE
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

        private bool MatchText()
        {
            char[] wc = workingDoc.Content.Text.ToCharArray(),
                mc = modelDoc.Content.Text.ToCharArray();
            int i = 0, j = 0;
            for(; i < wc.Length && j < mc.Length; ++i, ++j)
                if(wc[i] != mc[j])
                {
                    workingDoc.Range(i, Missing.Value).Select();
                    modelDoc.Range(j, Missing.Value).Select();
                    return false;
                }
            if(i < wc.Length)
            {
                workingDoc.Range(i, Missing.Value).Select();
                return false;
            }
            if (j < mc.Length)
            {
                modelDoc.Range(j, Missing.Value).Select();
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
            catch (System.Runtime.InteropServices.COMException ex)
            {
                MessageBox.Show("Văn bản chưa được mở.\n" +
                    "Hướng dẫn:\n- Đóng tất cả cửa sổ MS Word.\n- Nhấn nút \"Khôi phục lại\".");
                return false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(!IsValid())
				return;
            workingApp.Selection.Collapse();
            modelApp.Selection.Collapse();
            if (!MatchText() ||
                !MatchAlignment() ||
                !MatchFont())
            {
                MessageBox.Show("Hai văn bản không khớp.\n" +
                    "Hướng dẫn:\nLần lượt chọn từng văn bản.\nKiểm tra vị trí không khớp được đánh dấu.");
                return;
            }
            MessageBox.Show("Xin chúc mừng!");
            CloseAllDocuments();
            NextProblem();
        }

        //MatchText already checked 2 documents have the same text
        private bool MatchAlignment()
        {
            Queue<Microsoft.Office.Interop.Word.Paragraph> wp =
                new Queue<Microsoft.Office.Interop.Word.Paragraph>();
            foreach (Microsoft.Office.Interop.Word.Paragraph p in workingDoc.Paragraphs)
                wp.Enqueue(p);
            Queue<Microsoft.Office.Interop.Word.Paragraph> mp =
                new Queue<Microsoft.Office.Interop.Word.Paragraph>();
            foreach (Microsoft.Office.Interop.Word.Paragraph p in modelDoc.Paragraphs)
                mp.Enqueue(p);
            int i = 0;
            while(wp.Count() > 0)
            {
                if(wp.Peek().Alignment != mp.Peek().Alignment)
                {
                    wp.Peek().Range.Select();
                    mp.Peek().Range.Select();
                    return false;
                }
                wp.Dequeue();
                mp.Dequeue();
                ++i;
            }

            return true;
        }

        //MatchText already checked 2 documents have the same text
        private bool MatchFont()
        {
            Range[] wr = workingDoc.Characters.Cast<Range>().ToArray(),
                mr = modelDoc.Characters.Cast<Range>().ToArray();
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
                    workingDoc.Range(m, Missing.Value).Select();
                    modelDoc.Range(n, Missing.Value).Select();
                    return false;
                }
            }
            //Range i = workingDoc.Characters.First,
            //    j = modelDoc.Characters.First;
            //while (i != null && j != null)
            //{
            //    if (i.Font.Bold != j.Font.Bold ||
            //        i.Font.Italic != j.Font.Italic ||
            //        i.Font.Size != j.Font.Size ||
            //        i.Font.Name != j.Font.Name ||
            //        i.Font.Color != j.Font.Color)
            //    {
            //        workingDoc.Range(i.Start, Missing.Value).Select();
            //        modelDoc.Range(j.Start, Missing.Value).Select();
            //        return false;
            //    }
            //    i = i.Next();
            //    j = j.Next();
            //}

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

            problem = new Problem();
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

        private void MainApp_Initialized(object sender, EventArgs e) => ScaleGUI();
    }
}
