using FluentMigrator;
using System;

[Migration(5)]
public class AddStatusToOrderTable : Migration
{
    public override void Up()
    {
        Execute.Sql(@"ALTER TABLE orders ADD COLUMN status TEXT NOT NULL DEFAULT 'created';");

        Execute.Sql(@"DROP TYPE IF EXISTS v1_order;");

        Execute.Sql(@"
            CREATE TYPE v1_order AS (
                id BIGINT,
                customer_id BIGINT,
                delivery_address TEXT,
                total_price_cents BIGINT,
                total_price_currency TEXT,
                status TEXT,
                created_at TIMESTAMPTZ,
                updated_at TIMESTAMPTZ
            );
        ");
    }

    public override void Down()
    {
        Execute.Sql(@"ALTER TABLE orders DROP COLUMN status;");
        Execute.Sql(@"DROP TYPE IF EXISTS v1_order;");
    }
}
