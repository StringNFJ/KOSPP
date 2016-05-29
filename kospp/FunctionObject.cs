using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kospp
{
    class FunctionObject : IKOSppObject, IBlockProcessor
    {
         private enum eParseShate
        {
            Start,
            MainLoop,
            Done,
            Error
        }

        #region private vars
        private eParseShate     parseState;
        private string          parseError;
        private bool            isPublic;
        private string          name;
        private string          internalName;
        private List<string>    parameters;
        private string          code;
        #endregion

        public FunctionObject(string pName,bool pIsPublic,string pInrernalName = null)
        {
            parameters = new List<string>();
            name = pName;
            if (pInrernalName == null)
                internalName = pName;
            else
                internalName = pInrernalName;
            isPublic = pIsPublic;
            parseState = eParseShate.MainLoop;
            code = "";
        }

        public bool AddParameter(string pName)
        {
            if (parameters.Contains(pName))
                return false;
            parameters.Add(pName);
            return true;
        }
        public int  ParamCount
        {
            get
            {
                return parameters.Count;
            }
        }

        #region IKOSObject
        public string   Name
        {
            get { return name; }
        }
        public bool     IsPublic
        {
            get { return isPublic; }
        }
        public string   LexiconEntry
        {
            get { return "\"" + name + "\"," + internalName + "@" + ":bind(class)"; }
        }        
        public string   GetKOSCode()
        {
            String KOSCode = "function " + internalName;
            KOSCode += "\r\n{\r\nparameter this";
            foreach (string param in parameters)
                KOSCode += ", " + param;
            KOSCode += ".\r\n" + code + "}\r\n";
            return KOSCode;
        }
        public string   CallString(bool pGet = true)
        {
            if (pGet)
                return "this" + (!isPublic ? "[\"_\"]" : "") + "[\"" + name +  "\"]({0})";
            else
                return null;
        }
        #region ICodeParser
        public bool     Parse(WordEngine oWordEngine)
        {
            switch(parseState)
            {
                case eParseShate.MainLoop:
                    String word = oWordEngine.CurrentNonWhitespace;
                    if (oWordEngine.IsBlockStart)
                        oWordEngine.RegisterBlockTest(this);
                    else
                    {
                        parseError = "Function " + name + " expecting a block.";
                        parseState = eParseShate.Error;
                        return false;
                    }
                    string StartOfFunctionMarker = oWordEngine.GetLineWithPositionMarker;
                    do
                    {                        
                        while (oWordEngine.NextWord != null && parseState == eParseShate.MainLoop)
                        {
                            if (oWordEngine.Current.Equals("."))
                                code = code.TrimEnd(' ') + oWordEngine.Current + "\r\n";
                            else
                                code += oWordEngine.Current;
                        }
                    } while (parseState == eParseShate.MainLoop && oWordEngine.NextLine());
                    if(oWordEngine.Current == null)
                    {
                        parseState = eParseShate.Error;
                        parseError = "Function " + name + " reached the end of file before finding the clock end.\r\n" + StartOfFunctionMarker;
                    }
                    return false;
                case eParseShate.Done:
                    return false;
                case eParseShate.Error:
                    if (!HasParseError)
                        parseError = "Compiler error! The parser " + name + " was put in Error state, but had no error.";
                    return false;
               default:                    
                    parseError = "Compiner error! Unhandled state in FunctionObject " + name + " : "  + parseState.ToString(); 
                    parseState = eParseShate.Error;
                    return false;
            }
        }
        public bool     IsParseComplete
        {
            get
            {
                if (parseState == eParseShate.Done)
                    return true;
                else
                    return false;
            }
        }
        public string   ParseError
        {
            get { return parseError; }
        }   
        public bool     HasParseError
        {
            get 
            {
                if (ParseError == null || parseError == "")
                    return false;
                else
                    return true;
            }
        }
        #endregion
        #endregion

        #region IBlockParser
        public string BlockEnd()
        {
            if(parseState == eParseShate.MainLoop)
            {
                parseState = eParseShate.Done;
                return null;
            }
            else
                return "A block in function " + name + " ended outside of the main loop.";
        }
        #endregion
    }
}
