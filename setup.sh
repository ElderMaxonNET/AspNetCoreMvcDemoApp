#!/bin/bash

# --- Self-Healing: Fix Windows Line Endings (CRLF to LF) ---
# This part ensures the script runs smoothly even if edited on Windows
if [[ $(type -P sed) ]]; then
    # Create a temporary clean version and execute it if CRLF is detected
    if grep -q $'\r' "$0"; then
        echo "--- Self-Correction: Converting Windows line endings to Linux format ---"
        sed -i 's/\r$//' "$0"
        exec /bin/bash "$0" "$@"
    fi
fi

# --- Configuration Variables ---
DB_NAME="AspNetCoreMvcDemoAppDb"
DB_PASSWORD="Sener_Dev_2026!"
CONTAINER_NAME="sql_express_docker"
HOST_BAK_PATH="./db/${DB_NAME}.bak"
CONTAINER_BAK_PATH="/var/opt/mssql/${DB_NAME}.bak"
SQL_DATA_DIR="/var/opt/mssql/data"
SQL_CMD_PATH="/opt/mssql-tools18/bin/sqlcmd"

# --- Retry Logic Settings ---
MAX_RETRIES=20
RETRY_COUNT=0

echo "--- 1. Starting SQL Server Container (Root Mode) ---"
# Remove existing container to avoid naming conflicts
docker rm -f $CONTAINER_NAME &> /dev/null

# Start SQL Server 2025 with root privileges to prevent volume permission issues
docker run --user root --restart unless-stopped -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=$DB_PASSWORD" \
  -p 1433:1433 --name $CONTAINER_NAME \
  -v MsSqlData:$SQL_DATA_DIR \
  -d mcr.microsoft.com/mssql/server:2025-latest

echo "--- 2. Waiting for SQL Server to be Ready ---"
# Check if SQL Server is accepting connections
until docker exec $CONTAINER_NAME $SQL_CMD_PATH -S localhost -U sa -P "$DB_PASSWORD" -C -Q "SET NOCOUNT ON; SELECT 1" &> /dev/null; do
    RETRY_COUNT=$((RETRY_COUNT+1))
    
    if [ $RETRY_COUNT -ge $MAX_RETRIES ]; then
        echo "--- ERROR: SQL Server startup timed out! ---"
        echo "Logs: docker logs $CONTAINER_NAME"
        exit 1
    fi
    
    echo "Attempt $RETRY_COUNT/$MAX_RETRIES: SQL Server is still warming up..."
    sleep 5
done

echo "--- 3. Starting Database Restoration ---"
# Transfer backup from host machine to container path
docker cp "$HOST_BAK_PATH" "${CONTAINER_NAME}:${CONTAINER_BAK_PATH}"

# Execute RESTORE command using optimized path variables
docker exec $CONTAINER_NAME $SQL_CMD_PATH -S localhost -U sa -P "$DB_PASSWORD" -C -Q "
RESTORE DATABASE [$DB_NAME] FROM DISK = '$CONTAINER_BAK_PATH'
WITH MOVE '$DB_NAME' TO '$SQL_DATA_DIR/$DB_NAME.mdf',
     MOVE '${DB_NAME}_log' TO '$SQL_DATA_DIR/${DB_NAME}_log.ldf', REPLACE;"

echo "--- 4. Cleaning Up Temporary Files ---"
# Clean up the temporary backup file inside the container
docker exec $CONTAINER_NAME rm "$CONTAINER_BAK_PATH"

echo "--- SETUP COMPLETED SUCCESSFULLY! ---"