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

                // 2. Connect directly to HospitalDb to create tables and execute migration batches
                using (var dbConn = new SqlConnection(defaultConnectionString))
                {
                    dbConn.Open();

                    // Batch 1: Create base tables
                    ExecuteSql(dbConn, @"
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

-- 2. Staff users
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StaffUsers')
BEGIN
    CREATE TABLE StaffUsers
    (
        StaffUserId INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(150) NOT NULL,
        Username NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(256) NOT NULL,
        Role NVARCHAR(50) NOT NULL CONSTRAINT DF_StaffUsers_Role DEFAULT ('Staff'),
        IsActive BIT NOT NULL CONSTRAINT DF_StaffUsers_IsActive DEFAULT (1),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_StaffUsers_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
END

-- 3. Patients Table
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

-- 4. Doctors Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Doctors')
BEGIN
    CREATE TABLE Doctors
    (
        DoctorId INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(150) NOT NULL,
        Specialty NVARCHAR(100) NOT NULL,
        Username NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(256) NOT NULL,
        PhoneNumber NVARCHAR(20) NULL,
        Email NVARCHAR(150) NULL,
        RoomNumber NVARCHAR(20) NULL,
        Availability NVARCHAR(100) NULL,
        ConsultationFee DECIMAL(10,2) NOT NULL CONSTRAINT DF_Doctors_ConsultationFee DEFAULT (0),
        IsActive BIT NOT NULL CONSTRAINT DF_Doctors_IsActive DEFAULT (1),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Doctors_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
END

-- 5. Appointments Table
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

-- 6. Prescriptions Table (Structured Medication History)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Prescriptions')
BEGIN
    CREATE TABLE Prescriptions
    (
        PrescriptionId INT IDENTITY(1,1) PRIMARY KEY,
        PatientId INT NOT NULL,
        DoctorId INT NOT NULL,
        AppointmentId INT NULL,
        MedicineName NVARCHAR(150) NOT NULL,
        Dosage NVARCHAR(100) NOT NULL,
        Frequency NVARCHAR(100) NOT NULL,
        Duration NVARCHAR(100) NOT NULL,
        Instructions NVARCHAR(300) NULL,
        PrescribedAt DATETIME2 NOT NULL CONSTRAINT DF_Prescriptions_PrescribedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_Prescriptions_Patients FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),
        CONSTRAINT FK_Prescriptions_Doctors FOREIGN KEY (DoctorId) REFERENCES Doctors(DoctorId)
    );
END

-- 7. HospitalBeds Table (Ward & Bed Occupancy)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'HospitalBeds')
BEGIN
    CREATE TABLE HospitalBeds
    (
        BedId INT IDENTITY(1,1) PRIMARY KEY,
        BedNumber NVARCHAR(50) NOT NULL UNIQUE,
        WardName NVARCHAR(100) NOT NULL,
        BedType NVARCHAR(50) NOT NULL,
        Floor NVARCHAR(50) NOT NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_HospitalBeds_Status DEFAULT ('Vacant'),
        PatientId INT NULL,
        AdmittedDoctorId INT NULL,
        AdmissionNotes NVARCHAR(300) NULL,
        AdmittedAt DATETIME2 NULL,
        CONSTRAINT FK_HospitalBeds_Patients FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),
        CONSTRAINT FK_HospitalBeds_Doctors FOREIGN KEY (AdmittedDoctorId) REFERENCES Doctors(DoctorId)
    );
END

-- 8. BedAdmissions Table (Inpatient Admission & Discharge History Log)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BedAdmissions')
BEGIN
    CREATE TABLE BedAdmissions
    (
        AdmissionId INT IDENTITY(1,1) PRIMARY KEY,
        BedId INT NOT NULL,
        BedNumber NVARCHAR(50) NOT NULL,
        WardName NVARCHAR(100) NOT NULL,
        PatientId INT NOT NULL,
        AdmittedDoctorId INT NOT NULL,
        AdmissionNotes NVARCHAR(500) NULL,
        AdmittedAt DATETIME2 NOT NULL CONSTRAINT DF_BedAdmissions_AdmittedAt DEFAULT (SYSUTCDATETIME()),
        DischargedAt DATETIME2 NULL,
        DischargeSummary NVARCHAR(500) NULL,
        DischargedByStaff NVARCHAR(150) NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_BedAdmissions_Status DEFAULT ('Active'),
        CONSTRAINT FK_BedAdmissions_Patients FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),
        CONSTRAINT FK_BedAdmissions_Doctors FOREIGN KEY (AdmittedDoctorId) REFERENCES Doctors(DoctorId)
    );
