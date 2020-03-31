using MyHorizons.Data.Save;
using MyHorizons.Data.TownData.Offsets;
using MyHorizons.History;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MyHorizons.Data.TownData
{
    public sealed class Town : Changeable
    {
        public TownID TownId;
        public Villager[] Villagers = new Villager[10];
        public DesignPattern[] Patterns = new DesignPattern[50];

        public Town()
        {
            var save = MainSaveFile.Singleton();
            var offsets = MainOffsets.GetOffsets(save.GetRevision());

            // Load Town Data
            TownId = save.ReadStruct<TownID>(offsets.Offset_TownId);

            // Load Villagers
            for (var i = 0; i < 10; i++)
                Villagers[i] = new Villager(i);

            // Load Patterns
            for (var i = 0; i < 50; i++)
                Patterns[i] = new DesignPattern(i);
        }

        public string GetName() => TownId.GetName();

        public unsafe void SetName(in string newName)
        {
            if (newName != TownId.GetName())
            {
                var save = MainSaveFile.Singleton();

                // Update any player references first
                foreach (var playerSave in save.GetPlayerSaves())
                    if (playerSave.Player.PersonalId.TownId == TownId)
                        playerSave.Player.PersonalId.TownId.SetName(newName);

                var townIdSize = Marshal.SizeOf(TownId);
                var bytes = new byte[townIdSize];
                fixed (byte* p = bytes)
                    Unsafe.WriteUnaligned(p, TownId);
                
                TownId.SetName(newName);
                
                var newBytes = new byte[townIdSize];
                fixed (byte* p = newBytes)
                    Unsafe.WriteUnaligned(p, TownId);

                // Main Save File
                save.ReplaceAllOccurrences(bytes, newBytes, 0x100);

                // Player Save Files
                foreach (var playerSave in save.GetPlayerSaves())
                    playerSave.GetPersonalSave().ReplaceAllOccurrences(bytes, newBytes, 0x100);
            }
        }

        public Villager GetVillager(int index)
            => index >= Villagers.Length ? throw new ArgumentOutOfRangeException(nameof(index), "Invalid index!") : Villagers[index];

        public DesignPattern GetDesignPattern(int index)
            => index >= Patterns.Length ? throw new ArgumentOutOfRangeException(nameof(index), "Invalid index!") : Patterns[index];

        public void Save()
        {
            var save = MainSaveFile.Singleton();
            var offsets = MainOffsets.GetOffsets(save.GetRevision());
            save.WriteStruct(offsets.Offset_TownId, TownId);

            foreach (var villager in Villagers)
                villager.Save();

            foreach (var pattern in Patterns)
                pattern.Save();
        }
    }
}
