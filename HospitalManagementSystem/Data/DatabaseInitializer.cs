using Microsoft.Data.SqlClient;

namespace HospitalManagementSystem.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            try
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                string? defaultConnectionString = config.GetConnectionString("DefaultConnection");

                if (string.IsNullOrWhiteSpace(defaultConnectionString))
                {
                    Console.WriteLine("[DatabaseInitializer] DefaultConnection string is missing.");
                    return;
                }

                // 1. Connect to master to ensure HospitalDb database exists
                var masterBuilder = new SqlConnectionStringBuilder(defaultConnectionString)
                {
                    InitialCatalog = "master"
                };

                using (var masterConn = new SqlConnection(masterBuilder.ConnectionString))
                {
                    masterConn.Open();
                    using var createDbCmd = masterConn.CreateCommand();
                    createDbCmd.CommandText = @"
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'HospitalDb')
BEGIN
    CREATE DATABASE [HospitalDb];
END";
                    createDbCmd.ExecuteNonQuery();
                }

                // 2. Connect directly to HospitalDb to create tables and seed default admin
                using (var dbConn = new SqlConnection(defaultConnectionString))
                {
                    dbConn.Open();
                    using var schemaCmd = dbConn.CreateCommand();
                    schemaCmd.CommandText = @"
-- 1. AdminUsers Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AdminUsers')
BEGIN
    CREATE TABLE AdminUsers
    (
        AdminUserId INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(256) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_AdminUsers_IsActive DEFAULT (1),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_AdminUsers_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
END

-- Seed Default Admin Account if not already present
IF NOT EXISTS (SELECT 1 FROM AdminUsers WHERE Username = 'admin')
BEGIN
    INSERT INTO AdminUsers (Username, PasswordHash, IsActive)
    VALUES ('admin', 'admin123', 1);
END

-- 2. Patients Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Patients')
BEGIN
    CREATE TABLE Patients
    (
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
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Patients_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Patients_PhoneNumber' AND object_id = OBJECT_ID('Patients'))
BEGIN
    CREATE INDEX IX_Patients_PhoneNumber ON Patients(PhoneNumber);
END

-- 3. Doctors Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Doctors')
BEGIN
    CREATE TABLE Doctors
    (
        DoctorId INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(150) NOT NULL,
        Specialty NVARCHAR(100) NOT NULL,
        PhoneNumber NVARCHAR(20) NULL,
        Email NVARCHAR(150) NULL,
        RoomNumber NVARCHAR(20) NULL,
        Availability NVARCHAR(100) NULL,
        ConsultationFee DECIMAL(10,2) NOT NULL CONSTRAINT DF_Doctors_ConsultationFee DEFAULT (0),
        IsActive BIT NOT NULL CONSTRAINT DF_Doctors_IsActive DEFAULT (1),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Doctors_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Doctors_Specialty' AND object_id = OBJECT_ID('Doctors'))
BEGIN
    CREATE INDEX IX_Doctors_Specialty ON Doctors(Specialty);
END

-- 4. Appointments Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Appointments')
BEGIN
    CREATE TABLE Appointments
    (
        AppointmentId INT IDENTITY(1,1) PRIMARY KEY,
        PatientId INT NOT NULL,
        DoctorId INT NOT NULL,
        AppointmentDateTime DATETIME2 NOT NULL,
        Reason NVARCHAR(300) NOT NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Appointments_Status DEFAULT ('Scheduled'),
        Notes NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Appointments_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_Appointments_Patients FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),
        CONSTRAINT FK_Appointments_Doctors FOREIGN KEY (DoctorId) REFERENCES Doctors(DoctorId)
    );
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Appointments_AppointmentDateTime' AND object_id = OBJECT_ID('Appointments'))
BEGIN
    CREATE INDEX IX_Appointments_AppointmentDateTime ON Appointments(AppointmentDateTime);
END
";
                    schemaCmd.ExecuteNonQuery();
                }

                Console.WriteLine("[DatabaseInitializer] HospitalDb and schema initialized successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseInitializer Warning] Could not initialize database automatically: {ex.Message}");
            }
        }
    }
}
