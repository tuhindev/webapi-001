CREATE DATABASE MyDB101
GO
CREATE TABLE MyDB101.dbo.T1(
	ID int identity primary key not null,
	[Name] varchar(100)
)