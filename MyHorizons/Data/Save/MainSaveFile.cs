using System.Collections.Generic;
using System.IO;

namespace MyHorizons.Data.Save
{
    public sealed class MainSaveFile : SaveBase
    {
        private static SaveBase _saveFile;
        private readonly List<PlayerSave> _playerSaves;

        public static SaveBase Singleton() => _saveFile;

        public int NumPlayers => _playerSaves.Count;
        public readonly Villager[] Villagers = new Villager[10];

        public MainSaveFile(in string headerPath, in string filePath)
        {
            // TODO: IProgress<float> needs to be passed to load
            if (AcceptsFile(headerPath, filePath) && Load(File.ReadAllBytes(headerPath), File.ReadAllBytes(filePath), null))
            {
                _saveFile = this;

                // Load player save files
                _playerSaves = new List<PlayerSave>();
                foreach (var dir in Directory.GetDirectories(Path.GetDirectoryName(filePath)))
                {
                    var playerSave = new PlayerSave(dir, _revision.Value);
                    if (playerSave.Valid)
                        _playerSaves.Add(playerSave);
                }

                // Load villagers
                for (var i = 0; i < 10; i++)
                    Villagers[i] = new Villager(i);
            }
        }

        public override bool AcceptsFile(in string headerPath, in string filePath)
        {
            return base.AcceptsFile(headerPath, filePath) && new FileInfo(filePath).Length == RevisionManager.GetSaveFileSizes(_revision)?.Size_main;
        }

        public Player GetPlayer(int index) => _playerSaves[index].Player;

        public List<PlayerSave> GetPlayerSaves() => _playerSaves;
    }
}
