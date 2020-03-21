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
            return base.AcceptsFile(headerPath, filePath) && new FileInfo(filePath).Length == RevisionManager.GetSaveFileSizes(_revision)?.Size_main;
        }
    }
}
