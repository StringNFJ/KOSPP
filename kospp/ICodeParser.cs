using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kospp
{
    interface ICodeParser
    {
        string ParseError { get; }
        bool ParseWord(string word);
        bool IsParseComplete { get; }   
        bool HasParseError { get; }

    }
}
