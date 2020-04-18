using MyHorizons.Data.Save;
using MyHorizons.Data.TownData.Offsets;

namespace MyHorizons.Data.TownData
{
    public sealed class Villager
    {
        public static readonly string[] Personalities = { "Lazy (M)", "Jock (M)", "Cranky (M)", "Smug (M)", "Normal (F)", "Peppy (F)", "Snooty (F)", "Uchi (F)", "Not Set" };

        public readonly int Index;
        private readonly int Offset;

        public byte Species;
        public byte VariantIdx;
        public byte Personality;
        public string Catchphrase;
        public ItemCollection Furniture;
        public ItemCollection Wallpaper;
        public ItemCollection Flooring;

        private ISaveFile SaveFile { get; }

        public Villager(ISaveFile saveFile, int idx)
        {
            SaveFile = saveFile;
            Index = idx;
            
            var offsets = MainOffsets.GetOffsets(SaveFile.GetRevision());
            Offset = offsets.Offset_Vilagers + idx * offsets.Villager_Size;

            Species = SaveFile.ReadU8(Offset + offsets.Villager_Species);
            VariantIdx = SaveFile.ReadU8(Offset + offsets.Villager_Variant);
            Personality = SaveFile.ReadU8(Offset + offsets.Villager_Personality);
            Catchphrase = SaveFile.ReadString(Offset + offsets.Villager_Catchphrase, offsets.Villager_CatchphraseLength); // Not sure about the size.

            var ftr = new Item[offsets.Villager_FurnitureCount];
            for (var i = 0; i < ftr.Length; i++)
                ftr[i] = new Item(SaveFile, Offset + offsets.Villager_Furniture + i * 0x2C);
            Furniture = new ItemCollection(ftr);

            Wallpaper = new ItemCollection(new Item[1] { new Item(SaveFile, Offset + offsets.Villager_Wallpaper) });
            Flooring = new ItemCollection(new Item[1] { new Item(SaveFile, Offset + offsets.Villager_Flooring) });
        }

        public bool IsMovingOut()
        {
            var offsets = MainOffsets.GetOffsets(SaveFile.GetRevision());
            return (SaveFile.ReadU8(Offset + offsets.Villager_StateFlags) & offsets.Villager_StateFlagMovingOut) != 0;
        }

        public void SetIsMovingOut(bool movingOut)
        {
            var offsets = MainOffsets.GetOffsets(SaveFile.GetRevision());
            var flags = SaveFile.ReadU8(Offset + offsets.Villager_StateFlags);
            if (movingOut)
                flags |= (byte)offsets.Villager_StateFlagMovingOut;
            else
                flags &= (byte)~offsets.Villager_StateFlagMovingOut;
            SaveFile.WriteU8(Offset + offsets.Villager_StateFlags, flags);
            // TODO: StateFlags + 0x10 always? changes from 0x0A => 0x00 when moving out. Not sure if that's needed.
        }

        public void Save()
        {
            var offsets = MainOffsets.GetOffsets(SaveFile.GetRevision());

            SaveFile.WriteU8(Offset + offsets.Villager_Species, Species);
            SaveFile.WriteU8(Offset + offsets.Villager_Variant, VariantIdx);
            SaveFile.WriteU8(Offset + offsets.Villager_Personality, Personality);
            SaveFile.WriteString(Offset + offsets.Villager_Catchphrase, Catchphrase, 12);

            for (var i = 0; i < Furniture.Count; i++)
                Furniture[i].Save(SaveFile, Offset + offsets.Villager_Furniture + i * 0x2C);
            Wallpaper[0].Save(SaveFile, Offset + offsets.Villager_Wallpaper);
            Flooring[0].Save(SaveFile, Offset + offsets.Villager_Flooring);
        }
    }
}
