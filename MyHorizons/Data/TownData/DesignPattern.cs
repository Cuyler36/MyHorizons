using System;
using System.Runtime.InteropServices;
using MyHorizons.Data.PlayerData;
using MyHorizons.Data.Save;
using MyHorizons.Data.TownData.Offsets;

namespace MyHorizons.Data.TownData
{
    public sealed class DesignPattern
    {
        public string Name;
        public PersonalID PersonalID;
        public readonly int Index;
        public readonly DesignColor[] Palette = new DesignColor[15];
        public readonly byte[] Pixels = new byte[32 * 16];

        private readonly int Offset;

        private ISaveFile SaveFile { get; }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 3)]
        public struct DesignColor
        {
            public byte R;
            public byte G;
            public byte B;

            public DesignColor(ISaveFile saveFile, int offset)
            {
                this = saveFile.ReadStruct<DesignColor>(offset);
            }

            public uint ToArgb() => 0xFF000000 | ((uint)R << 16) | ((uint)G << 8) | B;
        }

        public DesignPattern(ISaveFile saveFile, int idx)
        {
            SaveFile = saveFile;
            Index = idx;
            var offsets = MainOffsets.GetOffsets(SaveFile.GetRevision());
            Offset = offsets.Offset_Patterns + idx * offsets.Pattern_Size;

            Name = SaveFile.ReadString(Offset + offsets.Pattern_Name, 20);
            PersonalID = SaveFile.ReadStruct<PersonalID>(Offset + offsets.Pattern_PersonalID);

            for (int i = 0; i < 15; i++)
                Palette[i] = new DesignColor(saveFile, Offset + offsets.Pattern_Palette + i * 3);

            Pixels = SaveFile.ReadArray<byte>(Offset + offsets.Pattern_ImageData, Pixels.Length);
        }

        public byte GetPixel(int x, int y)
        {
            if (x < 0 || x > 31)
                throw new ArgumentException("Argument out of range (0-32)", "x");
            if (y < 0 || y > 31)
                throw new ArgumentException("Argument out of range (0-32)", "y");

            if (x % 2 == 0)
                return (byte) (Pixels[(x / 2) + y * 16] & 0x0F);
            else
                return (byte) ((Pixels[(x / 2) + y * 16] & 0xF0) >> 4);
        }

        public uint GetPixelArgb(int x, int y)
        {
            var paletteIdx = GetPixel(x, y);
            if (paletteIdx == 15)
                return 0; // Transparent
            return Palette[paletteIdx].ToArgb();
        }

        public void SetPixel(int x, int y, byte paletteColorIndex)
        {
            if (x < 0 || x > 31)
                throw new ArgumentException("Argument out of range (0-32)", nameof(x));
            if (y < 0 || y > 31)
                throw new ArgumentException("Argument out of range (0-32)", nameof(y));
            if (paletteColorIndex > 15)
                throw new ArgumentException("Argument out of range (0-15)", nameof(paletteColorIndex));

            var index = (x / 2) + y * 16;
            if (x % 2 == 0)
                Pixels[index] = (byte) ((paletteColorIndex & 0x0F) | Pixels[index] & 0xF0);
            else
                Pixels[index] = (byte) (((paletteColorIndex * 0x10) & 0xF0) | Pixels[index] & 0x0F);
        }

        public void Save()
        {
            var offsets = MainOffsets.GetOffsets(SaveFile.GetRevision());

            SaveFile.WriteString(Offset + offsets.Pattern_Name, Name, 20);
            SaveFile.WriteStruct(Offset + offsets.Pattern_PersonalID, PersonalID);

            for (int i = 0; i < 15; i++)
                SaveFile.WriteStruct(Offset + offsets.Pattern_Palette + i * 3, Palette[i]);

            SaveFile.WriteArray(Offset + offsets.Pattern_ImageData, Pixels);
        }
    }
}
