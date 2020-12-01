using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Misc.MmsAdmin.Services;
using Nop.Plugin.Misc.MmsDataImport;
using Nop.Services.Catalog;
using Nop.Services.ExportImport;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.MmsAdmin.Infrastructure
{
    /// <summary>
    /// Dependency registrar
    /// </summary>
    public class DependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<MmsAdminService>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<MmsDataImportDirect>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<MmsProductOverride>().As<IProductService>().InstancePerLifetimeScope();
            builder.RegisterType<ImportManagerOverride>().As<IImportManager>().InstancePerLifetimeScope();
            builder.RegisterType<MmsOrderProcessingOverride>().As<IOrderProcessingService>().InstancePerLifetimeScope();
        }

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order => 2000;
    }
}