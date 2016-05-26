using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kospp
{
    interface IKOSppObject : ICodeParser
    {
        string Name { get; }   
        string LexiconEntry { get; }
        bool IsPublic { get; }
        string GetKOSCode();
    }
}