END
");

                    // Batch 2: Alter tables to add new columns (Staff, Doctor, Patient, Appointment)
                    ExecuteSql(dbConn, @"
IF COL_LENGTH('StaffUsers', 'PhoneNumber') IS NULL ALTER TABLE StaffUsers ADD PhoneNumber NVARCHAR(30) NOT NULL CONSTRAINT DF_StaffUsers_PhoneNumber DEFAULT ('');
IF COL_LENGTH('StaffUsers', 'Email') IS NULL ALTER TABLE StaffUsers ADD Email NVARCHAR(150) NOT NULL CONSTRAINT DF_StaffUsers_Email DEFAULT ('');
IF COL_LENGTH('StaffUsers', 'Department') IS NULL ALTER TABLE StaffUsers ADD Department NVARCHAR(100) NOT NULL CONSTRAINT DF_StaffUsers_Department DEFAULT ('General');
IF COL_LENGTH('StaffUsers', 'JobTitle') IS NULL ALTER TABLE StaffUsers ADD JobTitle NVARCHAR(100) NOT NULL CONSTRAINT DF_StaffUsers_JobTitle DEFAULT ('Staff');
IF COL_LENGTH('StaffUsers', 'EmployeeId') IS NULL ALTER TABLE StaffUsers ADD EmployeeId NVARCHAR(30) NULL;
IF COL_LENGTH('StaffUsers', 'Qualification') IS NULL ALTER TABLE StaffUsers ADD Qualification NVARCHAR(150) NOT NULL CONSTRAINT DF_StaffUsers_Qualification DEFAULT ('');
IF COL_LENGTH('StaffUsers', 'AssignedRoomOrWard') IS NULL ALTER TABLE StaffUsers ADD AssignedRoomOrWard NVARCHAR(100) NOT NULL CONSTRAINT DF_StaffUsers_AssignedRoomOrWard DEFAULT ('');
IF COL_LENGTH('StaffUsers', 'ShiftTiming') IS NULL ALTER TABLE StaffUsers ADD ShiftTiming NVARCHAR(100) NOT NULL CONSTRAINT DF_StaffUsers_ShiftTiming DEFAULT ('');
IF COL_LENGTH('StaffUsers', 'HiredByDoctorId') IS NULL ALTER TABLE StaffUsers ADD HiredByDoctorId INT NULL;
IF COL_LENGTH('StaffUsers', 'HiredByDoctorName') IS NULL ALTER TABLE StaffUsers ADD HiredByDoctorName NVARCHAR(150) NULL;

IF COL_LENGTH('Patients', 'MedicalRecordNumber') IS NULL ALTER TABLE Patients ADD MedicalRecordNumber NVARCHAR(50) NULL;

IF COL_LENGTH('Doctors', 'Username') IS NULL ALTER TABLE Doctors ADD Username NVARCHAR(100) NULL;
IF COL_LENGTH('Doctors', 'PasswordHash') IS NULL ALTER TABLE Doctors ADD PasswordHash NVARCHAR(256) NULL;
IF COL_LENGTH('Doctors', 'Qualification') IS NULL ALTER TABLE Doctors ADD Qualification NVARCHAR(150) NOT NULL CONSTRAINT DF_Doctors_Qualification DEFAULT ('MBBS, MD');

IF COL_LENGTH('Appointments', 'IsEmergency') IS NULL ALTER TABLE Appointments ADD IsEmergency BIT NOT NULL CONSTRAINT DF_Appointments_IsEmergency DEFAULT (0);
IF COL_LENGTH('Appointments', 'EmergencyDescription') IS NULL ALTER TABLE Appointments ADD EmergencyDescription NVARCHAR(500) NULL;
IF COL_LENGTH('Appointments', 'EmergencyAcknowledged') IS NULL ALTER TABLE Appointments ADD EmergencyAcknowledged BIT NOT NULL CONSTRAINT DF_Appointments_EmergencyAcknowledged DEFAULT (0);
IF COL_LENGTH('Appointments', 'DoctorNotes') IS NULL ALTER TABLE Appointments ADD DoctorNotes NVARCHAR(1000) NULL;
IF COL_LENGTH('Appointments', 'VitalsSummary') IS NULL ALTER TABLE Appointments ADD VitalsSummary NVARCHAR(300) NULL;
IF COL_LENGTH('Appointments', 'ApprovedAt') IS NULL ALTER TABLE Appointments ADD ApprovedAt DATETIME2 NULL;
IF COL_LENGTH('Appointments', 'TokenNumber') IS NULL ALTER TABLE Appointments ADD TokenNumber INT NULL;
IF COL_LENGTH('Appointments', 'PaymentStatus') IS NULL ALTER TABLE Appointments ADD PaymentStatus NVARCHAR(50) NOT NULL CONSTRAINT DF_Appointments_PaymentStatus DEFAULT ('Pending');
IF COL_LENGTH('Appointments', 'PaymentAmount') IS NULL ALTER TABLE Appointments ADD PaymentAmount DECIMAL(10,2) NOT NULL CONSTRAINT DF_Appointments_PaymentAmount DEFAULT (0);
IF COL_LENGTH('Appointments', 'PaymentMethod') IS NULL ALTER TABLE Appointments ADD PaymentMethod NVARCHAR(50) NULL;
IF COL_LENGTH('Appointments', 'ESITriageLevel') IS NULL ALTER TABLE Appointments ADD ESITriageLevel NVARCHAR(50) NULL;
");

                    // Batch 3: Indexes
                    ExecuteSql(dbConn, @"
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Patients_PhoneNumber' AND object_id = OBJECT_ID('Patients'))
    CREATE INDEX IX_Patients_PhoneNumber ON Patients(PhoneNumber);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Doctors_Specialty' AND object_id = OBJECT_ID('Doctors'))
    CREATE INDEX IX_Doctors_Specialty ON Doctors(Specialty);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Appointments_AppointmentDateTime' AND object_id = OBJECT_ID('Appointments'))
    CREATE INDEX IX_Appointments_AppointmentDateTime ON Appointments(AppointmentDateTime);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Prescriptions_PatientId' AND object_id = OBJECT_ID('Prescriptions'))
    CREATE INDEX IX_Prescriptions_PatientId ON Prescriptions(PatientId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HospitalBeds_Status' AND object_id = OBJECT_ID('HospitalBeds'))
    CREATE INDEX IX_HospitalBeds_Status ON HospitalBeds(Status);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BedAdmissions_PatientId' AND object_id = OBJECT_ID('BedAdmissions'))
    CREATE INDEX IX_BedAdmissions_PatientId ON BedAdmissions(PatientId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BedAdmissions_Status' AND object_id = OBJECT_ID('BedAdmissions'))
    CREATE INDEX IX_BedAdmissions_Status ON BedAdmissions(Status);
");

                    // Batch 4: Seed default accounts & backfill doctor qualifications
                    ExecuteSql(dbConn, @"
IF NOT EXISTS (SELECT 1 FROM AdminUsers WHERE Username = 'admin')
BEGIN
    INSERT INTO AdminUsers (Username, PasswordHash, IsActive)
    VALUES ('admin', 'admin123', 1);
END

IF NOT EXISTS (SELECT 1 FROM StaffUsers WHERE Username = 'sarah.nurse')
BEGIN
    INSERT INTO StaffUsers (FullName, Username, PasswordHash, Role, PhoneNumber, Email, Department, JobTitle, EmployeeId, Qualification, AssignedRoomOrWard, ShiftTiming, HiredByDoctorName, IsActive)
    VALUES ('Nurse Sarah Connor', 'sarah.nurse', 'Staff@123', 'Nurse', '9800090001', 'sarah.nurse@cityhosp.test', 'Critical Care & Inpatient', 'Charge Nurse', 'EMP-N01', 'B.Sc Nursing, Critical Care Certified', 'Ward 3B - Post-Op Recovery', 'Morning (07:00 - 15:00)', 'Dr. Arjun Mehta', 1);
END

IF NOT EXISTS (SELECT 1 FROM StaffUsers WHERE Username = 'john.reception')
BEGIN
    INSERT INTO StaffUsers (FullName, Username, PasswordHash, Role, PhoneNumber, Email, Department, JobTitle, EmployeeId, Qualification, AssignedRoomOrWard, ShiftTiming, HiredByDoctorName, IsActive)
    VALUES ('John Kelly', 'john.reception', 'Staff@123', 'Receptionist', '9800090002', 'john.kelly@cityhosp.test', 'Front Desk Operations', 'Senior Desk Officer', 'EMP-R01', 'BBA in Healthcare Administration', 'Main Reception Desk - Counter 01', 'Day (08:00 - 16:30)', 'Dr. Vikram Nair', 1);
END

-- Backfill doctor qualifications if empty
UPDATE StaffUsers SET HiredByDoctorName = 'Dr. Arjun Mehta' WHERE Username = 'sarah.nurse' AND (HiredByDoctorName IS NULL OR HiredByDoctorName = '');
UPDATE StaffUsers SET HiredByDoctorName = 'Dr. Vikram Nair' WHERE Username = 'john.reception' AND (HiredByDoctorName IS NULL OR HiredByDoctorName = '');
UPDATE Doctors SET Qualification = 'MBBS, MD (Cardiology), FACC' WHERE Specialty = 'Cardiology' AND (Qualification IS NULL OR Qualification = 'MBBS, MD' OR Qualification = '');
UPDATE Doctors SET Qualification = 'MBBS, DM (Neurology)' WHERE Specialty = 'Neurology' AND (Qualification IS NULL OR Qualification = 'MBBS, MD' OR Qualification = '');
UPDATE Doctors SET Qualification = 'MBBS, MS (Orthopedics), DNB' WHERE Specialty = 'Orthopedics' AND (Qualification IS NULL OR Qualification = 'MBBS, MD' OR Qualification = '');
UPDATE Doctors SET Qualification = 'MBBS, MD (Pediatrics), DCH' WHERE Specialty = 'Pediatrics' AND (Qualification IS NULL OR Qualification = 'MBBS, MD' OR Qualification = '');
UPDATE Doctors SET Qualification = 'MBBS, MD (Internal Medicine)' WHERE Specialty = 'General Medicine' AND (Qualification IS NULL OR Qualification = 'MBBS, MD' OR Qualification = '');
UPDATE Doctors SET Qualification = 'MBBS, MD (Dermatology)' WHERE Specialty = 'Dermatology' AND (Qualification IS NULL OR Qualification = 'MBBS, MD' OR Qualification = '');
UPDATE Doctors SET Qualification = 'MBBS, MS (Ophthalmology)' WHERE Specialty = 'Ophthalmology' AND (Qualification IS NULL OR Qualification = 'MBBS, MD' OR Qualification = '');
UPDATE Doctors SET Qualification = 'MBBS, MS (OBG), FRCOG' WHERE Specialty = 'Gynecology' AND (Qualification IS NULL OR Qualification = 'MBBS, MD' OR Qualification = '');
UPDATE Doctors SET Qualification = 'MBBS, MD (Psychiatry)' WHERE Specialty = 'Psychiatry' AND (Qualification IS NULL OR Qualification = 'MBBS, MD' OR Qualification = '');
UPDATE Doctors SET Qualification = 'MBBS, MD, DM (Endocrinology)' WHERE Specialty = 'Endocrinology' AND (Qualification IS NULL OR Qualification = 'MBBS, MD' OR Qualification = '');

-- Seed Hospital Beds if empty
IF NOT EXISTS (SELECT 1 FROM HospitalBeds)
BEGIN
    INSERT INTO HospitalBeds (BedNumber, WardName, BedType, Floor, Status, PatientId, AdmittedDoctorId, AdmissionNotes, AdmittedAt)
    VALUES 
    ('3B-01', 'Ward 3B - Post-Op & Surgical', 'Post-Op', '3rd Floor', 'Occupied', (SELECT TOP 1 PatientId FROM Patients), (SELECT TOP 1 DoctorId FROM Doctors WHERE Specialty='Orthopedics'), 'Post-knee arthroscopy recovery and observation', DATEADD(HOUR, -18, SYSUTCDATETIME())),
    ('3B-02', 'Ward 3B - Post-Op & Surgical', 'Post-Op', '3rd Floor', 'Vacant', NULL, NULL, NULL, NULL),
    ('3B-03', 'Ward 3B - Post-Op & Surgical', 'Post-Op', '3rd Floor', 'Vacant', NULL, NULL, NULL, NULL),
    ('3B-04', 'Ward 3B - Post-Op & Surgical', 'Post-Op', '3rd Floor', 'Vacant', NULL, NULL, NULL, NULL),
    ('ICU-01', 'ICU Critical Care Bay', 'ICU', '2nd Floor', 'Occupied', (SELECT TOP 1 PatientId FROM Patients ORDER BY PatientId DESC), (SELECT TOP 1 DoctorId FROM Doctors WHERE Specialty='Cardiology'), 'Acute cardiac telemetry monitoring & continuous IV infusion', DATEADD(HOUR, -6, SYSUTCDATETIME())),
    ('ICU-02', 'ICU Critical Care Bay', 'ICU', '2nd Floor', 'Vacant', NULL, NULL, NULL, NULL),
    ('ICU-03', 'ICU Critical Care Bay', 'ICU', '2nd Floor', 'Vacant', NULL, NULL, NULL, NULL),
    ('ER-01', 'Emergency Observation Wing', 'Emergency', 'Ground Floor', 'Vacant', NULL, NULL, NULL, NULL),
    ('ER-02', 'Emergency Observation Wing', 'Emergency', 'Ground Floor', 'Vacant', NULL, NULL, NULL, NULL),
    ('ER-03', 'Emergency Observation Wing', 'Emergency', 'Ground Floor', 'Vacant', NULL, NULL, NULL, NULL),
    ('2A-01', 'General Medical Ward 2A', 'General', '2nd Floor', 'Occupied', (SELECT TOP 1 PatientId FROM Patients WHERE PhoneNumber='9900020002'), (SELECT TOP 1 DoctorId FROM Doctors WHERE Specialty='General Medicine'), 'Pneumonia IV antibiotic therapy & oxygen support', DATEADD(DAY, -1, SYSUTCDATETIME())),
    ('2A-02', 'General Medical Ward 2A', 'General', '2nd Floor', 'Vacant', NULL, NULL, NULL, NULL),
    ('2A-03', 'General Medical Ward 2A', 'General', '2nd Floor', 'Vacant', NULL, NULL, NULL, NULL),
    ('2A-04', 'General Medical Ward 2A', 'General', '2nd Floor', 'Vacant', NULL, NULL, NULL, NULL);
END

-- Seed sample historical prescriptions if empty
IF NOT EXISTS (SELECT 1 FROM Prescriptions)
BEGIN
    INSERT INTO Prescriptions (PatientId, DoctorId, AppointmentId, MedicineName, Dosage, Frequency, Duration, Instructions, PrescribedAt)
    VALUES
    ((SELECT TOP 1 PatientId FROM Patients), (SELECT TOP 1 DoctorId FROM Doctors WHERE Specialty='Cardiology'), NULL, 'Atorvastatin 20mg', '1 Tablet', '0-0-1 (Night only)', '30 Days', 'Take after dinner with water, monitor lipid panel', DATEADD(DAY, -15, SYSUTCDATETIME())),
    ((SELECT TOP 1 PatientId FROM Patients), (SELECT TOP 1 DoctorId FROM Doctors WHERE Specialty='Cardiology'), NULL, 'Aspirin 75mg', '1 Tablet', '1-0-0 (Morning)', '30 Days', 'Take with breakfast', DATEADD(DAY, -15, SYSUTCDATETIME())),
    ((SELECT TOP 1 PatientId FROM Patients), (SELECT TOP 1 DoctorId FROM Doctors WHERE Specialty='General Medicine'), NULL, 'Amoxicillin 500mg', '1 Capsule', '1-0-1 (Twice daily)', '5 Days', 'Complete full course, take after food', DATEADD(DAY, -45, SYSUTCDATETIME()));
END

-- Seed sample Inpatient Admission Logs if empty
IF NOT EXISTS (SELECT 1 FROM BedAdmissions)
BEGIN
    INSERT INTO BedAdmissions (BedId, BedNumber, WardName, PatientId, AdmittedDoctorId, AdmissionNotes, AdmittedAt, DischargedAt, DischargeSummary, DischargedByStaff, Status)
    SELECT BedId, BedNumber, WardName, PatientId, AdmittedDoctorId, AdmissionNotes, AdmittedAt, NULL, NULL, NULL, 'Active'
    FROM HospitalBeds
    WHERE Status = 'Occupied' AND PatientId IS NOT NULL;
END
");

                    // Batch 5: Execute SeedData.sql if available
                    string seedScriptPath = Path.Combine(AppContext.BaseDirectory, "Database", "SeedData.sql");
                    if (!File.Exists(seedScriptPath))
                    {
                        seedScriptPath = Path.Combine(Directory.GetCurrentDirectory(), "Database", "SeedData.sql");
                    }

                    if (File.Exists(seedScriptPath))
                    {
                        string seedSql = File.ReadAllText(seedScriptPath);
                        var scriptBatches = System.Text.RegularExpressions.Regex.Split(seedSql, @"^\s*GO\s*$", System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                        foreach (var batch in scriptBatches)
                        {
                            string trimmedBatch = batch.Trim();
                            if (string.IsNullOrWhiteSpace(trimmedBatch) || trimmedBatch.StartsWith("USE ", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            ExecuteSql(dbConn, trimmedBatch);
                        }
                    }
                }

                Console.WriteLine("[DatabaseInitializer] HospitalDb, schema, and seed data initialized successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseInitializer Warning] Could not initialize database automatically: {ex.Message}");
            }
        }

        private static void ExecuteSql(SqlConnection connection, string sql)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
    }
}
