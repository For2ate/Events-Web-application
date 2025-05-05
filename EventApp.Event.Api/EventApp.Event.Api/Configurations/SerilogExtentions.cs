using Serilog;
using Serilog.Filters;
using Serilog.Formatting.Compact;

namespace EventApp.Api.Configurations {

    public static class SerilogExtentions {

        public static IHostBuilder ConfigureSerilog(this IHostBuilder hostBuilder) {
            return hostBuilder.UseSerilog((context, services, config) => {

                var configuration = context.Configuration;

                // Base configuration
                config.ReadFrom.Configuration(configuration);

                // Component logs configuration
                ConfigureComponentLogs(config, configuration);

            });
        }

        private static void ConfigureComponentLogs(LoggerConfiguration config, IConfiguration configuration) {

            var componentLogs = configuration.GetSection("ComponentLogs");
            var children = componentLogs.GetChildren();

            foreach (var component in children) {
                var ns = component["Namespace"];
                var path = component["Path"];
                var formatter = component["formatter"];

                if (!string.IsNullOrEmpty(ns) && !string.IsNullOrEmpty(path)) {
                    var dir = Path.GetDirectoryName(path);
                    if (!Directory.Exists(dir)) {
                        Directory.CreateDirectory(dir);
                    }

                    config.WriteTo.Logger(lc => lc
                        .Filter.ByIncludingOnly(Matching.FromSource(ns))
                        .WriteTo.File(
                            new CompactJsonFormatter(),
                            path,
                            rollingInterval: RollingInterval.Day));
                }

            }

        }

    }

}
