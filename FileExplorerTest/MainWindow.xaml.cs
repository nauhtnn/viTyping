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
using System.IO.Compression;
using System.IO;
using ProfileLibrary;

namespace FileExplorerTest
{
    enum FILE_EXP_FMT
    {
        PROBLEM_DESC,
        WORKING_FILE,
        MODEL_FILE,
        TREE_VIEW
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Problem problem;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ScaleGUI()
        {
            var area = SystemParameters.WorkArea;
            double scale = area.Width / 1280;// MainApp.Width;
            MainApp.Width = MainApp.Width * scale;
            DescText.FontSize = DescText.FontSize * scale;
            DescText.Width = DescText.Width * scale;
            CheckBtn.FontSize = CheckBtn.FontSize * scale;
            CheckBtn.Width = CheckBtn.Width * scale;

            System.Windows.Window.GetWindow(this).Width = System.Windows.Window.GetWindow(this).Width * scale;
        }

        private void MainApp_Initialized(object sender, EventArgs e)
        {
            //ScaleGUI();
        }


        private void NextProblem()
        {
            problem.Next();

            DescText.Text = problem.Desc[FILE_EXP_FMT.PROBLEM_DESC.ToString()];

            string fullPath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" +
                problem.Desc[FILE_EXP_FMT.WORKING_FILE.ToString()];
            if(!Directory.Exists(fullPath))
                try
                {
                    Directory.CreateDirectory(fullPath);
                }
                catch(IOException e)
                {
                    MessageBox.Show(e.ToString());
                    Close();
                }
            if(problem.Desc.ContainsKey(FILE_EXP_FMT.TREE_VIEW.ToString()))
            {
                DirTree.Items.Clear();
                using (ZipArchive za = ZipFile.OpenRead(problem.LookupFullPath(problem.Desc[FILE_EXP_FMT.MODEL_FILE.ToString()])))
                {
                    foreach (ZipArchiveEntry i in za.Entries)
                    {
                        string path = i.FullName.Substring(0, i.FullName.Length - 1);//remove the last '/'
                        Stack<string> names = new Stack<string>();
                        int n = path.LastIndexOf('/');
                        while (n != -1)
                        {
                            names.Push(path.Substring(n + 1));
                            path = path.Substring(0, n);
                            n = path.LastIndexOf('/');
                        }
                        names.Push(path);
                        TreeViewItem item = null;
                        foreach(TreeViewItem it in DirTree.Items)
                            if(names.Peek() == (string)it.Header)
                            {
                                item = it;
                                names.Pop();
                                break;
                            }
                        if (item == null)
                        {
                            item = new TreeViewItem();
                            item.Header = names.Pop();
                            DirTree.Items.Add(item);
                        }
                            
                        while (names.Count > 0)
                        {
                            TreeViewItem j = null;
                            foreach (TreeViewItem it in item.Items)
                                if (names.Peek() == (string)it.Header)
                                {
                                    j = it;
                                    names.Pop();
                                    break;
                                }
                            if (j == null)
                            {
                                j = new TreeViewItem();
                                j.Header = names.Pop();
                                item.Items.Add(j);
                            }
                            item = j;
                        }
                    }
                    DirTree.Visibility = Visibility.Visible;
                }
            }
        }


        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Left = 0;
            Top = 0;

            ScaleGUI();

            problem = new Problem("FileExpTest");
            problem.ReadMap();
            problem.LoadID();

            NextProblem();
        }

        private void Zip_Click(object sender, RoutedEventArgs e)
        {
            SortedSet<string> entries = new SortedSet<string>();
            using (ZipArchive za = ZipFile.OpenRead(problem.LookupFullPath(problem.Desc[FILE_EXP_FMT.MODEL_FILE.ToString()])))
            {
                foreach (ZipArchiveEntry i in za.Entries)
                    entries.Add(i.FullName);
                string fullPath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" +
                    problem.Desc[FILE_EXP_FMT.WORKING_FILE.ToString()];
                int n = System.IO.Path.GetDirectoryName(fullPath).Length + 1;
                bool IsOK = true;
                try
                {
                    CheckDirectory(fullPath, n, ref entries);
                }
                catch (FileNotFoundException)
                {
                    IsOK = false;
                }
                if (entries.Count > 0)
                    MessageBox.Show("Thiếu " + entries.First());
                else if (IsOK)
                    MessageBox.Show("Xin chúc mừng!");
            }
        }

        private void CheckDirectory(string fullPath, int prefixLength, ref SortedSet<string> entries)
        {
            string curDir = fullPath.Substring(prefixLength).Replace("\\", "/") + "/";
            if(!entries.Contains(curDir))
            {
                MessageBox.Show("Dư thừa thư mục: " + curDir);
                throw new FileNotFoundException();
            }
            entries.Remove(curDir);
            foreach(string i in Directory.GetFiles(fullPath))
            {
                if (!entries.Contains(i.Substring(prefixLength).Replace("\\", "/")))
                {
                    MessageBox.Show("Dư thừa tập tin: " + i.Substring(prefixLength));
                    throw new FileNotFoundException();
                }
                entries.Remove(i.Substring(prefixLength).Replace("\\", "/"));
            }
            foreach(string i in Directory.GetDirectories(fullPath))
            {
                //if (!entries.Contains(i.Substring(prefixLength).Replace("\\", "/") + "/"))
                //{
                //    MessageBox.Show("Dư thừa thư mục: " + i.Substring(prefixLength));
                //    throw new FileNotFoundException();
                //}
                //entries.Remove(i.Substring(prefixLength).Replace("\\", "/") + "/");
                CheckDirectory(i, prefixLength, ref entries);
            }
        }
    }
}
