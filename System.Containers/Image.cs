namespace System.Containers.Oci;

/// <summary>
/// An OCI image.
/// </summary>
/// <seealso url="https://github.com/opencontainers/image-spec/blob/7b36cea86235157d78528944cb94c3323ee0905c/spec.md"/>
public class Image
{
    public IReadOnlyList<Layer> Layers;

	public Image(IEnumerable<Layer> layers)
	{
		Layers = layers.ToArray();
	}
}
