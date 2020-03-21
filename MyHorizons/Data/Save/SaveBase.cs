using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MyHorizons.Data.Save
{
    public unsafe abstract class SaveBase : ISaveFile
    {
        protected const int HEADER_FILE_SIZE = 0x300;

        protected byte[] _rawData;
        protected SaveRevision? _revision = null;

        protected static SaveBase _saveFile;

        public static SaveBase Singleton() => _saveFile;

        public virtual bool AcceptsFile(in string headerPath, in string filePath)
        {
            try
            {
                using (var reader = File.OpenRead(headerPath))
                {
                    var data = new byte[128];
                    if (reader.Read(data, 0, 128) == 128)
                        return (_revision = RevisionManager.GetFileRevision(data)) != null;
                }
            }
            finally { }
            return false;
        }

        public virtual bool Load(in string headerPath, in string filePath, IProgress<float> progress)
            => Load(File.ReadAllBytes(headerPath), File.ReadAllBytes(filePath), progress);
        public abstract bool Load(in byte[] headerData, in byte[] fileData, IProgress<float> progress);
        public abstract bool Save(in string filePath, IProgress<float> progress);

        public sbyte ReadS8(int offset) => (sbyte)_rawData[offset];

        public byte ReadU8(int offset) => _rawData[offset];

        public unsafe short ReadS16(int offset)
        {
            fixed (byte* p = _rawData)
            {
                return *(short*)(p + offset);
            }
        }

        public unsafe ushort ReadU16(int offset)
        {
            fixed(byte * p = _rawData)
            {
                return *(ushort*)(p + offset);
            }
        }

        public unsafe int ReadS32(int offset)
        {
            fixed (byte* p = _rawData)
            {
                return *(int*)(p + offset);
            }
        }

        public unsafe uint ReadU32(int offset)
        {
            fixed (byte* p = _rawData)
            {
                return *(uint*)(p + offset);
            }
        }

        public unsafe long ReadS64(int offset)
        {
            fixed (byte* p = _rawData)
            {
                return *(long*)(p + offset);
            }
        }

        public unsafe ulong ReadU64(int offset)
        {
            fixed (byte* p = _rawData)
            {
                return *(ulong*)(p + offset);
            }
        }

        public unsafe float ReadF32(int offset)
        {
            fixed (byte* p = _rawData)
            {
                return *(float*)(p + offset);
            }
        }

        public unsafe double ReadF64(int offset)
        {
            fixed (byte* p = _rawData)
            {
                return *(double*)(p + offset);
            }
        }

        public unsafe T[] ReadArray<T>(int offset, int count)
        {
            // This is probably not that performant and overall is bad.
            T[] arr = new T[count];
            var typeSize = Marshal.SizeOf<T>();
            for (int i = 0; i < count; i++)
            {
                fixed (byte* p = _rawData)
                {
                    arr[i] = Unsafe.ReadUnaligned<T>((void*)(p + offset + i * typeSize));
                }
            }
            return arr;
        }

        public void WriteS8(int offset, sbyte value)
        {
            throw new NotImplementedException();
        }

        public void WriteU8(int offset, byte value)
        {
            throw new NotImplementedException();
        }

        public void WriteS16(int offset, short value)
        {
            throw new NotImplementedException();
        }

        public void WriteU16(int offset, ushort value)
        {
            throw new NotImplementedException();
        }

        public void WriteS32(int offset, int value)
        {
            throw new NotImplementedException();
        }

        public void WrtieU32(int offset, uint value)
        {
            throw new NotImplementedException();
        }

        public void WriteS64(int offset, long value)
        {
            throw new NotImplementedException();
        }

        public void WriteU64(int offset, ulong value)
        {
            throw new NotImplementedException();
        }

        public void WriteF32(int offset, float value)
        {
            throw new NotImplementedException();
        }

        public void WriteF64(int offset, double value)
        {
            throw new NotImplementedException();
        }

        public void WriteArray<T>(int offset, T[] values)
        {
            throw new NotImplementedException();
        }
    }
}
