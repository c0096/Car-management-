set -e

/opt/mssql/bin/sqlservr &
server_pid=$!

until /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -Q "SELECT 1" >/dev/null 2>&1
do
    sleep 2
done

if [ ! -f /var/opt/mssql/.vehicle_orders_initialized ]
then
    /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -i /usr/config/init.sql
    touch /var/opt/mssql/.vehicle_orders_initialized
fi

wait "$server_pid"
