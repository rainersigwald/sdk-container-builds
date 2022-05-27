namespace System.Containers.Oci;

/// <summary>
/// Interface to the Docker Registry HTTP API.
/// </summary>
public class Registry
{
	private readonly Uri _baseUri;

	public Registry(Uri baseUri)
	{
		_baseUri = baseUri;
	}

	// TODO: Auth

	public void PullLayer(string digest) // TODO: pull to path? or maintain paths internally?
	{
		throw new NotImplementedException();
	}

	public void PushImage(Image image)
	{
		throw new NotImplementedException();
	}

	public bool DoesLayerExist(Layer layer)
	{
		throw new NotImplementedException();
	}

	public void PushLayer(Layer layer)
	{
		throw new NotImplementedException();
	}
}
