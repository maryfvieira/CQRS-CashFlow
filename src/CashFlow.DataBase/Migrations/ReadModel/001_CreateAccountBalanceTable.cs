using FluentMigrator;

namespace CashFlow.DataBase.Migrations.ReadModel;

[Migration(2023101501)]
public class CreateAccountBalanceTable : Migration
{
    public override void Up()
    {
        Create.Table("AccountBalances")
            .WithColumn("EntityIdentifier").AsString(36).PrimaryKey()
            .WithColumn("InitialBalance").AsDecimal(18, 2).NotNullable()
            .WithColumn("FinalBalance").AsDecimal(18, 2).NotNullable()
            .WithColumn("Date").AsDateTime().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("AccountBalances");
    }
}