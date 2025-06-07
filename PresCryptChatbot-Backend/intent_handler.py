import os
import re
from google.generativeai import GenerativeModel
import google.generativeai as genai
from dotenv import load_dotenv

load_dotenv()
genai.configure(api_key=os.getenv("GOOGLE_API_KEY"))

model = GenerativeModel(model_name="models/gemini-1.5-flash")

keyword_intents = {
    "show_doctors": ["doctors", "doctor", "available doctors", "list of doctors", "who are the doctors"],
    "doctor_availability": ["available today", "who is available", "doctor available", "availability", "working hours", "is the doctor available", "doctor schedule", "doctors on duty", "who is on call"],
    "book_appointment": ["book", "appointment", "schedule", "make an appointment", "see a doctor", "visit", "consultation"],
    "view_prescription": ["prescription", "medicine", "medications", "drugs", "treatment", "medication history"],
    "emergency_help": ["emergency", "help", "urgent", "ambulance", "immediate assistance", "critical", "life-threatening"],
    "navigate_profile": ["profile", "my profile", "account", "personal information", "my details", "settings"],
    "navigate_history": ["appointment history", "past appointments", "medical history", "previous visits", "last appointment"],
    "edit_details": ["edit", "update", "change", "personal details", "modify information", "update profile"],
    "login_issue": ["login", "forgot password", "can't log in", "password reset", "account access", "sign in problem"],
    "registration_help": ["register", "signup", "sign up", "new account", "create account", "join", "become a patient"],
    "general_navigation": ["go to", "show me", "take me to", "navigate to", "where is", "how to find", "website", "page"],
}

def detect_intent_fallback(user_input):
    for intent, keywords in keyword_intents.items():
        if any(re.search(rf"\b{re.escape(k)}\b", user_input, re.IGNORECASE) for k in keywords):
            return intent
    return "unknown"

def detect_intent_with_ai(user_input):
    try:
        # Prepare the list of possible intents from our keyword_intents dictionary
        intents_list = ", ".join(keyword_intents.keys())
        
        prompt = f"""You are a healthcare chatbot intent classifier. 
        Based on the user message, identify the most appropriate intent from this list: {intents_list}.
        Respond with only the intent name, nothing else.
        
        For example:
        - For "I want to see all doctors" → "show_doctors"
        - For "Is Dr. Smith available today?" → "doctor_availability"
        - For "Help me book an appointment" → "book_appointment"
        - For "I need to see my prescription" → "view_prescription"
        - For "I need immediate help" → "emergency_help"
        
        User message: "{user_input}"
        """
        
        result = model.generate_content(prompt)
        intent = result.text.strip().lower()
        
        # If the AI returns multiple words or a sentence, try to extract just the intent
        if " " in intent:
            # Try to find any of our known intents in the response
            for known_intent in keyword_intents.keys():
                if known_intent in intent:
                    return known_intent
            
            # If we couldn't find a match, fall back to keyword detection
            return detect_intent_fallback(user_input)
        
        # If the intent is not in our dictionary, fall back to keyword detection
        if intent not in keyword_intents:
            return detect_intent_fallback(user_input)
            
        return intent
    except Exception as e:
        print(f"AI intent detection failed: {str(e)}")
        return detect_intent_fallback(user_input)

def handle_intent(intent, patient_id=None):
    responses = {
        "show_doctors": "🌟 **Here’s a list of our amazing doctors:**\n\nLet me fetch the details for you!",
        "doctor_availability": "🩺 **Let’s find out which doctors are available!**\n\nI’ll check the schedules for you. Please specify the date if you have one in mind!",
        "book_appointment": "📅 **I’d love to help you book an appointment!**\n\nPlease tell me:\n- Which doctor you’d like to see\n- Your preferred date and time",
        "view_prescription": "💊 **Let’s get your prescription details!**\n\n**Note:** You need to be logged in to view this. If you’re ready, I’ll fetch the details for you.",
        "emergency_help": "🚨 **This looks like an emergency!**\n\nPlease take immediate action:\n- Use the **Emergency Button** on the page\n- Or call **911** for emergency services",
        "navigate_profile": "👤 **Want to visit your profile?**\n\nHere’s how:\n- Click on your **profile icon** in the top right corner of the page\n- You’ll be directed to your profile!",
        "navigate_history": "📜 **Let’s view your appointment history!**\n\nFollow these steps:\n- Go to the side navigation bar\n- Select the 3rd option: **‘Appointments’**",
        "navigate_dashboard": "🏠 **Head to your dashboard!**\n\nHere’s how:\n- Go to the side navigation bar\n- Select the 1st option: **‘Dashboard’**",
        "navigate_health_records": "📋 **View your health records easily!**\n\nFollow these steps:\n- Go to the side navigation bar\n- Select the 4th option: **‘Health Records’**",
        "edit_details": "✏️ **Need to edit your personal details?**\n\nHere’s how:\n- Click on your **profile icon** in the top right corner\n- You’ll be directed to your profile page\n- Select the **Edit** option",
        "login_issue": "🔒 **Having trouble logging in? No worries!**\n\nTry this:\n- Click on the **‘Forgot Password’** link on the login page\n- Follow the steps to reset your password",
        "registration_help": "📝 **Let’s get you registered!**\n\nHere’s how:\n- Click on the **‘Create Account’** button on the login page\n- Fill in your details\n\nNeed help with specific fields? Just let me know!",
        "general_navigation": "🗺️ **I can guide you around the website!**\n\nWhere would you like to go?\n- Dashboard\n- Appointments\n- Health Records\n- Profile\nOr anywhere else?",
        "show_appointments": "🕒 **Let’s check your appointments!**\n\nI’ll fetch your upcoming appointments for you.",
        "unknown": "🤔 **I’m not quite sure what you mean…**\n\nCould you rephrase your question? Or try one of these:\n- Book an appointment\n- Show my prescriptions\n- Check doctor availability"
    }
    return responses.get(intent, "🤷‍♀️ **I’m not sure how to help with that.** Could you please rephrase your question?")