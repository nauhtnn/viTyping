using System;
using System.Collections.Generic;

namespace ProfileLibrary
{
    public class Problem
    {
        private SortedDictionary<int, string> IndexMap;
        public SortedDictionary<string, string> Desc { get; private set; }

        public int ID { get; private set; }

        private string Topic;

        public Problem()
        {
            Topic = string.Empty;
            ID = 1;
            IndexMap = new SortedDictionary<int, string>();
            Desc = new SortedDictionary<string, string>();
        }

        public Problem(string topic)
        {
            Topic = topic;
            ID = 1;
            IndexMap = new SortedDictionary<int, string>();
            Desc = new SortedDictionary<string, string>();
        }

        public string LookupFullPath(string file)
        {
            string path = System.IO.Directory.GetCurrentDirectory() + "\\" + Topic + "Data\\" + file;
            if(System.IO.File.Exists(path))
                return path;
            path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                "\\" + Topic + "Data\\" + file;
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
                {
                    path = nextID.ToString() + ".txt";
                    path = LookupFullPath(path);
                }
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
                SaveID();
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
                {
                    if (Desc.ContainsKey(tokens[0]))
                        Desc[tokens[0]] = tokens[1];
                    else
                        Desc.Add(tokens[0], tokens[1]);
                }
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
                    {
                        if (IndexMap.ContainsKey(idx))
                            IndexMap[idx] = tokens[1];
                        else
                            IndexMap.Add(idx, tokens[1]);
                    }
                }
            }
        }

        public void SaveID()
        {
            System.IO.File.WriteAllText(System.IO.Directory.GetCurrentDirectory() + "\\save.txt", ID.ToString());
        }

        public void LoadID()
        {
            string path = System.IO.Directory.GetCurrentDirectory() + "\\save.txt";
            if (path != null)
                ID = int.Parse(System.IO.File.ReadAllText(path));
            --ID;//method Next() will increase the ID
        }
    }
}
