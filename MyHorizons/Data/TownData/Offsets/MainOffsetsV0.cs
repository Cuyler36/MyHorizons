namespace MyHorizons.Data.TownData.Offsets
{
    public sealed class MainOffsetsV0 : MainOffsets
    {
        public override int Offset_Vilagers => 0x110;

        public override int Villager_Size => 0x12AB0;
        public override int Villager_Species => 0x00;
        public override int Villager_Variant => 0x01;
        public override int Villager_Personality => 0x02;
        public override int Villager_Catchphrase => 0x10014;
        public override int Villager_Furniture => 0x105EC;
    }
}
