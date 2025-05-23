using FluentMigrator;

namespace CashFlow.DataBase.Migrations.ReadModel;

[Migration(2023101502)]
public class CreateTransactionTable : Migration
{
    public override void Up()
    {
        Create.Table("Transactions")
            .WithColumn("Id").AsString(36).PrimaryKey()
            .WithColumn("EntityIdentifier").AsString(36).NotNullable()
            .WithColumn("Amount").AsDecimal(18, 2).NotNullable()
            .WithColumn("OperationType").AsInt32().NotNullable()
            .WithColumn("Date").AsDateTime().NotNullable()
            .WithColumn("Description").AsString(255).Nullable();

        Create.ForeignKey("FK_Transactions_AccountBalances")
            .FromTable("Transactions").ForeignColumn("EntityIdentifier")
            .ToTable("AccountBalances").PrimaryColumn("EntityIdentifier");

        Create.Index("idx_transactions_bankaccount_date")
            .OnTable("Transactions")
            .OnColumn("EntityIdentifier").Ascending()
            .OnColumn("Date").Ascending();
    }

    public override void Down()
    {
        Delete.Table("Transactions");
    }
}