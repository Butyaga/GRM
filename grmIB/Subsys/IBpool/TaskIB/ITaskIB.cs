using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace grmIB.Subsys.IBpool.TaskIB
{
    interface ITaskIB
    {
        bool Predicate(AppData appData);
        bool SetContext(AppData appData);
        void Execute();
        void GetRezult(AppData appData);
    }
}
