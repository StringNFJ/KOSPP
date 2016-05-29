using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kospp
{
    class PropertyObject :IKOSppObject, IBlockProcessor
    {
        private enum eParseShate
        {
            Start,
            MainLoop,
            SendWordsEngineToObject,
            Done,
            Error
        }

        #region private vars
        private eParseShate     parseState;
        private string          parseError;
        private bool            isPublic;
        private string          name;
        private FunctionObject  getFunction;
        private FunctionObject  setFunction;
        private IKOSppObject    currentObject;
        #endregion

        public PropertyObject(string pName, bool pIsPublic)
        {
            isPublic = pIsPublic;
            name = pName;
            parseState = eParseShate.Start;
            getFunction = null;
            setFunction = null;
        }

        #region IKOSppObject
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
            get 
            { 
                String Lex = "";
                if (getFunction != null || setFunction != null)
                {
                    Lex += "\"" + name + "\",lexicon(\r\n\t";
                    if (getFunction != null)
                        Lex += getFunction.LexiconEntry + "\r\n\t";
                    if (setFunction != null)
                    {
                        if (getFunction != null)
                            Lex += ",";
                        Lex += setFunction.LexiconEntry + "\r\n";
                    }
                    Lex += "\t)";
                }
                return Lex;
            }
        }   
        public string   GetKOSCode()
        {
            String KOSCode = "";
            if(getFunction != null)
                KOSCode+= getFunction.GetKOSCode();
            if(setFunction != null)
                KOSCode+= setFunction.GetKOSCode();
            return KOSCode;
        }
        public string   CallString(bool pGet = true)
        {
            string ret = "this[\"" + name +  "\"]";
            if (pGet)
                return ret + "[\"get\"]()";
            else
                return ret + "[\"set\"]({0})";

        }

        #region ICodeParser
        public bool     Parse(WordEngine oWordEngine)
        {
           switch(parseState)
           {
                case eParseShate.Start:
                    if(!oWordEngine.RegisterBlockTest(this))
                    {
                        parseState = eParseShate.Error;
                        return false;
                    }
                    parseState = eParseShate.MainLoop;
                    break;
               case eParseShate.MainLoop:
                   switch(oWordEngine.CurrentNonWhitespace)
                   {
                     case "get":
                        if (getFunction == null)
                        {
                            getFunction = new FunctionObject("get", false, "_get" + name);
                            currentObject = getFunction;
                        }
                        else
                        {
                            parseError = "The can only be one get in a propety.";
                            parseState = eParseShate.Error; 
                            return false;
                        }
                        break;
                    case "set":
                        if (setFunction == null)
                        {
                            setFunction = new FunctionObject("set", false, "_set" + name);
                            setFunction.AddParameter("value");
                            currentObject = setFunction;
                        }
                        else
                        {
                            parseError = "The can only be one set in a propety.";
                            parseState = eParseShate.Error;
                            return false;
                        }
                        break;
                     case WordEngine.BlockEndChar:
                        parseState = eParseShate.Done;
                        return false;
                    default:   
                        if (oWordEngine.HasError)
                            parseError = oWordEngine.Error;
                        else
                            parseError = "Expecting get or set, found " + oWordEngine.Current;
                        parseState = eParseShate.Error; 
                        return false;
                    }
                   parseState = eParseShate.SendWordsEngineToObject;
                   break;
                case eParseShate.SendWordsEngineToObject:
                    if(currentObject == null)
                    {
                        parseError = "Compiler error! trying to send the word " + oWordEngine.Current + " to an null KOSObject.";
                        return false;
                    }
                    if(!currentObject.Parse(oWordEngine))
                    {
                        if (currentObject.HasParseError || !currentObject.IsParseComplete)
                        {
                            if (currentObject.HasParseError)
                                parseError = currentObject.ParseError;
                            else
                                parseError = "Compiler error! Parser " + currentObject.Name + " returned false, but is not complete and has no errors.";
                            return false;
                        }
                        else
                            parseState = eParseShate.MainLoop;
                    }
                    break;
                case eParseShate.Done:
                    return false;
               case eParseShate.Error:
                    if (!HasParseError)
                        parseError = "Compiler error! The parser " + name + " was put in Error state, but had no error.";
                    return false;
               default:                    
                    parseError = "Compiner error! Unhandled state in PropertyObject: " + name + ""  + parseState.ToString(); 
                    parseState = eParseShate.Error;
                    return false;
           }
           return true;
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

        #region IBlockObject
        //Name implemented in IKOSObject
        public string BlockEnd()
        {
            if (parseState != eParseShate.MainLoop)
                return "A block in " + name + " ended outside of the main loop.";
            else
                return null; 
        }
        #endregion
    }
}
