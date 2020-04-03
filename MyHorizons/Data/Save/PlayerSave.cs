using MyHorizons.Data.PlayerData;
using System;
using System.IO;

namespace MyHorizons.Data.Save
{
    public sealed class PlayerSave
    {
        public readonly int Index;
        public readonly bool Valid;

        public readonly Player Player;

        private PersonalSaveFile? _personalSave;
        private PhotoStudioIslandSaveFile? _photoStudioIslandSave;
        private PostBoxSaveFile? _postBoxSave;
        private ProfileSaveFile? _profileSave;

        private readonly SaveRevision _revision;

        public PlayerSave(in string folder, in SaveRevision revision)
        {
            if (Directory.Exists(folder))
            {
                var folderName = new DirectoryInfo(folder).Name;
                if (folderName.StartsWith("Villager") && int.TryParse(folderName.Substring(8, 1), out var idx) && idx > -1 && idx < 8)
                {
                    Index = idx;
                    _revision = revision;
                    ProcessFolder(folder);
                    if (_personalSave == null)
                        throw new NullReferenceException("Personal Save File could not be loaded!");
                    
                    Player = new Player(idx, _personalSave);
                    // TODO: Valid should only be set to true when all player save files are found and loaded correctly.
                    Valid = true;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("The player isn't a valid inde (0-7)!");
                }
            }
            else
            {
                throw new DirectoryNotFoundException("The player directory doesn't exist!");
            }
        }

        public PersonalSaveFile GetPersonalSave() => _personalSave ?? throw new NullReferenceException("PersonalSaveFile does not exist!");

        public bool Save(in string folderPath)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            Player.Save();
            var success = false;
            if (_personalSave != null)
                success |= !_personalSave.Save(null);
            if (_photoStudioIslandSave != null)
                success |= !_photoStudioIslandSave.Save(null);
            if (_postBoxSave != null)
                success |= !_postBoxSave.Save(null);
            if (_profileSave != null)
                success |= !_profileSave.Save(null);
            return !success;
        }

        private void ProcessFolder(in string folder)
        {
            var saveSizes = RevisionManager.GetSaveFileSizes(_revision);
            foreach (var file in Directory.GetFiles(folder, "*.dat"))
            {
                if (!file.EndsWith("Header.dat"))
                {
                    var headerFile = Path.Combine(folder, $"{Path.GetFileNameWithoutExtension(file)}Header.dat");
                    var fileSize = new FileInfo(file).Length;
                    if (fileSize == saveSizes.Size_personal && File.Exists(headerFile))
                    {
                        if (_personalSave == null)
                            _personalSave = new PersonalSaveFile(headerFile, file);
                    }
                    else if (fileSize == saveSizes.Size_photo_studio_island)
                    {
                        if (_photoStudioIslandSave == null)
                            _photoStudioIslandSave = new PhotoStudioIslandSaveFile(headerFile, file);
                    }
                    else if (fileSize == saveSizes.Size_postbox)
                    {
                        if (_postBoxSave == null)
                            _postBoxSave = new PostBoxSaveFile(headerFile, file);
                    }
                    else if (fileSize == saveSizes.Size_profile)
                    {
                        if (_profileSave == null)
                            _profileSave = new ProfileSaveFile(headerFile, file);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(file), "An invalid file was found in the Villager directory!"); 
                    }
                }
            }
        }
    }
}
