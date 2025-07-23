from flask import Blueprint, request, jsonify
from db import get_mssql_connection

emergency_routes = Blueprint('emergency_routes', __name__)

@emergency_routes.route('/api/emergency/nearby', methods=['GET'])
def nearby_emergency_services():
    """Get list of nearby emergency services"""
    try:
        conn = get_mssql_connection()
        cursor = conn.cursor()
        
        # Query to get nearby emergency services
        query = """
        SELECT 
            ServiceName,
            Address,
            PhoneNumber,
            EmergencyType
        FROM EmergencyServices
        WHERE IsActive = 1
        """
        
        cursor.execute(query)
        columns = [column[0] for column in cursor.description]
        results = [dict(zip(columns, row)) for row in cursor.fetchall()]
        
        conn.close()
        return jsonify({"success": True, "data": results})
    except Exception as e:
        return jsonify({"success": False, "error": str(e)}), 500

@emergency_routes.route('/api/emergency/contact', methods=['POST'])
def emergency_contact():
    """Contact emergency services on behalf of the patient"""
    data = request.get_json()
    patient_id = data.get('patient_id')
    emergency_type = data.get('emergency_type')
    location = data.get('location')
    
    if not all([patient_id, emergency_type]):
        return jsonify({"success": False, "error": "Missing required parameters"}), 400
    
    try:
        conn = get_mssql_connection()
        cursor = conn.cursor()
        
        # Log emergency request
        query = """
        INSERT INTO EmergencyRequests (PatientID, EmergencyType, Location, RequestTime)
        VALUES (?, ?, ?, GETDATE())
        """
        
        cursor.execute(query, (patient_id, emergency_type, location))
        conn.commit()
        
        # Get patient details for emergency response
        query = """
        SELECT 
            p.FirstName,
            p.LastName,
            p.ContactNumber,
            p.BloodGroup,
            p.EmergencyContactName,
            p.EmergencyContactNumber
        FROM Patient p
        WHERE p.PatientID = ?
        """
        
        cursor.execute(query, (patient_id,))
        patient_info = cursor.fetchone()
        
        conn.close()
        
        if patient_info:
            patient_data = {
                "name": f"{patient_info[0]} {patient_info[1]}",
                "contact": patient_info[2],
                "blood_group": patient_info[3],
                "emergency_contact": {
                    "name": patient_info[4],
                    "number": patient_info[5]
                }
            }
            return jsonify({
                "success": True, 
                "message": "Emergency services have been notified",
                "patient_info": patient_data
            })
        else:
            return jsonify({
                "success": True,
                "message": "Emergency request logged, but patient details not found"
            })
            
    except Exception as e:
        return jsonify({"success": False, "error": str(e)}), 500