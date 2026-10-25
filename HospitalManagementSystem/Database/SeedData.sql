-- =============================================================================
-- SeedData.sql  —  Safe, idempotent sample data for HospitalDb
-- Run against: Server=(localdb)\MSSQLLocalDB;Database=HospitalDb
-- This script uses IF NOT EXISTS guards so it is safe to run multiple times.
-- It never drops or truncates existing data.
-- Updated 2026-10-24: Added Username and PasswordHash for Doctors (new schema columns).
-- =============================================================================

USE HospitalDb;
GO

-- -----------------------------------------------------------------------------
-- Schema migration guard: add Username/PasswordHash to Doctors if missing
-- (needed if DB was created from old schema before teammate's changes)
-- -----------------------------------------------------------------------------
IF COL_LENGTH('dbo.Doctors', 'Username')     IS NULL ALTER TABLE dbo.Doctors ADD Username     NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.Doctors', 'PasswordHash') IS NULL ALTER TABLE dbo.Doctors ADD PasswordHash NVARCHAR(256) NULL;
GO

-- -----------------------------------------------------------------------------
-- 1. DOCTORS  (10 realistic fictitious doctors)
--    Username  = dr.<firstname> (lowercase)
--    Password  = 'Doctor@123'  (plain text — same as app default for new doctors)
-- -----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Doctors WHERE Email = 'arjun.mehta@cityhosp.test')
    INSERT INTO dbo.Doctors (FullName, Specialty, Username, PasswordHash, PhoneNumber, Email, RoomNumber, Availability, ConsultationFee, IsActive)
    VALUES ('Dr. Arjun Mehta',        'Cardiology',       'dr.arjun',   'Doctor@123', '9800010001', 'arjun.mehta@cityhosp.test',    'A-101', 'Mon-Fri 09:00-13:00', 800.00,  1);

IF NOT EXISTS (SELECT 1 FROM dbo.Doctors WHERE Email = 'priya.sharma@cityhosp.test')
    INSERT INTO dbo.Doctors (FullName, Specialty, Username, PasswordHash, PhoneNumber, Email, RoomNumber, Availability, ConsultationFee, IsActive)
    VALUES ('Dr. Priya Sharma',       'Neurology',        'dr.priya',   'Doctor@123', '9800010002', 'priya.sharma@cityhosp.test',   'B-202', 'Tue-Sat 10:00-14:00', 900.00,  1);

IF NOT EXISTS (SELECT 1 FROM dbo.Doctors WHERE Email = 'ravi.krishnan@cityhosp.test')
    INSERT INTO dbo.Doctors (FullName, Specialty, Username, PasswordHash, PhoneNumber, Email, RoomNumber, Availability, ConsultationFee, IsActive)
    VALUES ('Dr. Ravi Krishnan',      'Orthopedics',      'dr.ravi',    'Doctor@123', '9800010003', 'ravi.krishnan@cityhosp.test',  'C-303', 'Mon-Wed 08:00-12:00', 700.00,  1);

IF NOT EXISTS (SELECT 1 FROM dbo.Doctors WHERE Email = 'sunita.bose@cityhosp.test')
    INSERT INTO dbo.Doctors (FullName, Specialty, Username, PasswordHash, PhoneNumber, Email, RoomNumber, Availability, ConsultationFee, IsActive)
    VALUES ('Dr. Sunita Bose',        'Pediatrics',       'dr.sunita',  'Doctor@123', '9800010004', 'sunita.bose@cityhosp.test',    'D-104', 'Mon-Fri 09:00-17:00', 600.00,  1);

IF NOT EXISTS (SELECT 1 FROM dbo.Doctors WHERE Email = 'vikram.nair@cityhosp.test')
    INSERT INTO dbo.Doctors (FullName, Specialty, Username, PasswordHash, PhoneNumber, Email, RoomNumber, Availability, ConsultationFee, IsActive)
    VALUES ('Dr. Vikram Nair',        'General Medicine', 'dr.vikram',  'Doctor@123', '9800010005', 'vikram.nair@cityhosp.test',    'E-105', 'Mon-Sat 08:00-20:00', 400.00,  1);

