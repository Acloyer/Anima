using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Anima.Data;

namespace Anima.Infrastructure
{
    public class DatabaseInitializer : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(IServiceProvider serviceProvider, ILogger<DatabaseInitializer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🔧 Инициализация базы данных Anima AGI...");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AnimaDbContext>();

                // Создаем базу данных если она не существует
                await dbContext.Database.EnsureCreatedAsync(cancellationToken);
                
                // Применяем миграции
                if (dbContext.Database.GetPendingMigrations().Any())
                {
                    _logger.LogInformation("📦 Применение миграций базы данных...");
                    await dbContext.Database.MigrateAsync(cancellationToken);
                }

                // Проверяем подключение
                var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
                if (canConnect)
                {
                    _logger.LogInformation("✅ База данных успешно инициализирована и доступна");
                }
                else
                {
                    _logger.LogError("❌ Не удалось подключиться к базе данных");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при инициализации базы данных: {Message}", ex.Message);
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🛑 Остановка инициализатора базы данных");
            return Task.CompletedTask;
        }
    }
} 