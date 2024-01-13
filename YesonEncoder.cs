using System.Text;

namespace Yeson
{
    public enum YesonTypes : byte // technically it's less than byte
    {
        NULL = 0,
        BOOL = 1,
        STRING = 2,
        INT = 3,
        FLOAT = 4,
        ARRAY = 5,
        OBJECT = 6,
    }

    public class YesonEncoder
    {
        private MemoryStream stream;
        private BinaryWriter writer;
        public YesonEncoder()
        {
            stream = new MemoryStream();
            writer = new BinaryWriter(stream);
        }

        public byte[] GetBytes()
        {
            return stream.ToArray();
        }

        public void Encode(object val)
        {
            if (val == null) EncodeNull();
            else if (val is bool) Encode((bool)val);
            else if (val is string) Encode((string)val);
            else if (val is byte) Encode((byte)val);
            else if (val is short) Encode((short)val);
            else if (val is ushort) Encode((ushort)val);
            else if (val is int) Encode((int)val);
            else if (val is uint) Encode((uint)val);
            else if (val is long) Encode((long)val);
            else if (val is ulong) Encode((ulong)val);
            else if (val is float) Encode((float)val);
            else if (val is double) Encode((double)val);
            else if (val is Array) Encode((Array)val);
            else if (val is Dictionary<string, object?>) Encode((Dictionary<string, object?>)val);
            else
                throw new Exception("Unsupported type");
        }

        private void EncodeNull()
        {
            writer.Write((byte)YesonTypes.NULL);
        }

        private void Encode(bool val)
        {
            byte header = (byte)YesonTypes.BOOL << 4;
            if (val)
                header |= 1;
            writer.Write(header);
        }

        private void Encode(string val)
        {
            byte header = (byte)YesonTypes.STRING << 4;
            var len = Math.Min(val.Length, 0b1111);
            header |= (byte)len;
            writer.Write(header);
            if (len == 0b1111)
                writer.Write((byte)val.Length);
            writer.Write(Encoding.UTF8.GetBytes(val));
        }

        private void Encode(byte val)
        {
            byte header = (byte)YesonTypes.INT << 4;
            header |= 0b_001;
            writer.Write(header);
            writer.Write(val);
        }

        private void Encode(short val)
        {
            byte header = (byte)YesonTypes.INT << 4;
            header |= 0b_1_010; // signed
            writer.Write(header);
            writer.Write(val);
        }

        private void Encode(ushort val)
        {
            byte header = (byte)YesonTypes.INT << 4;
            header |= 0b_0_010; // unsigned
            writer.Write(header);
            writer.Write(val);
        }

        private void Encode(int val)
        {
            byte header = (byte)YesonTypes.INT << 4;
            header |= 0b_1_011; // signed
            writer.Write(header);
            writer.Write(val);
        }

        private void Encode(uint val)
        {
            byte header = (byte)YesonTypes.INT << 4;
            header |= 0b_0_011; // unsigned
            writer.Write(header);
            writer.Write(val);
        }

        private void Encode(long val)
        {
            byte header = (byte)YesonTypes.INT << 4;
            header |= 0b_1_100; // signed
            writer.Write(header);
            writer.Write(val);
        }

        private void Encode(ulong val)
        {
            byte header = (byte)YesonTypes.INT << 4;
            header |= 0b_0_100; // unsigned
            writer.Write(header);
            writer.Write(val);
        }

        private void Encode(float val)
        {
            byte header = (byte)YesonTypes.FLOAT << 4;
            header |= 0; // float
            writer.Write(header);
            writer.Write(val);
        }

        private void Encode(double val)
        {
            byte header = (byte)YesonTypes.FLOAT << 4;
            header |= 1; // double
            writer.Write(header);
            writer.Write(val);
        }

        private void Encode(Array val)
        {
            byte header = (byte)YesonTypes.ARRAY << 4;
            var len = Math.Min(val.Length, 0b1111);
            header |= (byte)len;
            writer.Write(header);
            if (len == 0b1111)
                writer.Write((byte)val.Length);
            foreach (var item in val)
                Encode(item);
        }

