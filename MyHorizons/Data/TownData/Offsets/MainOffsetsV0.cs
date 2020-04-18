namespace MyHorizons.Data.TownData.Offsets
{
    public sealed class MainOffsetsV0 : MainOffsets
    {
        public override int Offset_Vilagers => 0x110;
        public override int Offset_TownId => 0x1D72D0;
        public override int Offset_Patterns => 0x1D72F0;
        public override int Offset_Buildings => 0x2D0EFC;
        public override int Offset_Turnips => 0x4118C0;

        public override int Villager_Size => 0x12AB0;
        public override int Villager_Species => 0x00;
        public override int Villager_Variant => 0x01;
        public override int Villager_Personality => 0x02;
        public override int Villager_Catchphrase => 0x10014;
        public override int Villager_Furniture => 0x105EC;
        public override int Villager_StateFlags => 0x11EFA;
        public override int Villager_Wallpaper => 0x12108;
        public override int Villager_Flooring => 0x12110;

        public override int Pattern_Size => 0x2A8;
        public override int Pattern_Name => 0x10;
        public override int Pattern_PersonalID => 0x38;
        public override int Pattern_Palette => 0x78;
        public override int Pattern_ImageData => 0xA5;
    }
}
