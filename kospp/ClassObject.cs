using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kospp
{
    class ClassObject : IKOSppObject, IBlockProcessor
    {
        private enum eParseShate
        {
            Start,
            MainLoop,
            Done,            
            FuncVar,
            GetFuncPrams,
            GetVarInitValue,
            Property,
            SendWordsEngineToObject,
            GetBlockStart,
            GetBlock,
            Error
        }

        #region private vars
        private                     eParseShate parseState;
        private string              name;
        private List<IKOSppObject>  KOSObjects;
        private IKOSppObject        currentObject;
        private string              parseError;
        private bool                currentIsPublic;        
        private bool                canGetParameter;
        #endregion

        public ClassObject(string pName)
        {
            name = pName;
            KOSObjects = new List<IKOSppObject>();
            parseState = eParseShate.Start;
            parseError = "";
        }

        #region private code
        private string      ConstructorDefinition
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
                funcCode = funcCode.Insert(funcCode.IndexOf("{\r\n")+3, "//Class lexicon\r\n\t" + LexiconEntry + 
                    "\t\tset this to " + name + "." +
                    "\r\n//Do not edit the code above here unless you know what you doing!!!\r\n//User constructor code\r\n"
                    );
                funcCode = "//Constructor\r\nglobal " + name + " is _" + name + "@.\r\n\r\n" + funcCode;
                return funcCode;
            }
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
            return eParseShate.MainLoop;
        }
        private bool        validateName(string word)
        {
            return true;
        }
        #endregion

        #region IKOSppObject
        public string   Name
        {
            get { return name; }
        }
        public string   LexiconEntry
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
        public bool     IsPublic
        {
            get { return true; }
        }
        public string   GetKOSCode()
        {            
            string func = "//Properties\r\n";
            foreach(IKOSppObject p in KOSObjects.Where(x=> x.GetType() == typeof(PropertyObject)))
                func += p.GetKOSCode();
            func += "//public functions\r\n";
            foreach(IKOSppObject f in KOSObjects.Where(x=>x.IsPublic == true && x.Name != name && x.GetType() == typeof(FunctionObject)))
                func += f.GetKOSCode();
            func += "//private functions\r\n";
            foreach(IKOSppObject f in KOSObjects.Where(x=>x.IsPublic == false && x.Name != name && x.GetType() == typeof(FunctionObject)))
                func += f.GetKOSCode();
            return ConstructorDefinition + func;
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
                        case "private":
                            currentIsPublic = false;
                            parseState = eParseShate.FuncVar;
                            break;
                        case "public":
                            currentIsPublic = true;
                            parseState = eParseShate.FuncVar;
                            break;
                        case "property":
                            currentIsPublic = true;
                            parseState = eParseShate.Property;
                            break;
                        case WordEngine.BlockEndChar:
                            parseState = eParseShate.Done;
                            return false;
                        default:
                            if (oWordEngine.HasError)
                                parseError = oWordEngine.Error;
                            else
                                parseError = "Expecting private public or property, found " + oWordEngine.Current;
                            parseState = eParseShate.Error;
                            return false;
                    }
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
                            parseState = addObject(currentObject);
                    }
                    break;
                case eParseShate.Property:
                    if (validateName(oWordEngine.NextNonWhitespace))
                    {
                        currentObject = new PropertyObject(oWordEngine.Current, currentIsPublic);
                        parseState = eParseShate.SendWordsEngineToObject;
                    }
                    else
                    {
                        parseError =  "The name " + oWordEngine.Current + " is not a valid name for a property.";
                        parseState = eParseShate.Error;
                        return false;
                    }
                    break;
                case eParseShate.FuncVar:
                    string funcVarName = oWordEngine.CurrentNonWhitespace;
                    if(oWordEngine.NextNonWhitespace.Equals("("))  //function
                    {
                        if (validateName(funcVarName))
                        {
                            currentObject = new FunctionObject(funcVarName, currentIsPublic);
                            parseState = eParseShate.GetFuncPrams;
                            canGetParameter = true;
                        }
                        else
                        {
                            parseError =  "The name " + oWordEngine.Current + " is not a valid name for a function.";
                            parseState = eParseShate.Error;
                            return false;
                        }
                    }
                    else if(oWordEngine.Current.Equals("=") || oWordEngine.Current.Equals("."))
                    {
                        if(validateName(oWordEngine.Current))
                        {
                            currentObject = new VariableObject(funcVarName,currentIsPublic);
                            if (oWordEngine.Current.Equals("="))
                                parseState = eParseShate.SendWordsEngineToObject;
                            else
                                parseState = addObject(currentObject);
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
                        parseError = "Expecting ( = or ., found " + oWordEngine.Current;
                        return false;
                    }
                    break;
                case eParseShate.GetFuncPrams:
                    if (oWordEngine.CurrentNonWhitespace.Equals(")"))
                        parseState = eParseShate.SendWordsEngineToObject;
                    else if(oWordEngine.Current.Equals(","))
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
                        if(validateName(oWordEngine.Current))
                        {
                            FunctionObject func = currentObject as FunctionObject;
                            if(func != null)
                            {
                                if (!func.AddParameter(oWordEngine.Current))
                                {
                                    parseError = "The function " + func.Name + " has two parameters withe the same name (" + oWordEngine.Current + ").";
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
                            parseError =  "The name " + oWordEngine.Current + " is not a valid name for a function parameter.";
                            parseState = eParseShate.Error;
                            return false;
                        }
                    }
                    else
                    {
                        parseError =  "Expecting to find a , or ) but found " + oWordEngine.Current + " insted";
                        parseState = eParseShate.Error;
                        return false;
                    }
                    break;
                case eParseShate.Error:
                    if (!HasParseError)
                        parseError = "Compiler error! The parser " + name + " was put in Error state, but had no error.";
                    return false;
                case eParseShate.Done:
                    return false;
                default:                    
                    parseError = "Compiner error! Unhandled state in ClassObject: " + name + "" + parseState.ToString(); 
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
