using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kospp
{
    class PropertyObject :IKOSppObject,ICodeParser
    {
        private enum eParseShate
        {
            Start,
            GetBlockStart,
            GetBlock,
            Done,
            Error
            
        }
        private eParseShate parseState;
        private string parseError;
        private BlockParser blockParser;
        private bool isPublic;
        private string name;
        private FunctionObject getFunction = null;
        private FunctionObject setFunction = null;
        private IKOSppObject currentObject;
        
        public PropertyObject(string pName, bool pIsPublic)
        {
            isPublic = pIsPublic;
            name = pName;
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
        public string LexiconEntry
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
        public string GetKOSCode()
        {
            String KOSCode = "";
            if(getFunction != null)
                KOSCode+= getFunction.GetKOSCode();
            if(setFunction != null)
                KOSCode+= setFunction.GetKOSCode();
            return KOSCode;
        }

        public bool ParseWord(string word)
        {
           switch(parseState)
           {
               case eParseShate.Start:
                   switch(word)
                   {
                     case "get":
                        getFunction = new FunctionObject("get",false,"_get" + name);
                        currentObject = getFunction;
                        break;
                    case "set":
                        setFunction = new FunctionObject("set",false,"_set" + name);
                        setFunction.AddParameter("value");
                        currentObject = setFunction;
                        break;
                    case "}":
                        parseState = eParseShate.Done;
                        return false;
                    default:   
                        parseState = eParseShate.Error;
                        parseError = "Expecting get or set, found " + word;
                        return false;
                    }
                   parseState = eParseShate.GetBlockStart;
                   break;
                case eParseShate.GetBlockStart:
                    blockParser = new BlockParser(currentObject,"PropertyObject:" + currentObject.Name);
                    if (blockParser.BlockStart(word))
                        parseState = eParseShate.GetBlock;
                    else
                    {
                        parseError =  blockParser.Error;
                        parseState = eParseShate.Error;
                        return false;
                    }
                    break;
                case eParseShate.GetBlock:
                    if(!blockParser.GetBlock(word))
                    {
                        if (blockParser.HasParseError)
                        {
                            parseError = blockParser.Error;
                            parseState = eParseShate.Error; 
                            return false;
                        }
                        else
                        {
                            currentObject = null;
                            blockParser = null;
                            parseState = eParseShate.Start;
                        }                       
                    }
                    break;
                case eParseShate.Done:
                    return false;
               default:                    
                    parseError = "Compiner error! Unhandled state in PropertyObject: "  + parseState.ToString(); 
                    parseState = eParseShate.Error;
                    return false;
           }
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
