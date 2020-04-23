using System;

namespace MyHorizons.Data.PlayerData.Offsets
{
    public abstract class PersonalOffsets
    {
        public abstract int PersonalId { get; }
        public abstract int NookMiles { get; }
        public abstract int Photo { get; }
        public abstract int Pockets { get; }
        public abstract int Wallet { get; }
        public abstract int Storage { get; }
        public abstract int Bank { get; }

        // Sizes
        public virtual int PocketSlotsCount { get; } = 40;
        public virtual int StorageSlotsCount { get; } = 5000;
        public virtual int MaxRecipeId { get; } = 0x2A0;

        public static PersonalOffsets GetOffsets(int rev) =>
            rev switch
            {
                0 => new PersonalOffsetsV0(),
                1 => new PersonalOffsetsV1(),
                2 => new PersonalOffsetsV2(),
                _ => throw new IndexOutOfRangeException("Unknown Save Revision!")
            };
    }
}
