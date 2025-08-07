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
    /// Gets the display name for the external authentication provider
    /// </summary>
    string DisplayName { get; }

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
public class ExternalAuthService : IExternalAuthService
{
    private readonly ExternalAuthConfiguration _config;

    public ExternalAuthService(ExternalAuthConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <inheritdoc />
    public bool IsEnabled => Protocol != AuthenticationProtocol.None && ValidateConfiguration();

    /// <inheritdoc />
    public AuthenticationProtocol Protocol => _config.GetProtocol();

    /// <inheritdoc />
    public string DisplayName => Protocol switch
    {
        AuthenticationProtocol.OIDC => "Microsoft",
        AuthenticationProtocol.SAML => "SAML SSO",
        _ => "External Login"
    };

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

    private bool ValidateOidcConfiguration()
    {
        if (_config.Oidc == null)
            return false;

        return !string.IsNullOrEmpty(_config.Oidc.ClientId) &&
               !string.IsNullOrEmpty(_config.Oidc.ClientSecret);
    }

    private bool ValidateSamlConfiguration()
    {
        if (_config.Saml == null)
            return false;

        return !string.IsNullOrEmpty(_config.Saml.EntityId) &&
               !string.IsNullOrEmpty(_config.Saml.MetadataUrl);
    }

    public OidcConfiguration? GetOidcConfiguration()
    {
        return Protocol == AuthenticationProtocol.OIDC ? _config.Oidc : null;
    }

    public SamlConfiguration? GetSamlConfiguration()
    {
        return Protocol == AuthenticationProtocol.SAML ? _config.Saml : null;
    }
}