IF NOT EXISTS (SELECT 1 FROM dbo.Doctors WHERE Email = 'ananya.reddy@cityhosp.test')
    INSERT INTO dbo.Doctors (FullName, Specialty, Username, PasswordHash, PhoneNumber, Email, RoomNumber, Availability, ConsultationFee, IsActive)
    VALUES ('Dr. Ananya Reddy',       'Dermatology',      'dr.ananya',  'Doctor@123', '9800010006', 'ananya.reddy@cityhosp.test',   'F-206', 'Wed-Sun 11:00-15:00', 650.00,  1);

IF NOT EXISTS (SELECT 1 FROM dbo.Doctors WHERE Email = 'suresh.patel@cityhosp.test')
    INSERT INTO dbo.Doctors (FullName, Specialty, Username, PasswordHash, PhoneNumber, Email, RoomNumber, Availability, ConsultationFee, IsActive)
    VALUES ('Dr. Suresh Patel',       'Ophthalmology',    'dr.suresh',  'Doctor@123', '9800010007', 'suresh.patel@cityhosp.test',   'G-307', 'Mon-Thu 10:00-16:00', 550.00,  1);

IF NOT EXISTS (SELECT 1 FROM dbo.Doctors WHERE Email = 'meena.iyer@cityhosp.test')
    INSERT INTO dbo.Doctors (FullName, Specialty, Username, PasswordHash, PhoneNumber, Email, RoomNumber, Availability, ConsultationFee, IsActive)
    VALUES ('Dr. Meena Iyer',         'Gynecology',       'dr.meena',   'Doctor@123', '9800010008', 'meena.iyer@cityhosp.test',     'H-108', 'Tue-Fri 09:00-13:00', 750.00,  1);

IF NOT EXISTS (SELECT 1 FROM dbo.Doctors WHERE Email = 'rohit.desai@cityhosp.test')
    INSERT INTO dbo.Doctors (FullName, Specialty, Username, PasswordHash, PhoneNumber, Email, RoomNumber, Availability, ConsultationFee, IsActive)
    VALUES ('Dr. Rohit Desai',        'Psychiatry',       'dr.rohit',   'Doctor@123', '9800010009', 'rohit.desai@cityhosp.test',    'I-209', 'Mon-Fri 14:00-18:00', 850.00,  1);

IF NOT EXISTS (SELECT 1 FROM dbo.Doctors WHERE Email = 'kavitha.rao@cityhosp.test')
    INSERT INTO dbo.Doctors (FullName, Specialty, Username, PasswordHash, PhoneNumber, Email, RoomNumber, Availability, ConsultationFee, IsActive)
    VALUES ('Dr. Kavitha Rao',        'Endocrinology',    'dr.kavitha', 'Doctor@123', '9800010010', 'kavitha.rao@cityhosp.test',    'J-310', 'Mon-Wed 10:00-14:00', 950.00,  0);
-- Note: Kavitha Rao is IsActive=0 — tests "inactive doctor" filter on Appointment booking page.
GO

-- -----------------------------------------------------------------------------
-- 2. PATIENTS  (15 realistic fictitious patients)
-- -----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Patients WHERE PhoneNumber = '9900020001')
    INSERT INTO dbo.Patients (FirstName, LastName, DateOfBirth, Gender, PhoneNumber, Email, Address, BloodGroup, EmergencyContactName, EmergencyContactNumber)
    VALUES ('Aarav',    'Gupta',     '1985-03-12', 'Male',   '9900020001', 'aarav.gupta@mail.test',     '12 MG Road, Bengaluru',       'B+', 'Sneha Gupta',     '9900029901');

IF NOT EXISTS (SELECT 1 FROM dbo.Patients WHERE PhoneNumber = '9900020002')
    INSERT INTO dbo.Patients (FirstName, LastName, DateOfBirth, Gender, PhoneNumber, Email, Address, BloodGroup, EmergencyContactName, EmergencyContactNumber)
    VALUES ('Divya',    'Menon',     '1992-07-25', 'Female', '9900020002', 'divya.menon@mail.test',     '5 Lake View, Kochi',          'A-', 'Raj Menon',       '9900029902');

