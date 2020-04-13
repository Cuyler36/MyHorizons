using System.Runtime.InteropServices;
using MyHorizons.Data.Save;
using MyHorizons.Data.TownData.Offsets;

namespace MyHorizons.Data.TownData
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StalkMarket
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DayPrices
        {
            public uint MorningPrice;
            public uint EveningPrice;
        }

        public uint BuyPrice;
        public DayPrices Sunday; // always 0 / unused by the game
        public DayPrices Monday;
        public DayPrices Tuesday;
        public DayPrices Wednesday;
        public DayPrices Thursday;
        public DayPrices Friday;
        public DayPrices Saturday;

        public StalkMarket(ISaveFile saveFile)
        {
            var offsets = MainOffsets.GetOffsets(saveFile.GetRevision());
            this = saveFile.ReadStruct<StalkMarket>(offsets.Offset_Turnips);
        }
    }
}
