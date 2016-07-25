using System;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace Graphics
{
    public static class Assets
    {
        #region Fields

        private readonly static List<string> ignorableFiles = new List<string>() { ".DS_Store" };
        private readonly static List<string> availableFiles = new List<string>();
        private readonly static List<string> importedFiles = new List<string>();

        private readonly static Dictionary<string, AssetImporter> importers = new Dictionary<string, AssetImporter>();
        private readonly static List<Asset> assets = new List<Asset>();

        #endregion

        #region Importers

        static Assets()
        {
            // search for any asset importers
            var importerTypes = Assembly.GetExecutingAssembly()
                                        .GetTypes()
                                        .Where(t => t != typeof(AssetImporter) && typeof(AssetImporter).IsAssignableFrom(t));

            foreach(var type in importerTypes)
                RegisterImporter((AssetImporter)Activator.CreateInstance(type));

            // scan all files in assets directory
            foreach(var f in Directory.EnumerateFiles("Assets", "*", SearchOption.AllDirectories))
            {
                string file = Path.GetFileName(f);
                if(!ignorableFiles.Contains(file))
                    availableFiles.Add(file);
            }
        }

        public static void RegisterImporter(AssetImporter importer)
        {
            foreach(var e in importer.FileExtensions)
                importers.Add(e, importer);
        }

        #endregion

        #region Importing

        public static void ImportFile<T>(string file)
        {            
            if(importedFiles.Contains(file))
                return;
            importedFiles.Add(file);

            string extension = Path.GetExtension(file);
            if(!importers.ContainsKey(extension))
            {
                Console.WriteLine($"Asset unsupported format ({extension}): {file}");
                return;
            }

            string path = Path.Combine("Assets", importers[extension].AssetType.Name, file);

            importers[extension].Import(path);
            Console.WriteLine($"{typeof(T).Name} imported: {file}");
        }

        public static void ImportByName<T>(string name)
        {
            string extension;

            if(!TryGetExtension<T>(name, out extension))
                return;

            ImportFile<T>(name + extension);
        }

        private static bool TryGetExtension<T>(string name, out string extension)
        {
            foreach(var i in importers.Where(x => x.Value.AssetType == typeof(T)))
            {
                if(availableFiles.Exists(x => name + i.Key == x))
                {
                    extension = i.Key;
                    return true;
                }
            }

            extension = "";
            return false;
        }

        public static void ImportAll()
        {
            ImportAll<VertexShader>();
            ImportAll<FragmentShader>();
            ImportAll<Texture>();
            ImportAll<Material>();
            ImportAll<Model>();
            ImportAll<Animation>();
        }

        public static void ImportAll<T>() where T : Asset
        {
            foreach(var r in importers.Where(x => x.Value.AssetType == typeof(T)).Select(x => x.Key))
            {
                foreach(var f in availableFiles)
                {
                    if(f.EndsWith(r, StringComparison.InvariantCulture))
                        ImportFile<T>(f);
                }
            }
        }

        public static void Register(Asset asset)
        {
            if(!assets.Contains(asset))
                assets.Add(asset);
        }

        #endregion

        #region Retrieve

        public static T Retrieve<T>(string name) where T : Asset
        {
            var asset = (T)assets.Find(x => x is T && x.Name == name);

            if(asset == null)
            {
                ImportByName<T>(name);
                asset = (T)assets.Find(x => x is T && x.Name == name);
            }

            return asset;
        }

        public static T RetrieveFile<T>(string file) where T : Asset
        {
            var asset = (T)assets.Find(x => x is T && x.File == file);

            if(asset == null)
            {
                ImportFile<T>(file);
                asset = (T)assets.Find(x => x is T && x.File == file);
            }

            return asset;
        }

        public static IEnumerable<T> RetrieveAll<T>() where T : Asset
        {
            return assets.Where(x => x is T).Select(x => (T)x);
        }

        #endregion
    }
}