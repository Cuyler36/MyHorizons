using MyHorizons.Data.Save;
using MyHorizons.Encryption;

namespace MyHorizons.Data
{
    public sealed class Player
    {
        public readonly int Index;

        public string Name;
        public uint PlayerUID;
        public string TownName;
        public uint TownUID;
        public Item[] Pockets;
        public EncryptedInt32 Wallet;
        public EncryptedInt32 Bank;
        public EncryptedInt32 NookMiles;

        private readonly PersonalSaveFile _personalFile;

        private readonly struct Offsets
        {
            public readonly int PersonalId;
            public readonly int Pockets;
            public readonly int Wallet;
            public readonly int Bank;
            public readonly int NookMiles;
            public readonly int Photo;

            public Offsets(int pid, int pockets, int wallet, int bank, int nookMiles, int photo)
            {
                PersonalId = pid;
                Pockets = pockets;
                Wallet = wallet;
                Bank = bank;
                NookMiles = nookMiles;
                Photo = photo;
            }
        }

        private static readonly Offsets[] PlayerOffsetsByRevision =
        {
            new Offsets(0xB0A0, 0x35BD4, 0x11578, -1, 0x11570, 0x11598),
            new Offsets(0xB0B8, 0x35C20, 0x11590, -1, 0x11588, 0x115C4)
        };

        private static Offsets GetOffsetsFromRevision() => PlayerOffsetsByRevision[MainSaveFile.Singleton().GetRevision()];

        public Player(int idx, PersonalSaveFile personalSave)
        {
            _personalFile = personalSave;
            var offsets = GetOffsetsFromRevision();
            Index = idx;
            // TODO: Convert this to a "PersonalID" struct
            TownUID = personalSave.ReadU32(offsets.PersonalId);
            TownName = personalSave.ReadString(offsets.PersonalId + 4, 10);
            PlayerUID = personalSave.ReadU32(offsets.PersonalId + 0x1C);
            Name = personalSave.ReadString(offsets.PersonalId + 0x20, 10);

            Wallet = new EncryptedInt32(personalSave, offsets.Wallet);
            // Bank
            NookMiles = new EncryptedInt32(personalSave, offsets.NookMiles);
        }

        public byte[] GetPhotoData()
        {
            var offset = GetOffsetsFromRevision().Photo;
            if (_personalFile.ReadU8(offset) != 0xFF || _personalFile.ReadU8(offset + 1) != 0xD8)
                return null;
            // TODO: Determine actual size buffer instead of using this.
            var size = 2;
            while (_personalFile.ReadU8(offset + size) != 0xFF && _personalFile.ReadU8(offset + size + 1) != 0xD9)
                size++;
            return _personalFile.ReadArray<byte>(offset, size + 2);
        }
    }
}
