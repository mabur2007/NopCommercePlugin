using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.MmsAdmin.Domain;

namespace Nop.Plugin.Misc.MmsAdmin.Data
{
    [SkipMigrationOnUpdate]
    [NopMigration("2020/01/01 10:32:18:1234568", "Misc.MmsAdmin base schema")]
    public class SchemaMigration : AutoReversingMigration
    {
        protected IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            _migrationManager.BuildTable<MmsNopVideo>(Create);
        }
    }
}