using System.ComponentModel.DataAnnotations;

namespace NEXUSDataLayerScaffold.Models;

public class MinioOptions
{
    /// <summary>
    /// MinIO/S3 endpoint in the form host:port or full host name. Do not include scheme.
    /// </summary>
    [Required]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Whether to use SSL when connecting to MinIO.
    /// </summary>
    public bool UseSsl { get; set; } = true;

    /// <summary>
    /// Default bucket name used by the application for object operations.
    /// </summary>
    [Required]
    public string Bucket { get; set; } = string.Empty;

    /// <summary>
    /// Optional region if required by the backend.
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// Access key for MinIO. Prefer providing via environment variables in production.
    /// </summary>
    public string? AccessKey { get; set; }

    /// <summary>
    /// Secret key for MinIO. Prefer providing via environment variables in production.
    /// </summary>
    public string? SecretKey { get; set; }
}
