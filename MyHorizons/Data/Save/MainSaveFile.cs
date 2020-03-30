using MyHorizons.Data.PlayerData;
using MyHorizons.Data.TownData;
using System;
using System.Collections.Generic;
using System.IO;

namespace MyHorizons.Data.Save
{
    public sealed class MainSaveFile : SaveBase
    {
        private static MainSaveFile? _saveFile;
        private readonly List<PlayerSave> _playerSaves = new List<PlayerSave>();
        
        public static MainSaveFile Singleton() => _saveFile ?? throw new NullReferenceException("Main save file was null!");

        public int NumPlayers => _playerSaves.Count;
        public readonly Villager[] Villagers = new Villager[10];
        public readonly DesignPattern[] DesignPatterns = new DesignPattern[50];

        public MainSaveFile(in string headerPath, in string filePath)
        {
            // TODO: IProgress<float> needs to be passed to load
            if (AcceptsFile(headerPath, filePath) && Load(headerPath, filePath, null))
            {
                _saveFile = this;

                // Load player save files
                foreach (var dir in Directory.GetDirectories(Path.GetDirectoryName(filePath)))
                {
                    var playerSave = new PlayerSave(dir, _revision.Value);
                    if (playerSave.Valid)
                        _playerSaves.Add(playerSave);
                }

                // Load villagers
                for (var i = 0; i < 10; i++)
                    Villagers[i] = new Villager(i);

                for (var i = 0; i < 50; i++)
                    DesignPatterns[i] = new DesignPattern(i);
            }
        }

        public override bool AcceptsFile(in string headerPath, in string filePath)
        {
            return base.AcceptsFile(headerPath, filePath) && new FileInfo(filePath).Length == RevisionManager.GetSaveFileSizes(_revision)?.Size_main;
        }

        public override bool Save(in string filePath, IProgress<float>? progress)
        {
            // Save Villagers
            foreach (var villager in Villagers)
                villager.Save();

            foreach (var pattern in DesignPatterns)
                pattern.Save();

            if (base.Save(filePath, progress))
            {
                // Save Players
                var dir = Path.GetDirectoryName(filePath);
                foreach (var playerSave in _playerSaves)
                {
                    if (!playerSave.Save(Path.Combine(dir, $"Villager{playerSave.Index}")))
                        return false;
                }
                return true;
            }
            return false;
        }

        public Player GetPlayer(int index) => _playerSaves[index].Player;

        public List<PlayerSave> GetPlayerSaves() => _playerSaves;
    }
}
