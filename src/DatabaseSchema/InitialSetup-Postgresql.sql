

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
	name TEXT NOT NULL,
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
	url VARCHAR(256) NOT NULL UNIQUE,
	title TEXT NOT NULL,
	css TEXT NULL,
	
	created_at timestamp NOT NULL DEFAULT (now() at time zone 'utc'),
	updated_at timestamp NOT NULL DEFAULT (now() at time zone 'utc'),
	
	created_by_munger INT NOT NULL REFERENCES munger(munger_id),
	updated_by_munger INT NOT NULL REFERENCES munger(munger_id)
);



CREATE TABLE graph (
	graph_id serial PRIMARY KEY,
	dashboard_id INT NOT NULL REFERENCES dashboard(dashboard_id),
	connection_id INT NOT NULL REFERENCES connection(connection_id),
	title TEXT NOT NULL,
	html TEXT NULL,
	sql TEXT NULL,
	js TEXT NULL,
	x FLOAT NOT NULL,
	y FLOAT NOT NULL,
	width FLOAT NOT NULL,
	height FLOAT NOT NULL
);


INSERT INTO munger(munger_id, email, name, is_admin)
	SELECT -1, 'system@mung.io', 'System', TRUE

;

INSERT INTO provider(name)
	VALUES ('SQL Server'), ('PostgreSQL'), ('Realtime')

;