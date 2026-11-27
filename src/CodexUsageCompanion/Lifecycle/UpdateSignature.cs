using System.Security.Cryptography;

namespace CodexUsageCompanion.Lifecycle;

/// <summary>
/// Verifies the ECDSA P-256 signature that the release workflow makes over SHA256SUMS.
/// The signature is the DER output of <c>openssl dgst -sha256 -sign</c>.
/// </summary>
public static class UpdateSignature
{
    private const string ResourceName = "CodexUsageCompanion.update-signing-key.pub.pem";

    public static string EmbeddedPublicKeyPem
    {
        get
        {
            using var stream = typeof(UpdateSignature).Assembly.GetManifestResourceStream(ResourceName)
                ?? throw new InvalidOperationException("The update signing key is missing.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }

    public static bool Verify(byte[] data, byte[] signature, string? publicKeyPem = null)
    {
        try
        {
            using var key = ECDsa.Create();
            key.ImportFromPem(publicKeyPem ?? EmbeddedPublicKeyPem);
            return key.VerifyData(data, signature, HashAlgorithmName.SHA256,
                DSASignatureFormat.Rfc3279DerSequence);
        }
        catch (CryptographicException)
        {
            return false;
        }
    }
}
