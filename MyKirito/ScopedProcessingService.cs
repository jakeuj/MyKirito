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
        //總獲得屬性點
        private static int _totalPoints;

        // 亂數產生器
        private static readonly Random RandomCd = new Random();

        private readonly ILogger _logger;

        // 排程主要使用遊戲主服務
        private readonly IMyService _myService;
        private int _executionCount;

        // Pvp計時器
        private DateTime _nextPvpTime = DateTime.Now.AddSeconds(Const.PvpTime);

        public ScopedProcessingService(ILogger<ScopedProcessingService> logger, IMyService myService)
        {
            _logger = logger;
            _myService = myService;
        }

        // 執行工作
        public async Task DoWork(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _executionCount++;

                _logger.LogInformation(
                    "Scoped Processing Service is working. Count: {Count}", _executionCount);
                // 取得角色資料
                var myKirito = await _myService.GetMyKiritoFn();
                if (myKirito != null)
                {
                    // 轉生限制條件：滿十等或死亡
                    if (myKirito.Dead || AppSettings._defaultReIncarnationLevel > 0 &&
                        myKirito.Lv >= AppSettings._defaultReIncarnationLevel)
                    {
                        // 計算轉生屬性點數
                        var freePoints = CheckPoint(myKirito.Lv);
                        // 開始轉生
                        if (await _myService.ReIncarnation(freePoints))
                            _totalPoints += freePoints;
                    }
                    else
                    {
                        // 日常動作：汁妹之類的
                        if (await _myService.DoAction(AppSettings._defaultAct))
                            // PVP 
                            if (AppSettings._defaultFight != FightEnum.None && DateTime.Now > _nextPvpTime)
                                if (await _myService.GetUserList(myKirito.Exp + AppSettings._expPVP))
                                    _nextPvpTime = _nextPvpTime.AddSeconds(Const.PvpTime);
                    }
                }

                // 定時執行
                int addTime;
                if (AppSettings._randTime > 0)
                    addTime = Const.CheckTime + RandomCd.Next(1, AppSettings._randTime);
                else
                    addTime = Const.CheckTime;
                Console.WriteLine($"屬性小計：{_totalPoints}, 下次戰鬥： {_nextPvpTime}, 等待 {addTime} 秒...");
                await Task.Delay(addTime * 1000, stoppingToken);
            }
        }

        //檢查獲得屬性點數
        private static int CheckPoint(int input)
        {
            //超過最大等級時：超過幾級就拿幾點 + 所有獎勵等級各拿一點(有幾個限制就拿幾點)
            if (input > AppSettings.AddPointLevel.Max())
                return input - AppSettings.AddPointLevel.Max() + AppSettings.AddPointLevel.Count();
            //否則等級滿足幾個門檻就拿點幾額外屬性點
            return AppSettings.AddPointLevel.Count(x => input >= x);
        }
    }
}