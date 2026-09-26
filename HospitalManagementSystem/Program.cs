using HospitalManagementSystem.Data;
using HospitalManagementSystem.Data.Repositories;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

EnsureDatabaseIsReady(builder.Configuration);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register DbConnectionHelper for Dependency Injection
builder.Services.AddSingleton<DbConnectionHelper>();
builder.Services.AddScoped<AdminRepository>();
builder.Services.AddScoped<StaffRepository>();
builder.Services.AddScoped<PatientRepository>();
builder.Services.AddScoped<DoctorRepository>();
builder.Services.AddScoped<AppointmentRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

static void EnsureDatabaseIsReady(IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return;
    }

    var masterConnectionString = connectionString
        .Replace("Database=HospitalDb", "Database=master")
        .Replace("Initial Catalog=HospitalDb", "Initial Catalog=master");

    using var masterConnection = new SqlConnection(masterConnectionString);
    masterConnection.Open();

    using (var createDb = new SqlCommand(@"IF DB_ID(N'HospitalDb') IS NULL CREATE DATABASE HospitalDb;", masterConnection))
    {
        createDb.ExecuteNonQuery();
    }

    using var hospitalConnection = new SqlConnection(connectionString);
    hospitalConnection.Open();

    using (var createSchema = new SqlCommand(@"
IF OBJECT_ID(N'dbo.AdminUsers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AdminUsers (
        AdminUserId INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(256) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END;

IF OBJECT_ID(N'dbo.Patients', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Patients (
        PatientId INT IDENTITY(1,1) PRIMARY KEY,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        DateOfBirth DATE NULL,
        Gender NVARCHAR(20) NOT NULL,
        PhoneNumber NVARCHAR(20) NOT NULL,
        Email NVARCHAR(150) NULL,
        Address NVARCHAR(300) NULL,
        BloodGroup NVARCHAR(10) NULL,
        EmergencyContactName NVARCHAR(100) NULL,
        EmergencyContactNumber NVARCHAR(20) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END;

IF OBJECT_ID(N'dbo.Doctors', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Doctors (
        DoctorId INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(150) NOT NULL,
        Specialty NVARCHAR(100) NOT NULL,
        PhoneNumber NVARCHAR(20) NULL,
        Email NVARCHAR(150) NULL,
        RoomNumber NVARCHAR(20) NULL,
        Availability NVARCHAR(100) NULL,
        ConsultationFee DECIMAL(10,2) NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END;

IF OBJECT_ID(N'dbo.Appointments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Appointments (
        AppointmentId INT IDENTITY(1,1) PRIMARY KEY,
        PatientId INT NOT NULL,
        DoctorId INT NOT NULL,
        AppointmentDateTime DATETIME2 NOT NULL,
        Reason NVARCHAR(300) NOT NULL,
        Status NVARCHAR(30) NOT NULL DEFAULT 'Scheduled',
        Notes NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Appointments_Patients FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),
        CONSTRAINT FK_Appointments_Doctors FOREIGN KEY (DoctorId) REFERENCES Doctors(DoctorId)
    );
END;
", hospitalConnection))
    {
        createSchema.ExecuteNonQuery();
    }

    using (var ensureStaffTable = new SqlCommand(@"
IF OBJECT_ID(N'dbo.StaffUsers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StaffUsers (
        StaffUserId INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(150) NOT NULL,
        Username NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(256) NOT NULL,
        Role NVARCHAR(50) NOT NULL DEFAULT 'Staff',
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END;
", hospitalConnection))
    {
        ensureStaffTable.ExecuteNonQuery();
    }

    using (var seedData = new SqlCommand(@"
IF NOT EXISTS (SELECT 1 FROM dbo.AdminUsers WHERE Username = 'admin')
BEGIN
    INSERT INTO dbo.AdminUsers (Username, PasswordHash, IsActive)
    VALUES ('admin', 'admin123', 1);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.StaffUsers WHERE Username = 'staff')
BEGIN
    INSERT INTO dbo.StaffUsers (FullName, Username, PasswordHash, Role, IsActive)
    VALUES ('Hospital Staff', 'staff', 'staff123', 'Staff', 1);
END;
", hospitalConnection))
    {
        seedData.ExecuteNonQuery();
    }
}