IF NOT EXISTS (SELECT 1 FROM dbo.Patients WHERE PhoneNumber = '9900020003')
    INSERT INTO dbo.Patients (FirstName, LastName, DateOfBirth, Gender, PhoneNumber, Email, Address, BloodGroup, EmergencyContactName, EmergencyContactNumber)
    VALUES ('Karan',    'Singh',     '1978-11-05', 'Male',   '9900020003', 'karan.singh@mail.test',     '33 Civil Lines, Delhi',       'O+', 'Meera Singh',     '9900029903');

IF NOT EXISTS (SELECT 1 FROM dbo.Patients WHERE PhoneNumber = '9900020004')
    INSERT INTO dbo.Patients (FirstName, LastName, DateOfBirth, Gender, PhoneNumber, Email, Address, BloodGroup, EmergencyContactName, EmergencyContactNumber)
    VALUES ('Pooja',    'Shah',      '2001-01-18', 'Female', '9900020004', 'pooja.shah@mail.test',      '7 Satellite Road, Ahmedabad', 'AB+','Nilesh Shah',     '9900029904');

IF NOT EXISTS (SELECT 1 FROM dbo.Patients WHERE PhoneNumber = '9900020005')
    INSERT INTO dbo.Patients (FirstName, LastName, DateOfBirth, Gender, PhoneNumber, Email, Address, BloodGroup, EmergencyContactName, EmergencyContactNumber)
    VALUES ('Rahul',    'Verma',     '1990-06-30', 'Male',   '9900020005', 'rahul.verma@mail.test',     '21 Station Road, Pune',       'B-', 'Rekha Verma',     '9900029905');

IF NOT EXISTS (SELECT 1 FROM dbo.Patients WHERE PhoneNumber = '9900020006')
    INSERT INTO dbo.Patients (FirstName, LastName, DateOfBirth, Gender, PhoneNumber, Email, Address, BloodGroup, EmergencyContactName, EmergencyContactNumber)
    VALUES ('Shalini',  'Nambiar',   '1966-09-14', 'Female', '9900020006', 'shalini.nambiar@mail.test', '9 Beach Road, Chennai',       'O-', 'Vijay Nambiar',   '9900029906');

IF NOT EXISTS (SELECT 1 FROM dbo.Patients WHERE PhoneNumber = '9900020007')
    INSERT INTO dbo.Patients (FirstName, LastName, DateOfBirth, Gender, PhoneNumber, Email, Address, BloodGroup, EmergencyContactName, EmergencyContactNumber)
    VALUES ('Tarun',    'Agarwal',   '2005-04-22', 'Male',   '9900020007', 'tarun.agarwal@mail.test',   '88 Park Street, Kolkata',     'A+', 'Sunita Agarwal', '9900029907');

IF NOT EXISTS (SELECT 1 FROM dbo.Patients WHERE PhoneNumber = '9900020008')
    INSERT INTO dbo.Patients (FirstName, LastName, DateOfBirth, Gender, PhoneNumber, Email, Address, BloodGroup, EmergencyContactName, EmergencyContactNumber)
    VALUES ('Uma',      'Pillai',    '1955-12-01', 'Female', '9900020008', 'uma.pillai@mail.test',      '14 Temple Street, Madurai',   'AB-','Rajesh Pillai',   '9900029908');

IF NOT EXISTS (SELECT 1 FROM dbo.Patients WHERE PhoneNumber = '9900020009')
    INSERT INTO dbo.Patients (FirstName, LastName, DateOfBirth, Gender, PhoneNumber, Email, Address, BloodGroup, EmergencyContactName, EmergencyContactNumber)
    VALUES ('Vivek',    'Joshi',     '1983-08-17', 'Male',   '9900020009', 'vivek.joshi@mail.test',     '6 Old Town, Nashik',          'B+', 'Asha Joshi',      '9900029909');

