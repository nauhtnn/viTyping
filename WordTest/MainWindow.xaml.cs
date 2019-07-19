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
                MessageBox.Show("Document is not valid!\n");
                return false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(!IsValid())
				return;
            workingApp.Selection.Collapse();
            modelApp.Selection.Collapse();
            if (!MatchAlignment())
                return;
            if (!MatchFont())
                return;
            CloseAllDocuments();
            NextProblem();
        }

        private bool MatchAlignment()
        {
            if (workingDoc.Paragraphs.Count != modelDoc.Paragraphs.Count)
            {
                MessageBox.Show("Khác số lượng đoạn văn.");
                return false;
            }
            Queue<Microsoft.Office.Interop.Word.Paragraph> workingParagraphs =
                new Queue<Microsoft.Office.Interop.Word.Paragraph>();
            foreach (Microsoft.Office.Interop.Word.Paragraph p in workingDoc.Paragraphs)
                workingParagraphs.Enqueue(p);
            Queue<Microsoft.Office.Interop.Word.Paragraph> modelParagraphs =
                new Queue<Microsoft.Office.Interop.Word.Paragraph>();
            foreach (Microsoft.Office.Interop.Word.Paragraph p in modelDoc.Paragraphs)
                modelParagraphs.Enqueue(p);
            int i = 0;
            while(workingParagraphs.Count() > 0)
            {
                if(workingParagraphs.Dequeue().Alignment != modelParagraphs.Dequeue().Alignment)
                {
                    MessageBox.Show("Unmatched alignment at line " + i);
                    return false;
                }
                ++i;
            }

            return true;
        }

        private void MatchFont2()
        {
            Range i = modelDoc.Words.First,
                j = workingDoc.Words.First;
            while(i != null && j != null)
            {
                if(//i.Font.Name != j.Font.Name ||
                    i.Font.Size != j.Font.Size)// ||
                    //i.Font.Color != j.Font.Color)
                {
                    MessageBox.Show(i.Text);
                    return;
                }
                i = i.Next();
                j = j.Next();
            }
            if (i != null || j != null)
                MessageBox.Show("one not null");
        }

        private bool MatchFont()
        {
            Range i = workingDoc.Characters.First,
                j = modelDoc.Characters.First;
            int k = 0;
            while (i != null && j != null)
            {
                Console.Write(i.Text + j.Text + " ");
                if (i.Text != j.Text ||
                    i.Font.Bold != j.Font.Bold ||
                    i.Font.Italic != j.Font.Italic ||
                    i.Font.Size != j.Font.Size ||
                    i.Font.Name != j.Font.Name ||
                    i.Font.Color != j.Font.Color)
                {
                    workingDoc.Range(i.Start, Missing.Value).Select();
                    workingDoc.Activate();
                    modelDoc.Range(j.Start, Missing.Value).Select();
                    modelDoc.Activate();
                    return false;
                }
                ++k;
                i = i.Next();
                j = j.Next();
            }
            while (i != null)
            {
                if(i.Text != " " && i.Text != "\t" && i.Text != "\n" && i.Text != "\r")
                {
                    workingDoc.Range(i.Start, Missing.Value).Select();
                    return false;
                }
                i = i.Next();
            }
            while(j != null)
            {
                if (j.Text != " " && j.Text != "\t" && j.Text != "\n" && j.Text != "\r")
                {
                    modelDoc.Range(j.Start, Missing.Value).Select();
                    return false;
                }
                j = j.Next();
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
                MessageBox.Show("Opening Word app exception!");
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
