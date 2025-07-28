using CoffeeShopApp.Data;
using CoffeeShopApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

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

// Check if Microsoft authentication is configured
var microsoftClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
var microsoftClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
var isMicrosoftAuthEnabled = !string.IsNullOrEmpty(microsoftClientId) && !string.IsNullOrEmpty(microsoftClientSecret);

// Add authentication
var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    if (isMicrosoftAuthEnabled)
    {
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
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

// Only add OpenID Connect if Microsoft authentication is configured
if (isMicrosoftAuthEnabled)
{
    authBuilder.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        // Microsoft Azure AD / Office 365 configuration
        options.Authority = "https://login.microsoftonline.com/common/v2.0";
        options.ClientId = microsoftClientId!;
        options.ClientSecret = microsoftClientSecret!;
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.CallbackPath = "/signin-microsoft";
        options.SignedOutCallbackPath = "/signout-callback-microsoft";

        // Scopes
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

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
                context.Response.Redirect("/Account/Login?error=microsoft_auth_failed");
                context.HandleResponse();
                return Task.CompletedTask;
            }
        };
    });
}

// Register the Microsoft auth availability as a service
builder.Services.AddSingleton<IMicrosoftAuthService>(provider =>
    new MicrosoftAuthService(isMicrosoftAuthEnabled));

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // Force HTTPS in development for OAuth
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
