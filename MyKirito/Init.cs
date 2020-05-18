using System;

namespace MyKirito
{
    public class Init
    {
        public static void Initialization(string[] args)
        {
            Console.WriteLine("======================================================================");
            Console.WriteLine("[提示] 可以給參數執行");
            Console.WriteLine("[格式] dotnet MyKirito.dll {參數}");
            Console.WriteLine("[參數] Token 重生等級 行為 角色 PVP  CD 安靜 PVP等級 PVP暱稱 PVP識別碼");
            Console.WriteLine("[範例] MyKirito.exe ABC123.456 15 Eat Eugeo Hard 100 1 5 桐人 a1b2c3d4");
            Console.WriteLine("[結果1] PVP會先找a1b2c3d4使用者再找桐人最後找高於自身5級的人打");
            Console.WriteLine("[結果2] 不會再詢問設定值，每100~200秒執行一次動作並且認真對戰");
            Console.WriteLine("[結果3] 死亡會重生為尤吉歐並自動野餐，使用的使用者權杖為ABC123.456");
            Console.WriteLine("[建議] PVP如要針對，最好用識別碼 A 0 Eat Eugeo Hard 0 1 5 \"\" a1b2c3d4");
            Console.WriteLine("======================================================================");
            // 接收Token參數
            if (args.Length > 0)
                AppSettings.Token = args[0];
            // 接收重生等級參數
            if (args.Length > 1 && int.TryParse(args[1], out var newReLevel))
                AppSettings.DefaultReIncarnationLevel = newReLevel;
            // 接收行為參數
            if (args.Length > 2 && Enum.TryParse(args[2], true, out ActionEnum newActEnum))
                AppSettings.DefaultAct = newActEnum;
            // 接收角色參數
            if (args.Length > 3 && Enum.TryParse(args[3], true, out CharEnum newCharEnum))
                AppSettings.DefaultChar = newCharEnum;
            // 接收戰鬥參數
            if (args.Length > 4 && Enum.TryParse(args[4], true, out FightEnum newFightEnum))
                AppSettings.DefaultFight = newFightEnum;
            // 接收亂數最大值參數
            if (args.Length > 5 && int.TryParse(args[5], out var newRandTime))
                AppSettings.RandTime = newRandTime;
            // 接收安靜模式參數
            if (args.Length > 6 && int.TryParse(args[6], out var isSilence) && isSilence > 0)
                AppSettings.IsAsk = false;
            // 接收PVP對手等級值增量參數
            if (args.Length > 7 && int.TryParse(args[7], out var newExpPVP))
                AppSettings.PvpLevel = newExpPVP;
            // 接收PVP對手暱稱參數
            if (args.Length > 8)
                AppSettings.PvpNickName = args[8];
            // 接收PVP對手Uid參數
            if (args.Length > 9)
                AppSettings.PvpUid = args[9];
            Asker();
            Console.WriteLine("======================================================================");
            if (!AppSettings.IsAsk)
                Console.WriteLine($"目前模式設定為安靜模式(不詢問設定值)");
            else
                Console.WriteLine($"目前模式設定為一般模式(會問設定值)");
            Console.WriteLine("Token 目前設定為：");
            Console.WriteLine(AppSettings.Token);
            Console.WriteLine($"等擊達到 {AppSettings.DefaultReIncarnationLevel} 時自動重生");
            Console.WriteLine($"日常行為模式 {AppSettings.DefaultAct}");
            Console.WriteLine($"重生時將選擇角色 {AppSettings.DefaultChar}");
            Console.WriteLine($"PVP戰鬥模式為 {AppSettings.DefaultFight}");
            Console.WriteLine($"冷卻時間設定介於 {Const.CheckTime} 秒 ~ {Const.CheckTime + AppSettings.RandTime} 秒");
            Console.WriteLine($"[一般]PVP目前會找高於自身等級 {AppSettings.PvpLevel} 的對手");
            Console.WriteLine($"[其次]PVP目前會集中攻擊 {AppSettings.PvpLevel}");
            Console.WriteLine($"[優先]PVP目前會集中攻擊 Uid= {AppSettings.PvpUid} 的玩家");
            Console.WriteLine("======================================================================");
        }

