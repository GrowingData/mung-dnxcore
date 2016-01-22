set PGPASSWORD=postgres

if [ ! -f db-created.txt ]; then
	psql -u postgres -p 5432 -h mung-psql -c "CREATE DATABASE mung;"
	psql -u postgres -p 5432 -h mung-psql -c "CREATE DATABASE mung_events;"
	psql -u postgres -p 5432 -h mung-psql -f "/mung/src/DatabaseSchema/InitialSetup-Postgresql.sql"
	echo "created" >> db-created.txt
fi 


dotnet web
