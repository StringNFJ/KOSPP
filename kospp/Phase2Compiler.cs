using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace kospp
{
    class Phase2Compiler 
    {
        private char[]              strimgSep = new char[] {'"'};
        private char[]              SeperatorChars = new char[] {'=','+','-','*','(',')','[',']','.','/',' ','\t'};   
        private List<IKOSppObject>  KOSObjects;
        private WordEngine          oWordEngine;
        private List<string>        errors;
        public Phase2Compiler(string pCode , List<IKOSppObject> pKOSObjects)
        {
            KOSObjects = pKOSObjects;
            oWordEngine = new WordEngine(pCode,SeperatorChars, strimgSep);
            errors = new List<string>();
        }
        public string Errors
        {
            get 
            { 
                string result = "";
                foreach (string err in errors)
                    result += err + "\r\n";
                return result;
            }
        }
        public bool HasError
        {
            get { return errors.Count > 0; }
        }
        public string parse()
        {
            SetToObject oSetToObject = null;
            string result = "";
            //skip to start of user code.
            //while(oWordEngine.NextWord != "code@!$")
            //{
            //   if(oWordEngine.Current == null)
            //   {
            //       errors.Add("User code marker not found.");
            //       return null;
            //   }
            //   result += oWordEngine.Current;
            //}
            while (oWordEngine.NextLine())
            {
                
              //  for (int i = 0; i < oWordEngine.BlockNumber; i++ )
               //     result+="\t";
                while (oWordEngine.NextWord != null)
                {
                    #region Main Processor

                    if (oWordEngine.Current != null)
                    {
                        if (!oWordEngine.IsWhitespace)
                        {
                            if (oWordEngine.Current.Equals("set"))
                            {
                                oSetToObject = new SetToObject(KOSObjects);
                                oSetToObject.Parse(oWordEngine);
                                result += oSetToObject.KOSCode;
                            }
                            else
                                result += changeVariable(oWordEngine.Current, KOSObjects);
                        }
                        else
                            result += oWordEngine.Current;
                    }
                    #endregion
                }
                result += "\r\n";
            }
            if(oWordEngine.HasError)
            {
                errors.Add(oWordEngine.FormatedPosition + oWordEngine.Error + "\r\n" + oWordEngine.GetLineWithPositionMarker);
                return null;
            }
           return result;
        }     
        public static string changeVariable(string word, List<IKOSppObject> ObjectList)
        {
            IKOSppObject KOSObject =  ObjectList.SingleOrDefault(x => x.Name ==word);
            if (KOSObject != null)
            {
                if (KOSObject.GetType() == typeof(VariableObject))
                    return KOSObject.CallString();
                else if (KOSObject.GetType() == typeof(PropertyObject))
                    return KOSObject.CallString();
            }
            return word;
        }
    }
}
