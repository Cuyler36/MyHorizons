using System.IO;

namespace MyHorizons.Data.Save
{
    public sealed class PhotoStudioIslandSaveFile : SaveBase
    {
        public PhotoStudioIslandSaveFile(in string headerPath, in string filePath)
        {
            if (AcceptsFile(headerPath, filePath))
                Load(headerPath, filePath, null);
        }

        public override bool AcceptsFile(in string headerPath, in string filePath)
        {
            return base.AcceptsFile(headerPath, filePath) && new FileInfo(filePath).Length == RevisionManager.GetSaveFileSizes(_revision).Size_photo_studio_island;
        }
    }
}
