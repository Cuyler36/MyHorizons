using System;

namespace MyHorizons.Data.TownData.Offsets
{
    public abstract class MainOffsets
    {
        public abstract int Offset_Vilagers { get; }
        public abstract int Offset_TownId { get; }
        public abstract int Offset_Patterns { get; }
        public abstract int Offset_Buildings { get; }

        public abstract int Offset_Turnips { get; }

        public virtual int Building_Count { get; } = 46; // Unsure.

        #region VILLAGERS
        public abstract int Villager_Size { get; }
        public abstract int Villager_Species { get; }
        public abstract int Villager_Variant { get; }
        public abstract int Villager_Personality { get; }
        public abstract int Villager_Catchphrase { get; }
        public abstract int Villager_Furniture { get; }

        // Sizes
        public virtual int Villager_CatchphraseLength { get; } = 12;
        public virtual int Villager_FurnitureCount { get; } = 16;
        public virtual int Villager_SpeciesMax { get; } = 0x23;
        #endregion

        #region PATTERNS
        public abstract int Pattern_Size { get; }
        public abstract int Pattern_Name { get; }
        public abstract int Pattern_PersonalID { get; }
        public abstract int Pattern_Palette { get; }
        public abstract int Pattern_ImageData { get; }
        #endregion

        public static MainOffsets GetOffsets(int rev) =>
            rev switch
            {
                0 => new MainOffsetsV0(),
                1 => new MainOffsetsV1(),
                _ => throw new IndexOutOfRangeException("Unknown Save Revision!")
            };
    }
}
