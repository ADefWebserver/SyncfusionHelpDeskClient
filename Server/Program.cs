using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using SyncfusionHelpDeskClient.Server.Data;
using SyncfusionHelpDeskClient.Server.Models;
using SyncfusionHelpDesk.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// To access HelpDesk tables.
builder.Services.AddDbContext<SyncfusionHelpDeskContext>(options =>
options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>()
    .AddRoles<Microsoft.AspNetCore.Identity.IdentityRole>() // Add roles
    .AddEntityFrameworkStores<ApplicationDbContext>();

// From: https://github.com/dotnet/AspNetCore.Docs/issues/17649
// Configure identity server to put the role claim into the id token 
// and the access token and prevent the default mapping for roles 
// in the JwtSecurityTokenHandler.
builder.Services.AddIdentityServer()
    .AddApiAuthorization<ApplicationUser, ApplicationDbContext>(options =>
    {
        options.IdentityResources["openid"].UserClaims.Add("role");
        options.ApiResources.Single().UserClaims.Add("role");
    });

// Need to do this as it maps "role" to ClaimTypes.Role and causes issues.
System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler
    .DefaultInboundClaimTypeMap.Remove("role");

builder.Services.AddAuthentication()
    .AddIdentityServerJwt();

builder.Services.AddControllersWithViews()
                .AddNewtonsoftJson(
                options =>
                options.SerializerSettings.ReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore
                );

builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();