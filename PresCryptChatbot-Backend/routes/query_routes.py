from flask import Blueprint, request, jsonify
from db import get_mssql_connection
import google.generativeai as genai
import os
from intent_handler import detect_intent_with_ai, handle_intent

query_routes = Blueprint('query_routes', __name__)

def get_gemini_response(question, system_prompt=""):
    model = genai.GenerativeModel('models/gemini-1.5-flash')
    full_prompt = f"{system_prompt}\n\nUser question: {question}"
    response = model.generate_content(full_prompt)
    return response.text.strip()

def execute_doctor_query(query_type, params=None):
    """Execute different types of doctor-related queries"""
    try:
        conn = get_mssql_connection()
        cursor = conn.cursor()
        
        if query_type == "all_doctors":
            sql = """
            SELECT 
                DoctorID,
                FirstName, 
                LastName, 
                Specialization,
                Education,
                YearsOfExperience
            FROM Doctor
            ORDER BY Specialization, LastName
            """
            cursor.execute(sql)
            
        elif query_type == "doctor_by_id":
            doctor_id = params.get('doctor_id')
            sql = """
            SELECT 
                DoctorID,
                FirstName, 
                LastName, 
                Specialization,
                Education,
                YearsOfExperience,
                ContactNumber,
                Email
            FROM Doctor
            WHERE DoctorID = ?
            """
            cursor.execute(sql, (doctor_id,))
            
        elif query_type == "doctors_by_specialization":
            specialization = params.get('specialization')
            sql = """
            SELECT 
                DoctorID,
                FirstName, 
                LastName, 
                Specialization,
                Education,
                YearsOfExperience
            FROM Doctor
            WHERE Specialization = ?
            ORDER BY LastName
            """
            cursor.execute(sql, (specialization,))
            
        elif query_type == "available_doctors":
            date = params.get('date', 'GETDATE()')
            sql = """
            SELECT 
                d.DoctorID,
                d.FirstName, 
                d.LastName, 
                d.Specialization,
                CASE 
                    WHEN COUNT(a.AppointmentID) = 0 THEN 'Available' 
                    ELSE 'Booked' 
                END AS Status
            FROM Doctor d
            LEFT JOIN Appointment a ON d.DoctorID = a.DoctorID 
                AND a.AppointmentDate = CONVERT(date, GETDATE())
                AND a.Status != 'Cancelled'
            GROUP BY 
                d.DoctorID,
                d.FirstName, 
                d.LastName, 
                d.Specialization
            ORDER BY d.Specialization, d.LastName
            """
            cursor.execute(sql)
        
        columns = [column[0] for column in cursor.description]
        results = []
        for row in cursor.fetchall():
            # Clean binary data
            clean_row = []
            for val in row:
                if isinstance(val, bytes):
                    val = val.decode(errors='ignore')
                clean_row.append(val)
            results.append(dict(zip(columns, clean_row)))
            
        conn.close()
        return {"success": True, "data": results, "columns": columns}
    except Exception as e:
        return {"success": False, "error": str(e)}

@query_routes.route('/api/doctors', methods=['GET'])
def get_all_doctors():
    """Get list of all doctors"""
    result = execute_doctor_query("all_doctors")
    return jsonify(result)

@query_routes.route('/api/doctors/<int:doctor_id>', methods=['GET'])
def get_doctor_by_id(doctor_id):
    """Get doctor by ID"""
    result = execute_doctor_query("doctor_by_id", {"doctor_id": doctor_id})
    return jsonify(result)

@query_routes.route('/api/doctors/specialization/<specialization>', methods=['GET'])
def get_doctors_by_specialization(specialization):
    """Get doctors by specialization"""
    result = execute_doctor_query("doctors_by_specialization", {"specialization": specialization})
    return jsonify(result)

@query_routes.route('/api/doctors/available', methods=['GET'])
def get_available_doctors():
    """Get available doctors"""
    result = execute_doctor_query("available_doctors")
    return jsonify(result)

@query_routes.route('/api/chat', methods=['POST'])
def chat_endpoint():
    """General chat endpoint with intent detection"""
    data = request.get_json()
    question = data.get("question", "")
    
    if not question:
        return jsonify({"success": False, "error": "Question is required"}), 400
    
    # Detect the intent of the user's question
    intent = detect_intent_with_ai(question)
    
    # Route to appropriate handler based on intent
    if intent == "show_doctors":
        # Get doctor data
        result = execute_doctor_query("all_doctors")
        
        # Format doctor data for the response
        doctors_text = ""
        if result.get("success") and result.get("data"):
            for doc in result["data"]:
                doctors_text += f"- Dr. {doc['FirstName']} {doc['LastName']}, {doc['Specialization']}\n"
        
        # Get a conversational response
        system_prompt = """
        You are a friendly medical assistant. Answer the user's question about doctors
        in a helpful and concise way. Here is the list of doctors in our system:
        """
        response = get_gemini_response(question, system_prompt + doctors_text)
        
        return jsonify({
            "success": True,
            "intent": intent,
            "response": response,
            "data": result.get("data", [])
        })
        
    elif intent == "doctor_availability":
        # Get availability data
        result = execute_doctor_query("available_doctors")
        
        # Format availability data
        availability_text = ""
        if result.get("success") and result.get("data"):
            for doc in result["data"]:
                availability_text += f"- Dr. {doc['FirstName']} {doc['LastName']}, {doc['Specialization']}: {doc['Status']}\n"
        
        system_prompt = """
        You are a friendly medical assistant. Answer the user's question about doctor availability
        in a helpful and concise way. Here is the current availability:
        """
        response = get_gemini_response(question, system_prompt + availability_text)
        
        return jsonify({
            "success": True,
            "intent": intent,
            "response": response,
            "data": result.get("data", [])
        })
    
    else:
        # Handle other intents with the intent handler
        response = handle_intent(intent)
        return jsonify({
            "success": True,
            "intent": intent,
            "response": response
        })