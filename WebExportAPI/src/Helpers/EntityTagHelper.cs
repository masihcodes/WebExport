using System.Security.Cryptography;
using Microsoft.Net.Http.Headers;

namespace WebExportAPI.Helpers;

public static class EntityTagHelper
{
    public static EntityTagHeaderValue Create(byte[] content)
    {
        byte[] hash = SHA256.HashData(content);
        string hex = Convert.ToHexString(hash);

        return new EntityTagHeaderValue($"\"{hex}\"");
    }
}