using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kospp
{
    class ClassObject : ICodeParser
    {
        private enum eParseShate
        {
            Start,
            Done,
            FuncVarName,
            FuncVar,
            GetFuncPrams,
            GetVarInitValue,
            Property,
            GetBlockStart,
            GetBlock,
            Error
        }
        private eParseShate parseState;
        private string name;
        private string ConstructorUserCode;
        private List<IKOSppObject> KOSObjects;
        private IKOSppObject currentObject;
        private string parseError;
        private bool currentIsPublic;
        private BlockParser blockParser;
        private string funcVarName;
        private bool canGetParameter;
        public string LexiconDefinition
        {
            get
            {
                String lex = "set " + name + " to lexicon(\r\n";
                bool first = true;
                foreach(IKOSppObject publicObject in KOSObjects.Where(x=>x.IsPublic == true && x.Name != name))
                {
                    if (first)
                    {
                        lex += "\t" + publicObject.LexiconEntry.Replace("\t","\t\t");
                        first = false;
                    }
                    else
                        lex += ",\r\n\t" + publicObject.LexiconEntry.Replace("\t","\t\t");
                    
                }
                lex += ",\r\n\t\"_\",lexicon(\r\n";
                first = true;
                foreach(IKOSppObject publicObject in KOSObjects.Where(x=>x.IsPublic == false && x.Name != name))
                {
                    if (first)
                    {
                        lex += "\t\t" + publicObject.LexiconEntry;
                        first = false;
                    }
                    else
                        lex += ",\r\n\t\t" + publicObject.LexiconEntry;
                    
                }
                lex += "\r\n\t\t)\r\n\t)\r\n";
                return lex;
            }
        }
        public string ConstructorDefinition
        {
            get
            {
                FunctionObject func = KOSObjects.Single(x => x.Name == name) as FunctionObject;
                String funcCode = func.GetKOSCode();
                funcCode = funcCode.Replace("function " + name, "function _" + name);
                if (func.ParamCount == 0)
                    funcCode = funcCode.Replace("parameter this.", "");
                else
                    funcCode = funcCode.Replace("this,", "");   
                funcCode = funcCode.Insert(funcCode.IndexOf("{\r\n")+3, "//Class lexicon\r\n\t" + LexiconDefinition + 
                    "\t\tset this to " + name + "." +
                    "\r\n//Do not edit the code above here unless you know what you doing!!!\r\n//User constructor code\r\n"
                    );
                funcCode = "//Constructor\r\nglobal " + name + " is _" + name + "@.\r\n\r\n" + funcCode;
                return funcCode;
            }
        }
        public string FunctionDefinitions
        {
            get
            {
                string func = "//Properties\r\n";
                foreach(IKOSppObject p in KOSObjects.Where(x=> x.GetType() == typeof(PropertyObject)))
                    func += p.GetKOSCode();
                func += "//public functions\r\n";
                foreach(IKOSppObject f in KOSObjects.Where(x=>x.IsPublic == true && x.Name != name))
                    func += f.GetKOSCode();
                func += "//private functions\r\n";
                foreach(IKOSppObject f in KOSObjects.Where(x=>x.IsPublic == false && x.Name != name))
                    func += f.GetKOSCode();
                return func;
            }
        }
        public ClassObject(string pName)
        {
            name = pName;
            KOSObjects = new List<IKOSppObject>();
            parseState = eParseShate.Start;
            parseError = "";
        }
        public void AddKosObject(IKOSppObject pKosObject)
        {
            KOSObjects.Add(pKosObject);            
        }
        public String getKOSCode()
        {
            return "";
        }
        public bool ParseWord(string word)
        {
            switch(parseState)
            {
                case eParseShate.Start:
                    switch(word)
                    {
                        case "private":
                            currentIsPublic = false;
                            parseState = eParseShate.FuncVarName;
                            break;
                        case "public":
                            currentIsPublic = true;
                            parseState = eParseShate.FuncVarName;
                            break;
                        case "property":
                            currentIsPublic = true;
                            parseState = eParseShate.Property;
                            break;
                        case "}":
                            parseState = eParseShate.Done;
                            return false;
                        default:
                            parseState = eParseShate.Error;
                            parseError = "Expecting private public or property, found " + word;
                            return false;
                    }
                    break;
                case eParseShate.Property:
                    if (validateName(word))
                    {
                        currentObject = new PropertyObject(word, currentIsPublic);
                        parseState = eParseShate.GetBlockStart;
                    }
                    else
                    {
                        parseError =  "The name " + word + " is not a valid name for a property.";
                        parseState = eParseShate.Error;
                        return false;
                    }
                    break;
                case eParseShate.FuncVarName:
                    funcVarName = word;
                    parseState = eParseShate.FuncVar;
                    break;
                case eParseShate.FuncVar:
                    if(word.Equals("("))  //function
                    {
                        if (validateName(funcVarName))
                        {
                            currentObject = new FunctionObject(funcVarName, currentIsPublic);
                            parseState = eParseShate.GetFuncPrams;
                            canGetParameter = true;
                        }
                        else
                        {
                            parseError =  "The name " + word + " is not a valid name for a function.";
                            parseState = eParseShate.Error;
                            return false;
                        }
                    }
                    else if(word.Equals("=") || word.Equals("."))
                    {
                        if(validateName(word))
                        {
                            currentObject = new VariableObject(funcVarName,currentIsPublic);
                            if (word.Equals("="))
                                parseState = eParseShate.GetVarInitValue;
                            else
                            {
                                currentObject.ParseWord("\"\"");
                                parseState = addObject(currentObject);                                
                            }
                        }
                        else
                        {
                            parseError =  "The name " + funcVarName + " is not a valid name for a variable.";
                            parseState = eParseShate.Error;
                            return false;
                        }                        
                    }
                    else
                    {
                        parseState = eParseShate.Error;
                        parseError = "Expecting ( = or ., found " + word;
                        return false;
                    }
                    break;
                case eParseShate.GetVarInitValue:
                    if (word.Equals("."))
                        parseState = addObject(currentObject);
                    else if (!currentObject.ParseWord(word))
                    {
                        parseError = currentObject.ParseError;
                        parseState = eParseShate.Error;
                        return false;
                    }
                                                            
                    break;
                case eParseShate.GetFuncPrams:
                    if (word.Equals(")"))
                        parseState = eParseShate.GetBlockStart;
                    else if(word.Equals(","))
                    {
                        if (canGetParameter)
                        {
                            parseError = "Expecting to find a paramter and found a , insted.";
                            parseState = eParseShate.Error;
                            return false;
                        }
                        else
                            canGetParameter = true;
                    }
                    else if(canGetParameter)
                    {
                        if(validateName(word))
                        {
                            FunctionObject func = currentObject as FunctionObject;
                            if(func != null)
                            {
                                if (!func.AddParameter(word))
                                {
                                    parseError = "The function " + func.Name + " has two parameters withe the same name (" + word + ").";
                                    parseState = eParseShate.Error;
                                    return false;
                                }
                                else
                                    canGetParameter = false;
                            }
                            else
                            {
                                parseError =  "Compiler error! Expecting a type of function object in currentObject(IKOSppObject).";
                                parseState = eParseShate.Error;
                                return false;
                            }
                        }
                        else
                        {
                            parseError =  "The name " + word + " is not a valid name for a function parameter.";
                            parseState = eParseShate.Error;
                            return false;
                        }
                    }
                    else
                    {
                        parseError =  "Expecting to find a , or ) but found " + word + " insted";
                        parseState = eParseShate.Error;
                        return false;
                    }
                    break;
                case eParseShate.GetBlockStart:
                    blockParser = new BlockParser(currentObject,"ClassObject:"+currentObject.Name);
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
                            parseState = addObject(currentObject);                           
                            blockParser = null;
                            funcVarName = null;
                            parseState = eParseShate.Start;
                        }
                    }
                    break;
                case eParseShate.Error:
                    return false;
                case eParseShate.Done:
                    return false;
                default:                    
                    parseError = "Compiner error! Unhandled state in ClassObject: " + parseState.ToString(); 
                    parseState = eParseShate.Error;
                    return false;
            }
            return true;
        }
        private eParseShate addObject(IKOSppObject obj)
        {
            if (KOSObjects.Any(x => x.Name == obj.Name))
            {
                parseError = "The object " + obj.Name + " has the same name a another object in this class.";
                return eParseShate.Error;
            }
            else
            {
                KOSObjects.Add(obj);
                currentObject = null;              
            }
            return eParseShate.Start;
        }
        private bool validateName(string word)
        {
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
