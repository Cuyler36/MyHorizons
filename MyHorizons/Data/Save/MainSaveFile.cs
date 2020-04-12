using MyHorizons.Data.PlayerData;
using MyHorizons.Data.TownData;
using System;
using System.Collections.Generic;
using System.IO;

namespace MyHorizons.Data.Save
{
    public sealed class MainSaveFile : SaveBase
    {
        private readonly List<PlayerSave> _playerSaves = new List<PlayerSave>();
        
        public int NumPlayers => _playerSaves.Count;
        public readonly Town? Town;

        public MainSaveFile(in string headerPath, in string filePath)
        {
            // TODO: IProgress<float> needs to be passed to load
            if (AcceptsFile(headerPath, filePath) && Load(headerPath, filePath, null))
            {
                // Load player save files
                foreach (var dir in Directory.GetDirectories(Path.GetDirectoryName(filePath)))
                {
                    var playerSave = new PlayerSave(this, dir, _revision);
                    if (playerSave.Valid)
                        _playerSaves.Add(playerSave);
                }

                Town = new Town(this);
            }
        }

        public override bool AcceptsFile(in string headerPath, in string filePath)
        {
            return base.AcceptsFile(headerPath, filePath) && new FileInfo(filePath).Length == RevisionManager.GetSaveFileSizes(_revision).Size_main;
        }

        public override bool Save(in string? filePath, IProgress<float>? progress)
        {
            Town?.Save();

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
