﻿using System.Containers;

namespace Test.System.Containers.Filesystem;

[TestClass]
public class EndToEnd
{
    [TestMethod]
    public async Task Magic()
    {
        Registry registry = new Registry(new Uri("http://localhost:5000"));

        Image x = await registry.GetImageManifest("dotnet/sdk", "6.0");

        Layer l = Layer.FromDirectory(@"S:\play\helloworld6\bin\Debug\net6.0\publish\", "/app");

        await registry.Push(l, "foo/bar");

        x.AddLayer(l);

        await registry.Push(x, "foo/bar");
    }
}
