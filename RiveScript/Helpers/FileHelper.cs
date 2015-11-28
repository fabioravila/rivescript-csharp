using System.IO;

namespace RiveScript
{
    internal static class FileHelper
    {
        public static bool CanRead(string path)
        {
            return Can(path, FileAccess.Read);
        }

        public static bool Can(string path, FileAccess access)
        {
            try
            {
                using (var fs = File.Open(path, FileMode.Open, access))
                {
                    fs.Close();
                    return true;
                }
            }
            catch (IOException)
            {
                return false;
            }
        }
    }
}
