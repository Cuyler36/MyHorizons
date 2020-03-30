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

        public Villager(int idx)
        {
            Index = idx;
            var save = MainSaveFile.Singleton();
            var offsets = MainOffsets.GetOffsets(save.GetRevision());
            Offset = offsets.Offset_Vilagers + idx * offsets.Villager_Size;

            Species = save.ReadU8(Offset + offsets.Villager_Species);
            VariantIdx = save.ReadU8(Offset + offsets.Villager_Variant);
            Personality = save.ReadU8(Offset + offsets.Villager_Personality);
            Catchphrase = save.ReadString(Offset + offsets.Villager_Catchphrase, offsets.Villager_CatchphraseLength); // Not sure about the size.

            var ftr = new Item[offsets.Villager_FurnitureCount];
            for (var i = 0; i < ftr.Length; i++)
                ftr[i] = new Item(save, Offset + offsets.Villager_Furniture + i * 0x2C);
            Furniture = new ItemCollection(ftr);
        }

        public void Save()
        {
            var save = MainSaveFile.Singleton();
            var offsets = MainOffsets.GetOffsets(save.GetRevision());

            save.WriteU8(Offset + offsets.Villager_Species, Species);
            save.WriteU8(Offset + offsets.Villager_Variant, VariantIdx);
            save.WriteU8(Offset + offsets.Villager_Personality, Personality);
            save.WriteString(Offset + offsets.Villager_Catchphrase, Catchphrase, 12);

            for (var i = 0; i < Furniture.Count; i++)
                Furniture[i].Save(save, Offset + offsets.Villager_Furniture + i * 0x2C);
        }
    }
}
