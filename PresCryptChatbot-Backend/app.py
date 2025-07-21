# from flask import Flask, request, jsonify
# from flask_cors import CORS
# from db import get_mssql_connection
# import google.generativeai as genai
# import os
# from dotenv import load_dotenv
# import re  # Make sure this import is present
# from cachetools import TTLCache
# from datetime import datetime

# # Import intent handler
# from intent_handler import detect_intent_with_ai, handle_intent

# # Import route blueprints
# from routes.emergency_routes import emergency_routes

# load_dotenv()
# app = Flask(__name__)
# CORS(app)

# # Register blueprints
# app.register_blueprint(emergency_routes)

# # Gemini API Key
# genai.configure(api_key=os.getenv("GOOGLE_API_KEY"))

# # Cache for query responses
# query_cache = TTLCache(maxsize=200, ttl=3600)

# # Allowed tables for SQL queries
# ALLOWED_TABLES = [
#     'Doctor', 'Patient', 'Appointments', 'Hospitals', 
#     'DoctorAvailability', 'PatientNotifications', 'DoctorRequest', 'RequestAvailability'
# ]

# def is_malicious_query(sql_query):
#     """
#     Enhanced malicious query detection with proper case handling
#     """
#     malicious_keywords = [
#         'DROP', 'DELETE', 'UPDATE', 'INSERT', 'ALTER', 'CREATE', 
#         'TRUNCATE', 'EXEC', 'EXECUTE', 'MERGE', 'REPLACE',
#         'GRANT', 'REVOKE', 'BULK', 'OPENROWSET', 'OPENDATASOURCE'
#     ]
    
#     sql_upper = sql_query.upper().strip()
    
#     # Check for malicious keywords as whole words
#     for keyword in malicious_keywords:
#         if re.search(rf'\b{keyword}\b', sql_upper):
#             return True
    
#     # Must start with SELECT
#     if not sql_upper.startswith('SELECT'):
#         return True
    
#     # Convert allowed tables to uppercase for comparison
#     allowed_tables_upper = [table.upper() for table in ALLOWED_TABLES]
    
#     # Check if query contains at least one allowed table
#     if not any(table in sql_upper for table in allowed_tables_upper):
#         return True
    
#     return False

# def get_gemini_sql_response(question, patient_id=None, intent=None):
#     model = genai.GenerativeModel('models/gemini-1.5-flash')
    
#     # Handle None patient_id
#     patient_id_str = f"'{patient_id}'" if patient_id and patient_id != 'None' else "'PLACEHOLDER_PATIENT_ID'"
    
#     prompt = f"""
# You are a SQL assistant for a Microsoft SQL Server healthcare database. 
# Only generate SELECT queries from: {', '.join(ALLOWED_TABLES)}.
# Never generate data modification queries.

# Database Schema:
# - Doctor: DoctorID, FirstName, LastName, Specialization, ContactInfo
# - Patient: PatientID, FirstName, LastName, Email, Phone
# - Appointments: AppointmentID, PatientID, DoctorID, AppointmentDate, Status
# - Hospitals: HospitalID, Name, Location
# - DoctorAvailability: DoctorID, AvailableDate, StartTime, EndTime
# - PatientNotifications: NotificationID, PatientID, Message, CreatedDate
# - DoctorRequest: RequestID, DoctorID, PatientID, Status
# - RequestAvailability: RequestID, DoctorID, AvailableDate

# Return only a clean SQL SELECT query.

# Examples:
# - "show all doctors" → SELECT FirstName, LastName, Specialization FROM Doctor
# - "who is in charge of cardiology" → SELECT FirstName, LastName FROM Doctor WHERE Specialization LIKE '%Cardiology%'
# - "my upcoming appointments" → SELECT a.AppointmentID, a.AppointmentDate, d.FirstName, d.LastName, d.Specialization FROM Appointments a JOIN Doctor d ON a.DoctorID = d.DoctorID WHERE a.PatientID = {patient_id_str} AND a.AppointmentDate >= GETDATE()

