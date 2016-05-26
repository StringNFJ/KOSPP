using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kospp
{
    class BlockParser
    {
        private string name;
        private ICodeParser parser;
        private string error;
        private int internalBlockCount;   
        public string Error {get {return error;}}
        public BlockParser(ICodeParser pParser,string pName)
        {
            name = pName;
            parser = pParser;
            error = "";
            internalBlockCount = 0;
        }
         public bool HasParseError
        {
            get 
            {
                if (error == null || error == "")
                    return false;
                else
                    return true;
            }
        } 
        public bool BlockStart(string word)
        {
            if (word.Equals("{"))
            {
                return true;
            }
            else
            {
                error = "Expecting { at start of block. found " + word + " instead.";
                return false;
            }
        }
        public bool GetBlock(string word)
        {

            if (word.Equals("{"))
            {
                internalBlockCount++;
                Console.WriteLine(name + " bracket count: " + internalBlockCount);
            }
            else if (word.Equals("}") && internalBlockCount == 0) //end of block
            {
                if (!parser.IsParseComplete)
                {
                    parser.ParseWord(word);
                    if (!parser.IsParseComplete)
                         error = "Block ended before pharsing was complete";
                }                   
                return false;
            }
            else if (word.Equals("}"))
            {
                internalBlockCount--;
                Console.WriteLine(name + " bracket count: " + internalBlockCount);
            }
            bool parseResult = parser.ParseWord(word);
            error = parser.ParseError;
            return parseResult;
        }
    }
}
