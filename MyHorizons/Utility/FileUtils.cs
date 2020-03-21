using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace MyHorizons.Utility
{
    public static class FileUtils
    {
        private static string _resourcesFolderPath;

        public static string GetResourcesPath()
            => _resourcesFolderPath ?? (_resourcesFolderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources"));
    }


    public static class ItemDatabaseLoader
    {
        public static Dictionary<ushort, string> LoadItemDatabase(uint revision)
        {
            var resourcesDir = FileUtils.GetResourcesPath();
            if (Directory.Exists(resourcesDir))
            {
                // TODO: other languages
                var itemDatabasePath = Path.Combine(resourcesDir, $"v{revision}", "Text", "Items", "ItemNames_en.txt");
                if (File.Exists(itemDatabasePath))
                {
                    var dict = new Dictionary<ushort, string>();
                    using (var reader = File.OpenText(itemDatabasePath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Length > 6 && ushort.TryParse(line.Substring(0, 4), NumberStyles.HexNumber, null, out var itemId))
                            {
                                dict.Add(itemId, line.Substring(6));
                            }
                        }
                    }
                    return dict;
                }
            }
            return null;
        }
    }
}
