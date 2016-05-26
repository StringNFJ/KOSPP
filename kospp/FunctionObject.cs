using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kospp
{
    class FunctionObject : IKOSppObject
    {
         private enum eParseShate
        {
            Start,
            Done,
            Error
            
        }
        private eParseShate parseState;
        private string parseError;
        private BlockParser blockParser;
        private bool isPublic;
        private string name;
        private string internalName;
        private List<string> parameters;
        private string code;
        private bool ignoreString;
        public FunctionObject(string pName,bool pIsPublic,string pInrernalName = null)
        {
            parameters = new List<string>();
            name = pName;
            if (pInrernalName == null)
                internalName = pName;
            else
                internalName = pInrernalName;
            isPublic = pIsPublic;
            parseState = eParseShate.Done; //function will accept empty blocks as well.
            code = "\t\t";
            ignoreString = false;

        }
        public bool AddParameter(string pName)
        {
            if (parameters.Contains(pName))
                return false;
            parameters.Add(pName);
            return true;
        }
        public string Name
        {
            get { return name; }
        }
        public bool IsPublic
        {
            get { return isPublic; }
        }
        public string LexiconEntry
        {
            get { return "\"" + name + "\"," + internalName + "@" + ":bind(class)"; }
        }
        public int ParamCount
        {
            get
            {
                return parameters.Count;
            }
        }
        public string GetKOSCode()
        {
            String KOSCode = "\tfunction " + internalName;
            KOSCode += "\r\n\t{\r\n\t\tparameter this";
            foreach (string param in parameters)
                KOSCode += ", " + param;
            KOSCode += ".\r\n" + code + "\r\n\t}\r\n";
            return KOSCode;
        }
        public bool ParseWord(string word)
        {
            if (word.Equals("."))
            {
                code = code.TrimEnd(' ') + word + "\r\n\t\t";
            }
            else
                code += word + " ";
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
                if (ParseError == null || parseError == "")
                    return false;
                else
                    return true;
            }
        }
    }
}
