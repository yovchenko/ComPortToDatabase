USE [Com_Port]
GO

/****** Object:  Table [dbo].[omni_scanner]    Script Date: 10.11.2020 17:31:02 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[omni_scanner](
	[id_port_data] [int] NOT NULL CONSTRAINT [DF_omni_scanner_data_id_port_data]  DEFAULT ((0)),
	[bar_code] [varchar](50) NULL,
	[quality] [tinyint] NULL,
	[read_status] [tinyint] NULL,
	[reading_angle] [smallint] NULL,
	[object_distance] [smallint] NULL,
	[read_flag] [bit] NOT NULL CONSTRAINT [DF_omni_scanner_data_read_flag]  DEFAULT ((0)),
 CONSTRAINT [PK_OmniScanner] PRIMARY KEY CLUSTERED 
(
	[id_port_data] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[omni_scanner]  WITH CHECK ADD  CONSTRAINT [FK_omni_scanner_port_data] FOREIGN KEY([id_port_data])
REFERENCES [dbo].[port_data] ([id])
GO

ALTER TABLE [dbo].[omni_scanner] CHECK CONSTRAINT [FK_omni_scanner_port_data]
GO


