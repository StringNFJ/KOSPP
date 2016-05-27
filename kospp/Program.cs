using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace kospp
{
    class Program
    {
        static void Main(string[] args)
        {
            string FileName = "Class1.kpp";
            Compiler a = new Compiler(@"C:\Games\SteamApps\common\Kerbal Space Program\Ships\Script\", FileName);
            Console.ReadKey();
        
        }
    }
}
