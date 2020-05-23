﻿using MyHorizons.Encryption;
using MyHorizons.Hash;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MyHorizons.Data.Save
{
    public abstract unsafe class SaveBase : ISaveFile
    {
        protected const int HEADER_FILE_SIZE = 0x300;

        protected byte[] _rawData = Array.Empty<byte>();
        protected string? _filePath;
        protected SaveRevision _revision;

        public bool Loaded { get; protected set; } = false;

        public virtual bool AcceptsFile(in string headerPath, in string filePath)
        {
            try
            {
                if (new FileInfo(headerPath).Length == HEADER_FILE_SIZE)
                {
                    using (var reader = File.OpenRead(headerPath))
                    {
                        var data = new byte[128];
                        if (reader.Read(data, 0, 128) == 128)
                        {
                            try
                            {
                                _revision = RevisionManager.GetFileRevision(data);
                            }
                            catch(Exception e)
                            {
                                _ = e;
                                return false;
                            }

                            return true;
                        }
                    }
                }
            }
            finally { }
            return false;
        }

        public virtual bool Load(in string headerPath, in string filePath, IProgress<float>? progress)
        {
            _filePath = filePath;
            return Load(File.ReadAllBytes(headerPath), File.ReadAllBytes(filePath), progress);
        }

        public virtual bool Load(in byte[] headerData, in byte[] fileData, IProgress<float>? progress)
        {
            try
            {
                _rawData = SaveEncryption.Decrypt(headerData, fileData);
                Loaded = true;
            }
            finally { }
            return Loaded;
        }

        public virtual bool Save(in string? filePath, IProgress<float>? progress)
        {
            if (filePath == null || !Directory.Exists(Path.GetDirectoryName(filePath)))
                throw new ArgumentException("The file path is invalid!", nameof(filePath));
            if (_rawData != null)
            {
                try
                {
                    // Update hashes
                    TryUpdateFileHashes(_rawData, _revision);

                    // Encrypt and save file + header
                    var (fileData, headerData) = SaveEncryption.Encrypt(_rawData, (uint)DateTime.Now.Ticks);
                    var headerFilePath = Path.Combine(Path.GetDirectoryName(filePath), $"{Path.GetFileNameWithoutExtension(filePath)}Header.dat");
                    File.WriteAllBytes(filePath, fileData);
                    File.WriteAllBytes(headerFilePath, headerData);
                    return true;
                }
                finally { }
            }
            return false;
        }

        public virtual bool SaveDecrypted(in string? filePath)
        {
            if (filePath == null || !Directory.Exists(Path.GetDirectoryName(filePath)))
                throw new ArgumentException("The file path is invalid!", nameof(filePath));
            if (_rawData != null)
            {
                try
                {
                    // Update hashes
                    TryUpdateFileHashes(_rawData, _revision);

                    // Save file
                    File.WriteAllBytes(filePath, _rawData);
                    return true;
                }
                finally { }
            }
            return false;
        }

        public virtual bool Save(IProgress<float>? progress) => Save(_filePath, progress);

        public virtual int GetRevision() => _revision.Revision;
        public virtual string GetRevisionString() => _revision.GameVersion;

        public virtual byte[] GetRawData() => _rawData;

        private static void TryUpdateFileHashes(in byte[] data, in SaveRevision revision)
        {
            var selectedInfo = HashInfo.VersionHashInfoList[revision.HashVersion];
            if (selectedInfo != null)
            {
                var thisFileSet = selectedInfo[(uint)data.Length];
                if (thisFileSet != null)
                {
                    Console.WriteLine($"{thisFileSet.FileName} rev {selectedInfo.RevisionId} detected!");
                    foreach (var hashRegion in thisFileSet)
                        Murmur3.UpdateMurmur32(data, hashRegion.HashOffset, hashRegion.BeginOffset, hashRegion.Size);
                }
            }
        }

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

        public string ReadString(int offset, int size) => Encoding.Unicode.GetString(_rawData, offset, size * 2).Trim('\0');

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

        public unsafe T ReadStruct<T>(int offset) where T : struct
        {
            fixed (byte* p = _rawData)
                return Unsafe.ReadUnaligned<T>((void*)(p + offset));
        }

        public unsafe void WriteS8(int offset, sbyte value) => _rawData[offset] = (byte)value;

        public unsafe void WriteU8(int offset, byte value) => _rawData[offset] = value;

        public unsafe void WriteS16(int offset, short value)
        {
            fixed (byte* p = _rawData)
            {
                *(short*)(p + offset) = value;
            }
        }

        public unsafe void WriteU16(int offset, ushort value)
        {
            fixed (byte* p = _rawData)
            {
                *(ushort*)(p + offset) = value;
            }
        }

        public unsafe void WriteS32(int offset, int value)
        {
            fixed (byte* p = _rawData)
            {
                *(int*)(p + offset) = value;
            }
        }

        public unsafe void WriteU32(int offset, uint value)
        {
            fixed (byte* p = _rawData)
            {
                *(uint*)(p + offset) = value;
            }
        }

        public unsafe void WriteS64(int offset, long value)
        {
            fixed (byte* p = _rawData)
            {
                *(long*)(p + offset) = value;
            }
        }

        public unsafe void WriteU64(int offset, ulong value)
        {
            fixed (byte* p = _rawData)
            {
                *(ulong*)(p + offset) = value;
            }
        }

        public unsafe void WriteF32(int offset, float value)
        {
            fixed (byte* p = _rawData)
            {
                *(float*)(p + offset) = value;
            }
        }

        public unsafe void WriteF64(int offset, double value)
        {
            fixed (byte* p = _rawData)
            {
                *(double*)(p + offset) = value;
            }
        }

        public void WriteString(int offset, string value, int maxSize)
        {
            Array.Clear(_rawData, offset, maxSize * 2);
            var bytes = Encoding.Unicode.GetBytes(value);
            Array.Copy(bytes, 0, _rawData, offset, bytes.Length);
        }

        public unsafe void WriteArray<T>(int offset, T[] values)
        {
            var typeSize = Marshal.SizeOf<T>();
            for (int i = 0; i < values.Length; i++)
            {
                fixed (byte* p = _rawData)
                {
                    Unsafe.WriteUnaligned((void*)(p + offset + i * typeSize), values[i]);
                }
            }
        }

        public unsafe void WriteStruct<T>(int offset, in T structure) where T : struct
        {
            fixed (byte* p = _rawData)
                Unsafe.WriteUnaligned((void*)(p + offset), structure);
        }

        public void ReplaceAllOccurrences(in byte[] data, in byte[] newData, int startOffset = 0, int alignment = 1)
        {
            for (var i = startOffset; i < _rawData.Length; i += alignment)
            {
                if (i + data.Length > _rawData.Length)
                    break;
                var valid = true;
                for (var x = 0; x < data.Length; x++)
                {
                    if (_rawData[i + x] != data[x])
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                {
                    Array.Copy(newData, 0, _rawData, i, newData.Length);
                    i += newData.Length - alignment;
                }
            }
        }
    }
}
