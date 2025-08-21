import os
import re
from google.generativeai import GenerativeModel
import google.generativeai as genai
from flask import request
from dotenv import load_dotenv
import pyodbc

load_dotenv()
genai.configure(api_key=os.getenv("GOOGLE_API_KEY"))

model = GenerativeModel(model_name="models/gemini-1.5-flash")

def get_mssql_connection():
    return pyodbc.connect(
        f"DRIVER={{ODBC Driver 17 for SQL Server}};"
        f"SERVER={os.getenv('DB_SERVER')};"
        f"DATABASE={os.getenv('DB_NAME')};"
        f"UID={os.getenv('DB_USERNAME')};"
        f"PWD={os.getenv('DB_PASSWORD')}"
    )

def execute_sql_query(query):
    try:
        conn = get_mssql_connection()
        cursor = conn.cursor()
        cursor.execute(query)
        columns = [column[0] for column in cursor.description]
        results = [dict(zip(columns, row)) for row in cursor.fetchall()]
        cursor.close()
        conn.close()
        return results
    except Exception as e:
        return {"error": str(e), "success": False}

# Updated safe tables with correct field names
SAFE_TABLES = {
    'Doctor': ['DoctorId', 'FirstName', 'LastName', 'Specialization', 'ContactNumber', 'Charge', 'Description'],
    'Appointments': ['AppointmentId', 'PatientId', 'DoctorId', 'Date', 'Time', 'Status', 'TypeOfAppointment', 'Charge', 'HospitalId'],
    'Patient': ['PatientId', 'FirstName', 'LastName', 'ContactNumber'],  # Excluding email and other private data
    'Hospitals': ['HospitalId', 'HospitalName', 'Number', 'Address', 'City'],
    'DoctorAvailability': ['AvailabilityId', 'DoctorId', 'AvailableDay', 'HospitalId', 'AvailableStartTime', 'AvailableEndTime']
}

# Define safe SELECT pattern - Updated with correct field names
SAFE_SELECT_PATTERN = re.compile(
    r"^\s*SELECT\s+(?!\s*\*\s*(?:,|FROM))([\w\s,\.\(\)]+)\s+FROM\s+(Doctor|Appointments|Patient|Hospitals|DoctorAvailability)(?:\s+\w+)?(?:\s+(?:INNER\s+)?JOIN\s+(?:Doctor|Appointments|Patient|Hospitals|DoctorAvailability)(?:\s+\w+)?\s+ON\s+[\w\s=\.]+)*(?:\s+WHERE\s+[\w\s='\.]+)?(?:\s+ORDER\s+BY\s+[\w\s,\.]+)?\s*;?\s*$",
    re.IGNORECASE
)

# Enhanced keyword intents with appointment-specific keywords
keyword_intents = {
    "greeting": ["hello", "hi", "hey", "good morning", "good afternoon", "good evening", "greetings"],
    "registration_help": ["register", "sign up", "create account", "new user", "registration", "join"],
    "login_issue": ["login", "log in", "sign in", "password", "forgot password", "can't login", "authentication"],
    "general_navigation": ["navigate", "how to", "where is", "find", "menu", "go to", "guide me"],
    "general_help": ["help", "support", "assist", "question", "need help", "guidance"],
    "show_doctors": ["doctor", "doctors", "find doctor", "show doctors", "available doctors", "physician", "specialist"],
    "view_appointments": ["appointment", "appointments", "my appointments", "schedule", "booking", "visit", "consultation"],
    "search_appointments": ["search appointment", "find appointment", "appointment details", "when is my appointment"],
    "doctor_availability": ["available", "availability", "doctor schedule", "when available", "free time"],
    "restricted_access": ["admin", "database", "system", "server", "config", "private"],
    "unknown": []
}

def is_authorized(patient_id):
    token = request.headers.get("Authorization", "").replace("Bearer ", "")
    if not token or not patient_id:
        return False
    try:
        from jwt import decode
        decoded = decode(token, os.getenv("JWT_SECRET"), algorithms=["HS256"])
        return decoded.get("role") == "patient" and decoded.get("patient_id") == patient_id
    except Exception:
        return False

def detect_intent_fallback(user_input):
    user_input_lower = user_input.lower().strip()
    for intent, keywords in keyword_intents.items():
        for keyword in keywords:
            if re.search(rf"\b{re.escape(keyword)}\b", user_input_lower):
                return intent
    return "unknown"

