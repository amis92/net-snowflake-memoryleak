using System;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Snowflake.Data.Client;

namespace SnowflakeMemoryLeak
{
    class Program
    {
        static async Task Main()
        {
            var connectionString = Environment.GetEnvironmentVariable("SNOWFLAKE_CONNECTIONSTRING");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception("Requires SNOWFLAKE_CONNECTIONSTRING env variable.");
            }
            var runner = new QueryRunner(connectionString, new Logger());
            await runner.RunQueryAsync(default);
        }
    }

    public class Logger
    {
        public virtual void Information(string message)
        {
            Console.WriteLine(message);
        }
    }

    public class QueryRunner
    {
        private readonly string connectionString;
        private readonly Logger logger;

        public QueryRunner(string connectionString, Logger logger)
        {
            this.connectionString = connectionString;
            this.logger = logger;
        }

        public virtual async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = new SnowflakeDbConnection
            {
                ConnectionString = connectionString
            };
            await connection.OpenAsync(cancellationToken);
            return connection;
        }

        public virtual async Task RunQueryAsync(CancellationToken stoppingToken = default)
        {
            logger.Information($"Using snowflake {typeof(SnowflakeDbFactory).Assembly.FullName}");
            using var proc = Process.GetCurrentProcess();
            PrintMessageWithMemoryDetails("Opening connection");
            using (var connection = await OpenConnectionAsync(stoppingToken))
            {
                PrintMessageWithMemoryDetails("Building command");
                var factory = SnowflakeDbFactory.Instance;
                var cmd = factory.CreateCommand();
                cmd.Connection = connection;
                cmd.CommandText = "select 'a'";
                PrintMessageWithMemoryDetails("Executing command");
                using var _ = await cmd.ExecuteReaderAsync(default, stoppingToken);
                PrintMessageWithMemoryDetails("Executed command");
            }
            PrintMessageWithMemoryDetails("Connection disposed.");

            void PrintMessageWithMemoryDetails(string message = null)
            {
                var gcMem = GC.GetTotalMemory(forceFullCollection: true) / (decimal)(1 << 20);
                proc.Refresh();
                var privMem = proc.PrivateMemorySize64 / (decimal)(1 << 20);
                decimal workMem = proc.WorkingSet64 / (decimal)(1 << 20);
                logger.Information($"{message}\n- PrivateMemory {privMem:N2} MB\n- WorkingSet {workMem:N2} MB\n- ManagedMemory {gcMem:N2} MB");
            }
        }
    }
}