IF NOT EXISTS (SELECT 1 FROM dbo.Patients WHERE PhoneNumber = '9900020010')
    INSERT INTO dbo.Patients (FirstName, LastName, DateOfBirth, Gender, PhoneNumber, Email, Address, BloodGroup, EmergencyContactName, EmergencyContactNumber)
    VALUES ('Yasmin',   'Khan',      '1997-02-09', 'Female', '9900020010', 'yasmin.khan@mail.test',     '3 Banjara Hills, Hyderabad',  'O+', 'Imran Khan',      '9900029910');

IF NOT EXISTS (SELECT 1 FROM dbo.Patients WHERE PhoneNumber = '9900020011')
    INSERT INTO dbo.Patients (FirstName, LastName, DateOfBirth, Gender, PhoneNumber, Email, Address, BloodGroup, EmergencyContactName, EmergencyContactNumber)
    VALUES ('Nikhil',   'Chavan',   '1975-05-28', 'Male',   '9900020011', NULL,                         '45 Paud Road, Pune',          'A+', NULL,              NULL);

IF NOT EXISTS (SELECT 1 FROM dbo.Patients WHERE PhoneNumber = '9900020012')
    INSERT INTO dbo.Patients (FirstName, LastName, DateOfBirth, Gender, PhoneNumber, Email, Address, BloodGroup, EmergencyContactName, EmergencyContactNumber)
    VALUES ('Rekha',    'Deshpande','1948-10-03', 'Female', '9900020012', 'rekha.d@mail.test',          '19 Deccan, Pune',             NULL, 'Amit Deshpande', '9900029912');

IF NOT EXISTS (SELECT 1 FROM dbo.Patients WHERE PhoneNumber = '9900020013')
    INSERT INTO dbo.Patients (FirstName, LastName, DateOfBirth, Gender, PhoneNumber, Email, Address, BloodGroup, EmergencyContactName, EmergencyContactNumber)
    VALUES ('Sanjay',   'Tiwari',   '1988-07-11', 'Male',   '9900020013', 'sanjay.t@mail.test',         '77 Mall Road, Shimla',        'O-', 'Kavita Tiwari',   '9900029913');

IF NOT EXISTS (SELECT 1 FROM dbo.Patients WHERE PhoneNumber = '9900020014')
    INSERT INTO dbo.Patients (FirstName, LastName, DateOfBirth, Gender, PhoneNumber, Email, Address, BloodGroup, EmergencyContactName, EmergencyContactNumber)
    VALUES ('Lalitha',  'Subramaniam','1960-03-20','Female','9900020014', 'lalitha.s@mail.test',        '2 South Usman Rd, Chennai',   'B-', 'Sudha Raman',    '9900029914');

IF NOT EXISTS (SELECT 1 FROM dbo.Patients WHERE PhoneNumber = '9900020015')
    INSERT INTO dbo.Patients (FirstName, LastName, DateOfBirth, Gender, PhoneNumber, Email, Address, BloodGroup, EmergencyContactName, EmergencyContactNumber)
    VALUES ('Aryan',    'Malhotra',  '2010-09-05', 'Male',   '9900020015', NULL,                         '55 Model Town, Ludhiana',     'A-', 'Geeta Malhotra', '9900029915');
GO

-- -----------------------------------------------------------------------------
-- 3. APPOINTMENTS
-- These reference the doctors and patients inserted above.
-- We identify their IDs by the unique emails / phone numbers used above.
-- Status values used: Scheduled, Completed, Cancelled, Requested
-- -----------------------------------------------------------------------------

-- Helper: We use a sub-select to look up IDs so this script is not fragile.

