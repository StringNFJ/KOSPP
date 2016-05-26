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
            string FileName = "Class1";   
            Compiler a = new Compiler(new StreamReader(@"C:\Games\SteamApps\common\Kerbal Space Program\Ships\Script\" + FileName + ".kpp"), new StreamWriter(@"C:\Games\SteamApps\common\Kerbal Space Program\Ships\Script\" + FileName + ".ks"));
            Console.ReadKey();
           
        }
    }
}
