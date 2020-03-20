namespace MyHorizons.Encryption
{
    public sealed class EncryptedInt32
    {
        private uint EncryptedValue;

        private ushort Adjust;
        private byte Shift;
        private byte Checksum;

        public EncryptedInt32()
        {
            EncryptedValue = 0;
            Adjust = 0;
            Shift = 0;
            Checksum = 0;
        }

        public EncryptedInt32(uint encryptedValue, ushort adjust, byte shift, byte checksum)
        {
            EncryptedValue = encryptedValue;
            Adjust = adjust;
            Shift = shift;
            Checksum = checksum;
        }

        public EncryptedInt32(uint value) => SetEncryptedValue(value);

        public bool IsValid() => Checksum == CalculateChecksum();

        // Calculates a checksum for a given encrypted value
        // Checksum calculation is every byte of the encrypted in added together minus 0x2D.
        public byte CalculateChecksum()
        {
            return (byte)(((byte)EncryptedValue + (byte)(EncryptedValue >> 16) + (byte)(EncryptedValue >> 24) + (byte)(EncryptedValue >> 8)) - 0x2D);
        }

        // Decrypts an encrypted int with the given encrypt params
        // Encrypt params are made up of the following:
        //  ushort adjust -- changes the constant value, additive
        //  byte    shift -- shifts the encrypted int. 3 is the current base.
        //  byte checksum -- the embedded checksum for the encrypted int.
        public uint GetDecryptedValue()
        {
            // If both are 0, then it's just 0.
            if (EncryptedValue == 0 && Adjust == 0 && Shift == 0 && Checksum == 0) return 0;
            // Verify embedded checksum is correct
            if (IsValid())
            {
                // Decrypt the encrypted int using the given params.
                ulong val = ((ulong)EncryptedValue) << ((0x1D - Shift) & 0x3F);
                int valConcat = (int)val + (int)(val >> 32);
                return (uint)((0x80E32B11u - Adjust) + valConcat);
            }
            return 0;
        }

        public (uint, uint) GetEncryptedValue() => (EncryptedValue, (((uint)Adjust) << 16) | (((uint)Shift) << 8) | Checksum);

        // Encrypts a given uint. See above for encrypt params.
        public (uint, uint) SetEncryptedValue(uint value)
        {
            // Create random generator
            var random = new SEADRandom();
            // Get our adjust value
            Adjust = (ushort)(random.GetU32() >> 16);
            // Get our shift value
            Shift = (byte)((((ulong)random.GetU32()) * 0x1B) >> 32);
            // Do the encryption
            ulong adjustedValue = (ulong)(value + (Adjust - 0x80E32B11u)) << (Shift + 3);
            EncryptedValue = (uint)((adjustedValue >> 32) + adjustedValue);
            Checksum = CalculateChecksum();
            return GetEncryptedValue();
        }
    }
}
