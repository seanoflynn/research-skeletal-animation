using System;

namespace Graphics
{
    public interface AssetImporter
    {
        Type AssetType { get; }
        string[] FileExtensions { get; }
        void Import(string file);
    }
}