using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ProfileLibrary
{
    public class Profile
    {
        public int CurrentProblemID { get; private set; }
        public string CurrentProblemPath()
        {
            return FolderPath + CurrentProblemID + ".txt";
        }
        public string FolderPath { get; private set; }
        public string ProgressSavePath { get; private set; }

        public Profile()
        {
            CurrentProblemID = -1;
        }

        public void SetPath(string path)
        {
            FolderPath = path;
            ProgressSavePath = FolderPath + "sav.txt";

            LoadProfile();
        }

        public void Save()
        {
            if (CurrentProblemID < 0)
                CurrentProblemID = 0;
            File.WriteAllText(ProgressSavePath, CurrentProblemID.ToString());
        }

        public void NextProblem()
        {
            if (CurrentProblemID < 0)
            {
                if (File.Exists(ProgressSavePath))
                    CurrentProblemID = int.Parse(File.ReadAllText(ProgressSavePath));
                else
                    CurrentProblemID = 0;
            }
            else
                ++CurrentProblemID;
        }

        public void LoadProfile()
        {
            if (File.Exists(ProgressSavePath))
                CurrentProblemID = int.Parse(File.ReadAllText(ProgressSavePath));

            if (CurrentProblemID < 0)
                CurrentProblemID = 0;
        }
    }
}
