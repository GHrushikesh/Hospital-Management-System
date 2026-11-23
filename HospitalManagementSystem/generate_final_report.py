import os
import sys
import docx
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_ALIGN_VERTICAL
from docx.oxml import parse_xml, OxmlElement
from docx.oxml.ns import nsdecls, qn
import win32com.client

LOGO_PATH = r"C:\Users\Akshar\.gemini\config\skills\dotnet-lab-practical\resources\mu_logo.jpg"
SCREENSHOTS_DIR = r"E:\Study\SEM 5\.NET\p1\Hospital-Management-System\HospitalManagementSystem\Screenshots"
OUTPUT_DOCX = r"E:\Study\SEM 5\.NET\p1\Hospital-Management-System\HospitalManagementSystem\Hospital_Management_System_Project_Report.docx"
OUTPUT_PDF = r"E:\Study\SEM 5\.NET\p1\Hospital-Management-System\HospitalManagementSystem\Hospital_Management_System_Project_Report.pdf"

def set_cell_margins(cell, top=100, bottom=100, left=150, right=150):
    tcPr = cell._tc.get_or_add_tcPr()
    tcMar = OxmlElement('w:tcMar')
    for m, val in [('top', top), ('bottom', bottom), ('left', left), ('right', right)]:
        node = OxmlElement(f'w:{m}')
        node.set(qn('w:w'), str(val))
        node.set(qn('w:type'), 'dxa')
        tcMar.append(node)
    tcPr.append(tcMar)

def set_cell_background(cell, fill_hex):
    tcPr = cell._tc.get_or_add_tcPr()
    shd = parse_xml(f'<w:shd {nsdecls("w")} w:fill="{fill_hex}"/>')
    tcPr.append(shd)

def set_cell_border(cell, **kwargs):
    """
    kwargs: top, bottom, left, right
    values: dict(sz=12, val='single', color='003366')
    """
    tcPr = cell._tc.get_or_add_tcPr()
    tcBorders = OxmlElement('w:tcBorders')
    for border_name in ['top', 'left', 'bottom', 'right']:
        if border_name in kwargs:
            b_info = kwargs[border_name]
            node = OxmlElement(f'w:{border_name}')
            node.set(qn('w:val'), b_info.get('val', 'single'))
            node.set(qn('w:sz'), str(b_info.get('sz', 4)))
            node.set(qn('w:space'), '0')
            node.set(qn('w:color'), b_info.get('color', 'CCCCCC'))
            tcBorders.append(node)
        else:
            node = OxmlElement(f'w:{border_name}')
            node.set(qn('w:val'), 'none')
            tcBorders.append(node)
    tcPr.append(tcBorders)

def add_footer_page_number(run):
    fldSimple = parse_xml(r'<w:fldSimple %s w:instr="PAGE"/>' % nsdecls('w'))
    run._r.append(fldSimple)

