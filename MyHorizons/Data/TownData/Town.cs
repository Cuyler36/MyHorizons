using MyHorizons.Data.Save;
using MyHorizons.History;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MyHorizons.Data.TownData
{
    public sealed class Town : Changeable
    {
        public TownID TownId;

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
    }
}
