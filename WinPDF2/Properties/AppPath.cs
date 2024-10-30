using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openus.WinPDF2.Properties
{
    public static class AppPath
    {
        public static string AppMainPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Openus.NET", "winpdf");
        public static string AppDataPath = Path.Combine(AppMainPath, "data");
        public static string AppIconPath = Path.Combine(AppMainPath, "openus.ico");
    }
}
