USE [Com_Port]
GO

/****** Object:  Trigger [dbo].[omni_scanner_trigger]    Script Date: 10.11.2020 17:30:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE TRIGGER [dbo].[omni_scanner_trigger] ON [dbo].[port_data] 
 AFTER UPDATE
AS
BEGIN
       SET NOCOUNT ON;
	   DECLARE @Response VARCHAR(255);
	   DECLARE @Num INT;
	   DECLARE @Sub VARCHAR(255);
	   DECLARE @Flag BIT;

       SELECT @Response = INSERTED.response_data      
       FROM INSERTED
	  
	   SELECT @Flag = read_flag FROM [Com_Port].dbo.omni_scanner
	   
		IF CHARINDEX('n=', @Response) <> 0 
		AND CHARINDEX('AK=', @Response) <> 0 
		BEGIN
			SELECT @Sub = SUBSTRING(@Response, CHARINDEX( 'n=', @Response) + 2 , 
			CHARINDEX('AK=', @Response) - CHARINDEX('n=', @Response) - 2 ) 
			--Number of codes detected
			SELECT @Num = SUBSTRING(@Sub, PATINDEX('%[0-9]%', @Sub), LEN(@Sub))
			--No code!

			IF @Num = 0
			BEGIN
				UPDATE [Com_Port].dbo.omni_scanner
				SET [bar_code] = NULL
				,[quality] = NULL
				,[read_status] = NULL
				,[reading_angle] = NULL
				,[object_distance] = NULL
				,[read_flag] = 0
			END;
			ELSE 
			BEGIN
				UPDATE [Com_Port].dbo.omni_scanner
				SET [read_flag] = 1
				SELECT @Flag = 0
			END;
		END;

		IF @Flag = 1
		BEGIN		
			UPDATE [Com_Port].dbo.omni_scanner
			SET [read_flag] = 0
			,[bar_code] = @Response	
		END;

		IF CHARINDEX('C39', @Response) <> 0 
		AND CHARINDEX('ST=', @Response) <> 0 
		AND CHARINDEX('RA=', @Response) <> 0
		AND CHARINDEX('OD=', @Response) <> 0
		BEGIN
			--Set Identification quality (%)	
			SELECT @Sub = SUBSTRING(@Response, CHARINDEX( 'C39', @Response) + 3 , 
			CHARINDEX('%', @Response) - CHARINDEX('C39', @Response) - 3 ) 
			SELECT @Num = SUBSTRING(@Sub, PATINDEX('%[0-9]%', @Sub), LEN(@Sub))
			UPDATE [Com_Port].dbo.omni_scanner
			SET quality = @Num

			--Set Read status (ST = 0: Good Read)
			SELECT @Sub = SUBSTRING(@Response, CHARINDEX( 'ST=', @Response) + 3 , 
			CHARINDEX('CL=', @Response) - CHARINDEX('ST=', @Response) - 3 ) 
			SELECT @Sub = CONCAT('0x0', @Sub) 
			SELECT @Num = CAST(CONVERT(VARBINARY(16), @Sub, 1) AS INT)
			UPDATE [Com_Port].dbo.omni_scanner
			SET read_status = @Num

			--Set Reading angle
			SELECT @Sub = SUBSTRING(@Response, CHARINDEX( 'RA=', @Response) + 3 , 
			CHARINDEX('OD=', @Response) - CHARINDEX('RA=', @Response) - 3 ) 
			SELECT @Num = SUBSTRING(@Sub, PATINDEX('%[0-9]%', @Sub), LEN(@Sub))
			UPDATE [Com_Port].dbo.omni_scanner
			SET reading_angle = @Num

			--Set Object distance, radial measured (mm) 
			SELECT @Sub = SUBSTRING(@Response, CHARINDEX( 'OD=', @Response) + 3 , 
			CHARINDEX('CS=', @Response) - CHARINDEX('OD=', @Response) - 3 ) 
			SELECT @Num = SUBSTRING(@Sub, PATINDEX('%[0-9]%', @Sub), LEN(@Sub))
			UPDATE [Com_Port].dbo.omni_scanner
			SET object_distance = @Num
			END;
END;

GO


