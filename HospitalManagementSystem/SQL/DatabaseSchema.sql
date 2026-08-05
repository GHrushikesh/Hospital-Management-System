-- =========================================
-- Hospital Management System - SQL Schema
-- =========================================

-- Create the Database (Uncomment the next two lines if running in SSMS for the first time)
-- CREATE DATABASE HospitalDb;
-- GO
-- USE HospitalDb;
-- GO

-- 1. Users Table (For Authentication / Admin Login)
CREATE TABLE Users (
	UserId INT IDENTITY(1,1) PRIMARY KEY,
	Username NVARCHAR(50) NOT NULL UNIQUE,
	Password NVARCHAR(100) NOT NULL, -- For a mini-project, we keep it simple. In production, always hash passwords!
	Role NVARCHAR(20) NOT NULL DEFAULT 'Admin'
);

-- Insert a default Admin user so we can test login right away in Milestone 2
INSERT INTO Users (Username, Password, Role) 
VALUES ('admin', 'admin123', 'Admin');


-- 2. Patients Table
CREATE TABLE Patients (
	PatientId INT IDENTITY(1,1) PRIMARY KEY,
	FullName NVARCHAR(100) NOT NULL,
	Age INT NOT NULL CHECK (Age >= 0),
	Gender NVARCHAR(10) CHECK (Gender IN ('Male', 'Female', 'Other')),
	ContactNumber NVARCHAR(15) NOT NULL,
	Address NVARCHAR(250),
	RegistrationDate DATETIME DEFAULT GETDATE()
);


-- 3. Doctors Table
CREATE TABLE Doctors (
	DoctorId INT IDENTITY(1,1) PRIMARY KEY,
	FullName NVARCHAR(100) NOT NULL,
	Specialization NVARCHAR(100) NOT NULL,
	ContactNumber NVARCHAR(15) NOT NULL
);


-- 4. Appointments Table
CREATE TABLE Appointments (
	AppointmentId INT IDENTITY(1,1) PRIMARY KEY,
	PatientId INT NOT NULL,
	DoctorId INT NOT NULL,
	AppointmentDate DATETIME NOT NULL,
	Reason NVARCHAR(250),
	Status NVARCHAR(20) DEFAULT 'Scheduled' CHECK (Status IN ('Scheduled', 'Completed', 'Cancelled')),

	-- Foreign Keys to establish relationships
	FOREIGN KEY (PatientId) REFERENCES Patients(PatientId) ON DELETE CASCADE,
	FOREIGN KEY (DoctorId) REFERENCES Doctors(DoctorId) ON DELETE CASCADE
);


-- 5. Medical Records Table
CREATE TABLE MedicalRecords (
	RecordId INT IDENTITY(1,1) PRIMARY KEY,
	PatientId INT NOT NULL,
	DoctorId INT NOT NULL,
	Diagnosis NVARCHAR(MAX) NOT NULL,
	Prescription NVARCHAR(MAX) NOT NULL,
	RecordDate DATETIME DEFAULT GETDATE(),

	-- Foreign Keys 
	-- We use ON DELETE NO ACTION so deleting a patient doesn't accidentally wipe medical history unconditionally depending on business rules, 
	-- but for simplicity, you can change this to CASCADE if you want everything wiped when a patient is deleted.
	FOREIGN KEY (PatientId) REFERENCES Patients(PatientId) ON DELETE CASCADE,
	FOREIGN KEY (DoctorId) REFERENCES Doctors(DoctorId) ON DELETE NO ACTION
);


-- 6. Bills Table
CREATE TABLE Bills (
	BillId INT IDENTITY(1,1) PRIMARY KEY,
	PatientId INT NOT NULL,
	Amount DECIMAL(18,2) NOT NULL CHECK (Amount >= 0),
	BillDate DATETIME DEFAULT GETDATE(),
	Status NVARCHAR(20) DEFAULT 'Unpaid' CHECK (Status IN ('Unpaid', 'Paid')),

	-- Foreign Key
	FOREIGN KEY (PatientId) REFERENCES Patients(PatientId) ON DELETE CASCADE
);
