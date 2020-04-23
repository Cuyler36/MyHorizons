using System;
using System.Runtime.CompilerServices;

namespace MyHorizons.Data.Save
{
    public readonly struct SaveRevisionHeader
    {
        public readonly uint Major;
        public readonly uint Minor;
        public readonly ushort Unk1;
        public readonly ushort HeaderFileRevision; // ?
        public readonly ushort Unk2;
        public readonly ushort SaveFileRevision; // 

        public SaveRevisionHeader(uint maj, uint min, ushort u1, ushort headerR, ushort u2, ushort saveR)
        {
            Major = maj;
            Minor = min;
            Unk1 = u1;
            HeaderFileRevision = headerR;
            Unk2 = u2;
            SaveFileRevision = saveR;
        }

        public override bool Equals(object obj)
        {
            return obj is SaveRevisionHeader rev && Major == rev.Major && Minor == rev.Minor && Unk1 == rev.Unk1
                && HeaderFileRevision == rev.HeaderFileRevision && Unk2 == rev.Unk2 && SaveFileRevision == rev.SaveFileRevision;
        }

        public static bool operator ==(SaveRevisionHeader a, SaveRevisionHeader b) => a.Equals(b);
        public static bool operator !=(SaveRevisionHeader a, SaveRevisionHeader b) => !(a == b);
    }

    public readonly struct SaveRevision
    {
        public readonly SaveRevisionHeader Header;

        public readonly string GameVersion; // Game version string
        public readonly int HashVersion; // What HashInfo values to use when updating hashes.
        public readonly int Revision; // MyHorizons Revision Id

        public SaveRevision(uint maj, uint min, ushort u1, ushort headerR, ushort u2, ushort saveR, string gameVersion, int hashVer, int rev)
        {
            Header = new SaveRevisionHeader(maj, min, u1, headerR, u2, saveR);
            GameVersion = gameVersion;
            HashVersion = hashVer;
            Revision = rev;
        }
    }

    public readonly struct SaveFileSizes
    {
        public readonly uint Size_main;
        public readonly uint Size_personal;
        public readonly uint Size_photo_studio_island;
        public readonly uint Size_postbox;
        public readonly uint Size_profile;

        public SaveFileSizes(uint main, uint personal, uint photo, uint postbox, uint profile)
        {
            Size_main = main;
            Size_personal = personal;
            Size_photo_studio_island = photo;
            Size_postbox = postbox;
            Size_profile = profile;
        }
    }

    public static class RevisionManager
    {
        // Table of known revision data for each game version
        private static readonly SaveRevision[] KnownRevisions =
        {
            new SaveRevision(   0x67,    0x6F, 2, 0, 2, 0, "1.0.0", 0, 0), // 1.0.0
            new SaveRevision(   0x6D,    0x78, 2, 0, 2, 1, "1.1.0", 1, 1), // 1.1.0
            new SaveRevision(   0x6D,    0x78, 2, 0, 2, 2, "1.1.1", 1, 1), // 1.1.1
            new SaveRevision(   0x6D,    0x78, 2, 0, 2, 3, "1.1.2", 1, 1), // 1.1.2
            new SaveRevision(   0x6D,    0x78, 2, 0, 2, 4, "1.1.3", 1, 1), // 1.1.3
            new SaveRevision(   0x6D,    0x78, 2, 0, 2, 5, "1.1.4", 1, 1), // 1.1.4
            new SaveRevision(0x20006, 0x20008, 2, 0, 2, 6, "1.2.0", 2, 2)  // 1.2.0
        };

        // Table of save file sizes by revision
        private static readonly SaveFileSizes[] SizesByRevision =
        {
            new SaveFileSizes(0xAC0938, 0x6BC50, 0x263B4, 0xB44580, 0x69508),
            new SaveFileSizes(0xAC2AA0, 0x6BED0, 0x263C0, 0xB44590, 0x69560),
            new SaveFileSizes(0xACECD0, 0x6D6C0, 0x2C9C0, 0xB44590, 0x69560)
        };

        // Gets the revision info for a given file data.
        public static unsafe SaveRevision GetFileRevision(in byte[] data)
        {
            // Revision data seems to be 0x40 in length. First one is current revision, second one is "creation revision"
            if (data.Length < 0x80) throw new ArgumentException("Data must be at least 128 bytes long to detect save revision!");

            SaveRevisionHeader fileRevision;
            fixed (byte* p = data)
                fileRevision = Unsafe.ReadUnaligned<SaveRevisionHeader>(p);

            foreach (var revision in KnownRevisions)
                if (revision.Header == fileRevision)
                    return revision;
            throw new ArgumentOutOfRangeException("Couldn't find a known save revision for the supplied file!");
        }

        // Gets the save file sizes set for the given data.
        public static SaveFileSizes GetSaveFileSizes(in byte[] data) => GetSaveFileSizes(GetFileRevision(data));

        // Gets the save file sizes for a given revision
        public static SaveFileSizes GetSaveFileSizes(SaveRevision revision)
        {
            if (revision.Revision < SizesByRevision.Length)
                return SizesByRevision[revision.Revision];
            throw new IndexOutOfRangeException("The given save revision doesn't have a set of save file sizes associated with it!");
        }
    }
}
