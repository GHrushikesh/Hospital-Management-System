using HospitalManagementSystem.Data;
using HospitalManagementSystem.Data.Repositories;
using HospitalManagementSystem.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Services (Dependency Injection)
// Add MVC Support (Controllers & Views)
builder.Services.AddControllersWithViews();

// Register DbConnectionHelper for Dependency Injection (Singleton because it only reads config)
builder.Services.AddSingleton<DbConnectionHelper>();
builder.Services.AddScoped<AdminRepository>();
builder.Services.AddScoped<PatientRepository>();
builder.Services.AddScoped<DoctorRepository>();
builder.Services.AddScoped<AppointmentRepository>();

// Register AuthRepository for Dependency Injection
builder.Services.AddScoped<AuthRepository>();

// 2. Configure Session (For Admin Login)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session expires after 30 mins
    options.Cookie.HttpOnly = true;                 // Secure cookie
    options.Cookie.IsEssential = true;              // Required for login functionality
});

// Configure HttpContextAccessor mapping to access sessions in Views if needed
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var app = builder.Build();

// 3. Configure the HTTP Request Pipeline (Middleware)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// 4. Configure Static Files (CSS, JS, Images from wwwroot)
app.UseStaticFiles(); 
// Note: We use UseStaticFiles instead of MapStaticAssets for broader compatibility and simplicity in standard MVC.

// 5. Configure Routing
app.UseRouting();

// Enable Session Middleware (MUST be before UseAuthorization)
app.UseSession();

app.UseAuthorization();

// Setup Default Route Pattern
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 6. Automatic Database Initialization (Create HospitalDb & seed tables/defaults if missing)
using (var scope = app.Services.CreateScope())
{
    DatabaseInitializer.Initialize(scope.ServiceProvider);
}

app.Run();
