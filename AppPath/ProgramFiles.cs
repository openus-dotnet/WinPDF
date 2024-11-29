namespace Openus.AppPath
{
    public static class ProgramFiles
    {
        private static string ProgramRoot
            = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        private static string Desktop
            = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        public static string GitHubApiUrl = @$"https://api.github.com/repos/openus-dotnet/{Constant.AppName.ToLower()}/releases/latest";
        public static string DesktopShortcutPath = Path.Combine(Desktop, $"{Constant.AppName}.lnk");

        public static string ProgramPath = Path.Combine(ProgramRoot, Constant.Company, Constant.AppName);
        public static string ProgramName = $"Openus.{Constant.AppName}.exe";

        public static string InstallerPath = Path.Combine(ProgramRoot, Constant.Company, Constant.Installer);
        public static string InstallerName = $"Openus.{Constant.AppName}.{Constant.Installer}.exe";
    }
}