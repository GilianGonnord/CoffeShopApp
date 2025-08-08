using CoffeeShopApp.Configuration;
using Microsoft.AspNetCore.Authentication;

namespace CoffeeShopApp.Services;

/// <summary>
/// Interface for external authentication service
/// </summary>
public interface IExternalAuthService
{
    /// <summary>
    /// Gets whether external authentication is enabled
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Gets the configured authentication protocol
    /// </summary>
    AuthenticationProtocol Protocol { get; }

    /// <summary>
    /// Gets the authentication scheme name for challenges
    /// </summary>
    string AuthenticationScheme { get; }

    /// <summary>
    /// Validates the current configuration
    /// </summary>
    /// <returns>True if configuration is valid, false otherwise</returns>
    bool ValidateConfiguration();

    /// <summary>
    /// Gets the OIDC configuration if protocol is OIDC
    /// </summary>
    OidcConfiguration? GetOidcConfiguration();

    /// <summary>
    /// Gets the SAML configuration if protocol is SAML
    /// </summary>
    SamlConfiguration? GetSamlConfiguration();
}

/// <summary>
/// Service for managing external authentication (OIDC and SAML)
/// </summary>
public class ExternalAuthService(ExternalAuthConfiguration config) : IExternalAuthService
{
    /// <inheritdoc />
    public bool IsEnabled => Protocol != AuthenticationProtocol.None;

    /// <inheritdoc />
    public AuthenticationProtocol Protocol => config.GetProtocol();

    /// <inheritdoc />
    public string AuthenticationScheme => Protocol switch
    {
        AuthenticationProtocol.OIDC => "OpenIdConnect",
        AuthenticationProtocol.SAML => "Saml2",
        _ => throw new InvalidOperationException("No external authentication configured")
    };

    /// <inheritdoc />
    public bool ValidateConfiguration()
    {
        return Protocol switch
        {
            AuthenticationProtocol.None => true,
            AuthenticationProtocol.OIDC => ValidateOidcConfiguration(),
            AuthenticationProtocol.SAML => ValidateSamlConfiguration(),
            _ => false
        };
    }

    /// <inheritdoc />
    public OidcConfiguration? GetOidcConfiguration()
    {
        return Protocol == AuthenticationProtocol.OIDC ? config.Oidc : null;
    }

    /// <inheritdoc />
    public SamlConfiguration? GetSamlConfiguration()
    {
        return Protocol == AuthenticationProtocol.SAML ? config.Saml : null;
    }

    private bool ValidateOidcConfiguration()
    {
        if (config.Oidc == null)
            return false;

        return !string.IsNullOrEmpty(config.Oidc.ClientId) &&
               !string.IsNullOrEmpty(config.Oidc.ClientSecret);
    }

    private bool ValidateSamlConfiguration()
    {
        if (config.Saml == null)
            return false;

        return !string.IsNullOrEmpty(config.Saml.EntityId) &&
               !string.IsNullOrEmpty(config.Saml.MetadataUrl);
    }
}