using System;
using System.IO;
using System.Reflection;

namespace MinecraftSharp.Engine;

public class EmbeddedResource
{
    private readonly Assembly _assembly = Assembly.GetCallingAssembly();

    private readonly string _name;

    public EmbeddedResource(string name)
    {
        if (_assembly.GetManifestResourceInfo(name) is null)
        {
            throw new ArgumentOutOfRangeException(nameof(name), name, "Resource could not be found");
        }

        _name = name;
    }

    public Stream GetStream()
        => _assembly.GetManifestResourceStream(_name) ?? throw new Exception($"Resource {_name} is not visible to the caller");

    public string GetStringContents()
    {
        using Stream stream = GetStream();
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}