-- Past completed appointments (for history view)
IF NOT EXISTS (SELECT 1 FROM dbo.Appointments
               WHERE PatientId = (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020001')
                 AND DoctorId  = (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='arjun.mehta@cityhosp.test')
                 AND AppointmentDateTime = '2026-10-01 10:00:00')
    INSERT INTO dbo.Appointments (PatientId, DoctorId, AppointmentDateTime, Reason, Status, Notes)
    VALUES (
        (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020001'),
        (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='arjun.mehta@cityhosp.test'),
        '2026-10-01 10:00:00',
        'Chest pain and shortness of breath',
        'Completed',
        'ECG normal. Prescribed lifestyle changes.'
    );

IF NOT EXISTS (SELECT 1 FROM dbo.Appointments
               WHERE PatientId = (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020002')
                 AND DoctorId  = (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='priya.sharma@cityhosp.test')
                 AND AppointmentDateTime = '2026-10-05 11:00:00')
    INSERT INTO dbo.Appointments (PatientId, DoctorId, AppointmentDateTime, Reason, Status, Notes)
    VALUES (
        (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020002'),
        (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='priya.sharma@cityhosp.test'),
        '2026-10-05 11:00:00',
        'Persistent migraines for 3 weeks',
        'Completed',
        'MRI ordered. Prescribed Topiramate.'
    );

IF NOT EXISTS (SELECT 1 FROM dbo.Appointments
               WHERE PatientId = (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020003')
                 AND DoctorId  = (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='ravi.krishnan@cityhosp.test')
                 AND AppointmentDateTime = '2026-10-08 09:00:00')
    INSERT INTO dbo.Appointments (PatientId, DoctorId, AppointmentDateTime, Reason, Status, Notes)
    VALUES (
        (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020003'),
        (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='ravi.krishnan@cityhosp.test'),
        '2026-10-08 09:00:00',
        'Right knee pain after gym injury',
        'Completed',
        'X-ray shows minor ACL strain. Rest advised.'
    );

IF NOT EXISTS (SELECT 1 FROM dbo.Appointments
               WHERE PatientId = (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020004')
                 AND DoctorId  = (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='sunita.bose@cityhosp.test')
                 AND AppointmentDateTime = '2026-10-10 14:00:00')
    INSERT INTO dbo.Appointments (PatientId, DoctorId, AppointmentDateTime, Reason, Status, Notes)
    VALUES (
        (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020004'),
        (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='sunita.bose@cityhosp.test'),
        '2026-10-10 14:00:00',
        'Routine pediatric check-up',
        'Completed',
        'Healthy. Vaccinations up to date.'
    );

-- Cancelled appointment (for filter testing)
IF NOT EXISTS (SELECT 1 FROM dbo.Appointments
               WHERE PatientId = (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020005')
                 AND DoctorId  = (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='vikram.nair@cityhosp.test')
                 AND AppointmentDateTime = '2026-10-12 08:30:00')
    INSERT INTO dbo.Appointments (PatientId, DoctorId, AppointmentDateTime, Reason, Status, Notes)
    VALUES (
        (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020005'),
        (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='vikram.nair@cityhosp.test'),
        '2026-10-12 08:30:00',
        'Follow-up fever and cough',
        'Cancelled',
        'Patient cancelled — will reschedule.'
    );

-- Today appointments (for dashboard "Today" widget). Date = 2026-10-24 per system time.
IF NOT EXISTS (SELECT 1 FROM dbo.Appointments
               WHERE PatientId = (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020006')
                 AND DoctorId  = (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='vikram.nair@cityhosp.test')
                 AND AppointmentDateTime = '2026-10-24 09:00:00')
    INSERT INTO dbo.Appointments (PatientId, DoctorId, AppointmentDateTime, Reason, Status, Notes)
    VALUES (
        (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020006'),
        (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='vikram.nair@cityhosp.test'),
        '2026-10-24 09:00:00',
        'Diabetes management consultation',
        'Scheduled',
        NULL
    );

IF NOT EXISTS (SELECT 1 FROM dbo.Appointments
               WHERE PatientId = (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020007')
                 AND DoctorId  = (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='arjun.mehta@cityhosp.test')
                 AND AppointmentDateTime = '2026-10-24 11:00:00')
    INSERT INTO dbo.Appointments (PatientId, DoctorId, AppointmentDateTime, Reason, Status, Notes)
    VALUES (
        (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020007'),
        (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='arjun.mehta@cityhosp.test'),
        '2026-10-24 11:00:00',
        'Annual heart screening',
        'Scheduled',
        'Family history of cardiac disease.'
    );

IF NOT EXISTS (SELECT 1 FROM dbo.Appointments
               WHERE PatientId = (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020008')
                 AND DoctorId  = (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='ananya.reddy@cityhosp.test')
                 AND AppointmentDateTime = '2026-10-24 13:00:00')
    INSERT INTO dbo.Appointments (PatientId, DoctorId, AppointmentDateTime, Reason, Status, Notes)
    VALUES (
        (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020008'),
        (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='ananya.reddy@cityhosp.test'),
        '2026-10-24 13:00:00',
        'Skin rash for 2 weeks',
        'Scheduled',
        NULL
    );

-- Future upcoming appointments
IF NOT EXISTS (SELECT 1 FROM dbo.Appointments
               WHERE PatientId = (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020009')
                 AND DoctorId  = (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='rohit.desai@cityhosp.test')
                 AND AppointmentDateTime = '2026-10-28 15:00:00')
    INSERT INTO dbo.Appointments (PatientId, DoctorId, AppointmentDateTime, Reason, Status, Notes)
    VALUES (
        (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020009'),
        (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='rohit.desai@cityhosp.test'),
        '2026-10-28 15:00:00',
        'Anxiety and sleep disorder evaluation',
        'Scheduled',
        NULL
    );

IF NOT EXISTS (SELECT 1 FROM dbo.Appointments
               WHERE PatientId = (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020010')
                 AND DoctorId  = (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='meena.iyer@cityhosp.test')
                 AND AppointmentDateTime = '2026-10-30 10:30:00')
    INSERT INTO dbo.Appointments (PatientId, DoctorId, AppointmentDateTime, Reason, Status, Notes)
    VALUES (
        (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020010'),
        (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='meena.iyer@cityhosp.test'),
        '2026-10-30 10:30:00',
        'Monthly gynaecology follow-up',
        'Scheduled',
        NULL
    );

IF NOT EXISTS (SELECT 1 FROM dbo.Appointments
               WHERE PatientId = (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020011')
                 AND DoctorId  = (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='suresh.patel@cityhosp.test')
                 AND AppointmentDateTime = '2026-11-02 10:00:00')
    INSERT INTO dbo.Appointments (PatientId, DoctorId, AppointmentDateTime, Reason, Status, Notes)
    VALUES (
        (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020011'),
        (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='suresh.patel@cityhosp.test'),
        '2026-11-02 10:00:00',
        'Blurry vision and headache',
        'Scheduled',
        NULL
    );

IF NOT EXISTS (SELECT 1 FROM dbo.Appointments
               WHERE PatientId = (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020012')
                 AND DoctorId  = (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='kavitha.rao@cityhosp.test')
                 AND AppointmentDateTime = '2026-11-04 11:00:00')
-- kavitha.rao is *inactive*; booking via admin override to test inactive-doctor data integrity
    INSERT INTO dbo.Appointments (PatientId, DoctorId, AppointmentDateTime, Reason, Status, Notes)
    VALUES (
        (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020012'),
        (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='kavitha.rao@cityhosp.test'),
        '2026-11-04 11:00:00',
        'Thyroid function review',
        'Scheduled',
        'Booked before doctor went inactive.'
    );

-- Guest-submitted (Requested) appointment — no prior patient record, inserted via seed
IF NOT EXISTS (SELECT 1 FROM dbo.Appointments
               WHERE PatientId = (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020013')
                 AND DoctorId  = (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='vikram.nair@cityhosp.test')
                 AND AppointmentDateTime = '2026-11-05 09:30:00')
    INSERT INTO dbo.Appointments (PatientId, DoctorId, AppointmentDateTime, Reason, Status, Notes)
    VALUES (
        (SELECT PatientId FROM dbo.Patients WHERE PhoneNumber='9900020013'),
        (SELECT DoctorId  FROM dbo.Doctors  WHERE Email='vikram.nair@cityhosp.test'),
        '2026-11-05 09:30:00',
        'General health check',
        'Requested',
        'Walk-in guest request via website form.'
    );

GO

PRINT 'SeedData.sql completed successfully. Safe to run again — all inserts are guarded.';
GO