def is_malicious_query(sql_query):
    sql_query_clean = sql_query.strip()
    sql_upper = sql_query_clean.upper()
    
    # Check for forbidden keywords that modify data
    forbidden_keywords = [
        'DROP', 'DELETE', 'INSERT', 'UPDATE', 'ALTER', 'CREATE',
        'TRUNCATE', 'MERGE', 'REPLACE', 'GRANT', 'REVOKE', 'EXEC',
        'EXECUTE', 'DECLARE', 'CURSOR', 'BULK', 'OPENROWSET'
    ]
    
    # Check if any forbidden keyword is used as a standalone word
    for keyword in forbidden_keywords:
        if re.search(rf"\b{keyword}\b", sql_upper):
            return True
    
    # Must start with SELECT
    if not sql_query_clean.upper().startswith('SELECT'):
        return True
    
    # Check for private/sensitive fields
    private_fields = ['NIC', 'PASSWORD', 'SLMCREGID', 'SLMCIDIMAGE', 'EMAILVERIFIED', 'LASTLOGIN', 'EMAIL']
    for field in private_fields:
        if field in sql_upper:
            return True
    
    # Check if it only accesses allowed tables
    allowed_tables = list(SAFE_TABLES.keys())
    if not any(table.upper() in sql_upper for table in allowed_tables):
        return True
    
    # If we get here, it's likely a safe SELECT query
    return False

def format_appointment_data(rows):
    if not rows or (isinstance(rows, dict) and rows.get("error")):
        return "You have no scheduled appointments. You can book new appointments through the Appointments page or provide your Patient ID to view existing ones."
    
    if len(rows) == 0:
        return "No appointments found. Navigate to the Appointments page to book new ones or ensure you've provided the correct Patient ID."
        
    result = "📅 **Your Appointments:**\n\n"
    for row in rows:
        # Using correct field names
        date = row.get('Date', 'Not specified')
        time = row.get('Time', 'Not specified')
        doctor_name = f"Dr. {row.get('FirstName', 'Unknown')} {row.get('LastName', '')}"
        specialization = row.get('Specialization', 'General')
        status = row.get('Status', 'Unknown')
        
        result += f"🗓️ **{date}** at **{time}**\n"
        result += f"👨‍⚕️ **Doctor:** {doctor_name} ({specialization})\n"
        result += f"📊 **Status:** {status}\n\n"
    
    result += "\n💡 **Tip:** You can manage your appointments through the Appointments page."
    return result

def format_doctor_data(rows):
    if not rows or (isinstance(rows, dict) and rows.get("error")):
        return "No doctors found in our system."
        
    if len(rows) == 0:
        return "No doctors currently available. Please try again later."
        
    result = "👨‍⚕️ **Available Doctors:**\n\n"
    for row in rows:
        name = f"Dr. {row.get('FirstName', 'Unknown')} {row.get('LastName', '')}"
        specialization = row.get('Specialization', 'General Practice')
        charge = row.get('Charge', 'Contact for pricing')
        
        result += f"🩺 **{name}**\n"
        result += f"   Specialization: {specialization}\n"
        result += f"   Consultation Fee: ${charge}\n\n"
    
    result += "\n📞 **Contact:** Use the provided contact information to book appointments."
    return result

def format_doctor_availability_data(rows):
    if not rows or (isinstance(rows, dict) and rows.get("error")):
        return "No availability information found."
        
    if len(rows) == 0:
        return "No doctor availability found for the specified criteria."
        
    result = "📅 **Doctor Availability:**\n\n"
    for row in rows:
        doctor_name = f"Dr. {row.get('FirstName', 'Unknown')} {row.get('LastName', '')}" if 'FirstName' in row else 'Doctor'
        day = row.get('AvailableDay', 'Not specified')
        start_time = row.get('AvailableStartTime', 'N/A')
        end_time = row.get('AvailableEndTime', 'N/A')
        
        result += f"👨‍⚕️ **{doctor_name}**\n"
        result += f"   📅 Day: {day}\n"
        result += f"   🕐 Time: {start_time} - {end_time}\n\n"
    
    return result

def detect_intent_with_ai(user_input):
    try:
        prompt = f"""
You are a medical chatbot intent classifier. Analyze the user input and return ONLY the most likely intent.

Available intents:
{', '.join(keyword_intents.keys())}

User input: "{user_input}"

Return only one of the listed intents in lowercase.
"""
        result = model.generate_content(prompt)
        intent = result.text.strip().lower().replace('"', '').replace("'", "")
        
        # Validate that the returned intent is in our allowed list
        return intent if intent in keyword_intents else detect_intent_fallback(user_input)
    except Exception as e:
        print(f"AI Intent Detection Error: {e}")
        return detect_intent_fallback(user_input)

