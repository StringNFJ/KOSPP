using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kospp
{
    class VariableObject : IKOSppObject
    {
         private enum eParseShate
        {
            Start,
            Done,
            Error,
            
        }
        private eParseShate parseState;
        private string parseError;
        private bool isPublic;
        private string name;
        private string initValue;
        public VariableObject(String pName, bool pIsPublic)
        {
            isPublic = pIsPublic;
            name = pName;
            initValue = "";
            parseState = eParseShate.Start;
        }
        public string Name
        {
            get { return name; }
        }
        public bool IsPublic
        {
            get { return isPublic; }
        }
        public string InitValue
        {
            get { return initValue; }
            set { initValue = value; }
        }
        public string LexiconEntry
        {
            get { return "\"" + name + "\"," + (initValue.Trim().Length == 0 ? "\"\"":initValue.Trim());}
        }

        public string GetKOSCode()
        {
            return "";
        }
         public bool Parse(WordEngine oWordEngine)
        {
             //TODO: in the futuer I might want to check that this is a leagal init value for a parameter.
            while(oWordEngine.Current != null)
            {
                initValue += oWordEngine.Current;
                if(oWordEngine.NextNonWhitespace.Equals("."))
                {
                    parseState = eParseShate.Done;
                    return false;
                }
            }
            parseError = "Expecting . found end of file.";
            return true;
        }
        public bool IsParseComplete
        {
            get
            {
                if (parseState == eParseShate.Done)
                    return true;
                else
                    return false;
            }
        }
         
        public string ParseError  
        {
            get { return parseError; }
        }


        public bool HasParseError
        {
            get 
            {
                if (ParseError == null ||parseError == "")
                    return false;
                else
                    return true;
            }
        }
    }
}
