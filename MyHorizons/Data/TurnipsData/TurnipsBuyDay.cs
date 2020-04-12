using System.Runtime.InteropServices;

namespace MyHorizons.Data.TurnipsData
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TurnipsBuyDay
    {
        public uint MorningPrice;
        public uint EveningPrice;
    }
}
