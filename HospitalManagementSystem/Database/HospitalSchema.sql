CREATE DATABASE HospitalDb;
GO

USE HospitalDb;
GO

CREATE TABLE AdminUsers
(
    AdminUserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_AdminUsers_IsActive DEFAULT (1),
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_AdminUsers_CreatedAt DEFAULT (SYSUTCDATETIME())
);

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

CREATE INDEX IX_Patients_PhoneNumber ON Patients(PhoneNumber);
CREATE INDEX IX_Doctors_Specialty ON Doctors(Specialty);
CREATE INDEX IX_Appointments_AppointmentDateTime ON Appointments(AppointmentDateTime);
CREATE TABLE Bills
(
    BillId INT IDENTITY(1,1) PRIMARY KEY,
    PatientId INT NOT NULL,
    AppointmentId INT NULL,
    DoctorId INT NULL,
    BillDate DATETIME2 NOT NULL CONSTRAINT DF_Bills_BillDate DEFAULT (SYSUTCDATETIME()),
    SubTotal DECIMAL(10,2) NOT NULL CONSTRAINT DF_Bills_SubTotal DEFAULT (0),
    Tax DECIMAL(10,2) NOT NULL CONSTRAINT DF_Bills_Tax DEFAULT (0),
    Discount DECIMAL(10,2) NOT NULL CONSTRAINT DF_Bills_Discount DEFAULT (0),
    Total DECIMAL(10,2) NOT NULL CONSTRAINT DF_Bills_Total DEFAULT (0),
    PaymentStatus NVARCHAR(50) NOT NULL CONSTRAINT DF_Bills_PaymentStatus DEFAULT ('Unpaid'),
    PaymentMethod NVARCHAR(50) NULL,
    Notes NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Bills_CreatedAt DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT FK_Bills_Patients
        FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),

    CONSTRAINT FK_Bills_Appointments
        FOREIGN KEY (AppointmentId) REFERENCES Appointments(AppointmentId),

    CONSTRAINT FK_Bills_Doctors
        FOREIGN KEY (DoctorId) REFERENCES Doctors(DoctorId)
);

CREATE TABLE BillItems
(
    BillItemId INT IDENTITY(1,1) PRIMARY KEY,
    BillId INT NOT NULL,
    Description NVARCHAR(300) NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_BillItems_CreatedAt DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT FK_BillItems_Bills
        FOREIGN KEY (BillId) REFERENCES Bills(BillId)
        ON DELETE CASCADE
);

CREATE INDEX IX_Bills_PatientId
    ON Bills(PatientId);

CREATE INDEX IX_Bills_BillDate
    ON Bills(BillDate);

CREATE INDEX IX_Bills_PaymentStatus
    ON Bills(PaymentStatus);

CREATE INDEX IX_BillItems_BillId
    ON BillItems(BillId);