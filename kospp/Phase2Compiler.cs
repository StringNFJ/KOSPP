using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kospp
{
    class Phase2Compiler 
    {
        private char[] SeperatorChars = new char[] {'=','+','-','*','(',')','[',']','.'};   
        private List<IKOSppObject> KOSObjects;
        private string code;
        public Phase2Compiler(List<IKOSppObject> pKOSObjects, string pCode)
        {
            code = pCode;
            KOSObjects = pKOSObjects;
        }
     
    }
}
