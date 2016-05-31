using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
namespace kospp
{
    
    class WordEngine
    {
        public const int    TAB_SIZE         = 4;
        public const string BlockStartChar   = "{";   
        public const string BlockEndChar     = "}";
        #region private vars
        private string                              originalLine;
        private List<string>                        errorList;
        private char[]                              seperators;
        private char[]                              stringSeperators;
        private int                                 blockNumber;
        private List<string>                        words;
        private int                                 currentWordPosition;
        private int                                 columb;
        private int                                 lineNumber;
        private StreamReader                        inputStream;
        private Dictionary<IBlockProcessor, int>    blockTests;
        private bool                                endOfLine;
        #endregion

        //fixme: need to always make sure the block chars are in the seperator list.
        public WordEngine(StreamReader pInputStream, char[] pSeperators, char[] pStringSeperators = null)
        {
            init(pInputStream, pSeperators, pStringSeperators);
        }
        public WordEngine(string pInputString, char[] pSeperators, char[] pStringSeperators = null)
        {
            //Convert the string to a streamreader.
            byte[] byteArray = Encoding.UTF8.GetBytes(pInputString);
            MemoryStream stream = new MemoryStream(byteArray);
            init(new StreamReader(stream), pSeperators, pStringSeperators);
        }

        #region properties
        public string   Error
        {
            get
            {
                string err = "";
                foreach (string s in errorList)
                    err += s;
                return err;
            }
        }
        public bool     HasError
        {
            get
            {
                if (Error.Trim().Length == 0)
                    return false;
                else
                    return true;
            }
        }
        public bool     EndOfLine
        {
            get { return endOfLine; }
        }
        public string   Current
        {
            get
            {
                if (words != null)
                {
                    if (currentWordPosition >= 0 && currentWordPosition < words.Count)
                        return words[currentWordPosition];
                    else
                        return null;
                }
                //fixme: should this be an error?
                return null;
            }
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string   CurrentNonWhitespace
        {
            get
            {
                if (IsWhitespace)
                    return nextNonWhitespace();
                else
                    return Current;
            }
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string   NextWord
        {
            get { return nextWord(); }
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string   NextNonWhitespaceOnLine
        {
            get { return nextNonWhitespaceOnLine();}
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string   NextNonWhitespace
        {
            get { return nextNonWhitespace(); }
        }
        public string   FormatedPosition
        {
            get
            {
                return LineNumber + ":" + Columb + " :: ";
            }
        }
        public string   GetLineWithPositionMarker
        {
            get
            {
                string markerLine = "";
                for (int i = columb; i > 0; i--)
                    markerLine += " ";
                markerLine += "^";
                return originalLine + "\r\n" + markerLine;
            }
        }
        public int      Columb
        {
            get
            {
                return columb;
            }
        }
        public int      LineNumber
        {
            get
            {
                return lineNumber;
            }
        }
        public int      BlockNumber
        {
            get
            {
                return blockNumber;
            }
        }
        public bool     IsBlockStart
        {
            get
            {
                string currentWord = Current;
                return (currentWord != null  && currentWord.Equals(BlockStartChar.ToString()));
            }
        }
        public bool     IsBlockEnd
        {
            get
            {
                string currentWord = Current;
                return (currentWord != null  && currentWord.Equals(BlockEndChar));
            }
        }
        public bool     IsWhitespace
        {
            get
            {
                if (Current == null || Current.Trim().Length == 0)
                    return true;
                else
                    return false;
            }
        }
        #endregion


        public bool RegisterBlockTest(IBlockProcessor blockProcessor)
        {
            if (IsBlockStart)
                blockTests.Add(blockProcessor, blockNumber);
            else
            {
                if (Current == null || Current.Trim().Length == 0)
                {
                    nextNonWhitespace();
                    if (IsBlockStart)
                        blockTests.Add(blockProcessor, blockNumber);
                    else
                         errorList.Add("Expecting a block, found " + Current);
                }
                else
                    errorList.Add("Expecting a block, found " + Current);
            }            
            return !HasError;
        }
        public bool NextLine()
        {
            if (inputStream.EndOfStream)
                return false;
            originalLine = inputStream.ReadLine();
            currentWordPosition = -1;
            columb = 0;
            lineNumber++;
            endOfLine = false;
            if (stringSeperators != null)
            {
                char currentChar = '\0';
                words = new List<string>(); //possbly reuse?
                int index = originalLine.IndexOfAny(stringSeperators);
                int pos = 0;                
                while(index > -1)
                {
                    String Before = originalLine.Substring(pos, index - pos);
                    if(Before.Length > 0)
                        words.AddRange(specialSplit(Before, seperators));
                    currentChar = originalLine[index];
                    pos = index+1;
                    index = originalLine.IndexOf(currentChar,pos);
                    if(index == -1)
                    {
                        errorList.Add("Found a " + currentChar + " without another one on the same line.");
                        words = null;
                        return false;
                    }
                    else
                    {
                        words.Add(originalLine.Substring(pos-1, (index - pos)+2));
                        pos = index + 1;
                        currentChar = '\0';
                        index = originalLine.IndexOfAny(stringSeperators,pos);
                    }
                }
                words.AddRange(specialSplit(originalLine.Substring(pos), seperators));
            }
            else                
                words = specialSplit(originalLine,seperators);
            return true;
        }
        public string   Peek(int distance)
        {
            int pos = currentWordPosition + distance;
            if (pos >= 0 && pos < words.Count)
                return words[pos];
            else
                return null;
 
        }
        #region private methods
        private void            init(StreamReader pInputStream, char[] pSeperators, char[] pStringSeperators = null)
        {
            seperators = pSeperators;
            stringSeperators = pStringSeperators;
            errorList = new List<string>();
            inputStream = pInputStream;
            lineNumber = 0;
            blockNumber = 0;
            blockTests = new Dictionary<IBlockProcessor, int>();
        }
        private string          nextNonWhitespaceOnLine()
        {
            if (words != null)
            {
                while (nextWord() != null) 
                    if(Current.Trim().Length > 0)                             
                        return Current;
                return null;
            }
            errorList.Add("Called NextNonWhitespace before the first NextLine.");
            return null;
        }
        private string          nextNonWhitespace()
        {
            string w = nextNonWhitespaceOnLine();
            while(endOfLine)
            {
                if (!NextLine())
                    return null;
                w = nextNonWhitespaceOnLine();
            }
            return w;
        }
        private string          nextWord()
        {
            if (words != null)
            {
                if (++currentWordPosition < words.Count)
                {
                    columb += words[currentWordPosition].Length;
                    if (currentWordPosition >= 0 && words[currentWordPosition].Equals("\t"))
                    {
                        while (words[currentWordPosition].Equals("\t"))  //fixme: Skip tabs, still not sure if i want to do this here.
                        {
                            columb += TAB_SIZE;
                            currentWordPosition++;
                            if (currentWordPosition >= words.Count)
                                return null;
                        }
                    }
                    #region block tracking
                    if (words[currentWordPosition].Equals(BlockStartChar))
                        blockNumber++;
                    if (words[currentWordPosition].Equals(BlockEndChar))
                    {
                        blockNumber--;
                        if (blockNumber < 0)
                        {
                            errorList.Add("Block ended before one was started.");
                            return null;
                        }
                        List<IBlockProcessor> removeList= new List<IBlockProcessor>();
                        foreach (IBlockProcessor blockProcessor in blockTests.Keys) //OPT:sorting by the valuse could make this more eficiant...
                        {
                            if (BlockNumber < blockTests[blockProcessor])
                            {
                                String err = blockProcessor.BlockEnd();
                                if (err != null)
                                {
                                    errorList.Add(err);
                                    return null;
                                }
                                removeList.Add(blockProcessor);                              
                            }
                        }
                        foreach (IBlockProcessor blockProcessor in removeList)
                            blockTests.Remove(blockProcessor);
                    }
                    #endregion
                    return words[currentWordPosition];
                }
                else
                {
                    endOfLine = true;
                    return null;
                }               
            }
            errorList.Add("Called NextWord before the first NextLine.");
            return null;
        }
        private List<string>    specialSplit(string str, char[] sep)
        {
            if (str.Length == 1)
                return new List<string>() { str };
            List<string> result = new List<string>();
            int index = str.IndexOfAny(sep);
            int pos =0;
            while(index > -1)
            {
                if(index - pos > 0)
                    result.Add(str.Substring(pos, index - pos));
                result.Add(str[index].ToString());
                pos = index+1;
                if (pos >= str.Length)
                    break;
                index = str.IndexOfAny(sep,pos);
            }
            if(pos < str.Length)
                result.Add(str.Substring(pos));
            return result;
        }
        #endregion
    }
}
