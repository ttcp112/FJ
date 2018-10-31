Create TRIGGER [dbo].[ReceiptNo] ON [dbo].[I_ReceiptNote]
AFTER INSERT
AS 
BEGIN
	DECLARE @Number int
	SELECT @Number = Count(*) FROM [dbo].[I_ReceiptNote] WHERE cast(CreatedDate as date) = cast(getdate() as date)
	UPDATE [dbo].[I_ReceiptNote]   
	SET [ReceiptNo] = FORMAT(@Number, 'RN' + Replace(CONVERT(CHAR(10), getdate(), 103), '/', '') + '-####')
	WHERE [ID] = (SELECT ID FROM inserted)
END