def handle_intent(intent, patient_id=None):
    # Check authorization for data-sensitive intents
    sensitive_intents = ["view_appointments", "search_appointments"]
    if intent in sensitive_intents:
        if not patient_id or patient_id == 'None' or patient_id == 'null':
            return {
                "message": "🔐 **Authentication Required**\n\nTo view your appointments, please:\n1. Provide your Patient ID, or\n2. Navigate to the **Appointments** page from the main menu\n\nThis ensures your personal information remains secure and private.",
                "action": "request_authentication",
                "navigation_target": "appointments",
                "requires_patient_id": True
            }

    if intent == "restricted_access":
        return {
            "message": "🚫 **Access Denied**\n\nYou cannot access administrative or system data. Please contact support if you need assistance.",
            "action": "access_denied"
        }

    if intent == "show_doctors":
        # Use safe columns only
        safe_columns = ', '.join(SAFE_TABLES['Doctor'])
        query = f"SELECT {safe_columns} FROM Doctor"
        
        if is_malicious_query(query):
            return {
                "error": "Query blocked for security.", 
                "message": "Sorry, I can't show doctor details right now. Please try again later.",
                "intent": intent
            }
            
        rows = execute_sql_query(query)
        return {
            "intent": intent, 
            "message": format_doctor_data(rows),
            "action": "show_doctors"
        }

    if intent == "doctor_availability":
        query = """
        SELECT da.AvailabilityId, da.DoctorId, da.AvailableDay, 
               da.AvailableStartTime, da.AvailableEndTime,
               d.FirstName, d.LastName, d.Specialization
        FROM DoctorAvailability da
        JOIN Doctor d ON da.DoctorId = d.DoctorId
        ORDER BY da.AvailableDay, da.AvailableStartTime
        """
        
        if is_malicious_query(query):
            return {
                "error": "Query blocked for security.",
                "message": "Sorry, I can't show availability right now. Please try again later.",
                "intent": intent
            }
            
        rows = execute_sql_query(query)
        return {
            "intent": intent,
            "message": format_doctor_availability_data(rows),
            "action": "show_availability"
        }

    if intent in ["view_appointments", "search_appointments"]:
        # Double-check patient_id
        if not patient_id or patient_id == 'None' or patient_id == 'null':
            return {
                "message": "📋 **Patient ID Required**\n\nTo view your appointments:\n• Provide your Patient ID in your query, or\n• Navigate to the **Appointments** page and view the calender view to get an overview\n\nThis helps us show only your personal appointments for privacy and security.",
                "action": "request_patient_id",
                "navigation_target": "appointments",
                "requires_patient_id": True,
                "intent": intent
            }
        
        # Using correct field names: Date instead of AppointmentDate
        query = f"""
        SELECT a.AppointmentId, a.Date, a.Time, a.Status, a.TypeOfAppointment,
               d.FirstName, d.LastName, d.Specialization, d.Charge,
               h.HospitalName
        FROM Appointments a
        JOIN Doctor d ON a.DoctorId = d.DoctorId
        LEFT JOIN Hospitals h ON a.HospitalId = h.HospitalId
        WHERE a.PatientId = '{patient_id}'
        AND a.Date >= CAST(GETDATE() AS DATE)
        ORDER BY a.Date, a.Time
        """
        
        if is_malicious_query(query):
            return {
                "error": "Query blocked for security.",
                "message": "Sorry, I can't show appointments right now. Please navigate to the Appointments page.",
                "intent": intent,
                "navigation_target": "appointments"
            }
        
        rows = execute_sql_query(query)
        
        # Handle database errors
        if isinstance(rows, dict) and rows.get("error"):
            return {
                "error": rows["error"],
                "message": "Unable to retrieve appointments at this time. Please try navigating to the Appointments page or contact support.",
                "intent": intent,
                "navigation_target": "appointments"
            }
        
        return {
            "intent": intent,
            "message": format_appointment_data(rows),
            "action": "show_appointments"
        }

    # Static responses for other intents
    static_responses = {
        "greeting": {
            "message": "👋 **Welcome to PresCrypt!**\n\nI'm your medical assistant, here to help you with:\n• Finding doctors and specialists\n• Viewing your appointments\n• General navigation assistance\n• Account-related questions\n\nHow can I assist you today?",
            "action": "greeting"
        },
        "registration_help": {
            "message": "📝 **Registration Guide:**\n\n1. **Click 'Sign Up'** on the main page\n2. **Fill in your details** (name, email, phone)\n3. **Verify your email** through the confirmation link\n4. **Complete your medical profile** for better service\n\n💡 **Tip:** Keep your information accurate for the best healthcare experience!",
            "action": "registration_guide"
        },
        "login_issue": {
            "message": "🔐 **Login Troubleshooting:**\n\n1. **Check credentials** - Verify email and password\n2. **Reset password** - Use 'Forgot Password' if needed\n3. **Account verification** - Ensure your email is verified\n4. **Clear browser cache** - Sometimes helps with login issues\n5. **Contact support** - If problems persist\n\n📧 **Need help?** Our support team is here for you!",
            "action": "login_help"
        },
        "general_navigation": {
            "message": "🧭 **Navigation Guide:**\n\n• **🏠 Dashboard** - Your health overview and quick actions\n• **📅 Appointments** - Book, view, and manage appointments\n• **💊 Prescriptions** - View your current medications\n• **👤 Profile** - Update personal and medical information\n• **🏥 Find Doctors** - Search specialists and healthcare providers\n\n💡 **Tip:** Use the main menu to access any section quickly!",
            "action": "navigation_guide"
        },
        "general_help": {
            "message": "ℹ️ **How Can I Help You?**\n\nI can assist you with:\n• **👨‍⚕️ Finding doctors** and specialists\n• **📅 Viewing appointments** and schedules  \n• **🧭 Navigation help** around the platform\n• **🔧 Account issues** and troubleshooting\n• **📋 General questions** about PresCrypt\n\nJust ask me anything you need help with!",
            "action": "general_assistance"
        },
        "unknown": {
            "message": "❓ **I'm not sure I understand.**\n\nCould you please rephrase your question? I can help you with:\n• **👨‍⚕️ Finding doctors** and specialists\n• **📅 Viewing appointments** (provide Patient ID)\n• **🧭 Navigation** help\n• **🔧 Account** support\n\nWhat would you like to know?",
            "action": "clarification_needed"
        }
    }
    
    return static_responses.get(intent, static_responses["unknown"])

