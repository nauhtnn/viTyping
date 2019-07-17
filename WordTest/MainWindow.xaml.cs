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
using ProfileLibrary;

namespace WordTest
{
    enum WORD_FMT
    {
        PROBLEM_DESC,
        WORKING_FILE,
        MODEL_FILE
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
            MatchAlignment();
            MatchFont();
            //MatchFont2();
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
            while (i != null && j != null)
            {
                if (i.Font.Size != j.Font.Size
                    || i.Font.Name != j.Font.Name
                    || i.Font.Color != j.Font.Color)
                {
                    MessageBox.Show(i.Text);
                    return false;
                }
                i = i.Next();
                j = j.Next();
            }
            if (i != null || j != null)
            {
                MessageBox.Show("one null");
                return false;
            }

            return true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            QuitWordApps();
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
            problem.LoadID();
            problem.Next();

            // Open documents
            modelDoc = OpenDocument(
                problem.LookupFullPath(problem.Desc[WORD_FMT.MODEL_FILE.ToString()]), modelApp);
            workingDoc = OpenDocument(
                problem.LookupFullPath(problem.Desc[WORD_FMT.WORKING_FILE.ToString()]), workingApp);
        }

        private void MainApp_Initialized(object sender, EventArgs e) => ScaleGUI();
    }
}
