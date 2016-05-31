using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kospp
{
    class SetToObject : ICodeParser
    {
        private bool isSet;
        private string error;
        private bool done;
        private string name;
        private string value;
        private List<IKOSppObject> KOSObjects;
        public SetToObject(List<IKOSppObject> pKOSObjects)
        {
            done = false;
            error = null;
            name = "";
            value = "";
            KOSObjects = pKOSObjects;
            isSet = false;
        }
        public string Name { get { return name; } }
        public string Value { get { return value; } }
        public string KOSCode 
        { 
            get 
            {
                if (isSet)
                    return name + ".";
                else
                    return "set " + name + " to " + value + ".";
            } 
        }
        public string ParseError
        {
            get { return error; }
        }

        public bool IsParseComplete
        {
            get { return done; }
        }

        public bool HasParseError
        {
            get { return (error != null); }
        }

        public bool Parse(WordEngine oWordEngine)
        {
            done = false;
            error = null;
            name = "";
            value = "";
            isSet = false;
            if (oWordEngine.Current == "set")
            {
                name = oWordEngine.NextNonWhitespace;
                IKOSppObject nameObject = KOSObjects.SingleOrDefault(x => x.Name == name);
                if (nameObject != null && nameObject.GetType() == typeof(PropertyObject))
                    isSet = true;
                else
                    name = Phase2Compiler.changeVariable(name, KOSObjects);
                if (name != null)
                {
                    if (oWordEngine.NextNonWhitespace == "to")
                    {
                        while (oWordEngine.NextNonWhitespace != ".")
                        {
                            if (oWordEngine.Current == null)
                            {
                                error = "Expecting  ., found end of file insted.";
                                return false;
                            }
                            else
                                value += Phase2Compiler.changeVariable(oWordEngine.Current, KOSObjects);
                        }
                        done = true;
                        if (isSet)
                            name = string.Format(nameObject.CallString(false), value);
                        return true;
                    }
                    else
                        error = "Expecting to, found " + oWordEngine.Current + " insted.";
                }
                else
                    error = "Expecting to, end of file insted.";
            }
            else
                error = "Expecting set, found " + oWordEngine.Current + " insted.";
            return false;
        }


    }
}
