using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kospp
{
    interface IBlockProcessor
    {
        string Name { get; }
        string BlockEnd();
    }
}