# Additional utility function for query execution with enhanced safety
def execute_query(query, patient_id=None):
    """Execute a query with additional safety checks and proper field names"""
    if is_malicious_query(query):
        return {"error": "Query blocked for security reasons", "success": False}
    
    # Add patient_id filter for sensitive queries if needed
    if patient_id and patient_id != 'None' and patient_id != 'null':
        if "Appointments" in query and f"PatientId = '{patient_id}'" not in query:
            return {"error": "Unauthorized query - missing patient filter", "success": False}
    
    return execute_sql_query(query)

# Utility function to validate patient_id format
def is_valid_patient_id(patient_id):
    """Validate patient ID format"""
    if not patient_id or patient_id in ['None', 'null', '']:
        return False
    # Add additional validation logic as needed (length, format, etc.)
    return True

# Helper function to create patient-specific appointment queries
def create_appointment_query(patient_id, query_type="upcoming"):
    """Create safe appointment queries with correct field names"""
    if not is_valid_patient_id(patient_id):
        return None
    
    base_query = f"""
    SELECT a.AppointmentId, a.Date, a.Time, a.Status, a.TypeOfAppointment,
           d.FirstName, d.LastName, d.Specialization,
           h.HospitalName
    FROM Appointments a
    JOIN Doctor d ON a.DoctorId = d.DoctorId
    LEFT JOIN Hospitals h ON a.HospitalId = h.HospitalId
    WHERE a.PatientId = '{patient_id}'
    """
    
    if query_type == "upcoming":
        base_query += " AND a.Date >= CAST(GETDATE() AS DATE)"
    elif query_type == "all":
        pass  # No additional filter
    elif query_type == "past":
        base_query += " AND a.Date < CAST(GETDATE() AS DATE)"
    
    base_query += " ORDER BY a.Date DESC, a.Time DESC"
    
    return base_query

# Enhanced error handling for database operations
def safe_execute_query(query, context="general"):
    """Execute query with enhanced error handling and logging"""
    try:
        if is_malicious_query(query):
            print(f"SECURITY: Blocked malicious query in context '{context}': {query[:100]}...")
            return {"error": "Query blocked for security reasons", "success": False}
        
        result = execute_sql_query(query)
        
        if isinstance(result, dict) and result.get("error"):
            print(f"DATABASE ERROR in context '{context}': {result['error']}")
            return {"error": "Database operation failed", "success": False, "details": result["error"]}
        
        return {"data": result, "success": True}
        
    except Exception as e:
        print(f"EXECUTION ERROR in context '{context}': {str(e)}")
        return {"error": "Query execution failed", "success": False, "details": str(e)}