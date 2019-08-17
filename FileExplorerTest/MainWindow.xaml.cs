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

namespace FileExplorerTest
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

        private void Zip_Click(object sender, RoutedEventArgs e)
        {
            //string zipPath = System.IO.Path.GetFileNameWithoutExtension(DirToZip.Text) + ".zip";
            //ZipFile.CreateFromDirectory(DirToZip.Text, zipPath);
            SortedSet<string> entries = new SortedSet<string>();
            using (ZipArchive za = ZipFile.OpenRead(@"D:\thuan\proj\viTyping\FileExplorerTest\bin\Debug\User.zip"))
            {
                foreach (ZipArchiveEntry i in za.Entries)
                    entries.Add(i.FullName);
                string fullPath = @"D:\thuan\proj\viTyping\FileExplorerTest\bin\Debug\User";
                Console.WriteLine(System.IO.Path.GetDirectoryName(fullPath));
                int n = System.IO.Path.GetDirectoryName(fullPath).Length + 1;
                bool IsOK = true;
                try
                {
                    CheckDirectory(fullPath, n, ref entries);
                }
                catch(FileNotFoundException) {
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
            string curDir = fullPath.Substring(prefixLength);
            if(!entries.Contains(curDir.Replace("\\", "/") + "/"))
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
                entries.Remove(i);
            }
            foreach(string i in Directory.GetDirectories(fullPath))
            {
                if (!entries.Contains(i.Substring(prefixLength).Replace("\\", "/") + "/"))
                {
                    MessageBox.Show("Dư thừa thư mục: " + i.Substring(prefixLength));
                    throw new FileNotFoundException();
                }
                entries.Remove(i);
                CheckDirectory(i, prefixLength, ref entries);
            }
        }
    }
}