def build_report():
    doc = docx.Document()

    # Base margins
    for section in doc.sections:
        section.top_margin = Inches(0.7)
        section.bottom_margin = Inches(0.7)
        section.left_margin = Inches(0.8)
        section.right_margin = Inches(0.8)
        section.header_distance = Inches(0.4)
        section.footer_distance = Inches(0.4)

    # ----------------------------------------------------
    # COVER / TITLE PAGE
    # ----------------------------------------------------
    p_title = doc.add_paragraph()
    p_title.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_title.paragraph_format.space_before = Pt(10)
    p_title.paragraph_format.space_after = Pt(4)
    run_t = p_title.add_run("HOSPITAL MANAGEMENT SYSTEM")
    run_t.bold = True
    run_t.font.name = "Arial"
    run_t.font.size = Pt(20)
    run_t.font.color.rgb = RGBColor(0, 51, 102) # Navy Blue

    p_sub = doc.add_paragraph()
    p_sub.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_sub.paragraph_format.space_before = Pt(0)
    p_sub.paragraph_format.space_after = Pt(2)
    run_s = p_sub.add_run("MINI PROJECT")
    run_s.bold = True
    run_s.font.name = "Arial"
    run_s.font.size = Pt(13)
    run_s.font.color.rgb = RGBColor(80, 80, 80)

    p_course = doc.add_paragraph()
    p_course.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_course.paragraph_format.space_before = Pt(0)
    p_course.paragraph_format.space_after = Pt(24)
    run_c = p_course.add_run(".NET TECHNOLOGIES (01CE1523)")
    run_c.bold = True
    run_c.font.name = "Arial"
    run_c.font.size = Pt(12)
    run_c.font.color.rgb = RGBColor(0, 51, 102)

    p_by = doc.add_paragraph()
    p_by.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_by.paragraph_format.space_before = Pt(0)
    p_by.paragraph_format.space_after = Pt(6)
    run_by = p_by.add_run("Submitted by")
    run_by.font.name = "Calibri"
    run_by.font.size = Pt(11)
    run_by.italic = True

    # Candidates table on title page
    cand_table = doc.add_table(rows=3, cols=2)
    cand_table.alignment = WD_TABLE_ALIGNMENT.CENTER
    candidates = [
        ("Akshar Parekh", "92400120523"),
        ("Hrushikesh Gangajadiya", "92400120641"),
        ("Venish Parmar", "92400120643")
    ]
    for idx, (name, enr) in enumerate(candidates):
        row = cand_table.rows[idx]
        c0, c1 = row.cells[0], row.cells[1]
        c0.width = Inches(2.8)
        c1.width = Inches(1.8)
        
        # remove borders
        for cell in (c0, c1):
            set_cell_border(cell)
            set_cell_margins(cell, top=40, bottom=40, left=60, right=60)
            
        p0 = c0.paragraphs[0]
        p0.alignment = WD_ALIGN_PARAGRAPH.RIGHT
        p0.paragraph_format.space_after = Pt(2)
        r0 = p0.add_run(name)
        r0.bold = True
        r0.font.name = "Arial"
        r0.font.size = Pt(11)

        p1 = c1.paragraphs[0]
        p1.alignment = WD_ALIGN_PARAGRAPH.LEFT
        p1.paragraph_format.space_after = Pt(2)
        r1 = p1.add_run(f"({enr})")
        r1.font.name = "Calibri"
        r1.font.size = Pt(11)
        r1.font.color.rgb = RGBColor(80, 80, 80)

    p_deg = doc.add_paragraph()
    p_deg.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_deg.paragraph_format.space_before = Pt(24)
    p_deg.paragraph_format.space_after = Pt(2)
    run_deg = p_deg.add_run("BACHELOR OF TECHNOLOGY")
    run_deg.bold = True
    run_deg.font.name = "Arial"
    run_deg.font.size = Pt(12)

    p_in = doc.add_paragraph()
    p_in.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_in.paragraph_format.space_before = Pt(0)
    p_in.paragraph_format.space_after = Pt(2)
    run_in = p_in.add_run("in")
    run_in.font.name = "Calibri"
    run_in.font.size = Pt(11)
    run_in.italic = True

    p_dept = doc.add_paragraph()
    p_dept.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_dept.paragraph_format.space_before = Pt(0)
    p_dept.paragraph_format.space_after = Pt(18)
    run_dept = p_dept.add_run("Computer Engineering")
    run_dept.bold = True
    run_dept.font.name = "Arial"
    run_dept.font.size = Pt(12)
    run_dept.font.color.rgb = RGBColor(0, 51, 102)

    # University Logo
    if os.path.exists(LOGO_PATH):
        p_logo = doc.add_paragraph()
        p_logo.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p_logo.paragraph_format.space_before = Pt(0)
        p_logo.paragraph_format.space_after = Pt(18)
        run_logo = p_logo.add_run()
        run_logo.add_picture(LOGO_PATH, width=Inches(3.3))

    p_fac = doc.add_paragraph()
    p_fac.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_fac.paragraph_format.space_before = Pt(0)
    p_fac.paragraph_format.space_after = Pt(2)
    run_fac = p_fac.add_run("Faculty of Engineering & Technology")
    run_fac.bold = True
    run_fac.font.name = "Arial"
    run_fac.font.size = Pt(12)

    p_uni = doc.add_paragraph()
    p_uni.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_uni.paragraph_format.space_before = Pt(0)
    p_uni.paragraph_format.space_after = Pt(2)
    run_uni = p_uni.add_run("Marwadi University, Rajkot")
    run_uni.font.name = "Arial"
    run_uni.font.size = Pt(11.5)

    p_date = doc.add_paragraph()
    p_date.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_date.paragraph_format.space_before = Pt(4)
    p_date.paragraph_format.space_after = Pt(0)
    run_date = p_date.add_run("November, 2026")
    run_date.bold = True
    run_date.font.name = "Arial"
    run_date.font.size = Pt(11)
    run_date.font.color.rgb = RGBColor(100, 100, 100)

    # ----------------------------------------------------
    # SECTION BREAK / NEW SECTION WITH HEADERS & FOOTERS
    # ----------------------------------------------------
    doc.add_page_break()
    sec2 = doc.sections[-1]
    sec2.header.is_linked_to_previous = False
    sec2.footer.is_linked_to_previous = False

    # Header
    hp = sec2.header.paragraphs[0]
    hp.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    hrun = hp.add_run("Hospital Management System | .NET Technologies (01CE1523)")
    hrun.font.name = "Calibri"
    hrun.font.size = Pt(8.5)
    hrun.font.color.rgb = RGBColor(128, 128, 128)

    # Footer
    fp = sec2.footer.paragraphs[0]
    fp.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    frun1 = fp.add_run("Page ")
    frun1.font.name = "Calibri"
    frun1.font.size = Pt(9)
    frun1.font.color.rgb = RGBColor(100, 100, 100)
    run_pg = fp.add_run()
    run_pg.font.name = "Calibri"
    run_pg.font.size = Pt(9)
    run_pg.font.color.rgb = RGBColor(100, 100, 100)
    add_footer_page_number(run_pg)

    # Helper functions for formatted headings
    def add_chapter_title(title_text):
        p = doc.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.LEFT
        p.paragraph_format.space_before = Pt(14)
        p.paragraph_format.space_after = Pt(6)
        r = p.add_run(title_text)
        r.bold = True
        r.font.name = "Arial"
        r.font.size = Pt(15)
        r.font.color.rgb = RGBColor(0, 51, 102)

    def add_section_title(sec_text):
        p = doc.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.LEFT
        p.paragraph_format.space_before = Pt(10)
        p.paragraph_format.space_after = Pt(4)
        r = p.add_run(sec_text)
        r.bold = True
        r.font.name = "Arial"
        r.font.size = Pt(12)
        r.font.color.rgb = RGBColor(0, 51, 102)

    def add_body(text):
        p = doc.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
        p.paragraph_format.space_before = Pt(2)
        p.paragraph_format.space_after = Pt(6)
        p.paragraph_format.line_spacing = 1.15
        r = p.add_run(text)
        r.font.name = "Calibri"
        r.font.size = Pt(11)
        r.font.color.rgb = RGBColor(30, 30, 30)
        return p

    def add_bullet(bold_prefix, text):
        p = doc.add_paragraph(style='List Bullet')
        p.paragraph_format.space_before = Pt(2)
        p.paragraph_format.space_after = Pt(3)
        p.paragraph_format.line_spacing = 1.15
        r0 = p.add_run(bold_prefix)
        r0.bold = True
        r0.font.name = "Calibri"
        r0.font.size = Pt(10.5)
        r1 = p.add_run(text)
        r1.font.name = "Calibri"
        r1.font.size = Pt(10.5)

    # ----------------------------------------------------
    # CHAPTER 1: PROJECT OBJECTIVE
    # ----------------------------------------------------
    add_chapter_title("CHAPTER 1: PROJECT OBJECTIVE")

    add_section_title("1.1  AIM OF THE PROJECT")
    add_body("The primary aim of this mini project is to design and develop an enterprise-grade Hospital Management System (HMS) using ASP.NET Core MVC (.NET 10.0), C#, and ADO.NET with Microsoft SQL Server to streamline healthcare workflows. It provides an automated, centralized digital solution for patient registration, emergency triage queues, specialist doctor appointments, inpatient bed allocations, and clinical prescription handling, eliminating paper-bound bottlenecks and administrative delays.")

    add_section_title("1.2  OBJECTIVES")
    add_bullet("Modern Web Interface: ", "To design a clean, responsive, and user-friendly interface using ASP.NET Core MVC, Razor Views, and Bootstrap 5.")
    add_bullet("High-Performance Data Access: ", "To store, query, and manage hospital data in Microsoft SQL Server using parameterized ADO.NET (Microsoft.Data.SqlClient) and repository architecture.")
    add_bullet("Comprehensive CRUD Operations: ", "To implement end-to-end Create, Read, Update, and Delete capabilities for core modules including Patients, Doctors, Staff, Beds, and Appointments.")
    add_bullet("Role-Based Security: ", "To implement secure session authentication and role-based access control filters separating Administrator, Doctor, Receptionist, and Nurse privileges.")
    add_bullet("Dual-Layer Validation: ", "To enforce robust client-side (jQuery Validation / Unobtrusive) and server-side (ModelState) validation to prevent duplicate records and malformed inputs.")
    add_bullet("Emergency & Clinical Workflows: ", "To facilitate instant emergency triage token generation, live appointment tracking, inpatient bed occupancy monitoring, and digital prescription writing.")

    add_section_title("1.3  SCOPE OF THE PROJECT")
    add_body("Target Users: The system serves hospital administrators, specialist physicians, front-desk receptionists, inpatient charge nurses, as well as patients and the public seeking online appointment bookings.")
    add_body("Modules Covered: The mini project encompasses Authentication & Role-Based Access Control, Receptionist & Walk-In Desk, Doctor Clinical Consultation & Prescriptions, Inpatient Bed Allocation & Ward Occupancy, Patient Self-Service Portal (Online Booking, Status Lookup, Emergency Ticket Generation), and Administrator Staff Governance.")
    add_body("Future Enhancements (Outside Current Scope): Integration of online payment gateways (Stripe / UPI) for automatic medical billing settlement, SMS / WhatsApp automated notification gateways for queue reminders, biometric doctor attendance, and PACS / DICOM medical imaging viewer integration for diagnostic radiology.")

    # ----------------------------------------------------
    # CHAPTER 2: INTRODUCTION
    # ----------------------------------------------------
    doc.add_page_break()
    add_chapter_title("CHAPTER 2: INTRODUCTION")

    add_section_title("2.1  TOOLS AND TECHNOLOGIES USED")
    add_body("The following tools and frameworks were utilized to architect, design, build, and deploy the Hospital Management System:")

    tech_data = [
        ("Front End", "HTML5, CSS3, JavaScript, Razor Views (.cshtml), Bootstrap 5"),
        ("Back End / Language", "C# (.NET 10.0)"),
        ("Framework", "ASP.NET Core MVC"),
        ("Data Access", "ADO.NET (Microsoft.Data.SqlClient) with Repository Pattern"),
        ("Database", "Microsoft SQL Server (LocalDB / T-SQL)"),
        ("Architecture Pattern", "Multi-Tier MVC with Dependency Injection (DI)"),
        ("IDE / Tools", "Visual Studio 2022 / Visual Studio Code"),
        ("Version Control", "Git & GitHub")
    ]
    tech_table = doc.add_table(rows=len(tech_data) + 1, cols=2)
    tech_table.alignment = WD_TABLE_ALIGNMENT.CENTER
    # Header row
    hrow = tech_table.rows[0]
    c0, c1 = hrow.cells[0], hrow.cells[1]
    c0.width = Inches(2.2)
    c1.width = Inches(4.6)
    set_cell_background(c0, "003366")
    set_cell_background(c1, "003366")
    set_cell_margins(c0, 120, 120, 150, 150)
    set_cell_margins(c1, 120, 120, 150, 150)
    
    p0 = c0.paragraphs[0]
    r0 = p0.add_run("Component")
    r0.bold = True
    r0.font.name = "Arial"
    r0.font.size = Pt(10)
    r0.font.color.rgb = RGBColor(255, 255, 255)

    p1 = c1.paragraphs[0]
    r1 = p1.add_run("Technology Used")
    r1.bold = True
    r1.font.name = "Arial"
    r1.font.size = Pt(10)
    r1.font.color.rgb = RGBColor(255, 255, 255)

    border_spec = dict(
        top=dict(sz=4, val='single', color='B0C4DE'),
        bottom=dict(sz=4, val='single', color='B0C4DE'),
        left=dict(sz=4, val='single', color='B0C4DE'),
        right=dict(sz=4, val='single', color='B0C4DE')
    )
    set_cell_border(c0, **border_spec)
    set_cell_border(c1, **border_spec)

    for idx, (comp, tech) in enumerate(tech_data):
        row = tech_table.rows[idx + 1]
        cell_a, cell_b = row.cells[0], row.cells[1]
        cell_a.width = Inches(2.2)
        cell_b.width = Inches(4.6)
        
        bg_col = "F8FAFC" if idx % 2 == 0 else "FFFFFF"
        set_cell_background(cell_a, bg_col)
        set_cell_background(cell_b, bg_col)
        set_cell_margins(cell_a, 80, 80, 150, 150)
        set_cell_margins(cell_b, 80, 80, 150, 150)
        set_cell_border(cell_a, **border_spec)
        set_cell_border(cell_b, **border_spec)

        pa = cell_a.paragraphs[0]
        ra = pa.add_run(comp)
        ra.bold = True
        ra.font.name = "Calibri"
        ra.font.size = Pt(10)

        pb = cell_b.paragraphs[0]
        rb = pb.add_run(tech)
        rb.font.name = "Calibri"
        rb.font.size = Pt(10)

    p_tbl_cap = doc.add_paragraph()
    p_tbl_cap.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_tbl_cap.paragraph_format.space_before = Pt(4)
    p_tbl_cap.paragraph_format.space_after = Pt(12)
    rcap = p_tbl_cap.add_run("Table 2.1: Tools and Technologies Used")
    rcap.font.name = "Arial"
    rcap.font.size = Pt(9.5)
    rcap.bold = True

    add_section_title("2.2  MODULES OF THE PROJECT")
    add_bullet("Login & Security Module: ", "Centralized, secure credential verification with role-based session assignment for Administrators, Medical Specialists, Charge Nurses, and Receptionists.")
    add_bullet("Patient Self-Service Portal: ", "Public-facing portal enabling patients to request OPD appointments, register emergency triage tickets, and monitor queue status using their contact numbers.")
    add_bullet("Inpatient Bed & Ward Management Module: ", "Live ward monitoring (ICU, Post-Op, Emergency, General) with bed allocation, patient assignment, and discharge summary generation.")
    add_bullet("Doctor Clinical Consultation Module: ", "Specialist console allowing physicians to review patient queues, update diagnoses, prescribe medications with dosage instructions, and supervise assigned staff.")
    add_bullet("Receptionist & Walk-In Desk Module: ", "Front-office module for on-the-spot walk-in patient registration, OPD token printing, and patient directory search.")
    add_bullet("Patient Records & Demographics Module: ", "Comprehensive medical directory storing patient MRN (Medical Record Number), age, blood group, address, and emergency contact details.")
    add_bullet("Doctor Directory & Specialty Module: ", "Maintains specialist profiles, medical council qualifications, consulting chambers, contact details, and department allocations.")
    add_bullet("Emergency Triage Queue Module: ", "Dedicated triage management system prioritizing critical cases, allocating emergency observation beds, and expediting physician attention.")
    add_bullet("Administrator Governance Module: ", "Executive dashboard providing high-level operational analytics, staff onboarding, doctor directory maintenance, and automated database seeding.")

    # ----------------------------------------------------
    # CHAPTER 3: SCREENSHOTS
    # ----------------------------------------------------
    doc.add_page_break()
    add_chapter_title("CHAPTER 3: SCREENSHOTS")
    add_body("This chapter illustrates the core graphical user interfaces (GUIs) of the Hospital Management System, demonstrating the responsive front-end design, workflow navigation, and live system capabilities.")

    screenshots_info = [
        ("fig3_1_login.png", "Figure 3.1: Hospital Staff and Doctor Login Portal",
         "The unified authentication portal allowing doctors, charge nurses, and front-desk receptionists to log into their respective role-based dashboards securely using encrypted credentials."),
        
        ("fig3_2_home.png", "Figure 3.2: Hospital Management System Home / Landing Page",
         "The main landing page featuring quick-access shortcuts to outpatient appointment booking, emergency triage admission, live status tracking, and specialist consultation."),
        
        ("fig3_3_appointment_request.png", "Figure 3.3: Online Appointment Booking Request Form",
         "The patient self-service booking form allowing visitors to select their preferred department, consulting specialist, date, and time slot with dual client/server validation."),
        
        ("fig3_4_emergency_ticket.png", "Figure 3.4: Emergency OPD & Fast-Track Triage Registration",
         "The rapid emergency admission form enabling critical patients or attenders to generate priority triage tokens and alert on-duty trauma staff immediately."),
        
        ("fig3_5_check_status.png", "Figure 3.5: Real-Time Appointment Status & Live Queue Tracking",
         "The live verification portal where patients enter their registered phone number to view appointment confirmation status, assigned doctor, and live token number."),
        
        ("fig3_6_patient_create.png", "Figure 3.6: Patient Registration & Inpatient Profile Entry Form",
         "The comprehensive demographic entry form capturing the patient's full name, MRN, date of birth, blood group, emergency contact details, and residential address."),
        
        ("fig3_7_patient_list.png", "Figure 3.7: Inpatient & Outpatient Records Directory",
         "The master patient registry displaying medical record numbers, contact information, blood groups, and quick-action links for editing or reviewing consultation histories."),
        
        ("fig3_8_doctor_directory.png", "Figure 3.8: Specialist Medical Faculty & Doctor Directory",
         "The medical faculty catalog showcasing specialist physicians, qualifications, department affiliations, room numbers, and consultation contact details."),
        
        ("fig3_9_appointment_list.png", "Figure 3.9: Outpatient Appointment Schedule & Booking Management",
         "The administrative appointment schedule displaying appointment IDs, patient names, assigned doctors, appointment slots, and operational status."),
        
        ("fig3_10_admin_dashboard.png", "Figure 3.10: Administrator Central Control Dashboard",
         "The executive management console presenting hospital metrics, doctor/staff headcounts, patient statistics, and administrative navigation shortcuts."),
        
        ("fig3_11_receptionist_dashboard.png", "Figure 3.11: Front Desk Receptionist Operations Dashboard",
         "The receptionist operational center displaying today's patient queue, scheduled appointments, walk-in quick booking controls, and OPD token generation tools."),
        
        ("fig3_12_ward_occupancy.png", "Figure 3.12: Hospital Bed Allocation & Ward Occupancy Monitor",
         "The inpatient ward console tracking live bed availability across ICU, Surgical, Post-Op, and General wards with patient admission and discharge controls."),
        
        ("fig3_13_doctor_dashboard.png", "Figure 3.13: Clinical Consultation & Doctor Prescription Portal",
         "The physician workspace facilitating patient queue examination, clinical progress recording, medication prescriptions, and staff delegation."),
        
        ("fig3_14_walkin_booking.png", "Figure 3.14: Receptionist Walk-In OPD Registration & Token Generation",
         "The front-desk walk-in registration form allowing staff to create immediate on-spot appointments, allocate available doctors, and print outpatient slips.")
    ]

    for filename, caption, explanation in screenshots_info:
        img_path = os.path.join(SCREENSHOTS_DIR, filename)
        if os.path.exists(img_path):
            # Bordered single cell table for screenshot
            shot_table = doc.add_table(rows=1, cols=1)
            shot_table.alignment = WD_TABLE_ALIGNMENT.CENTER
            cell = shot_table.cell(0, 0)
            cell.width = Inches(6.5)
            set_cell_background(cell, "FAFAFA")
            set_cell_margins(cell, top=80, bottom=80, left=80, right=80)
            
            border_spec = dict(
                top=dict(sz=6, val='single', color='CCCCCC'),
                bottom=dict(sz=6, val='single', color='CCCCCC'),
                left=dict(sz=6, val='single', color='CCCCCC'),
                right=dict(sz=6, val='single', color='CCCCCC')
            )
            set_cell_border(cell, **border_spec)

            cp = cell.paragraphs[0]
            cp.alignment = WD_ALIGN_PARAGRAPH.CENTER
            cp.paragraph_format.space_before = Pt(2)
            cp.paragraph_format.space_after = Pt(2)
            crun = cp.add_run()
            crun.add_picture(img_path, width=Inches(6.2))

            # Figure caption below
            p_cap = doc.add_paragraph()
            p_cap.alignment = WD_ALIGN_PARAGRAPH.CENTER
            p_cap.paragraph_format.space_before = Pt(4)
            p_cap.paragraph_format.space_after = Pt(2)
            rcap = p_cap.add_run(caption)
            rcap.bold = True
            rcap.font.name = "Arial"
            rcap.font.size = Pt(9.5)
            rcap.font.color.rgb = RGBColor(0, 51, 102)

            # Explanation
            p_exp = doc.add_paragraph()
            p_exp.alignment = WD_ALIGN_PARAGRAPH.CENTER
            p_exp.paragraph_format.space_before = Pt(0)
            p_exp.paragraph_format.space_after = Pt(14)
            rexp = p_exp.add_run(explanation)
            rexp.font.name = "Calibri"
            rexp.font.size = Pt(10)
            rexp.italic = True
            rexp.font.color.rgb = RGBColor(70, 70, 70)

    # ----------------------------------------------------
    # CHAPTER 4: CODE
    # ----------------------------------------------------
    doc.add_page_break()
    add_chapter_title("CHAPTER 4: CODE")
    add_body("This chapter contains key code extracts representing the architectural layers of the Hospital Management System, including the Controller request handling, ADO.NET repository data access, Domain Models, automated SQL database provisioning, and Razor Views.")

    def add_code_listing(listing_title, code_snippet):
        p_title = doc.add_paragraph()
        p_title.alignment = WD_ALIGN_PARAGRAPH.LEFT
        p_title.paragraph_format.space_before = Pt(12)
        p_title.paragraph_format.space_after = Pt(3)
        rt = p_title.add_run(listing_title)
        rt.bold = True
        rt.font.name = "Arial"
        rt.font.size = Pt(10.5)
        rt.font.color.rgb = RGBColor(0, 51, 102)

        code_table = doc.add_table(rows=1, cols=1)
        code_table.alignment = WD_TABLE_ALIGNMENT.CENTER
        cell = code_table.cell(0, 0)
        cell.width = Inches(6.8)
        set_cell_background(cell, "F5F7FA")
        set_cell_margins(cell, top=100, bottom=100, left=140, right=140)
        
        border_spec = dict(
            top=dict(sz=6, val='single', color='CBD5E1'),
            bottom=dict(sz=6, val='single', color='CBD5E1'),
            left=dict(sz=16, val='single', color='003366'), # Thick navy accent left border
            right=dict(sz=6, val='single', color='CBD5E1')
        )
        set_cell_border(cell, **border_spec)

        cp = cell.paragraphs[0]
        cp.alignment = WD_ALIGN_PARAGRAPH.LEFT
        cp.paragraph_format.space_before = Pt(0)
        cp.paragraph_format.space_after = Pt(0)
        cp.paragraph_format.line_spacing = 1.05
        
        crun = cp.add_run(code_snippet.strip())
        crun.font.name = "Consolas"
        crun.font.size = Pt(8.5)
        crun.font.color.rgb = RGBColor(20, 30, 45)

        p_spacer = doc.add_paragraph()
        p_spacer.paragraph_format.space_before = Pt(0)
        p_spacer.paragraph_format.space_after = Pt(6)

    # Listing 4.1: AppointmentController.cs
    code_ctrl = '''using HospitalManagementSystem.Data.Repositories;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly AppointmentRepository _appointmentRepository;
        private readonly PatientRepository _patientRepository;
        private readonly DoctorRepository _doctorRepository;

        public AppointmentController(
            AppointmentRepository appointmentRepository,
            PatientRepository patientRepository,
            DoctorRepository doctorRepository)
        {
            _appointmentRepository = appointmentRepository;
            _patientRepository = patientRepository;
            _doctorRepository = doctorRepository;
        }

        // GET: /Appointment/GuestRequest -> Online Booking Form
        [HttpGet]
        public async Task<IActionResult> GuestRequest(bool submitted = false, bool isEmergency = false, int appointmentId = 0)
        {
            ViewBag.Doctors = await _doctorRepository.GetAllAsync();
            return View(new GuestAppointmentViewModel());
        }

        // POST: /Appointment/GuestRequest -> Handle Online Booking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuestRequest(GuestAppointmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Doctors = await _doctorRepository.GetAllAsync();
                return View(model);
            }

            // Find or Register Patient by Phone Number
            var patient = await _patientRepository.GetByPhoneNumberAsync(model.PhoneNumber.Trim());
            int patientId;
            if (patient == null)
            {
                patientId = await _patientRepository.AddAsync(new Patient
                {
                    FirstName = model.FirstName.Trim(),
                    LastName = model.LastName.Trim(),
                    PhoneNumber = model.PhoneNumber.Trim(),
                    Gender = model.Gender,
                    Email = model.Email
                });
            }
            else
            {
                patientId = patient.PatientId;
            }

            var appointment = new Appointment
            {
                PatientId = patientId,
                DoctorId = model.DoctorId,
                AppointmentDate = model.AppointmentDate.Date.Add(model.AppointmentTime),
                AppointmentTime = model.AppointmentTime,
                Reason = model.Reason,
                Status = "Scheduled"
            };

            int appointmentId = await _appointmentRepository.AddAsync(appointment);
            return RedirectToAction(nameof(GuestRequest), new { submitted = true, appointmentId });
        }

        // GET: /Appointment/CheckStatus -> Real-time status lookup
        [HttpGet]
        public IActionResult CheckStatus() => View();

        [HttpPost]
        public async Task<IActionResult> CheckStatus(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                ViewBag.Error = "Please enter a valid phone number.";
                return View();
            }

            var appointments = await _appointmentRepository.GetByPatientPhoneAsync(phoneNumber.Trim());
            return View(appointments);
        }
    }
}'''
    add_code_listing("Listing 4.1: AppointmentController.cs (Appointment Request & Status Tracking)", code_ctrl)

    # Listing 4.2: PatientRepository.cs
    code_repo = '''using HospitalManagementSystem.Models;
using Microsoft.Data.SqlClient;

namespace HospitalManagementSystem.Data.Repositories
{
    public class PatientRepository : AdoNetRepositoryBase
    {
        public PatientRepository(DbConnectionHelper dbConnectionHelper) : base(dbConnectionHelper) { }

        public async Task<int> AddAsync(Patient patient)
        {
            if (string.IsNullOrWhiteSpace(patient.MedicalRecordNumber))
            {
                patient.MedicalRecordNumber = GenerateMrn();
            }

            const string query = @"
INSERT INTO Patients
    (MedicalRecordNumber, FirstName, LastName, DateOfBirth, Gender, PhoneNumber, Email, Address, BloodGroup, EmergencyContactName, EmergencyContactNumber)
OUTPUT INSERTED.PatientId
VALUES
    (@MedicalRecordNumber, @FirstName, @LastName, @DateOfBirth, @Gender, @PhoneNumber, @Email, @Address, @BloodGroup, @EmergencyContactName, @EmergencyContactNumber);";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@MedicalRecordNumber", DbValue(patient.MedicalRecordNumber));
            command.Parameters.AddWithValue("@FirstName", patient.FirstName.Trim());
            command.Parameters.AddWithValue("@LastName", patient.LastName.Trim());
            command.Parameters.AddWithValue("@DateOfBirth", (object?)patient.DateOfBirth ?? DBNull.Value);
            command.Parameters.AddWithValue("@Gender", patient.Gender.Trim());
            command.Parameters.AddWithValue("@PhoneNumber", patient.PhoneNumber.Trim());
            command.Parameters.AddWithValue("@Email", DbValue(patient.Email));
            command.Parameters.AddWithValue("@Address", DbValue(patient.Address));
            command.Parameters.AddWithValue("@BloodGroup", DbValue(patient.BloodGroup));
            command.Parameters.AddWithValue("@EmergencyContactName", DbValue(patient.EmergencyContactName));
            command.Parameters.AddWithValue("@EmergencyContactNumber", DbValue(patient.EmergencyContactNumber));

            object? result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<Patient?> GetByPhoneNumberAsync(string phoneNumber)
        {
            const string query = @"
SELECT TOP 1 PatientId, MedicalRecordNumber, FirstName, LastName, DateOfBirth, Gender, PhoneNumber, Email, Address, BloodGroup
FROM Patients
WHERE PhoneNumber = @PhoneNumber;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PhoneNumber", phoneNumber.Trim());

            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapPatient(reader) : null;
        }
    }
}'''
    add_code_listing("Listing 4.2: PatientRepository.cs (ADO.NET Patient Data Access)", code_repo)

    # Listing 4.3: BedRepository.cs
    code_bed = '''using HospitalManagementSystem.Models;
using Microsoft.Data.SqlClient;

namespace HospitalManagementSystem.Data.Repositories
{
    public class BedRepository : AdoNetRepositoryBase
    {
        public BedRepository(DbConnectionHelper dbConnectionHelper) : base(dbConnectionHelper) { }

        public async Task<IReadOnlyList<HospitalBed>> GetAllAsync()
        {
            const string query = @"
SELECT b.BedId, b.BedNumber, b.WardName, b.BedType, b.Floor, b.Status,
       b.PatientId, b.AdmittedDoctorId, b.AdmissionNotes, b.AdmittedAt,
       CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
       p.MedicalRecordNumber AS PatientMRN,
       d.FullName AS DoctorName, d.Specialty AS DoctorSpecialty
FROM HospitalBeds b
LEFT JOIN Patients p ON b.PatientId = p.PatientId
LEFT JOIN Doctors d ON b.AdmittedDoctorId = d.DoctorId
ORDER BY b.WardName, b.BedNumber;";

            List<HospitalBed> beds = new();
            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                beds.Add(MapHospitalBed(reader));
            }
            return beds;
        }

        public async Task<bool> AdmitPatientAsync(int bedId, int patientId, int doctorId, string notes)
        {
            const string query = @"
UPDATE HospitalBeds
SET Status = 'Occupied', PatientId = @PatientId, AdmittedDoctorId = @DoctorId,
    AdmissionNotes = @Notes, AdmittedAt = SYSUTCDATETIME()
WHERE BedId = @BedId AND Status = 'Vacant';

INSERT INTO BedAdmissions (BedId, BedNumber, WardName, PatientId, AdmittedDoctorId, AdmissionNotes, AdmittedAt, Status)
SELECT BedId, BedNumber, WardName, @PatientId, @DoctorId, @Notes, SYSUTCDATETIME(), 'Active'
FROM HospitalBeds WHERE BedId = @BedId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@BedId", bedId);
            command.Parameters.AddWithValue("@PatientId", patientId);
            command.Parameters.AddWithValue("@DoctorId", doctorId);
            command.Parameters.AddWithValue("@Notes", DbValue(notes));

            return await command.ExecuteNonQueryAsync() > 0;
        }
    }
}'''
    add_code_listing("Listing 4.3: BedRepository.cs (ADO.NET Bed Allocation & Admission Log)", code_bed)

    # Listing 4.4: HospitalBed.cs
    code_model = '''using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class HospitalBed
    {
        public int BedId { get; set; }

        [Required, StringLength(20)]
        [Display(Name = "Bed Number")]
        public string BedNumber { get; set; } = string.Empty;

        [Required, StringLength(100)]
        [Display(Name = "Ward / Unit")]
        public string WardName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Display(Name = "Bed Type")]
        public string BedType { get; set; } = "General"; // ICU, Post-Op, Emergency, General

        [StringLength(50)]
        public string Floor { get; set; } = "1st Floor";

        [Required, StringLength(30)]
        public string Status { get; set; } = "Vacant"; // Vacant, Occupied, Reserved, Maintenance

        public int? PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? PatientMRN { get; set; }

        public int? AdmittedDoctorId { get; set; }
        public string? DoctorName { get; set; }

        public string? AdmissionNotes { get; set; }
        public DateTime? AdmittedAt { get; set; }
    }
}'''
    add_code_listing("Listing 4.4: HospitalBed.cs (Inpatient Bed Domain Entity Model)", code_model)

    # Listing 4.5: DatabaseInitializer.cs
    code_init = '''using Microsoft.Data.SqlClient;

namespace HospitalManagementSystem.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            string defaultConnectionString = config.GetConnectionString("DefaultConnection")!;

            // 1. Ensure HospitalDb database exists on LocalDB
            var masterBuilder = new SqlConnectionStringBuilder(defaultConnectionString) { InitialCatalog = "master" };
            using (var masterConn = new SqlConnection(masterBuilder.ConnectionString))
            {
                masterConn.Open();
                using var cmd = masterConn.CreateCommand();
                cmd.CommandText = "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'HospitalDb') CREATE DATABASE [HospitalDb];";
                cmd.ExecuteNonQuery();
            }

            // 2. Connect to HospitalDb and run table migrations & seed scripts
            using (var dbConn = new SqlConnection(defaultConnectionString))
            {
                dbConn.Open();
                ExecuteSql(dbConn, @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Patients')
BEGIN
    CREATE TABLE Patients (
        PatientId INT IDENTITY(1,1) PRIMARY KEY,
        MedicalRecordNumber NVARCHAR(50) NOT NULL UNIQUE,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        DateOfBirth DATE NULL,
        Gender NVARCHAR(20) NOT NULL,
        PhoneNumber NVARCHAR(20) NOT NULL,
        Email NVARCHAR(150) NULL,
        Address NVARCHAR(300) NULL,
        BloodGroup NVARCHAR(10) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME())
    );
END;

IF NOT EXISTS (SELECT 1 FROM AdminUsers WHERE Username = 'admin')
BEGIN
    INSERT INTO AdminUsers (Username, PasswordHash, IsActive) VALUES ('admin', 'admin123', 1);
END;");
            }
        }

        private static void ExecuteSql(SqlConnection conn, string sql)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
    }
}'''
    add_code_listing("Listing 4.5: DatabaseInitializer.cs (Automated SQL Schema Migration & Seeding)", code_init)

    # Save document
    print(f"Saving Word document to {OUTPUT_DOCX}...")
    doc.save(OUTPUT_DOCX)
    print("Word document saved successfully!")

    # Export to PDF via Word COM
    print("Exporting to PDF via Word COM Automation...")
    try:
        word = win32com.client.Dispatch("Word.Application")
        word.Visible = False
        doc_com = word.Documents.Open(OUTPUT_DOCX)
        doc_com.SaveAs(OUTPUT_PDF, FileFormat=17) # 17 = wdFormatPDF
        doc_com.Close()
        word.Quit()
        print(f"PDF exported successfully to {OUTPUT_PDF}!")
    except Exception as e:
        print(f"Warning: PDF conversion encountered: {e}")

if __name__ == '__main__':
    build_report()
