namespace Openus.AppPath
{
    public static class AppData
    {
        private static string LocalAppData 
            = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        public static string Root = Path.Combine(LocalAppData, Constant.Company, Constant.App);
        public static string Data = Path.Combine(Root, "data");
        public static string Lang = Path.Combine(Root, "lang");
    }
}