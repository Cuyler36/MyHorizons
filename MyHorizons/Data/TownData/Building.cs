using MyHorizons.Data.Save;
using MyHorizons.Data.TownData.Offsets;
using System;
using System.Runtime.InteropServices;

namespace MyHorizons.Data.TownData
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x14)]
    public unsafe struct Building
    {
        public ushort Id;
        public ushort CoordinateX;
        public ushort CoordinateY;
        public ushort Rotation;
        public fixed byte Unknown[12];


        public Building(MainSaveFile mainSaveFile, int index)
        {
            var offsets = MainOffsets.GetOffsets(mainSaveFile.GetRevision());
            if (index >= offsets.Building_Count)
                throw new IndexOutOfRangeException("Index was greater than the number of building slots!");
            this = mainSaveFile.ReadStruct<Building>(offsets.Offset_Buildings + index * 0x14);
        }

        public void Save(MainSaveFile mainSaveFile, int index)
        {
            var offsets = MainOffsets.GetOffsets(mainSaveFile.GetRevision());
            if (index >= offsets.Building_Count)
                throw new IndexOutOfRangeException("Index was greater than the number of building slots!");
            mainSaveFile.WriteStruct(offsets.Offset_Buildings + index * 0x14, this);
        }
    }
}
