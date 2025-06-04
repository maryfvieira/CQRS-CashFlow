using System.Data;
using FluentMigrator;

namespace CashFlow.DataBase.Migrations.AuthModel;

[Migration(2023101504)]
public class CreateRoleTables : Migration
{
    public override void Up()
    {
        Create.Table("Roles")
            .WithColumn("Id").AsString(36).PrimaryKey()
            .WithColumn("Name").AsString(50).NotNullable().Unique();

        Create.Table("UserRoles")
            .WithColumn("UserId").AsString(36).NotNullable()
            .WithColumn("RoleId").AsString(36).NotNullable();

        Create.PrimaryKey("PK_UserRoles")
            .OnTable("UserRoles")
            .Columns("UserId", "RoleId");

        Create.ForeignKey("FK_UserRoles_Users")
            .FromTable("UserRoles").ForeignColumn("UserId")
            .ToTable("Users").PrimaryColumn("Id")
            .OnDelete(Rule.Cascade);

        Create.ForeignKey("FK_UserRoles_Roles")
            .FromTable("UserRoles").ForeignColumn("RoleId")
            .ToTable("Roles").PrimaryColumn("Id")
            .OnDelete(Rule.Cascade);
    }

    public override void Down()
    {
        Delete.Table("UserRoles");
        Delete.Table("Roles");
    }
}