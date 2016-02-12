

CREATE TABLE munger (
	munger_id serial PRIMARY KEY,
	email TEXT NULL UNIQUE,
	name TEXT NOT NULL,
	is_admin BOOLEAN NOT NULL,

	password_hash TEXT NULL,
	password_salt TEXT NULL,

	created_at timestamp NOT NULL DEFAULT (now() at time zone 'utc'),
	seen_at timestamp NOT NULL DEFAULT (now() at time zone 'utc'),
	
	invitation_code TEXT NULL
);

CREATE TABLE provider (
	provider_id serial PRIMARY KEY,
	Name VARCHAR(100) NOT NULL
) ;

CREATE TABLE connection (
	connection_id serial PRIMARY KEY,
	provider_id INT NOT NULL REFERENCES provider(provider_id),
	name TEXT NOT NULL UNIQUE,
	connection_string TEXT NOT NULL,
	created_at timestamp NOT NULL DEFAULT (now() at time zone 'utc'),
	updated_at timestamp NOT NULL DEFAULT (now() at time zone 'utc'),
	
	created_by_munger INT NOT NULL REFERENCES munger(munger_id),
	updated_by_munger INT NOT NULL REFERENCES munger(munger_id)
);

CREATE TABLE app (
	app_id serial PRIMARY KEY,
	name TEXT NOT NULL,
	app_secret TEXT NOT NULL,
	app_key TEXT NOT NULL,
	created_at timestamp NOT NULL DEFAULT (now() at time zone 'utc'),
	updated_at timestamp NOT NULL DEFAULT (now() at time zone 'utc'),
	created_by_munger INT NOT NULL REFERENCES munger(munger_id),
	updated_by_munger INT NOT NULL REFERENCES munger(munger_id)

);

CREATE TABLE dashboard (
	dashboard_id serial PRIMARY KEY,
	name TEXT NOT NULL UNIQUE,
	css TEXT NULL,
	
	created_at timestamp NOT NULL DEFAULT (now() at time zone 'utc'),
	updated_at timestamp NOT NULL DEFAULT (now() at time zone 'utc'),
	
	created_by_munger INT NOT NULL REFERENCES munger(munger_id),
	updated_by_munger INT NOT NULL REFERENCES munger(munger_id)
);


CREATE TABLE query (
	query_id serial PRIMARY KEY,
	path TEXT NOT NULL,
	name TEXT NOT NULL,
	code TEXT NOT NULL,
	
	created_at timestamp NOT NULL DEFAULT (now() at time zone 'utc'),
	updated_at timestamp NOT NULL DEFAULT (now() at time zone 'utc'),
	
	created_by_munger INT NOT NULL REFERENCES munger(munger_id),
	updated_by_munger INT NOT NULL REFERENCES munger(munger_id)
);

CREATE TABLE graph (
	graph_id serial PRIMARY KEY,
	dashboard_id INT NOT NULL REFERENCES dashboard(dashboard_id),
	name TEXT NOT NULL,
	html TEXT NULL,
	x FLOAT NOT NULL,
	y FLOAT NOT NULL,
	width FLOAT NOT NULL,
	height FLOAT NOT NULL
);

CREATE TABLE setting (
	key TEXT NOT NULL PRIMARY KEY,
	value TEXT NOT NULL
);

CREATE TABLE notification(
	notification_id serial PRIMARY KEY,
	name TEXT NOT NULL UNIQUE,
	event_type TEXT NOT NULL,
	template TEXT NOT NULL,
	is_paused BOOLEAN NOT NULL,

	created_at timestamp NOT NULL DEFAULT (now() at time zone 'utc'),
	updated_at timestamp NOT NULL DEFAULT (now() at time zone 'utc'),
	
	created_by_munger INT NOT NULL REFERENCES munger(munger_id),
	updated_by_munger INT NOT NULL REFERENCES munger(munger_id)
);


INSERT INTO munger(munger_id, email, name, is_admin)
	SELECT -1, 'system@mung.io', 'System', TRUE

;

INSERT INTO provider(name)
	VALUES ('SQL Server'), ('PostgreSQL')
;

UPDATE connection
	SET connection_string  = 'Host=localhost;Port=5433;Database=mung_events;Username=postgres;Password=postgres'
	WHERE connection_id=1
	
SELECT * FROM connection
