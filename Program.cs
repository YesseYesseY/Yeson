using Newtonsoft.Json;
using Yeson;

Dictionary<string, object?> dict = new Dictionary<string, object?>{
    {"null", null},
    {"bool", true},
    {"string", "Hello, world!"},
    {"int", 123},
    {"float", 123.456f},
    {"array", new object[]{1, 2, 3}},
    {"object", new Dictionary<string, object?>{
        {"a", 1},
        {"b", 2},
        {"c", 3},
    }},
};

// YesonEncoder encoder = new YesonEncoder();
// encoder.Encode(dict);
// byte[] bytes = encoder.GetBytes();
// File.WriteAllBytes("test.yeson", bytes);

byte[] bytes = File.ReadAllBytes("test.yeson");
YesonDecoder decoder = new YesonDecoder(bytes);
Dictionary<string, object?> decoded = decoder.Decode() as Dictionary<string, object?>;
File.WriteAllText("test.json", JsonConvert.SerializeObject(decoded, Formatting.Indented));


