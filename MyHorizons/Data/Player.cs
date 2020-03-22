using MyHorizons.Encryption;

namespace MyHorizons.Data
{
    public sealed class Player
    {
        public string Name;
        public uint PlayerUID;
        public string TownName;
        public uint TownUID;
        public Item[] Pockets;
        public EncryptedInt32 Wallet;
        public EncryptedInt32 Bank;
        public EncryptedInt32 NookMiles;
    }
}
