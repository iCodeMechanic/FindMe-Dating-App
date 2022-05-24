using System.Text;
using API.Data;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using API.Middleware;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<IPhotoService,PhotoService>();
builder.Services.AddScoped<LogUserActivity>();
builder.Services.AddScoped<ILikesRepository,LikesRepository>();
builder.Services.AddScoped<IMessageRepository,MessageRepository>();
builder.Services.AddScoped<ITokenService,TokenService>();
builder.Services.AddScoped<IUserRepository,UserRepository>();
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
builder.Services.AddDbContext<DataContext>(options =>{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddIdentityCore<AppUser>(opt =>
{
    opt.Password.RequireNonAlphanumeric = false;
})
.AddRoles<AppRole>()
.AddRoleManager<RoleManager<AppRole>>()
.AddSignInManager<SignInManager<AppUser>>()
.AddRoleValidator<RoleValidator<AppRole>>()
.AddEntityFrameworkStores<DataContext>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["TokenKey"])),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
});

builder.Services.AddAuthorization(opt =>{
    opt.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    opt.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin","Moderator"));
});
var app = builder.Build();
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try{
    var context = services.GetRequiredService<DataContext>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    await context.Database.MigrateAsync();
    await Seed.SeedUsers(userManager,roleManager);
}catch(Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occured during migration");
}

app.UseMiddleware<ExceptionMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(x=>x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

