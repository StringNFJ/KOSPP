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

        public static int currentLineNumber;
     //   private char[] whitespaces = new char[] { ' ', '\t' };
     //   private char[] specialChars = new char[] { '.', '{' ,'}','(',')','=',',','"'};
        private char[] allSpecChars = new char[] { '.', '{' ,'}','(',')','=',',','"',' ', '\t'};
        private List<string> errors;
        private List<string> warinings;
        private ClassObject classObj;
        private BlockParser blockParser;
        private Stack<eState> StateStack;
        private string stringData;
        private enum eState
        {
            Start,
            ClassName,
            StringStart,
            Skip,
            StringComplete,
            GetBlockStart,
            GetBlock,
            Commit,
            Done,
            Error
        }
        private eState State;   
        public Compiler(StreamReader headerFile,StreamWriter KOSFile)
        {
            StateStack = new Stack<eState>();
            errors = new List<string>();
            warinings = new List<string>();
            int lineCounter = 1;
            currentLineNumber = lineCounter;
            State = eState.Start;
            while(!headerFile.EndOfStream)
            {
                currentLineNumber = lineCounter;
                Console.WriteLine("*******************************************************");
                Console.WriteLine("Line number: " + lineCounter);
                if (!processLine(headerFile.ReadLine(), lineCounter++))
                    break;
            }
            Console.WriteLine("*******************************************************");
            foreach (string e in errors)
                Console.WriteLine(e);
            headerFile.Close();
            if (errors.Count == 0)
            {
                KOSFile.Write(classObj.ConstructorDefinition);
                KOSFile.Write(classObj.FunctionDefinitions);
            }
            KOSFile.Close();
            
        }
        private bool validateName(string word)
        {
            //TODO:Validate the word.
            return true;
        }
        private string[] specialSplit(string str, char[] chrToSpit)
        {
            List<string> result = new List<string>();
            int index = str.IndexOfAny(chrToSpit);
            int pos =0;
            while(index > -1)
            {
                if(index - pos > 0)
                    result.Add(str.Substring(pos, index - pos));
                result.Add(str[index].ToString());
                pos = index+1;
                if (pos >= str.Length)
                    break;
                index = str.IndexOfAny(chrToSpit,pos);
            }
            result.Add(str.Substring(pos));
            return result.ToArray<string>();
        }
        //private Queue<string> getWords(string line)
        //{
        //    Queue<string> wordQueue = new Queue<string>();
        //    string[] lineWords;
        //    lineWords = specialSplit(line, whitespaces);


        //     foreach (string word in lineWords)
        //     {
        //         string currentWord = word;
        //         int index = currentWord.IndexOfAny(specialChars);
        //         while (currentWord.Length > 0)
        //         {
        //             if (index > 0)
        //             {
        //                 wordQueue.Enqueue(currentWord.Substring(0, index));
        //                 currentWord = currentWord.Substring(index);
        //             }
        //             else if (index == 0)
        //             {
        //                 wordQueue.Enqueue(currentWord.Substring(0, 1));
        //                 currentWord = currentWord.Substring(1);
        //             }
        //             else
        //             {
        //                 wordQueue.Enqueue(currentWord);
        //                 currentWord = "";
        //             }
        //             index = currentWord.IndexOfAny(specialChars);
        //         };    
        //     }
        //     return wordQueue;
        //}
        private bool processLine(string line,int lineNumber)
        {
            //Queue<string> lineWords = getWords(line);
            Queue<string> lineWords = new Queue<string>(specialSplit(line, allSpecChars));
            if (lineWords.Count == 0)
            {
                if (State == eState.StringStart)
                {
                    errors.Add(lineNumber + " :: Expecting \" before the end of line.");
                    State = eState.Error;
                    return false;
                }
                else
                    return true;
            }

            while (lineWords.Count > 0)
            {
                String word;
                if(State == eState.StringComplete)
                {
                    word = stringData;
                    State = StateStack.Pop();
                }
                else
                    word = lineWords.Dequeue();
                if (State != eState.StringStart)
                {
                    if (word.Equals("\""))
                    {
                        StateStack.Push(State);
                        stringData = "";
                        State = eState.StringStart;
                    }
                    else
                    {
                        if(word.Trim().Length == 0)
                        {
                            StateStack.Push(State);
                            State = eState.Skip;
                        }
                    }
                }
                switch (State)
                {
                    case eState.Skip:
                        State = StateStack.Pop();
                        break;
                    case eState.StringStart:
                        stringData += word;
                        if (word.Equals("\"") && stringData.Length > 1)
                        {
                            State = eState.StringComplete;
                        }
                        break;
                    case eState.Start:
                        if (word.Equals("class"))
                        {                            
                            State = eState.ClassName;
                        }
                        else
                        {
                            errors.Add(lineNumber + " :: Expecting class definition.");
                            State = eState.Error;
                            return false;
                        }
                        break;
                    case eState.ClassName:
                        if (validateName(word))
                        {
                            classObj = new ClassObject(word);
                            State = eState.GetBlockStart;
                        }
                        else
                        {
                            errors.Add(lineNumber + " :: The name " + word + " is not a valid name for a class.");
                            State = eState.Error;
                            return false;
                        }
                        break;   
                    case eState.GetBlockStart:
                        blockParser = new BlockParser(classObj,"Compiler");
                        if (blockParser.BlockStart(word))
                            State = eState.GetBlock;
                        else
                        {
                            errors.Add(lineNumber + " :: " + blockParser.Error);
                            State = eState.Error;
                            return false;
                        }
                        break;
                     case eState.GetBlock:
                        if(!blockParser.GetBlock(word))
                        {
                            if (blockParser.HasParseError)
                            {
                                errors.Add(lineNumber + " :: " + blockParser.Error);
                                State = eState.Error;
                                return false;
                            }
                            else
                            {
                                State = eState.Commit;
                            }
                        }
                        break;
                    case eState.Commit:
                        blockParser = null;
                        State = eState.Done;
                        break;
                    case eState.Done:
                         return false;
                }
            }
            if(State == eState.StringComplete)
            {
                errors.Add(lineNumber + " :: \" cant be the last character on a line.");
                State = eState.Error;
                return false;
            }
            return true;
        }
    }
}


