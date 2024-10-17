using Azure.Messaging.ServiceBus.Administration;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using TransitPulse.Web.Models;

namespace TransitPulse.Web.Services;

public class DataSynchronizer : BackgroundService
{
    private readonly IDbContextFactory<QueueDbContext> _dbContextFactory;
    private readonly ServiceBusAdministrationClient _serviceBus;

    private readonly Task _firstTimeSetupTask;

    public DataSynchronizer(IDbContextFactory<QueueDbContext> dbContextFactory, ServiceBusAdministrationClient serviceBus)
    {
        _dbContextFactory = dbContextFactory;
        _serviceBus = serviceBus;
        _firstTimeSetupTask = FirstTimeSetupAsync();
    }

    public async Task<QueueDbContext> GetPreparedDbContextAsync()
    {
        await _firstTimeSetupTask;
        return await _dbContextFactory.CreateDbContextAsync();
    }

    private async Task FirstTimeSetupAsync()
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();
        await db.Database.EnsureCreatedAsync();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var db = await GetPreparedDbContextAsync();
            db.ChangeTracker.AutoDetectChangesEnabled = false;
            db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var connection = db.Database.GetDbConnection();
            await connection.OpenAsync(stoppingToken);

            var pages = _serviceBus.GetQueuesRuntimePropertiesAsync(stoppingToken);
            await foreach (var page in pages.AsPages())
            {
                var mappedQueues = page.Values.Select(p => new QueueState
                {
                    Name = p.Name,
                    ActiveCount = p.ActiveMessageCount,
                    TotalCount = p.TotalMessageCount,
                });

                await BulkInsert(connection, mappedQueues, stoppingToken);
            }

            await connection.CloseAsync();

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private static async Task BulkInsert(DbConnection connection, IEnumerable<QueueState> queues, CancellationToken cancellationToken = default)
    {
        // Since we're inserting so much data, we can save a huge amount of time by dropping down below EF Core and
        // using the fastest bulk insertion technique for Sqlite.
        using (var transaction = await connection.BeginTransactionAsync(cancellationToken))
        {
            var command = connection.CreateCommand();
            var name = AddNamedParameter(command, "$Name");
            var activeCount = AddNamedParameter(command, "$ActiveCount");
            var totalCount = AddNamedParameter(command, "$TotalCount");

            command.CommandText =
                $"INSERT OR REPLACE INTO Queues (Name, ActiveCount, TotalCount) " +
                $"VALUES ({name.ParameterName}, {activeCount.ParameterName}, {totalCount.ParameterName})";

            foreach (var queue in queues)
            {
                name.Value = queue.Name;
                activeCount.Value = queue.ActiveCount;
                totalCount.Value = queue.TotalCount;

                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }

        static DbParameter AddNamedParameter(DbCommand command, string name)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            command.Parameters.Add(parameter);
            return parameter;
        }
    }

    
}