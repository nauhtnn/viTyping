using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace viTyping
{
    public interface ProblemLoader
    {
        SortedDictionary<string, string> LoadConfigs();
        void LoadProblem(int level, int subID);
    }
}
