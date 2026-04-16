using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBackupServiceUserAndGrants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // create dev users if DEBUG is set
#if DEBUG
            migrationBuilder.Sql($"""
                              DO $$
                              BEGIN
                                  IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'backupservice') THEN
                                    CREATE USER "backupservice" WITH PASSWORD 'backupservice';
                                  END IF;
                              END $$;
                              """);
#endif

            migrationBuilder.Sql("""
                             DO $$
                             DECLARE
                                 db_name TEXT := current_database();
                                 schema_name TEXT := current_schema();
                                 tbl RECORD;
                                 seq RECORD;
                             BEGIN
                                 -- Grant permissions on the database
                                 EXECUTE 'GRANT CONNECT ON DATABASE ' || quote_ident(db_name) || ' TO "backupservice"';

                                 -- Grant permissions on the schema
                                 EXECUTE 'GRANT USAGE ON SCHEMA ' || quote_ident(schema_name) || ' TO "backupservice"';

                                 -- Grant SELECT on all existing tables owned by the current user.
                                 -- Granting on ALL TABLES would fail in managed clusters where extension-owned tables (e.g. pg_auth_mon) exist in the schema.
                                 FOR tbl IN
                                     SELECT tablename FROM pg_tables
                                     WHERE schemaname = schema_name AND tableowner = current_user
                                 LOOP
                                     EXECUTE 'GRANT SELECT ON ' || quote_ident(schema_name) || '.' || quote_ident(tbl.tablename) || ' TO "backupservice"';
                                 END LOOP;

                                 -- Grant SELECT on all future tables
                                 EXECUTE 'ALTER DEFAULT PRIVILEGES IN SCHEMA ' || quote_ident(schema_name) || ' GRANT SELECT ON TABLES TO "backupservice"';

                                 -- Grant SELECT on all sequences owned by the current user
                                 FOR seq IN
                                     SELECT sequencename FROM pg_sequences
                                     WHERE schemaname = schema_name AND sequenceowner = current_user
                                 LOOP
                                     EXECUTE 'GRANT SELECT ON SEQUENCE ' || quote_ident(schema_name) || '.' || quote_ident(seq.sequencename) || ' TO "backupservice"';
                                 END LOOP;
                             END $$;
                             """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new InvalidOperationException("down is not supported by this migration");
        }
    }
}
