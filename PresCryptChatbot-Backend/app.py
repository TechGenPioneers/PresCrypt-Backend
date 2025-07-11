from flask import Flask, request, jsonify
from flask_cors import CORS
from db import get_mssql_connection
import google.generativeai as genai
import os
from dotenv import load_dotenv

# Import intent handler
from intent_handler import detect_intent_with_ai, handle_intent

# Import route blueprints
from routes.emergency_routes import emergency_routes
# You'll need to create/import other route files similarly

load_dotenv()
app = Flask(__name__)
CORS(app)

# Register blueprints
app.register_blueprint(emergency_routes)
# Register other blueprints when available

# Gemini API Key
genai.configure(api_key=os.getenv("GEMINI_API_KEY"))

# SQL generation model
def get_gemini_sql_response(question):
    model = genai.GenerativeModel('models/gemini-1.5-flash')
    prompt = f"""
You are a helpful SQL assistant for Microsoft SQL Server. 
Only return a clean SQL SELECT query (no explanation, no markdown). 
Do NOT return anything other than the SQL statement.

Use this format:
SELECT FirstName, LastName, Specialization FROM Doctor WHERE Specialization = 'Cardiology';

If the question is vague like "show all doctors", just return:
SELECT FirstName, LastName, Specialization FROM Doctor;

Question: {question}
"""
    response = model.generate_content(prompt)
    return response.text.strip().splitlines()[0]  # Get first line only

# Natural language response model
def get_gemini_chat_response(question, context=None):
    model = genai.GenerativeModel('models/gemini-1.5-flash')
    
    system_prompt = """
    You are a medical assistant chatbot for a doctor-patient system called Prescrypt.
    Answer the user's question in a helpful, concise way.
    
    If you're given data about doctors or appointments, incorporate it in your answer.
    """
    
    full_prompt = system_prompt
    if context:
        full_prompt += f"\n\nContext information:\n{context}"
    
    full_prompt += f"\n\nUser question: {question}"
    
    response = model.generate_content(full_prompt)
    return response.text.strip()

def execute_sql_query(sql):
    try:
        conn = get_mssql_connection()
        cursor = conn.cursor()
        cursor.execute(sql)
        columns = [column[0] for column in cursor.description]
        rows = cursor.fetchall()
        
        result = []
        for row in rows:
            clean_row = []
            for val in row:
                if isinstance(val, bytes):
                    val = val.decode(errors='ignore')  # Avoid JSON error
                clean_row.append(val)
            result.append(dict(zip(columns, clean_row)))
        
        conn.close()
        return {"success": True, "data": result, "columns": columns}
    except Exception as e:
        return {"success": False, "error": str(e)}

def execute_doctors_availability_query(date=None):
    try:
        conn = get_mssql_connection()
        cursor = conn.cursor()
        
        sql = """
        SELECT 
            d.FirstName, 
            d.LastName, 
            d.Specialization,
            CASE 
                WHEN a.AppointmentDate IS NULL THEN 'Available' 
                ELSE 'Booked' 
            END AS Status
        FROM Doctor d
        LEFT JOIN (
            SELECT DoctorID, AppointmentDate 
            FROM Appointment 
            WHERE AppointmentDate = GETDATE()
        ) a ON d.DoctorID = a.DoctorID
        """
        
        if date:
            # If a specific date is provided, modify the query
            # This would need proper date formatting and validation in production
            pass
            
        cursor.execute(sql)
        columns = [column[0] for column in cursor.description]
        rows = cursor.fetchall()
        
        result = []
        for row in rows:
            clean_row = []
            for val in row:
                if isinstance(val, bytes):
                    val = val.decode(errors='ignore')
                clean_row.append(val)
            result.append(dict(zip(columns, clean_row)))
        
        conn.close()
        return {"success": True, "data": result, "columns": columns}
    except Exception as e:
        return {"success": False, "error": str(e)}

@app.route('/api/query', methods=['POST'])
def process_query():
    data = request.get_json()
    question = data.get("question", "")
    if not question:
        return jsonify({"success": False, "error": "Question is required"}), 400
    
    # First detect the intent of the question
    intent = detect_intent_with_ai(question)
    
    # Handle different intents
    if intent == "show_doctors":
        # Get list of all doctors
        sql_query = "SELECT FirstName, LastName, Specialization FROM Doctor"
        result = execute_sql_query(sql_query)
        
        # Generate a natural language response using the data
        doctors_data = ""
        if result.get("success", False) and result.get("data"):
            for doc in result["data"]:
                doctors_data += f"- Dr. {doc['FirstName']} {doc['LastName']}, {doc['Specialization']}\n"
        
        nl_response = get_gemini_chat_response(question, doctors_data)
        return jsonify({**result, "intent": intent, "response": nl_response, "sql_query": sql_query})
    
    elif intent == "doctor_availability":
        # Check doctor availability
        result = execute_doctors_availability_query()
        
        # Generate a natural language response
        availability_data = ""
        if result.get("success", False) and result.get("data"):
            for doc in result["data"]:
                availability_data += f"- Dr. {doc['FirstName']} {doc['LastName']}, {doc['Specialization']}: {doc['Status']}\n"
        
        nl_response = get_gemini_chat_response(question, availability_data)
        return jsonify({**result, "intent": intent, "response": nl_response})
    
    elif intent in ["book_appointment", "view_prescription", "navigate_profile", "navigate_history"]:
        # These intents require specific handling through dedicated endpoints
        # For now, respond with default responses from the intent handler
        response = handle_intent(intent)
        return jsonify({"success": True, "intent": intent, "response": response})
    
    elif intent == "emergency_help":
        # Emergency requests should be routed to the emergency endpoints
        # Here, just return a response to direct the user
        response = "This is an emergency situation. Please use the emergency button or call emergency services immediately."
        return jsonify({"success": True, "intent": intent, "response": response})
    
    else:
        # For unknown or general queries, fall back to SQL-based response
        try:
            sql_query = get_gemini_sql_response(question)
            result = execute_sql_query(sql_query)
            
            # Generate a natural language response
            if result.get("success", False) and result.get("data"):
                data_summary = f"Found {len(result['data'])} results."
                nl_response = get_gemini_chat_response(question, data_summary)
            else:
                nl_response = "I couldn't find the information you're looking for."
            
            return jsonify({**result, "intent": "general_query", "response": nl_response, "sql_query": sql_query})
        except Exception as e:
            return jsonify({"success": False, "error": str(e), "intent": "unknown"})

if __name__ == "__main__":
    app.run(debug=True, port=5000)