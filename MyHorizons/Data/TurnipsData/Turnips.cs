using System.Runtime.InteropServices;
using MyHorizons.Data.Save;
using MyHorizons.Data.TownData.Offsets;

namespace MyHorizons.Data.TurnipsData
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Turnips
    {
        public uint BuyPrice;
        public TurnipsBuyDay Sunday; // always 0, but it's a part of the struct in save
        public TurnipsBuyDay Monday;
        public TurnipsBuyDay Tuesday;
        public TurnipsBuyDay Wednesday;
        public TurnipsBuyDay Thursday;
        public TurnipsBuyDay Friday;
        public TurnipsBuyDay Saturday;

        public Turnips(MainSaveFile mainSaveFile)
        {
            var offsets = MainOffsets.GetOffsets(mainSaveFile.GetRevision());
            this = mainSaveFile.ReadStruct<Turnips>(offsets.Offset_Turnips);
        }
    }
}
