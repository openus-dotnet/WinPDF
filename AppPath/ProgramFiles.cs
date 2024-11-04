namespace Openus.AppPath
{
    public static class ProgramFiles
    {
        private static string ProgramRoot
            = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        private static string Desktop
            = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        public static string GitHubApiUrl = @"https://api.github.com/repos/openus-dotnet/winpdf/releases/latest";
        public static string DesktopShortcutPath = Path.Combine(Desktop, "WinPDFv2.lnk");

        public static string ProgramPath = Path.Combine(ProgramRoot, Constant.Company, Constant.App);
        public static string ProgramName = "Openus.WinPDFv2.exe";
        
        public static string InstallerPath = Path.Combine(ProgramRoot, Constant.Company, Constant.Installer);
        public static string InstallerName = "Openus.WinPDFv2.Installer.exe";
    }
}