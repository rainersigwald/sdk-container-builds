using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace System.Containers;

public record struct Registry(Uri BaseUri)
{
    public async Task GetImageManifest(string name, string reference)
    {
        using HttpClient client = new(new HttpClientHandler() { UseDefaultCredentials = true });
        
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.docker.distribution.manifest.v2+json"));

        client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

        var response = await client.GetAsync(new Uri(BaseUri, $"/v2/{name}/manifests/{reference}"));

        var s = await response.Content.ReadAsStringAsync();

        Image? image = JsonSerializer.Deserialize<Image>(s);
    }
}