using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kospp
{
    interface ICodeParser
    {
        string  ParseError      { get; }        
        bool    IsParseComplete { get; }   
        bool    HasParseError   { get; }
        bool    Parse(WordEngine oWordEngine);

    }
}
