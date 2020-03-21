using MyHorizons.Encryption;
using MyHorizons.Hash;
using System;
using System.IO;

namespace MyHorizons.Data.Save
{
    public sealed class MainSaveFile : SaveBase
    {
        public MainSaveFile(in string headerPath, in string filePath)
        {
            // TODO: IProgress<float> needs to be passed to load
            if (AcceptsFile(headerPath, filePath) && Load(File.ReadAllBytes(headerPath), File.ReadAllBytes(filePath), null))
            {
                _saveFile = this;
            }
        }

        public override bool AcceptsFile(in string headerPath, in string filePath)
        {
            return base.AcceptsFile(headerPath, filePath) && new FileInfo(headerPath).Length == HEADER_FILE_SIZE
                && new FileInfo(filePath).Length == RevisionManager.GetSaveFileSizes(_revision)?.Size_main;
        }

        public override bool Load(in byte[] headerData, in byte[] fileData, IProgress<float> progress)
        {
            _rawData = null;
            try
            {
                _rawData = SaveEncryption.Decrypt(headerData, fileData);
            }
            finally { }
            return _rawData != null;
        }

        public override bool Save(in string filePath, IProgress<float> progress)
        {
            if (_rawData != null)
            {
                try
                {
                    // Update hashes
                    TryUpdateFileHashes(_rawData);

                    // Encrypt and save file + header
                    var (fileData, headerData) = SaveEncryption.Encrypt(_rawData, (uint)DateTime.Now.Ticks);
                    var headerFilePath = Path.Combine(Path.GetDirectoryName(filePath), $"{Path.GetFileNameWithoutExtension(filePath)}Header.dat");
                    File.WriteAllBytes(filePath, fileData);
                    File.WriteAllBytes(headerFilePath, headerData);
                    return true;
                }
                finally { }
            }
            return false;
        }

        private static void TryUpdateFileHashes(in byte[] data)
        {
            HashInfo selectedInfo = null;
            foreach (var info in HashInfo.VersionHashInfoList)
            {
                var valid = true;
                for (var i = 0; i < 4; i++)
                {
                    if (info.RevisionMagic[i] != BitConverter.ToUInt32(data, i * 4))
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid)
                {
                    selectedInfo = info;
                    break;
                }
            }

            if (selectedInfo != null)
            {
                HashRegionSet thisFileSet = selectedInfo[(uint)data.Length];
                if (thisFileSet != null)
                {
                    Console.WriteLine($"{thisFileSet.FileName} rev {selectedInfo.RevisionId} detected!");
                    foreach (var hashRegion in thisFileSet)
                    {
                        Murmur3.UpdateMurmur32(data, hashRegion.HashOffset, hashRegion.BeginOffset, hashRegion.Size);
                    }
                }
            }
        }
    }
}
