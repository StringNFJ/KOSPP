using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace kospp
{
    class Compiler
    {
        #region private vars
        private char[]              strimgSep = new char[] {'"'};
        private char[]              allSpecChars = new char[] { '.', '{' ,'}','(',')','=',',','"',' ', '\t'};
        private List<string>        errors;
        private List<string>        warinings;
        private List<IKOSppObject>  KOSObjects;
        private IKOSppObject        currentObject;
        private WordEngine          oWordEngine;
        #endregion

        public Compiler(String ScriptDir, String KOSppFileName)
        {
            StreamReader KOSPPFile = new StreamReader(ScriptDir + "\\" + KOSppFileName);
            oWordEngine = new WordEngine(KOSPPFile, allSpecChars, strimgSep);
            errors = new List<string>();
            warinings = new List<string>();
            Console.WriteLine("Compiling....");
            KOSObjects = new List<IKOSppObject>();
            if(!processCode())
            {
                 foreach (string e in errors)
                     Console.WriteLine(e);
            }
            else
            {
                foreach (IKOSppObject KOSObj in KOSObjects)
                {
                    string filename = ScriptDir + "\\" + KOSObj.Name + ".ks";
                    Console.WriteLine("Writing class " + KOSObj.Name + " to " + filename);                    
                    StreamWriter KOSFile = new StreamWriter(filename);
                    string result = "";
                    string Text = "";
                    WordEngine oButifier = new WordEngine(KOSObj.GetKOSCode(), 
                            new char[] { '{','}', },
                            new char[] { '"' });
                    bool firstWord = true;
                    while (oButifier.NextLine())
                    {                        
                        while (oButifier.NextWord != null)
                        {
                            if (firstWord)
                            {
                                for (int i = 0; i < (oButifier.Current.Equals("{") ? oButifier.BlockNumber - 1 : oButifier.BlockNumber); i++)
                                    Text += "\t";
                                firstWord = false;
                            }
                            Text += oButifier.Current;
        
                        }
                        Text += "\r\n";
                        firstWord = true;
                    }
                    KOSFile.Write(Text);
                    KOSFile.Close();
                    Console.WriteLine("Write Complete.");
                }
            }
            KOSPPFile.Close();
            Console.WriteLine("Done.");
        }
        
        private bool validateName(string word)
        {
            //TODO:Validate the word.
            return true;
        }       
        private bool processCode()
        {
            currentObject = null;
            while (oWordEngine.NextLine())
            {
                while (oWordEngine.NextWord != null)
                {
                    #region Main Processor
                    switch (oWordEngine.Current.ToLower())
                    {
                        case "class":
                            if (validateName(oWordEngine.NextNonWhitespace))
                                currentObject = new ClassObject(oWordEngine.Current);
                            else
                            {
                                errors.Add(oWordEngine.FormatedPosition + "The name " + oWordEngine.Current + " is not a valid name for a class.\r\n" + oWordEngine.GetLineWithPositionMarker);                                
                                return false;
                            }
                            break;
                        default:
                            if(currentObject == null)
                            {
                                errors.Add(oWordEngine.FormatedPosition + "Expecting class definition, found " + oWordEngine.Current + "." + oWordEngine.GetLineWithPositionMarker);
                                return false;
                            }
                            if(!currentObject.Parse(oWordEngine))
                            {
                                if (currentObject.HasParseError || !currentObject.IsParseComplete)
                                {
                                    if (currentObject.HasParseError)
                                        errors.Add(oWordEngine.FormatedPosition + currentObject.ParseError + "\r\n" + oWordEngine.GetLineWithPositionMarker);
                                    else
                                        errors.Add(oWordEngine.FormatedPosition + "Compiler error! ClassObject parser returned false, but is not complete." + oWordEngine.GetLineWithPositionMarker);
                                    return false;
                                }
                                else
                                    KOSObjects.Add(currentObject);
                            }
                            break;
                    }
                    #endregion
                }
            }
            if(oWordEngine.HasError)
            {
                errors.Add(oWordEngine.FormatedPosition + oWordEngine.Error + "\r\n" + oWordEngine.GetLineWithPositionMarker);
                return false;
            }            
            return true;
        }
    }
}


