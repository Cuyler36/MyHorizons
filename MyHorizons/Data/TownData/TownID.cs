using MyHorizons.Data.Save;
using System.Runtime.InteropServices;

namespace MyHorizons.Data.TownData
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1, Size = 0x1C)]
    public unsafe struct TownID
    {
        public uint UniqueID;
        public fixed char Name[10];
        public uint Unknown;

        public TownID(ISaveFile save, int offset) => this = save.ReadStruct<TownID>(offset);

        public override bool Equals(object obj) => obj is TownID id && UniqueID == id.UniqueID && GetName() == id.GetName() && Unknown == id.Unknown;

        public string GetName()
        {
            fixed (char* name = Name)
                return new string(name);
        }

        public void SetName(in string newName)
        {
            fixed (char* name = Name)
            {
                for (var i = 0; i < 10; i++)
                    name[i] = i >= newName.Length ? '\0' : newName[i];
            }
        }

        public override string ToString() => GetName();

        public override int GetHashCode() => (base.GetHashCode() << 2) ^ (int)UniqueID ^ (GetName().GetHashCode() << ((int)Unknown & 0x1F));

        public static bool operator ==(TownID a, TownID b) => a.Equals(b);
        public static bool operator !=(TownID a, TownID b) => !(a == b);
    }
}
