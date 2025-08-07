namespace CoffeeShopApp.Services;

/// <summary>
/// Defines the external authentication protocols supported by the application
/// </summary>
public enum AuthenticationProtocol
{
    /// <summary>
    /// No external authentication configured - only local authentication
    /// </summary>
    None = 0,

    /// <summary>
    /// OpenID Connect authentication protocol
    /// </summary>
    OIDC = 1,

    /// <summary>
    /// SAML 2.0 authentication protocol
    /// </summary>
    SAML = 2
}