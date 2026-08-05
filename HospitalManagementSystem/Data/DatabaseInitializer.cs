using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;

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
                    Console.WriteLine("DatabaseInitializer Error: DefaultConnection is missing.");
                    return;
                }

                // 1. Connect to master database to ensure HospitalDb exists
                var builder = new SqlConnectionStringBuilder(defaultConnectionString)
                {
                    InitialCatalog = "master"
                };

                using (var masterConn = new SqlConnection(builder.ConnectionString))
                {
                    masterConn.Open();
                    using var cmd = masterConn.CreateCommand();
                    cmd.CommandText = @"
                        IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'HospitalDb')
                        BEGIN
                            CREATE DATABASE [HospitalDb];
                            PRINT 'Created HospitalDb Database successfully.';
                        END";
                    cmd.ExecuteNonQuery();
                }

                // 2. Connect directly to HospitalDb to generate schema and seed defaults
                using (var dbConn = new SqlConnection(defaultConnectionString))
                {
                    dbConn.Open();
                    using var tableCmd = dbConn.CreateCommand();
                    tableCmd.CommandText = @"
                        -- 1. Users Table (For Local Admin Authentication)
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
                        BEGIN
                            CREATE TABLE Users (
                                UserId INT IDENTITY(1,1) PRIMARY KEY,
                                Username NVARCHAR(50) NOT NULL UNIQUE,
                                Password NVARCHAR(100) NOT NULL,
                                Role NVARCHAR(20) NOT NULL DEFAULT 'Admin'
                            );
                            INSERT INTO Users (Username, Password, Role) VALUES ('admin', 'admin123', 'Admin');
                            PRINT 'Created Users table and seeded default admin credentials.';
                        END

                        -- 2. AdminUsers Table (Friend's Schema Foundation)
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AdminUsers')
                        BEGIN
                            CREATE TABLE AdminUsers (
                                AdminUserId INT IDENTITY(1,1) PRIMARY KEY,
                                Username NVARCHAR(100) NOT NULL UNIQUE,
                                PasswordHash NVARCHAR(256) NOT NULL,
                                IsActive BIT NOT NULL CONSTRAINT DF_AdminUsers_IsActive DEFAULT (1),
                                CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_AdminUsers_CreatedAt DEFAULT (SYSUTCDATETIME())
                            );
                        END

                        -- 3. Patients Table
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Patients')
                        BEGIN
                            CREATE TABLE Patients (
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
                            CREATE INDEX IX_Patients_PhoneNumber ON Patients(PhoneNumber);
                        END

                        -- 4. Doctors Table
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Doctors')
                        BEGIN
                            CREATE TABLE Doctors (
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
                            CREATE INDEX IX_Doctors_Specialty ON Doctors(Specialty);
                        END

                        -- 5. Appointments Table
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Appointments')
                        BEGIN
                            CREATE TABLE Appointments (
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
                            CREATE INDEX IX_Appointments_AppointmentDateTime ON Appointments(AppointmentDateTime);
                        END

                        -- 6. Medical Records Table
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MedicalRecords')
                        BEGIN
                            CREATE TABLE MedicalRecords (
                                RecordId INT IDENTITY(1,1) PRIMARY KEY,
                                PatientId INT NOT NULL,
                                DoctorId INT NOT NULL,
                                Diagnosis NVARCHAR(MAX) NOT NULL,
                                Prescription NVARCHAR(MAX) NOT NULL,
                                RecordDate DATETIME DEFAULT GETDATE(),
                                FOREIGN KEY (PatientId) REFERENCES Patients(PatientId) ON DELETE CASCADE,
                                FOREIGN KEY (DoctorId) REFERENCES Doctors(DoctorId) ON DELETE NO ACTION
                            );
                        END

                        -- 7. Bills Table
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Bills')
                        BEGIN
                            CREATE TABLE Bills (
                                BillId INT IDENTITY(1,1) PRIMARY KEY,
                                PatientId INT NOT NULL,
                                Amount DECIMAL(18,2) NOT NULL CHECK (Amount >= 0),
                                BillDate DATETIME DEFAULT GETDATE(),
                                Status NVARCHAR(20) DEFAULT 'Unpaid' CHECK (Status IN ('Unpaid', 'Paid')),
                                FOREIGN KEY (PatientId) REFERENCES Patients(PatientId) ON DELETE CASCADE
                            );
                        END
                    ";
                    tableCmd.ExecuteNonQuery();
                    Console.WriteLine("Database initialization and verification completed successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DatabaseInitializer failed: {ex.Message}");
            }
        }
    }
}
