using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVT.Core.Tests.LoaderTests
{
    public static class LoaderTesterHelper
    {
        public static string GetFileContent(string fileName)
        {        
            //Get the path of the executable
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            //Combine the executable path with the relative file path
            var filePath = Path.Combine(exePath, fileName);
            return System.IO.File.ReadAllText(filePath);
        }
    }
}