# Question: {question}
# Intent: {intent}
# Patient ID: {patient_id}
# """
#     response = model.generate_content(prompt)
#     return response.text.strip().replace('```sql', '').replace('```', '').strip()

# def get_gemini_chat_response(question, context=None):
#     model = genai.GenerativeModel('models/gemini-1.5-flash')
#     system_prompt = """
# You are a friendly medical assistant chatbot for Prescrypt.
# Answer in clean markdown. Be concise and professional.
# Incorporate provided data (e.g., doctors, appointments) into responses.
# Avoid diagnosing; suggest actions or specialists.
# Use emojis sparingly.
# """
#     full_prompt = system_prompt
#     if context:
#         full_prompt += f"\n\nData:\n{context}"
#     full_prompt += f"\n\nUser question: {question}"
#     response = model.generate_content(full_prompt)
#     return response.text.strip()

# def execute_sql_query(sql, patient_id=None):
#     if is_malicious_query(sql):
#         return {"success": False, "error": "Query not allowed for security reasons"}
#     try:
#         with get_mssql_connection() as conn:
#             cursor = conn.cursor()
#             cursor.execute(sql)
#             columns = [column[0] for column in cursor.description]
#             rows = cursor.fetchall()
#             result = []
#             for row in rows:
#                 clean_row = [val.decode(errors='ignore') if isinstance(val, bytes) else val for val in row]
#                 result.append(dict(zip(columns, clean_row)))
#             return {"success": True, "data": result, "columns": columns}
#     except Exception as e:
#         return {"success": False, "error": str(e)}

# @app.route('/api/query', methods=['POST'])
# def process_query():
#     data = request.get_json()
#     question = data.get("question", "")
#     patient_id = data.get("patient_id")
    
#     # Debug: Log the patient_id
#     print(f"DEBUG: Received patient_id: {patient_id}")
    
#     if not question:
#         return jsonify({"success": False, "error": "Question is required", "timestamp": datetime.utcnow().isoformat()}), 400

#     cache_key = f"{question}:{patient_id}"
#     if cache_key in query_cache:
#         return jsonify(query_cache[cache_key])

#     intent = detect_intent_with_ai(question)

#     if intent == "greeting":
#         response_data = {
#             "success": True,
#             "intent": intent,
#             "response": handle_intent(intent)["message"],
#             "timestamp": datetime.utcnow().isoformat()
#         }
#         query_cache[cache_key] = response_data
#         return jsonify(response_data)

#     urgent_keywords = ["chest pain", "difficulty breathing", "severe", "can't breathe", "heart attack", "stroke"]
#     if any(keyword in question.lower() for keyword in urgent_keywords):
#         response_data = {
#             "success": True,
#             "intent": "try_connecting_with_a_doctor",
#             "response": handle_intent("try_connecting_with_a_doctor")["message"],
#             "timestamp": datetime.utcnow().isoformat()
#         }
#         query_cache[cache_key] = response_data
#         return jsonify(response_data)

#     data_intents = ["show_doctors", "doctor_availability", "view_appointments", "search_appointments"]
#     if intent in data_intents:
#         sql_query = get_gemini_sql_response(question, patient_id, intent)
        
#         # Debug: Log the generated SQL
#         print(f"DEBUG: Generated SQL: {sql_query}")
#         print(f"DEBUG: Is malicious? {is_malicious_query(sql_query)}")
        
#         result = execute_sql_query(sql_query, patient_id)
#         context_data = ""
#         if result.get("success", False) and result.get("data"):
#             if intent == "show_doctors" or intent == "search_appointments":
#                 context_data = "Doctors:\n"
#                 for doc in result["data"]:
#                     context_data += f"- Dr. {doc['FirstName']} {doc['LastName']} ({doc['Specialization']})\n"
#             elif intent == "doctor_availability":
#                 context_data = "Today's availability:\n"
#                 for doc in result["data"]:
#                     status = "Available" if doc.get('AvailableDate') else "Not Available"
#                     context_data += f"- Dr. {doc['FirstName']} {doc['LastName']} ({doc['Specialization']}): {status} {doc.get('StartTime', '')}-{doc.get('EndTime', '')}\n"
#             elif intent == "view_appointments":
#                 context_data = "Your appointments:\n"
#                 for appt in result["data"]:
#                     context_data += f"- {appt['AppointmentDate']} with Dr. {appt['FirstName']} {appt['LastName']} ({appt['Specialization']})\n"
#         else:
#             context_data = "No data found for your request."
#         nl_response = get_gemini_chat_response(question, context_data)
#         response_data = {
#             **result,
#             "intent": intent,
#             "response": nl_response,
#             "sql_query": sql_query,
#             "timestamp": datetime.utcnow().isoformat()
#         }
#         query_cache[cache_key] = response_data
#         return jsonify(response_data)

