using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MyKirito
{
    // 背景工作排程服務
    internal interface IScopedProcessingService
    {
        Task DoWork(CancellationToken stoppingToken);
    }

    internal class ScopedProcessingService : IScopedProcessingService
    {
        // 排程主要使用遊戲主服務
        private readonly IYourKiritoService _yourService;
        private readonly ILogger _logger;        
        private int _executionCount;
        public ScopedProcessingService(ILogger<ScopedProcessingService> logger, IYourKiritoService yourService)
        {
            _logger = logger;
            _yourService = yourService;
        }
        // 執行工作
        public async Task DoWork(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Scoped Processing Service is working. Count: {Count}", _executionCount);
            while (!stoppingToken.IsCancellationRequested)
            {
                _executionCount++;

                
                // 啟動遊戲服務
                var output = await _yourService.StartLoop();
                // 定時執行
                await Task.Delay(output * 1000, stoppingToken);
            }
        }
    }
}