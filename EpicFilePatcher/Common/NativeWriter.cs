using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicFilePatcher.Common
{
    public enum Endian
    {
        Little,
        Big
    }
    public struct PDBData
    {
        public string Type;
        public int Size;
        public long Position;
    }
    public class NativeWriter : BinaryWriter
    {
        public long Position { get => BaseStream.Position; set => BaseStream.Position = value; }
        public long Length => BaseStream.Length;
        public bool UseDebug = false;
        private List<PDBData> pdbData = new List<PDBData>() { };

        public NativeWriter(Stream inStream, bool leaveOpen = false, bool wide = false)
            : base(inStream, wide ? Encoding.Unicode : Encoding.Default, leaveOpen)
        {
        }

        private void DebugPDBAdd(string type, int size = -1)
        {
            if (!UseDebug) return;

            pdbData.Add(new PDBData() { Size = size, Type = type, Position = Position });
        }

        public void OutPDBData(string path)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("typedef enum <byte>bool{True = 1 << 0,False = 0 << 1 }bool;typedef byte pad;");
            stringBuilder.AppendLine("typedef struct{local int result=0;local int i=0;while(true){local int b=ReadByte();FSeek(FTell()+1);result|=(b&127)<<i;if(b>>7==0)break;i+=7;}}SBEI<read=this.result>;");
            stringBuilder.AppendLine("typedef struct{local long result=0;local int i=0;while(true){local int b=ReadByte();FSeek(FTell()+1);result|=(b&127)<<i;if(b>>7==0)break;i+=7;}}SBEL<read=this.result>;");
            stringBuilder.AppendLine("typedef struct{SBEI size; char stringValue[size.result]<optimize=false>;}SizedString<read=this.stringValue>;");
            for (int i = 0; i < pdbData.Count; i++)
            {
                stringBuilder.Append($"FSeek(0x{pdbData[i].Position.ToString("X8")});");
                if (pdbData[i].Size == -1)
                    stringBuilder.AppendLine($"{pdbData[i].Type} __{i}__;");
                else
                    stringBuilder.AppendLine($"{pdbData[i].Type} __{i}__[{pdbData[i].Size}];");
            }
            File.WriteAllText(path, stringBuilder.ToString());
        }

        public void OutPDBData(FileInfo file)
        {
            OutPDBData(file.FullName);
        }

        private bool CheckLastSymbol(string s)
        {
            if (!UseDebug) return false;
            return pdbData.Last().Type == s;
        }

        public void Write(Guid value, Endian endian = Endian.Little)
        {
            DebugPDBAdd("GUID");
            if (endian == Endian.Big)
            {
                byte[] b = value.ToByteArray();
                Write(b[3]); Write(b[2]); Write(b[1]); Write(b[0]);
                Write(b[5]); Write(b[4]);
                Write(b[7]); Write(b[6]);
                for (int i = 0; i < 8; i++)
                    Write(b[8 + i]);
            }
            else
                Write(value);
        }


        public void Write(byte value, Endian endian = Endian.Little)
        {
            DebugPDBAdd("byte");
            base.Write(value);
        }
        public void Write(byte[] value, Endian endian = Endian.Little)
        {
            DebugPDBAdd("byte", value.Length);
            base.Write(value);
        }
        public void Write(bool value, Endian endian = Endian.Little)
        {
            DebugPDBAdd("bool");
            base.Write(value);
        }
        public void Write(float value, Endian endian = Endian.Little)
        {
            DebugPDBAdd("float");
            base.Write(value);
        }


        public void Write(short value, Endian endian = Endian.Little)
        {
            DebugPDBAdd("short");
            if (endian == Endian.Big)
                base.Write((short)(ushort)((value & 0xFF) << 8 | (value & 0xFF00) >> 8));
            else
                base.Write(value);
        }

        public void Write(ushort value, Endian endian = Endian.Little)
        {
            DebugPDBAdd("ushort");
            if (endian == Endian.Big)
                base.Write((ushort)((value & 0xFF) << 8 | (value & 0xFF00) >> 8));
            else
                base.Write(value);
        }

        public void Write(int value, Endian endian = Endian.Little)
        {
            DebugPDBAdd("int");
            if (endian == Endian.Big)
                base.Write((value & 0xFF) << 24 | (value & 0xFF00) << 8 | value >> 8 & 0xFF00 | value >> 24 & 0xFF);
            else
                base.Write(value);
        }

        public void Write(uint value, Endian endian = Endian.Little)
        {
            DebugPDBAdd("uint");
            if (endian == Endian.Big)
                base.Write((value & 0xFF) << 24 | (value & 0xFF00) << 8 | value >> 8 & 0xFF00 | value >> 24 & 0xFF);
            else
                base.Write(value);
        }

        public void Write(long value, Endian endian = Endian.Little)
        {
            DebugPDBAdd("int64");
            if (endian == Endian.Big)
            {
                base.Write((value & 0xFF) << 56 | (value & 0xFF00) << 40 | (value & 0xFF0000) << 24 | (value & 0xFF000000) << 8
                    | value >> 8 & 0xFF000000 | value >> 24 & 0xFF0000 | value >> 40 & 0xFF00 | value >> 56 & 0xFF);
            }
            else
                base.Write(value);
        }

        public void Write(ulong value, Endian endian = Endian.Little)
        {
            DebugPDBAdd("uint64");
            if (endian == Endian.Big)
            {
                base.Write((value & 0xFF) << 56 | (value & 0xFF00) << 40 | (value & 0xFF0000) << 24 | (value & 0xFF000000) << 8
                    | value >> 8 & 0xFF000000 | value >> 24 & 0xFF0000 | value >> 40 & 0xFF00 | value >> 56 & 0xFF);
            }
            else
                base.Write(value);
        }

        private void WriteString(string str)
        {
            for (int i = 0; i < str.Length; i++)
                base.Write(str[i]);
        }

        public void WriteNullTerminatedString(string str)
        {
            DebugPDBAdd("string");
            WriteString(str);
            base.Write((char)0x00);
        }

        public void WriteSizedString(string str)
        {
            DebugPDBAdd("SizedString");
            Write7BitEncodedInt(str.Length);
            WriteString(str);
        }

        public void WriteFixedSizedString(string str, int size)
        {
            DebugPDBAdd("string");
            WriteString(str);
            for (int i = 0; i < size - str.Length; i++)
                base.Write((char)0x00);
        }

        public new void Write7BitEncodedInt(int value)
        {
            if (!CheckLastSymbol("SizedString")) DebugPDBAdd("SBEI");
            uint v = (uint)value;
            while (v >= 0x80)
            {
                base.Write((byte)(v | 0x80));
                v >>= 7;
            }
            base.Write((byte)v);
        }

        public void Write7BitEncodedLong(long value)
        {
            DebugPDBAdd("SBEL");
            ulong v = (ulong)value;
            while (v >= 0x80)
            {
                Write((byte)(v | 0x80));
                v >>= 7;
            }
            base.Write((byte)v);
        }

        public void Write(Guid value) => Write(value.ToByteArray(), 0, 16);

        public void WriteLine(string str)
        {
            WriteString(str);
            Write((char)0x0D);
            Write((char)0x0A);
        }

        public void WritePadding(byte alignment)
        {
            DebugPDBAdd("pad", (int)(alignment - Position % alignment));
            while (Position % alignment != 0)
                base.Write((byte)0x00);
        }

        public byte[] ToByteArray() => BaseStream is MemoryStream stream ? stream.ToArray() : null;
    }
}
