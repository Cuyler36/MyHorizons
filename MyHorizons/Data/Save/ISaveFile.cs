using System;

namespace MyHorizons.Data.Save
{
    public interface ISaveFile
    {
        bool AcceptsFile(in string headerPath, in string filePath);
        bool Load(in string headerPath, in string filePath, IProgress<float> progress);
        bool Load(in byte[] headerData, in byte[] fileData, IProgress<float> progress);
        bool Save(in string filePath, IProgress<float> progress);
    }
}