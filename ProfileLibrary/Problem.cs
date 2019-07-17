using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfileLibrary
{
    public class Problem
    {
        private SortedDictionary<int, string> IndexMap;
        public SortedDictionary<string, string> Desc { get; private set; }

        public int ID { get; private set; }

        private string TopicPath;

        public Problem()
        {
            TopicPath = string.Empty;
            ID = 0;
            IndexMap = new SortedDictionary<int, string>();
            Desc = new SortedDictionary<string, string>();
        }

        public Problem(string topic)
        {
            TopicPath = topic;
            ID = 0;
            IndexMap = new SortedDictionary<int, string>();
            Desc = new SortedDictionary<string, string>();
        }

        public string LookupFullPath(string file)
        {
            string path = System.IO.Directory.GetCurrentDirectory() + "\\" + TopicPath + file;
            if(System.IO.File.Exists(path))
                return path;
            path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                "\\" + TopicPath + file;
            if (System.IO.File.Exists(path))
                return path;
            return null;
        }

        private string NextPath(out int nextID)
        {
            string path = null;
            int max_attempt = 11;
            nextID = ID;
            do
            {
                ++nextID;
                if(IndexMap.TryGetValue(nextID, out path) && path != null)
                    path = LookupFullPath(path);
                if (path == null)
                    path = nextID.ToString() + ".txt";
                path = LookupFullPath(path);
                --max_attempt;
            }
            while (0 < max_attempt && path == null);
            return path;
        }

        public bool Next()
        {
            int nextID;
            string p = NextPath(out nextID);
            if (p != null)
            {
                ReadDesc(p);
                ID = nextID;
                return true;
            }

            return false;
        }

        private void ReadDesc(string path)
        {
            Desc.Clear();

            foreach (string line in System.IO.File.ReadAllLines(path))
            {
                string[] tokens = line.Split('\t');
                if (tokens.Length == 2)
                    Desc.Add(tokens[0], tokens[1]);
            }
        }

        public void ReadMap()
        {
            string path = LookupFullPath("map.txt");
            if(path != null)
            {
                string[] s = System.IO.File.ReadAllLines(path);
                foreach(string i in s)
                {
                    string[] tokens = i.Split('\t');
                    int idx;
                    if (tokens.Length == 2 && int.TryParse(tokens[0], out idx))
                        IndexMap.Add(idx, tokens[1]);
                }
            }
        }

        public void SaveID()
        {
            System.IO.File.WriteAllText(System.IO.Directory.GetCurrentDirectory() + "\\save.txt", ID.ToString());
        }

        public void LoadID()
        {
            string path = LookupFullPath("save.txt");
            if (path != null)
                ID = int.Parse(System.IO.File.ReadAllText(path));
            --ID;//method Next() will increase the ID
        }
    }
}