#     response = handle_intent(intent, patient_id)
#     response_data = {
#         "success": True,
#         "intent": intent,
#         "response": response["message"],
#         "navigation_target": response.get("navigation_target", ""),
#         "action": response.get("action", ""),
#         "timestamp": datetime.utcnow().isoformat()
#     }
#     query_cache[cache_key] = response_data
#     return jsonify(response_data)

# if __name__ == "__main__":
#     app.run(debug=True, port=5000)

from flask import Flask, request, jsonify
from flask_cors import CORS
from db import get_mssql_connection
import google.generativeai as genai
import os
from dotenv import load_dotenv
import re  # Make sure this import is present
from cachetools import TTLCache
from datetime import datetime

# Import intent handler
from intent_handler import detect_intent_with_ai, handle_intent

# Import route blueprints
from routes.emergency_routes import emergency_routes

load_dotenv()
app = Flask(__name__)
CORS(app)

# Register blueprints
app.register_blueprint(emergency_routes)

# Gemini API Key
genai.configure(api_key=os.getenv("GOOGLE_API_KEY"))

# Cache for query responses
query_cache = TTLCache(maxsize=200, ttl=3600)

# Allowed tables for SQL queries with safe columns mapping
ALLOWED_TABLES = {
    'Doctor': ['DoctorId', 'FirstName', 'LastName', 'Specialization', 'ContactNumber', 'Charge', 'Description'],
    'Patient': ['PatientId', 'FirstName', 'LastName', 'Email', 'ContactNumber'],
    'Appointments': ['AppointmentId', 'PatientId', 'DoctorId', 'Date', 'Time', 'Status', 'TypeOfAppointment', 'Charge', 'HospitalId'],
    'Hospitals': ['HospitalId', 'HospitalName', 'Number', 'Address', 'City'],
    'DoctorAvailability': ['AvailabilityId', 'DoctorId', 'AvailableDay', 'HospitalId', 'AvailableStartTime', 'AvailableEndTime'],
    'PatientNotifications': ['NotificationId', 'PatientId', 'Message', 'CreatedAt'],
    'DoctorRequest': ['RequestId', 'DoctorId', 'PatientId', 'Status'],
    'RequestAvailability': ['RequestId', 'DoctorId', 'AvailableDate']
}

def is_malicious_query(sql_query):
    """
    Enhanced malicious query detection with proper case handling
    """
    malicious_keywords = [
        'DROP', 'DELETE', 'UPDATE', 'INSERT', 'ALTER', 'CREATE', 
        'TRUNCATE', 'EXEC', 'EXECUTE', 'MERGE', 'REPLACE',
        'GRANT', 'REVOKE', 'BULK', 'OPENROWSET', 'OPENDATASOURCE'
    ]
    
    sql_upper = sql_query.upper().strip()
    
    # Check for malicious keywords as whole words
    for keyword in malicious_keywords:
        if re.search(rf'\b{keyword}\b', sql_upper):
            return True
    
    # Must start with SELECT
    if not sql_upper.startswith('SELECT'):
        return True
    
    # Check if query contains at least one allowed table
    if not any(table.upper() in sql_upper for table in ALLOWED_TABLES.keys()):
        return True
    
    # Check for private fields access
    private_fields = ['NIC', 'EMAIL', 'PASSWORD', 'SLMCREGID', 'SLMCIDIMAGE', 'EMAILVERIFIED', 'LASTLOGIN', 'CREATEDAT', 'UPDATEDAT']
    for field in private_fields:
        if field in sql_upper:
            return True
    
    return False

