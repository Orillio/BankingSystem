CREATE TABLE [dbo].[Transactions] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    [ClientId]         INT            NOT NULL,
    [NameTarget]       NVARCHAR (MAX) NOT NULL,
    [LastnameTarget]   NVARCHAR (MAX) NOT NULL,
    [PatronymicTarget] NVARCHAR (MAX) NOT NULL,
    [CardTarget]       BIGINT         NOT NULL,
    [ClientTypeTarget] NVARCHAR (MAX) NOT NULL,
    [TransactionSum]   INT            NOT NULL,
    [Type]             INT            NOT NULL
);

CREATE TABLE [dbo].[Investments] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [clientId]       INT            NOT NULL,
    [investmentType] NVARCHAR (MAX) NOT NULL,
    [investmentDate] NVARCHAR (MAX) NOT NULL,
    [investmentSum]  INT            NOT NULL,
    [percentage]     INT            NOT NULL
);

CREATE TABLE [dbo].[Clients] (
    [id]               INT           IDENTITY (1, 1) NOT NULL,
    [clientName]       NVARCHAR (20) NOT NULL,
    [clientLastname]   NVARCHAR (20) NOT NULL,
    [clientPatronymic] NVARCHAR (20) NOT NULL,
    [clientAge]        INT           NOT NULL,
    [clientType]       NVARCHAR (20) NOT NULL,
    [cardNumber]       BIGINT        NULL,
    [bankBalance]      INT           NULL, 
    [checnkingAccount] BIGINT        NULL, 
    [accountBalance]   INT           NULL
);

