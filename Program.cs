using Yeson;

string[] bools = new string[] { 
    "string1",
    "string2",
    "string3",
};

YesonEncoder encoder = new YesonEncoder();
encoder.Encode(bools);
byte[] bytes = encoder.GetBytes();

YesonDecoder decoder = new YesonDecoder(bytes);
object? decoded = decoder.Decode();
foreach (var b in decoded as object[])
    Console.WriteLine((string)b);
