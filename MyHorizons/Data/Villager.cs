using MyHorizons.Data.Save;

namespace MyHorizons.Data
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

        private readonly struct Offsets
        {
            public readonly int BaseOffset;
            public readonly int Size;

            public readonly int Species;
            public readonly int Variant;
            public readonly int Personality;
            public readonly int Catchphrase;

            public Offsets(int baseOffset, int size, int species, int variant, int personality, int catchphrase)
            {
                BaseOffset = baseOffset;
                Size = size;
                Species = species;
                Variant = variant;
                Personality = personality;
                Catchphrase = catchphrase;
            }
        }

        private static readonly Offsets[] VillagerOffsetsByRevision =
        { 
            new Offsets(0x110, 0x12AB0, 0, 1, 2, 0x10014),
            new Offsets(0x120, 0x12AB0, 0, 1, 2, 0x10014)
        };

        private static Offsets GetOffsetsFromRevision() => VillagerOffsetsByRevision[MainSaveFile.Singleton().GetRevision()];

        public Villager(int idx)
        {
            Index = idx;
            var save = MainSaveFile.Singleton();
            var offsets = GetOffsetsFromRevision();
            Offset = offsets.BaseOffset + idx * offsets.Size;

            Species = save.ReadU8(Offset + offsets.Species);
            VariantIdx = save.ReadU8(Offset + offsets.Variant);
            Personality = save.ReadU8(Offset + offsets.Personality);
            Catchphrase = save.ReadString(Offset + offsets.Catchphrase, 12); // Not sure about the size.
        }

        public void Save()
        {
            var save = MainSaveFile.Singleton();
            var offsets = GetOffsetsFromRevision();

            save.WriteU8(Offset + offsets.Species, Species);
            save.WriteU8(Offset + offsets.Variant, VariantIdx);
            save.WriteU8(Offset + offsets.Personality, Personality);
            save.WriteString(Offset + offsets.Catchphrase, Catchphrase, 12);
        }
    }
}
