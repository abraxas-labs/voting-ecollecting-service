using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveToCustomSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // add new schema
            migrationBuilder.EnsureSchema(
                name: "ecollecting");

            // delete table in the new schema as it has just been created by the migration and is empty
            // afterwards move the actual table from the public schema to the new schema
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'ecollecting' AND table_name = 'UserNotifications')
                       AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'UserNotifications') THEN
                        DROP TABLE ecollecting."UserNotifications";
                        ALTER TABLE public."UserNotifications" SET SCHEMA ecollecting;
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'ecollecting' AND table_name = 'SecondFactorTransactions')
                       AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'SecondFactorTransactions') THEN
                        DROP TABLE ecollecting."SecondFactorTransactions";
                        ALTER TABLE public."SecondFactorTransactions" SET SCHEMA ecollecting;
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'ecollecting' AND table_name = 'AuditTrailEntries')
                       AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'AuditTrailEntries') THEN
                        DROP TABLE ecollecting."AuditTrailEntries";
                        ALTER TABLE public."AuditTrailEntries" SET SCHEMA ecollecting;
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'ecollecting' AND table_name = 'CollectionCitizenLogAuditTrailEntries')
                       AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'CollectionCitizenLogAuditTrailEntries') THEN
                        DROP TABLE ecollecting."CollectionCitizenLogAuditTrailEntries";
                        ALTER TABLE public."CollectionCitizenLogAuditTrailEntries" SET SCHEMA ecollecting;
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'ecollecting' AND table_name = 'Certificates')
                       AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'Certificates') THEN
                        DROP TABLE ecollecting."Certificates";
                        ALTER TABLE public."Certificates" SET SCHEMA ecollecting;
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'ecollecting' AND table_name = 'DomainOfInfluences')
                       AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'DomainOfInfluences') THEN
                        DROP TABLE ecollecting."DomainOfInfluences";
                        ALTER TABLE public."DomainOfInfluences" SET SCHEMA ecollecting;
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'ecollecting' AND table_name = 'CollectionPermissions')
                       AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'CollectionPermissions') THEN
                        DROP TABLE ecollecting."CollectionPermissions";
                        ALTER TABLE public."CollectionPermissions" SET SCHEMA ecollecting;
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'ecollecting' AND table_name = 'CollectionMessages')
                       AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'CollectionMessages') THEN
                        DROP TABLE ecollecting."CollectionMessages";
                        ALTER TABLE public."CollectionMessages" SET SCHEMA ecollecting;
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'ecollecting' AND table_name = 'CollectionCounts')
                       AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'CollectionCounts') THEN
                        DROP TABLE ecollecting."CollectionCounts";
                        ALTER TABLE public."CollectionCounts" SET SCHEMA ecollecting;
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'ecollecting' AND table_name = 'CollectionCitizenLogs')
                       AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'CollectionCitizenLogs') THEN
                        DROP TABLE ecollecting."CollectionCitizenLogs";
                        ALTER TABLE public."CollectionCitizenLogs" SET SCHEMA ecollecting;
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'ecollecting' AND table_name = 'FileContents')
                       AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'FileContents') THEN
                        DROP TABLE ecollecting."FileContents";
                        ALTER TABLE public."FileContents" SET SCHEMA ecollecting;
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'ecollecting' AND table_name = 'ImportStatistics')
                       AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'ImportStatistics') THEN
                        DROP TABLE ecollecting."ImportStatistics";
                        ALTER TABLE public."ImportStatistics" SET SCHEMA ecollecting;
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'ecollecting' AND table_name = 'InitiativeCommitteeMembers')
                       AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'InitiativeCommitteeMembers') THEN
                        DROP TABLE ecollecting."InitiativeCommitteeMembers";
                        ALTER TABLE public."InitiativeCommitteeMembers" SET SCHEMA ecollecting;
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'ecollecting' AND table_name = 'CollectionCitizens')
                       AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'CollectionCitizens') THEN
                        DROP TABLE ecollecting."CollectionCitizens";
                        ALTER TABLE public."CollectionCitizens" SET SCHEMA ecollecting;
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'ecollecting' AND table_name = 'CollectionSignatureSheets')
                       AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'CollectionSignatureSheets') THEN
                        DROP TABLE ecollecting."CollectionSignatureSheets";
                        ALTER TABLE public."CollectionSignatureSheets" SET SCHEMA ecollecting;
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'ecollecting' AND table_name = 'CollectionMunicipalities')
                       AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'CollectionMunicipalities') THEN
                        DROP TABLE ecollecting."CollectionMunicipalities";
                        ALTER TABLE public."CollectionMunicipalities" SET SCHEMA ecollecting;
                    END IF;
                END $$;
                """);

            // Break circular FK cycle
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'ecollecting' AND table_name = 'Collections')
                       AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'Collections') THEN
                        ALTER TABLE ecollecting."Files" DROP CONSTRAINT IF EXISTS "FK_Files_Collections_CommitteeListOfInitiativeId";
                        DROP TABLE ecollecting."Collections";
                        ALTER TABLE public."Collections" SET SCHEMA ecollecting;
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'ecollecting' AND table_name = 'InitiativeSubTypes')
                       AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'InitiativeSubTypes') THEN
                        DROP TABLE ecollecting."InitiativeSubTypes";
                        ALTER TABLE public."InitiativeSubTypes" SET SCHEMA ecollecting;
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'ecollecting' AND table_name = 'Decrees')
                       AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'Decrees') THEN
                        DROP TABLE ecollecting."Decrees";
                        ALTER TABLE public."Decrees" SET SCHEMA ecollecting;
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'ecollecting' AND table_name = 'Files')
                       AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'Files') THEN
                        DROP TABLE ecollecting."Files";
                        ALTER TABLE public."Files" SET SCHEMA ecollecting;
                    END IF;
                END $$;
                """);

            // drop the ef history table in the public schema, as it is not needed anymore
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS public."__EFMigrationsHistory";
                """);

            // revoke special privileges for public schema, as the application should only use the new schema
            migrationBuilder.Sql("""
                 DO $$
                 BEGIN
                     REVOKE USAGE ON SCHEMA public FROM "adminservice";
                     REVOKE USAGE ON SCHEMA public FROM "citizenservice";
                     REVOKE USAGE ON SCHEMA public FROM "backupservice";
                     ALTER DEFAULT PRIVILEGES IN SCHEMA public REVOKE SELECT ON TABLES FROM "backupservice";
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
