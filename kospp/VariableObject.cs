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
        private BlockParser blockParser;
        private bool isPublic;
        private string name;
        private string initValue;
        public VariableObject(String pName, bool pIsPublic)
        {
            isPublic = pIsPublic;
            name = pName;
            initValue = "";
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
            get { return "\"" + name + "\"," + initValue;}
        }

        public string GetKOSCode()
        {
            return "";
        }
         public bool ParseWord(string word)
        {
             //TODO: in the futuer I might want to check that this is a leagal init value for a parameter.
            initValue += word;
            parseState = eParseShate.Done; // one word in the block is theoreticly enough to be done.
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
