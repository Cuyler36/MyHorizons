using System;
using System.IO;

namespace MyHorizons.Data.Save
{
    public abstract class SaveBase : ISaveFile
    {
        protected const int HEADER_FILE_SIZE = 0x300;

        protected byte[] _rawData;
        protected SaveRevision? _revision = null;

        public virtual bool AcceptsFile(in string headerPath, in string filePath)
        {
            try
            {
                using (var reader = File.OpenRead(headerPath))
                {
                    var data = new byte[128];
                    if (reader.Read(data, 0, 128) == 128)
                        return (_revision = RevisionManager.GetFileRevision(data)) != null;
                }
            }
            finally { }
            return false;
        }

        public virtual bool Load(in string headerPath, in string filePath, IProgress<float> progress)
            => Load(File.ReadAllBytes(headerPath), File.ReadAllBytes(filePath), progress);
        public abstract bool Load(in byte[] headerData, in byte[] fileData, IProgress<float> progress);
        public abstract bool Save(in string filePath, IProgress<float> progress);
    }
}
