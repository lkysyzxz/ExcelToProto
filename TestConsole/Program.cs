// See https://aka.ms/new-console-template for more information

using System.Runtime.Loader;
using ActorConfig;
using Google.Protobuf;
using System.Runtime.CompilerServices;
using Google.Protobuf.Reflection;
using TypeInfo = System.Reflection.TypeInfo;

Console.WriteLine("Hello, World!");

byte[] buffer = File.ReadAllBytes("G:\\GameProjects\\BuildConfig\\SerializedDataWorkSpace\\Actor\\Actor_Config_Array.bytes");

ActorConfigArray array = ActorConfigArray.Parser.ParseFrom(buffer);

Console.WriteLine($"Id\tName\tSex\tSkillType\n");
for (int i = 0; i < array.Config.Count; i++)
{
	Actor ac = array.Config[i];
	Console.WriteLine($"{ac.Id}\t{ac.Name}\t{ac.SexId.ToString()}\t{ac.SkillType}");
}

Console.Read();