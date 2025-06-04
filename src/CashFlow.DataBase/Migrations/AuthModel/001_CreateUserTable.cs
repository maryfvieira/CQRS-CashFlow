using FluentMigrator;

namespace CashFlow.DataBase.Migrations.AuthModel;

[Migration(2023101503)]
public class CreateUserTable : Migration
{
    public override void Up()
    {
        Create.Table("Users")
            .WithColumn("Id").AsString(36).PrimaryKey()
            .WithColumn("Email").AsString(255).NotNullable().Unique()
            .WithColumn("Username").AsString(100).NotNullable().Unique()
            .WithColumn("Password").AsString(255).NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable();

        Create.Index("idx_users_email")
            .OnTable("Users")
            .OnColumn("Email").Ascending();

        Create.Index("idx_users_username")
            .OnTable("Users")
            .OnColumn("Username").Ascending();
    }

    public override void Down()
    {
        Delete.Table("Users");
    }
}