using MyHorizons.Data.PlayerData;
using System;
using System.IO;

namespace MyHorizons.Data.Save
{
    public sealed class PlayerSave
    {
        public readonly int Index;
        public readonly bool Valid;

        public readonly Player? Player;

        private PersonalSaveFile? _personalSave;
        private PhotoStudioIslandSaveFile? _photoStudioIslandSave;
        private PostBoxSaveFile? _postBoxSave;
        private ProfileSaveFile? _profileSave;

        private readonly SaveRevision _revision;

        public PlayerSave(ISaveFile saveFile, in string folder, in SaveRevision revision)
        {
            if (Directory.Exists(folder))
            {
                var folderName = new DirectoryInfo(folder).Name;
                if (folderName.StartsWith("Villager") && int.TryParse(folderName.Substring(folderName.Length - 1, 1), out var index) && index >= 0 && index <= 7)
                {
                    Index = index;
                    _revision = revision;
                    ProcessFolder(folder);
                    if (_personalSave == null)
                        throw new NullReferenceException("Personal Save File could not be loaded!");
                    
                    Player = new Player(saveFile, index, _personalSave);
                    Valid = _personalSave != null && _photoStudioIslandSave != null && _postBoxSave != null && _profileSave != null;
                }
                /* removed until a proper Exception handling
                else
                {
                    throw new ArgumentOutOfRangeException("The player isn't a valid index (0-7)!");
                }*/
            }
            /* removed until a proper Exception handling
            else
            {
                throw new DirectoryNotFoundException("The player directory doesn't exist!");
            }*/
        }

        public PersonalSaveFile GetPersonalSave() => _personalSave ?? throw new NullReferenceException("PersonalSaveFile does not exist!");

        public bool Save(in string folderPath)
        {
            if (Valid && Player != null)
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
            return false;
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