def get_gemini_sql_response(question, patient_id=None, intent=None):
    model = genai.GenerativeModel('models/gemini-1.5-flash')
    
    # Handle None patient_id
    patient_id_str = f"'{patient_id}'" if patient_id and patient_id != 'None' else "'PLACEHOLDER_PATIENT_ID'"
    
    # Create safe columns string for each table
    safe_columns_info = ""
    for table, columns in ALLOWED_TABLES.items():
        safe_columns_info += f"- {table}: {', '.join(columns)}\n"
    
    prompt = f"""
You are a SQL assistant for a Microsoft SQL Server healthcare database. 
Only generate SELECT queries from allowed tables with safe columns.
Never generate data modification queries.
NEVER select private fields like NIC, Password, Email verification status, etc.

Safe Database Schema:
{safe_columns_info}

Important Rules:
1. Use ONLY the column names provided above
2. For Appointments table, use 'Date' not 'AppointmentDate'  
3. For DoctorAvailability, use 'AvailableDay' not 'AvailableDate'
4. For Hospitals, use 'HospitalName' not 'Name'
5. Always use safe, public columns only
6. Use proper JOINs when needed

Return only a clean SQL SELECT query.

Examples:
- "show all doctors" → SELECT FirstName, LastName, Specialization FROM Doctor
- "who is in charge of cardiology" → SELECT FirstName, LastName FROM Doctor WHERE Specialization LIKE '%Cardiology%'
- "my upcoming appointments" → SELECT a.AppointmentId, a.Date, a.Time, d.FirstName, d.LastName, d.Specialization FROM Appointments a JOIN Doctor d ON a.DoctorId = d.DoctorId WHERE a.PatientId = {patient_id_str} AND a.Date >= CAST(GETDATE() AS DATE)

Question: {question}
Intent: {intent}
Patient ID: {patient_id}
"""
    response = model.generate_content(prompt)
    return response.text.strip().replace('```sql', '').replace('```', '').strip()

def get_gemini_chat_response(question, context=None):
    model = genai.GenerativeModel('models/gemini-1.5-flash')
    system_prompt = """
You are a friendly medical assistant chatbot for Prescrypt.
Answer in clean markdown. Be concise and professional.
Incorporate provided data (e.g., doctors, appointments) into responses.
Avoid diagnosing; suggest actions or specialists.
Use emojis sparingly.
If no appointments are found, guide users to navigate to appointments page or provide patient ID.
"""
    full_prompt = system_prompt
    if context:
        full_prompt += f"\n\nData:\n{context}"
    full_prompt += f"\n\nUser question: {question}"
    response = model.generate_content(full_prompt)
    return response.text.strip()

def execute_sql_query(sql, patient_id=None):
    if is_malicious_query(sql):
        return {"success": False, "error": "Query not allowed for security reasons"}
    try:
        with get_mssql_connection() as conn:
            cursor = conn.cursor()
            cursor.execute(sql)
            columns = [column[0] for column in cursor.description]
            rows = cursor.fetchall()
            result = []
            for row in rows:
                clean_row = [val.decode(errors='ignore') if isinstance(val, bytes) else val for val in row]
                result.append(dict(zip(columns, clean_row)))
            return {"success": True, "data": result, "columns": columns}
    except Exception as e:
        return {"success": False, "error": str(e)}