        private void Encode(Dictionary<string, object?> val)
        {
            byte header = (byte)YesonTypes.OBJECT << 4;
            var len = Math.Min(val.Count, 0b1111);
            header |= (byte)len;
            writer.Write(header);
            if (len == 0b1111)
                writer.Write((byte)val.Count);
            foreach (var item in val)
            {
                Encode(item.Key);
                Encode(item.Value);
            }
        }
    }

    
    public class YesonDecoder
    {
        private MemoryStream stream;
        private BinaryReader reader;
        public YesonDecoder(byte[] bytes)
        {
            stream = new MemoryStream(bytes);
            reader = new BinaryReader(stream);
        }

        private (byte, byte) DecodeHeader()
        {
            var header = reader.ReadByte();
            var type = (byte)(header >> 4);
            var info = (byte)(header & 0b_0000_1111);
            return (type, info);
        }

        private Type getType(byte type)
        {
            switch ((YesonTypes)type)
            {
                case YesonTypes.NULL:
                    return typeof(object);
                case YesonTypes.BOOL:
                    return typeof(bool);
                case YesonTypes.STRING:
                    return typeof(string);
                case YesonTypes.INT:
                    return typeof(int);
                case YesonTypes.FLOAT:
                    return typeof(float);
                case YesonTypes.ARRAY:
                    return typeof(Array);
                default:
                    throw new Exception("Unsupported type");
            }
        }

        public object? Decode()
        {
            var (type, info) = DecodeHeader();
            switch ((YesonTypes)type)
            {
                case YesonTypes.NULL:
                    return null;
                case YesonTypes.BOOL:
                    return DecodeBool(info);
                case YesonTypes.STRING:
                    return DecodeString(info);
                case YesonTypes.INT:
                    return DecodeInt(info);
                case YesonTypes.FLOAT:
                    return DecodeFloat(info);
                case YesonTypes.ARRAY:
                    return DecodeArray(info);
                case YesonTypes.OBJECT:
                    return DecodeObject(info);
                default:
                    throw new Exception("Unsupported type");
            }
        }

        private bool DecodeBool(byte info)
        {
            return (info & 1) == 1;
        }

        private string DecodeString(byte info)
        {
            var len = info;
            if (len == 0b1111)
                len = reader.ReadByte();
            var bytes = reader.ReadBytes(len);
            return Encoding.UTF8.GetString(bytes);
        }

        private object DecodeInt(byte info)
        {
            var signed = (info & 0b_1000) == 0b_1000;
            var type = info & 0b_0111;
            switch (type)
            {
                case 0b_001:
                    return reader.ReadByte();
                case 0b_010:
                    return signed ? reader.ReadInt16() : reader.ReadUInt16();
                case 0b_011:
                    return signed ? reader.ReadInt32() : reader.ReadUInt32();
                case 0b_100:
                    return signed ? reader.ReadInt64() : reader.ReadUInt64();
                default:
                    throw new Exception("Unsupported type");
            }
        }

        private object DecodeFloat(byte info)
        {
            var type = info & 1;
            switch (type)
            {
                case 0:
                    return reader.ReadSingle();
                case 1:
                    return reader.ReadDouble();
                default:
                    throw new Exception("Unsupported type");
            }
        }
        
        // TODO: make this better
        private object?[] DecodeArray(byte info)
        {
            var len = info;
            if (len == 0b1111)
                len = reader.ReadByte();
            var arr = new object?[len];
            for (int i = 0; i < len; i++)
                arr[i] = Decode();
            return arr;
        }

        private Dictionary<string, object?> DecodeObject(byte info)
        {
            var len = info;
            if (len == 0b1111)
                len = reader.ReadByte();
            var dict = new Dictionary<string, object?>(len);
            for (int i = 0; i < len; i++)
            {
                var header = DecodeHeader();
                var key = DecodeString(header.Item2);
                var val = Decode();
                dict.Add(key, val);
            }
            return dict;
        }
    }
}

/*

NOTES:
first byte is a header which includes:
 - type (4 bits)
 - info (4 bits)
    - for bool: 0 = false, 1 = true
    - for string: length (4 bits), if length = 0b1111, then next byte is length
    - for int:
      - first bit: 1 = signed, 0 = unsigned
      - next 3 bits: 
        - 0b001 = byte
        - 0b010 = short
        - 0b011 = int
        - 0b100 = long
    - for float: 0 = float, 1 = double
    - for array: length (4 bits), if length = 0b1111, then next byte is length
    - for object: length (4 bits), if length = 0b1111, then next byte is length
      - length is number of key-value pairs
      - key is string, value is any type

*/