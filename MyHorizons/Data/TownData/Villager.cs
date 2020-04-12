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

        private MainSaveFile MainSaveFile { get; }

        public Villager(MainSaveFile mainSaveFile, int idx)
        {
            MainSaveFile = mainSaveFile;
            Index = idx;
            
            var offsets = MainOffsets.GetOffsets(MainSaveFile.GetRevision());
            Offset = offsets.Offset_Vilagers + idx * offsets.Villager_Size;

            Species = MainSaveFile.ReadU8(Offset + offsets.Villager_Species);
            VariantIdx = MainSaveFile.ReadU8(Offset + offsets.Villager_Variant);
            Personality = MainSaveFile.ReadU8(Offset + offsets.Villager_Personality);
            Catchphrase = MainSaveFile.ReadString(Offset + offsets.Villager_Catchphrase, offsets.Villager_CatchphraseLength); // Not sure about the size.

            var ftr = new Item[offsets.Villager_FurnitureCount];
            for (var i = 0; i < ftr.Length; i++)
                ftr[i] = new Item(MainSaveFile, Offset + offsets.Villager_Furniture + i * 0x2C);
            Furniture = new ItemCollection(ftr);
        }

        public void Save()
        {
            var offsets = MainOffsets.GetOffsets(MainSaveFile.GetRevision());

            MainSaveFile.WriteU8(Offset + offsets.Villager_Species, Species);
            MainSaveFile.WriteU8(Offset + offsets.Villager_Variant, VariantIdx);
            MainSaveFile.WriteU8(Offset + offsets.Villager_Personality, Personality);
            MainSaveFile.WriteString(Offset + offsets.Villager_Catchphrase, Catchphrase, 12);

            for (var i = 0; i < Furniture.Count; i++)
                Furniture[i].Save(MainSaveFile, Offset + offsets.Villager_Furniture + i * 0x2C);
        }
    }
}