@app.route('/api/query', methods=['POST'])
def process_query():
    data = request.get_json()
    question = data.get("question", "")
    patient_id = data.get("patient_id")
    
    # Debug: Log the patient_id
    print(f"DEBUG: Received patient_id: {patient_id}")
    
    if not question:
        return jsonify({"success": False, "error": "Question is required", "timestamp": datetime.utcnow().isoformat()}), 400

    cache_key = f"{question}:{patient_id}"
    if cache_key in query_cache:
        return jsonify(query_cache[cache_key])

    intent = detect_intent_with_ai(question)

    if intent == "greeting":
        response_data = {
            "success": True,
            "intent": intent,
            "response": handle_intent(intent)["message"],
            "timestamp": datetime.utcnow().isoformat()
        }
        query_cache[cache_key] = response_data
        return jsonify(response_data)

    urgent_keywords = ["chest pain", "difficulty breathing", "severe", "can't breathe", "heart attack", "stroke"]
    if any(keyword in question.lower() for keyword in urgent_keywords):
        response_data = {
            "success": True,
            "intent": "try_connecting_with_a_doctor",
            "response": handle_intent("try_connecting_with_a_doctor")["message"],
            "timestamp": datetime.utcnow().isoformat()
        }
        query_cache[cache_key] = response_data
        return jsonify(response_data)

    data_intents = ["show_doctors", "doctor_availability", "view_appointments", "search_appointments"]
    if intent in data_intents:
        # Special handling for appointment-related queries
        if intent in ["view_appointments", "search_appointments"]:
            if not patient_id or patient_id == 'None' or patient_id == 'null':
                response_data = {
                    "success": True,
                    "intent": intent,
                    "response": "📋 To view your appointments, please provide your Patient ID or navigate to the Appointments page from the main menu. This ensures we show only your personal appointments for privacy and security.",
                    "navigation_target": "appointments",
                    "action": "request_patient_id",
                    "timestamp": datetime.utcnow().isoformat()
                }
                query_cache[cache_key] = response_data
                return jsonify(response_data)
        
        sql_query = get_gemini_sql_response(question, patient_id, intent)
        
        # Debug: Log the generated SQL
        print(f"DEBUG: Generated SQL: {sql_query}")
        print(f"DEBUG: Is malicious? {is_malicious_query(sql_query)}")
        
        result = execute_sql_query(sql_query, patient_id)
        context_data = ""
        if result.get("success", False) and result.get("data"):
            if intent == "show_doctors":
                context_data = "Available Doctors:\n"
                for doc in result["data"]:
                    context_data += f"- Dr. {doc['FirstName']} {doc['LastName']} ({doc['Specialization']}) - Charge: ${doc.get('Charge', 'N/A')}\n"
            elif intent == "doctor_availability":
                context_data = "Doctor Availability:\n"
                for doc in result["data"]:
                    context_data += f"- Dr. {doc.get('FirstName', 'Unknown')} {doc.get('LastName', '')}: {doc.get('AvailableDay', 'N/A')} {doc.get('AvailableStartTime', '')}-{doc.get('AvailableEndTime', '')}\n"
            elif intent in ["view_appointments", "search_appointments"]:
                if len(result["data"]) == 0:
                    context_data = "No appointments found. You can book new appointments through the Appointments page."
                else:
                    context_data = "Your appointments:\n"
                    for appt in result["data"]:
                        context_data += f"- {appt.get('Date', 'N/A')} at {appt.get('Time', 'N/A')} with Dr. {appt.get('FirstName', 'Unknown')} {appt.get('LastName', '')} ({appt.get('Specialization', 'N/A')}) - Status: {appt.get('Status', 'Unknown')}\n"
        else:
            if intent in ["view_appointments", "search_appointments"]:
                context_data = "No appointments found. You can view all your appointments by navigating to the Appointments page or book new ones."
            else:
                context_data = "No data found for your request."
                
        nl_response = get_gemini_chat_response(question, context_data)
        response_data = {
            **result,
            "intent": intent,
            "response": nl_response,
            "sql_query": sql_query,
            "timestamp": datetime.utcnow().isoformat()
        }
        query_cache[cache_key] = response_data
        return jsonify(response_data)

    response = handle_intent(intent, patient_id)
    response_data = {
        "success": True,
        "intent": intent,
        "response": response["message"],
        "navigation_target": response.get("navigation_target", ""),
        "action": response.get("action", ""),
        "timestamp": datetime.utcnow().isoformat()
    }
    query_cache[cache_key] = response_data
    return jsonify(response_data)

if __name__ == "__main__":
    app.run(debug=True, port=5000)