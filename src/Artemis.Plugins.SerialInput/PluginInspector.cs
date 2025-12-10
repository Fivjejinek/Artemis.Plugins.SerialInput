using System;
using System.Linq;
using System.Reflection;
using Artemis.Core;

public static class PluginInspector
{
    public static void DumpPluginApi(Plugin plugin)
    {
        var t = plugin.GetType();
        Console.WriteLine($"Plugin runtime type: {t.FullName}");
        Console.WriteLine("Public instance methods:");
        foreach (var m in t.GetMethods(BindingFlags.Instance | BindingFlags.Public).OrderBy(x => x.Name))
            Console.WriteLine($"  {m.Name}({string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name))})");

        Console.WriteLine("Non-public instance methods:");
        foreach (var m in t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).OrderBy(x => x.Name))
            Console.WriteLine($"  {m.Name}({string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name))})");

        Console.WriteLine("Properties:");
        foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).OrderBy(x => x.Name))
            Console.WriteLine($"  {p.Name} : {p.PropertyType.FullName}");

        Console.WriteLine("Fields:");
        foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).OrderBy(x => x.Name))
            Console.WriteLine($"  {f.Name} : {f.FieldType.FullName}");
    }
}
