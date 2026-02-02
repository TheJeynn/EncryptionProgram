using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PasswordApp;
using PasswordApp.Dtos;
using ÞifrelemeApp.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static PasswordApp.Dtos.ResponseDtos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDb>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityCore<AppUser>(o =>
{
    o.Password.RequiredLength = 6;
    o.Password.RequireDigit = true;
    o.Password.RequireNonAlphanumeric = false;
}).AddEntityFrameworkStores<AppDb>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p => p
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials());
});

builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<EncryptionService>();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PasswordApp API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Token."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PasswordApp v1"));
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();


app.MapPost("/auth/register", async (UserManager<AppUser> users, RegisterDto dto) =>
{
    var user = new AppUser { UserName = dto.Email, Email = dto.Email };
    var result = await users.CreateAsync(user, dto.Password);
    return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
});

app.MapPost("/auth/login", async (UserManager<AppUser> users, TokenService tokens, HttpResponse resp, LoginDto dto) =>
{
    var user = await users.FindByEmailAsync(dto.Email);
    if (user is null || !await users.CheckPasswordAsync(user, dto.Password))
        return Results.Unauthorized();

    var access = tokens.CreateAccessToken(user);

    resp.Cookies.Append("refreshToken", Guid.NewGuid().ToString("N"), new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None,
        Expires = DateTimeOffset.UtcNow.AddDays(7)
    });

    return Results.Ok(new { accessToken = access });
});

app.MapGet("/me", [Microsoft.AspNetCore.Authorization.Authorize] (ClaimsPrincipal user) =>
{
    return Results.Ok(new
    {
        id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
        email = user.FindFirst(JwtRegisteredClaimNames.Email)?.Value,
        name = user.Identity?.Name
    });
});


app.MapGet("/api/vaults", [Microsoft.AspNetCore.Authorization.Authorize] async (AppDb db, ClaimsPrincipal principal) =>
{
    var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var vaults = await db.Vaults
        .Where(v => v.UserId == userId)
        .Select(v => new VaultDto { Id = v.Id, Name = v.Name, Description = v.Description })
        .ToListAsync();
    return Results.Ok(vaults);
});

app.MapPost("/api/vaults", [Microsoft.AspNetCore.Authorization.Authorize] async (VaultCreateDto dto, AppDb db, ClaimsPrincipal principal) =>
{
    var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

    var vault = new Vault { Name = dto.Name, Description = dto.Description, UserId = userId };
    db.Vaults.Add(vault);
    await db.SaveChangesAsync();
    return Results.Created($"/api/vaults/{vault.Id}", vault);
});


app.MapPost("/api/vaults/{vaultId}/secrets", [Microsoft.AspNetCore.Authorization.Authorize]
async (int vaultId, SecretItemCreateDto dto, AppDb db, ClaimsPrincipal principal, EncryptionService encryptionService) =>
{
    var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var vault = await db.Vaults.FirstOrDefaultAsync(v => v.Id == vaultId && v.UserId == userId);

    if (vault is null)
        return Results.NotFound(new { message = "No Authorization." });

    var secretItem = new SecretItem
    {
        Title = dto.Title,
        UserName = dto.UserName,
        PasswordHash = encryptionService.Encrypt(dto.PasswordHash),
        WebsiteUrl = dto.WebsiteUrl,
        Notes = dto.Notes,
        VaultId = vault.Id
    };

    db.SecretItems.Add(secretItem);
    await db.SaveChangesAsync();

    return Results.Created($"/api/vaults/{vault.Id}/secrets/{secretItem.Id}", secretItem);
});

app.Run();