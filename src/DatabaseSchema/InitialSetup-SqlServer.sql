

CREATE TABLE mung.Munger(
	MungerId int IDENTITY(1,1) NOT NULL,
	Email nvarchar(256) NULL,
	Name nvarchar(256) NOT NULL,
	IsAdmin bit NOT NULL,

	PasswordHash nvarchar(max) NULL,
	PasswordSalt nvarchar(max) NULL,
	

	CreatedAt datetime NOT NULL DEFAULT (getutcdate()),
	LastSeenAt datetime NOT NULL DEFAULT (getutcdate()),
	
	InvitationCode nvarchar(100) NULL,

	CONSTRAINT PK_app.Users PRIMARY KEY CLUSTERED (MungerId ASC)
) 

GO

SET IDENTITY_INSERT mung.Munger ON
INSERT INTO mung.Munger(MungerId, Email, Name, IsAdmin)
	SELECT -1, 'system@mung.io', 'System', 1

SET IDENTITY_INSERT mung.Munger OFF

CREATE TABLE mung.Dashboard (
	DashboardId INT NOT NULL IDENTITY(1,1),
	Url VARCHAR(256) NOT NULL,
	Title VARCHAR(256) NOT NULL,
	Css NVARCHAR(MAX) NULL,
	CreatedAt DATETIME NOT NULL,
	UpdatedAt DATETIME NOT NULL,
	CreatedByUserId INT NOT NULL,
	ModifiedByUserId INT NOT NULL,
	CONSTRAINT PK_Dashboard PRIMARY KEY (DashboardId),
	CONSTRAINT FK_Dashboard_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES mung.Munger(MungerId),
	CONSTRAINT FK_Dashboard_ModifiedBy FOREIGN KEY (ModifiedByUserId) REFERENCES mung.Munger(MungerId),
	CONSTRAINT UQ_Dashboard_Url UNIQUE (Url)
)

CREATE TABLE mung.Graph (
	GraphId INT NOT NULL IDENTITY(1,1),
	DashboardId INT NOT NULL,
	Title NVARCHAR(MAX) NOT NULL,
	Html NVARCHAR(MAX),
	Sql NVARCHAR(MAX),
	Js NVARCHAR(MAX) NULL,
	X FLOAT NOT NULL,
	Y FLOAT NOT NULL,
	Width FLOAT NOT NULL,
	Height FLOAT NOT NULL

	CONSTRAINT PK_Graph PRIMARY KEY (GraphId),
	CONSTRAINT FK_Graph_Dashboard FOREIGN KEY (DashboardId) REFERENCES mung.Dashboard(DashboardId)
)

CREATE TABLE mung.ConnectionType (
	ConnectionTypeId INT NOT NULL IDENTITY(1,1),
	Name NVARCHAR(100) NOT NULL,
	CONSTRAINT PK_ConnectionType PRIMARY KEY (ConnectionTypeId)
) 




INSERT INTO mung.ConnectionType(Name)
	VALUES ('SQL Server'), ('PostgreSQL')

CREATE TABLE mung.Connection (
	ConnectionId INT NOT NULL IDENTITY(1,1),
	ConnectionTypeId INT NOT NULL,
	Name NVARCHAR(100) NOT NULL,
	ConnectionString NVARCHAR(MAX) NOT NULL,
	CONSTRAINT PK_Connection PRIMARY KEY (ConnectionId),
	CONSTRAINT FK_ConnectionConnectionType FOREIGN KEY (ConnectionTypeId) REFERENCES mung.ConnectionType(ConnectionTypeId)

)


SELECT * FROM mung.App

CREATE TABLE mung.App (
	AppId INT NOT NULL IDENTITY(1,1),
	Name NVARCHAR(256) NOT NULL,
	AppSecret VARCHAR(256) NOT NULL,
	AppKey VARCHAR(256) NOT NULL,
	CreatedAt DATETIME NOT NULL,
	UpdatedAt DATETIME NOT NULL,
	CreatedByUserId INT NULL,
	ModifiedByUserId INT NULL,
	CONSTRAINT PK_App PRIMARY KEY (AppId),
	CONSTRAINT FK_App_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES mung.Munger(MungerId),
	CONSTRAINT FK_App_ModifiedBy FOREIGN KEY (ModifiedByUserId) REFERENCES mung.Munger(MungerId),

)
GO

ALTER TABLE mung.Graph
	ADD ConnectionId INT NULL

ALTER TABLE mung.Graph
	ADD CONSTRAINT FK_Graph_Connection FOREIGN KEY (ConnectionId) REFERENCES mung.Connection(ConnectionId)
GO

CREATE TABLE #Blah (
	Id decimal

)
