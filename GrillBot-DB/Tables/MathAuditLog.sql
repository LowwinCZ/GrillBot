﻿CREATE TABLE [dbo].[MathAuditLog]
(
	[ID] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	[Expression] NVARCHAR(MAX) NOT NULL,
	[UserID] BIGINT NOT NULL,
	[ChannelID] VARCHAR(30) NOT NULL,
	[UnitInfo] NVARCHAR(MAX) NOT NULL,
	[Result] NVARCHAR(MAX) NOT NULL,
	[DateTime] DATETIME NOT NULL,

	CONSTRAINT [FK_MathAuditLog_UserID] FOREIGN KEY ([UserID]) REFERENCES [DiscordUsers]([ID])
);