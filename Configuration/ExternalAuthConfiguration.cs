using System.ComponentModel.DataAnnotations;

namespace CoffeeShopApp.Configuration;

/// <summary>
/// Configuration for external authentication settings
/// </summary>
public class ExternalAuthConfiguration
{
    /// <summary>
    /// The authentication protocol to use (None, OIDC, or SAML)
    /// </summary>
    public string Protocol { get; set; } = "None";

    /// <summary>
    /// OpenID Connect configuration (required if Protocol is OIDC)
    /// </summary>
    public OidcConfiguration? Oidc { get; set; }

    /// <summary>
    /// SAML configuration (required if Protocol is SAML)
    /// </summary>
    public SamlConfiguration? Saml { get; set; }

    /// <summary>
    /// Gets the parsed authentication protocol
    /// </summary>
    public Services.AuthenticationProtocol GetProtocol()
    {
        return Enum.TryParse<Services.AuthenticationProtocol>(Protocol, true, out var result)
            ? result
            : Services.AuthenticationProtocol.None;
    }
}

/// <summary>
/// OpenID Connect specific configuration
/// </summary>
public class OidcConfiguration
{
    /// <summary>
    /// The client ID for OIDC authentication
    /// </summary>
    [Required]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// The client secret for OIDC authentication
    /// </summary>
    [Required]
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// The OIDC authority (defaults to Microsoft common endpoint)
    /// </summary>
    public string Authority { get; set; } = "https://login.microsoftonline.com/common/v2.0";

    /// <summary>
    /// Additional scopes to request (beyond openid, profile, email)
    /// </summary>
    public List<string> AdditionalScopes { get; set; } = new();
}

/// <summary>
/// SAML 2.0 specific configuration
/// </summary>
public class SamlConfiguration
{
    /// <summary>
    /// The entity ID for SAML authentication (usually your application's URL)
    /// </summary>
    [Required]
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// The IdP metadata URL for SAML configuration
    /// </summary>
    [Required]
    public string MetadataUrl { get; set; } = string.Empty;

    /// <summary>
    /// Certificate path for SAML signing (optional)
    /// </summary>
    public string? CertificatePath { get; set; }

    /// <summary>
    /// Certificate password for SAML signing (optional)
    /// </summary>
    public string? CertificatePassword { get; set; }

    /// <summary>
    /// Whether to sign SAML requests (default: false)
    /// </summary>
    public bool SignRequests { get; set; } = false;

    /// <summary>
    /// Whether to require signed SAML responses (default: true)
    /// </summary>
    public bool RequireSignedResponses { get; set; } = true;
}

public class ExternalAuthConfigurationFactory(IConfiguration configuration)
{
    public ExternalAuthConfiguration Create()
    {
        var config = new ExternalAuthConfiguration();
        configuration.GetSection("Authentication:External").Bind(config);
        return config;
    }
}