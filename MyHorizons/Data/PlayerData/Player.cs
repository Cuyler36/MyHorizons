using MyHorizons.Data.PlayerData.Offsets;
using MyHorizons.Data.Save;
using MyHorizons.Encryption;

namespace MyHorizons.Data.PlayerData
{
    public sealed class Player
    {
        public readonly int Index;

        public PersonalID PersonalId;
        public ItemCollection Pockets; // TODO: Detect pockets size
        public ItemCollection Storage; // TODO: Same as pockets
        public EncryptedInt32 Wallet;
        public EncryptedInt32 Bank;
        public EncryptedInt32 NookMiles;

        private readonly PersonalSaveFile _personalFile;

        private ISaveFile SaveFile { get; }

        public Player(ISaveFile saveFile, int idx, PersonalSaveFile personalSave)
        {
            SaveFile = saveFile;
            _personalFile = personalSave;
            var offsets = PersonalOffsets.GetOffsets(SaveFile.GetRevision());
            Index = idx;

            PersonalId = new PersonalID(personalSave, offsets.PersonalId);
            Wallet = new EncryptedInt32(personalSave, offsets.Wallet);
            Bank = new EncryptedInt32(personalSave, offsets.Bank);
            NookMiles = new EncryptedInt32(personalSave, offsets.NookMiles);

            // TODO: This should be refactored to detect the "expanded pockets" state
            var pockets = new Item[40];
            for (var i = 0; i < 20; i++)
            {
                pockets[i] = new Item(personalSave, offsets.Pockets + 0xB8 + i * 8);
                pockets[i + 20] = new Item(personalSave, offsets.Pockets + i * 8);
            }

            Pockets = new ItemCollection(pockets);

            var storage = new Item[5000];
            for (var i = 0; i < 5000; i++)
                storage[i] = new Item(personalSave, offsets.Storage + i * 8);
            Storage = new ItemCollection(storage);
        }

        public void Save()
        {
            var offsets = PersonalOffsets.GetOffsets(SaveFile.GetRevision());
            _personalFile.WriteStruct(offsets.PersonalId, PersonalId);
            Wallet.Write(_personalFile, offsets.Wallet);
            Bank.Write(_personalFile, offsets.Bank);
            NookMiles.Write(_personalFile, offsets.NookMiles);

            for (var i = 0; i < 20; i++)
            {
                Pockets[i].Save(_personalFile, offsets.Pockets + 0xB8 + i * 8);
                Pockets[i + 20].Save(_personalFile, offsets.Pockets + i * 8);
            }

            for (var i = 0; i < 5000; i++)
                Storage[i].Save(_personalFile, offsets.Storage + i * 8);
        }

        public string GetName() => PersonalId.GetName();
        public void SetName(in string newName) => PersonalId.SetName(newName);

        public byte[] GetPhotoData()
        {
            var offset = PersonalOffsets.GetOffsets(SaveFile.GetRevision()).Photo;
            return _personalFile.ReadArray<byte>(offset + 4, _personalFile.ReadS32(offset));
        }

        public override string ToString() => GetName();
    }
}