        public static void Asker()
        {
            string newInput;
            if (string.IsNullOrWhiteSpace(AppSettings.Token) || AppSettings.IsAsk)
            {
                // 更新授權Token
                Console.WriteLine("[必要] 輸入 Token:");                
                while (true)
                {
                    newInput = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(newInput)) AppSettings.Token = newInput;
                    if (!string.IsNullOrWhiteSpace(AppSettings.Token))
                    {
                        Console.WriteLine($"Token 目前設定為 {AppSettings.Token}");
                        break;
                    }

                    Console.WriteLine("[必要] 輸入 token:");
                }
            }
            if (!AppSettings.IsAsk)
            {                
                return;
            }

            // 更新預設角色
            Console.WriteLine($"[選填] 設定自動重生等級,0=不自殺(預設：{AppSettings.DefaultReIncarnationLevel}):");
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput) && int.TryParse(newInput, out var newLevel))
                AppSettings.DefaultReIncarnationLevel = newLevel;
            Console.WriteLine($"等擊達到 {AppSettings.DefaultReIncarnationLevel} 時自動重生");

            // 更新預設動作
            Console.Write($"[選填] 設定主要動作(預設：{AppSettings.DefaultAct}):");
            foreach (var name in Enum.GetNames(typeof(ActionEnum))) Console.Write($" {name} ");
            Console.WriteLine();
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput) && Enum.TryParse(newInput, true, out ActionEnum newActEnum))
                AppSettings.DefaultAct = newActEnum;
            Console.WriteLine($"日常行為模式 {AppSettings.DefaultAct}");

            // 更新預設角色
            Console.Write($"[選填] 設定重生角色(預設：{AppSettings.DefaultChar}):");
            foreach (var name in Enum.GetNames(typeof(CharEnum))) Console.Write($" {name} ");
            Console.WriteLine();
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput) && Enum.TryParse(newInput, true, out CharEnum newCharEnum))
                AppSettings.DefaultChar = newCharEnum;
            Console.WriteLine($"重生時將選擇角色 {AppSettings.DefaultChar}");

            // 更新自動戰鬥
            Console.Write($"[選填] 設定戰鬥模式(預設：{AppSettings.DefaultFight}):");
            foreach (var name in Enum.GetNames(typeof(FightEnum))) Console.Write($" {name} ");
            Console.WriteLine();
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput) && Enum.TryParse(newInput, true, out FightEnum newFightEnum))
                AppSettings.DefaultFight = newFightEnum;
            Console.WriteLine($"PVP戰鬥模式為 {AppSettings.DefaultFight}");

            // 更新浮動CD時間
            Console.WriteLine(
                $"[選填] 設定基礎冷卻時間({Const.CheckTime})的額外浮動秒數上限(預設：{AppSettings.RandTime})，共{Const.CheckTime + AppSettings.RandTime}秒:");
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput) && int.TryParse(newInput, out var newRandTime))
                AppSettings.RandTime = newRandTime;
            Console.WriteLine($"冷卻時間設定介於 {Const.CheckTime} 秒 ~ {Const.CheckTime + AppSettings.RandTime} 秒");

            // 更新PVP對手經驗值增量
            Console.WriteLine($"[選填] 設定PVP對手等級增量(預設：{AppSettings.PvpLevel})");
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput) && int.TryParse(newInput, out var newExpPVP))
                AppSettings.PvpLevel = newExpPVP;
            Console.WriteLine($"PVP目前會找高於自身等級 {AppSettings.PvpLevel} 的對手");

            // 更新PVP對手暱稱
            Console.WriteLine($"[選填] 設定PVP對手暱稱(預設：{AppSettings.PvpNickName})");
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput))
                AppSettings.PvpNickName = newInput;
            Console.WriteLine($"PVP目前會集中攻擊 {AppSettings.PvpLevel}");

            // 更新PVP對手Uid
            Console.WriteLine($"[選填] 設定PVP對手暱稱(預設：{AppSettings.PvpUid})");
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput))
                AppSettings.PvpUid = newInput;
            Console.WriteLine($"PVP目前會集中攻擊 [Uid= {AppSettings.PvpLevel}] 的玩家");
        }
    }
}