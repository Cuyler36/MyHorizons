namespace MyHorizons.Data.PlayerData.Offsets
{
    public sealed class PersonalOffsetsV0 : PersonalOffsets
    {
        public override int PersonalId => 0xB0A0;
        public override int NookMiles => 0x11570;
        public override int Wallet => 0x11578;        
        public override int Photo => 0x11594;
        public override int Pockets => 0x35BD4;
        public override int Storage => 0x35D50;
        public override int Bank => 0x68BE4;
    }
}
