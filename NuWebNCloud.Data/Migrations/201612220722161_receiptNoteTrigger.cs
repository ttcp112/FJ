namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class receiptNoteTrigger : DbMigration
    {
        public override void Up()
        {
            
            Sql("IF NOT EXISTS ( SELECT  *   FROM    sys.objects      WHERE   type = 'TR'    AND name = 'ReceiptNo') BEGIN EXEC (N'Create TRIGGER [dbo].[ReceiptNo] ON [dbo].[I_ReceiptNote] AFTER INSERT AS BEGIN DECLARE @Number int SELECT @Number = Count(*) FROM [dbo].[I_ReceiptNote] WHERE cast(CreatedDate as date) = cast(getdate() as date) UPDATE[dbo].[I_ReceiptNote] SET[ReceiptNo] = FORMAT(@Number, ''RN'' + Replace(CONVERT(CHAR(10), getdate(), 103), ''/'', '''') + ''-####'') WHERE[ID] = (SELECT ID FROM inserted) END') END ");
        }
        
        public override void Down()
        {
            //Sql("DROP TRIGGER [ReceiptNo]");
        }
    }
}
