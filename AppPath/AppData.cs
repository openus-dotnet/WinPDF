namespace Openus.AppPath
{
    public static class AppData
    {
        private static string LocalAppData 
            = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public static string Root = Path.Combine(LocalAppData, Constant.Company, Constant.AppName);
        public static string Data = Path.Combine(Root, "data");
        public static string Lang = Path.Combine(Root, "lang");
    }
}