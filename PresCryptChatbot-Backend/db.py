import pyodbc
import os
from dotenv import load_dotenv

load_dotenv()

def get_mssql_connection():
    driver = os.getenv("DB_DRIVER")
    server = os.getenv("DB_SERVER")
    db = os.getenv("DB_NAME")
    use_windows_auth = os.getenv("USE_WINDOWS_AUTH", "False") == "True"

    if use_windows_auth:
        conn_str = f"DRIVER={{{driver}}};SERVER={server};DATABASE={db};Trusted_Connection=yes;"
    else:
        username = os.getenv("DB_USERNAME")
        password = os.getenv("DB_PASSWORD")
        conn_str = f"DRIVER={{{driver}}};SERVER={server};DATABASE={db};UID={username};PWD={password};"

    return pyodbc.connect(conn_str)
