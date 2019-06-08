using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Configurations
{
    bool IsInit = false;
    public SortedDictionary<string, string> _;
    public string EXT_DATA_PATH { get; private set; }
    const string RELATIVE_CFG_PATH = "/configs.txt";

    public void Init()
    {
        if (IsInit)
            return;
        IsInit = true;
        _ = new SortedDictionary<string, string>();
        //if (File.Exists(Directory.GetCurrentDirectory() + RELATIVE_CFG_PATH))
        EXT_DATA_PATH = Directory.GetCurrentDirectory();
        if (File.Exists(EXT_DATA_PATH + RELATIVE_CFG_PATH))
        {
            foreach(string line in File.ReadAllLines(EXT_DATA_PATH + RELATIVE_CFG_PATH))
            {
                string[] tokens = line.Split('\t');
                if (tokens.Length == 2)
                    _.Add(tokens[0], tokens[1]);
            }
            //foreach(string k in _.Keys)
            //{
            //    int v;
            //    if (_.TryGetValue(k, out v))
            //        Debug.Log(k + " = " + v);
            //}
        }
        else
        {
            _.Add(CFG.FONT_SIZE.ToString(), "14");
            _.Add(CFG.MINUTE.ToString(), "10");
            _.Add(CFG.SECOND.ToString(), "0");
        }
    }
}
