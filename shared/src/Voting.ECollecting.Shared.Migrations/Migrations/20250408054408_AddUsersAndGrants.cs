using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations;

/// <inheritdoc />
public partial class AddUsersAndGrants : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // create dev users if DEBUG is set
#if DEBUG
        migrationBuilder.Sql($"""
                              DO $$
                              BEGIN
                                  IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'adminservice') THEN
                                    CREATE USER "adminservice" WITH PASSWORD 'adminservice';
                                  END IF;

                                  IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'citizenservice') THEN
                                    CREATE USER "citizenservice" WITH PASSWORD 'citizenservice';
                                  END IF;
                              END $$;
                              """);
#endif

        migrationBuilder.Sql("""
                             DO $$
                             DECLARE
                                 db_name TEXT := current_database();
                                 schema_name TEXT := current_schema();
                             BEGIN
                                 -- Grant permissions on the database
                                 EXECUTE 'GRANT CONNECT ON DATABASE ' || quote_ident(db_name) || ' TO "adminservice"';
                                 EXECUTE 'GRANT CONNECT ON DATABASE ' || quote_ident(db_name) || ' TO "citizenservice"';

                                 -- Grant permissions on the schema
                                 EXECUTE 'GRANT USAGE ON SCHEMA ' || quote_ident(schema_name) || ' TO "adminservice"';
                                 EXECUTE 'GRANT USAGE ON SCHEMA ' || quote_ident(schema_name) || ' TO "citizenservice"';

                                 -- admin service
                                 GRANT INSERT, SELECT, UPDATE, DELETE ON "AccessControlListDois" TO adminservice;
                                 GRANT INSERT, SELECT, UPDATE, DELETE ON "CollectionBaseEntity" TO adminservice;
                                 GRANT INSERT, SELECT, UPDATE, DELETE ON "CollectionCounts" TO adminservice;
                                 GRANT INSERT, SELECT, UPDATE, DELETE ON "Decrees" TO adminservice;
                                 GRANT INSERT, SELECT, UPDATE, DELETE ON "ImportStatistics" TO adminservice;

                                 -- citizen service
                                 GRANT SELECT ON "AccessControlListDois" TO citizenservice;
                                 GRANT INSERT, SELECT, UPDATE ON "CollectionBaseEntity" TO citizenservice;
                                 GRANT INSERT, SELECT, UPDATE ON "CollectionCounts" TO citizenservice;
                                 GRANT SELECT ON "Decrees" TO citizenservice;
                             END $$;
                             """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new InvalidOperationException("down is not supported by this migration");
    }
}
