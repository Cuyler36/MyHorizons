using MyHorizons.Data.Save;

namespace MyHorizons.Data
{
    public sealed class Villager
    {
        public static readonly string[] Personalities = { "Lazy ♂", "Jock ♂", "Cranky ♂", "Smug ♂", "Normal ♀", "Peppy ♀", "Snooty ♀", "Uchi ♀", "Not Set" };

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
            var save = MainSaveFile.Singleton();
            var offsets = GetOffsetsFromRevision();
            var villagerOffset = offsets.BaseOffset + idx * offsets.Size;
            Species = save.ReadU8(villagerOffset + offsets.Species);
            VariantIdx = save.ReadU8(villagerOffset + offsets.Variant);
            Personality = save.ReadU8(villagerOffset + offsets.Personality);
            Catchphrase = save.ReadString(villagerOffset + offsets.Catchphrase, 12); // Not sure about the size.
        }
    }
}
