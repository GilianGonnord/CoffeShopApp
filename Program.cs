using System.Security.Cryptography.X509Certificates;
using CoffeeShopApp.Configuration;
using CoffeeShopApp.Data;
using CoffeeShopApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Sustainsys.Saml2;
using Sustainsys.Saml2.Metadata;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Add Entity Framework
builder.Services.AddDbContext<CoffeeShopContext>(options =>
    options.UseSqlite("Data Source=coffeeshop.sqlite3"));

// Configure HTTPS redirection for development
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
    options.HttpsPort = 5001;
});

// Configure external authentication
var externalAuthConfig = new ExternalAuthConfiguration();
builder.Configuration.GetSection("Authentication:External").Bind(externalAuthConfig);

// Register the external auth service
builder.Services.AddSingleton(externalAuthConfig);
builder.Services.AddSingleton<IExternalAuthService, ExternalAuthService>();

// Validate configuration
var tempServiceProvider = builder.Services.BuildServiceProvider();
var externalAuthService = tempServiceProvider.GetRequiredService<IExternalAuthService>();

if (!externalAuthService.ValidateConfiguration())
{
    var protocol = externalAuthService.Protocol;
    if (protocol != AuthenticationProtocol.None)
    {
        throw new InvalidOperationException(
            $"Invalid {protocol} configuration. Please check your appsettings.json Authentication:External section.");
    }
}

// Configure authentication
var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    if (externalAuthService.IsEnabled)
    {
        options.DefaultChallengeScheme = externalAuthService.AuthenticationScheme;
    }
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
});

// Configure external authentication based on protocol
if (externalAuthService.IsEnabled)
{
    switch (externalAuthService.Protocol)
    {
        case AuthenticationProtocol.OIDC:
            ConfigureOidcAuthentication(authBuilder, externalAuthService);
            break;

        case AuthenticationProtocol.SAML:
            ConfigureSamlAuthentication(authBuilder, externalAuthService);
            break;
    }
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // Force HTTPS in development for OAuth/SAML
    app.UseHttpsRedirection();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CoffeeShopContext>();
    context.Database.EnsureCreated();
}

app.Run();

// Helper methods for authentication configuration
static void ConfigureOidcAuthentication(AuthenticationBuilder authBuilder, IExternalAuthService externalAuthService)
{
    var oidcConfig = externalAuthService.GetOidcConfiguration();
    if (oidcConfig == null)
        throw new InvalidOperationException("OIDC configuration is missing");

    authBuilder.AddOpenIdConnect("OpenIdConnect", options =>
    {
        options.Authority = oidcConfig.Authority;
        options.ClientId = oidcConfig.ClientId;
        options.ClientSecret = oidcConfig.ClientSecret;
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.CallbackPath = "/signin-microsoft";
        options.SignedOutCallbackPath = "/signout-callback-microsoft";

        // Scopes
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        // Add any additional scopes
        foreach (var scope in oidcConfig.AdditionalScopes)
        {
            options.Scope.Add(scope);
        }

        // Token validation
        options.TokenValidationParameters.ValidateIssuer = false; // Allow multiple tenants
        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.RoleClaimType = "role";

        // Save tokens
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;

        // Events
        options.Events = new OpenIdConnectEvents
        {
            OnRemoteFailure = context =>
            {
                context.Response.Redirect("/Account/Login?error=external_auth_failed");
                context.HandleResponse();
                return Task.CompletedTask;
            }
        };
    });
}

static void ConfigureSamlAuthentication(AuthenticationBuilder authBuilder, IExternalAuthService externalAuthService)
{
    var samlConfig = externalAuthService.GetSamlConfiguration();
    if (samlConfig == null)
        throw new InvalidOperationException("SAML configuration is missing");

    authBuilder.AddSaml2("Saml2", options =>
    {
        // SP Options
        options.SPOptions.EntityId = new EntityId(samlConfig.EntityId);
        options.SPOptions.ReturnUrl = new Uri("/Account/ExternalCallback", UriKind.Relative);
        options.SPOptions.ModulePath = "/saml2";

        // Service Certificate (for signing requests)
        options.SPOptions.ServiceCertificates.Add(new ServiceCertificate
        {
            Use = CertificateUse.Signing,
            Certificate = new X509Certificate2(samlConfig.CertificatePath, samlConfig.CertificatePassword)
        });

        // Add Identity Provider from metadata
        options.IdentityProviders.Add(new IdentityProvider(
            new EntityId("IdP"), // This will be replaced by metadata
            options.SPOptions)
        {
            MetadataLocation = samlConfig.MetadataUrl,
            LoadMetadata = true
        });
    });
}