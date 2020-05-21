using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyKirito
{
    public class InitializationService: IInitializationService
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _hostEnvironment;

        public InitializationService(IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
        }

        private async Task ReadJson(string path, string name)
        {
            try
            {
                // Write the specified text asynchronously to a new file named "WriteTextAsync.txt".
                string output;
                using (StreamReader outputFile = new StreamReader(Path.Combine(path, name)))
                {
                    output = await outputFile.ReadToEndAsync();
                }
                if(output.Length>0)
                    Global.GameOptions = output.ReadAsJsonAsync<GameOptions>(false);
                Console.WriteLine($"載入位於 {path} 的設定檔 {name} 完成");
            }
            catch (Exception e)
            {
                Console.WriteLine($"載入位於 {path} 的設定檔 {name} 失敗");
                Console.WriteLine(path);
                Console.WriteLine(e.Message);
            }
        }

        public async Task Initialization()
        {
            // 接收Path參數
            if (GetParam("Path", out var path)) Global.JsonPath = path; else Global.JsonPath = _hostEnvironment.ContentRootPath;
            await ReadJson(Global.JsonPath, Global.JsonFileName);
            //Global.GameOptions = _configuration.GetSection("gameOptions").Get<GameOptions>();
            Init();
        }
        public bool GetParam(string input,out string output)
        {
            output = _configuration.GetSection(input).Value;
            return !string.IsNullOrEmpty(output);
        }
        public void Init()
        {            
            string output;
            // 接收Token參數
            if (GetParam("Token",out output)) Global.GameOptions.Token = output;
            // 接收對手參數
            if (GetParam("TargetUid", out output)) Global.GameOptions.PvpUid = output;
            // 接收行為參數
            if (GetParam("Action", out output) && Enum.TryParse(output, true, out ActionEnum newActEnum)) Global.GameOptions.DefaultAct = newActEnum;
            // 接收角色參數
            if (GetParam("Char", out output) && Enum.TryParse(output, true, out CharEnum newCharEnum)) Global.GameOptions.DefaultChar = newCharEnum;
            // 接收戰鬥參數
            if (GetParam("PVP", out output) && Enum.TryParse(output, true, out FightEnum newFightEnum)) Global.GameOptions.DefaultFight = newFightEnum;
            // 接收安靜參數
            if (GetParam("Quiet", out output) && int.TryParse(output,out var isQuiet) && isQuiet > 0) Global.GameOptions.IsAsk = false;
            Asker();
            Console.WriteLine("======================================================================");
            Console.WriteLine("[提示] 具名參數或appsettings.json設定參數請看ReadMe.txt");
            if (!Global.GameOptions.IsAsk)
                Console.WriteLine($"目前模式設定為安靜模式(不詢問設定值)");
            else
                Console.WriteLine($"目前模式設定為一般模式(會問設定值)");
            Console.WriteLine("Token 目前設定為：");
            Console.WriteLine(Global.GameOptions.Token);
            Console.WriteLine($"等擊達到 {Global.GameOptions.DefaultReIncarnationLevel} 時自動重生");
            Console.WriteLine($"日常行為模式 {Global.GameOptions.DefaultAct.GetDescriptionText()}");
            Console.WriteLine($"重生時將選擇角色 {Global.GameOptions.DefaultChar.GetDescriptionText()}");
            Console.WriteLine($"PVP戰鬥模式為 {Global.GameOptions.DefaultFight.GetDescriptionText()}");
            Console.WriteLine($"冷卻時間設定介於 {Global.CheckTime} 秒 ~ {Global.CheckTime + Global.GameOptions.RandTime} 秒");
            if(Global.GameOptions.DefaultFight != FightEnum.None)
            {
                if (!string.IsNullOrWhiteSpace(Global.GameOptions.PvpUid))
                    Console.WriteLine($"PVP目前會優先攻擊 Uid= {Global.GameOptions.PvpUid} 的玩家");
                else if(!string.IsNullOrWhiteSpace(Global.GameOptions.PvpNickName))
                    Console.WriteLine($"PVP目前會集中攻擊 {Global.GameOptions.PvpNickName}");
                if (string.IsNullOrWhiteSpace(Global.GameOptions.PvpUid) && string.IsNullOrWhiteSpace(Global.GameOptions.PvpNickName) && Global.GameOptions.MustIsModeEnable)
                {
                    if (Global.GameOptions.MustIsModeIgnore)
                        Console.WriteLine($"PVP目前會從最高等級對手開始進行地毯式無差別攻擊");
                    else if (Global.GameOptions.MustIsCharacterPVP != null && Global.GameOptions.MustIsCharacterPVP.Any())
                        Console.WriteLine($"PVP目前會從最高等級對手開始針對特定角色攻擊 " + string.Join(" ", Global.GameOptions.MustIsCharacterPVP));
                }
                Console.WriteLine($"[一般]PVP目前會找高於自身等級 {Global.GameOptions.PvpLevel} 的對手");
            }            
            Console.WriteLine("======================================================================");
        }

        public void Asker()
        {
            string newInput;
            if (string.IsNullOrWhiteSpace(Global.GameOptions.Token) || Global.GameOptions.IsAsk)
            {
                // 更新授權Token
                Console.WriteLine("[必要] 輸入 Token:");                
                while (true)
                {
                    newInput = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(newInput)) Global.GameOptions.Token = newInput;
                    if (!string.IsNullOrWhiteSpace(Global.GameOptions.Token))
                    {
                        Console.WriteLine($"Token 目前設定為 {Global.GameOptions.Token}");
                        Console.WriteLine("======================================================================");
                        break;
                    }

                    Console.WriteLine("[必要] 輸入 token:");
                }
            }
            if (!Global.GameOptions.IsAsk)
            {                
                return;
            }

            // 更新預設角色
            Console.WriteLine($"[選填] 設定自動重生等級,0=不自殺(預設：{Global.GameOptions.DefaultReIncarnationLevel}):");
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput) && int.TryParse(newInput, out var newLevel))
                Global.GameOptions.DefaultReIncarnationLevel = newLevel;
            Console.WriteLine($"等擊達到 {Global.GameOptions.DefaultReIncarnationLevel} 時自動重生");
            Console.WriteLine("======================================================================");
            // 更新預設動作
            Console.Write($"[選填] 設定主要行動(預設：{Global.GameOptions.DefaultAct.GetDescriptionText()}):");
            foreach(ActionEnum enumType in Enum.GetValues(typeof(ActionEnum)))
            {
                Console.WriteLine($"{enumType.GetDescriptionText()}：{enumType}");
            }
            Console.WriteLine("請輸入行動英文代號：");
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput) && Enum.TryParse(newInput, true, out ActionEnum newActEnum))
                Global.GameOptions.DefaultAct = newActEnum;
            Console.WriteLine($"主要行動模式 {Global.GameOptions.DefaultAct.GetDescriptionText()}");
            Console.WriteLine("======================================================================");
            // 更新預設角色
            Console.Write($"[選填] 設定重生角色(預設：{Global.GameOptions.DefaultChar.GetDescriptionText()}):");
            foreach (CharEnum enumType in Enum.GetValues(typeof(CharEnum)))
            {
                Console.WriteLine($"{enumType.GetDescriptionText()}：{enumType}");
            }
            Console.WriteLine("請輸入角色英文代號：");
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput) && Enum.TryParse(newInput, true, out CharEnum newCharEnum))
                Global.GameOptions.DefaultChar = newCharEnum;
            Console.WriteLine($"重生時將選擇角色 {Global.GameOptions.DefaultChar.GetDescriptionText()}");
            Console.WriteLine("======================================================================");
            // 更新自動戰鬥
            Console.Write($"[選填] 設定戰鬥模式(預設：{Global.GameOptions.DefaultFight.GetDescriptionText()}):");
            foreach (FightEnum enumType in Enum.GetValues(typeof(FightEnum)))
            {
                Console.WriteLine($"{enumType.GetDescriptionText()}：{enumType}");
            }
            Console.WriteLine("請輸入戰鬥英文代號：");
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput) && Enum.TryParse(newInput, true, out FightEnum newFightEnum))
                Global.GameOptions.DefaultFight = newFightEnum;
            Console.WriteLine($"PVP戰鬥模式為 {Global.GameOptions.DefaultFight.GetDescriptionText()}");
            Console.WriteLine("======================================================================");
            // 更新浮動CD時間
            Console.WriteLine(
                $"[選填] 設定基礎冷卻時間({Global.CheckTime})的額外浮動秒數上限(預設：{Global.GameOptions.RandTime})，共{Global.CheckTime + Global.GameOptions.RandTime}秒:");
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput) && int.TryParse(newInput, out var newRandTime))
                Global.GameOptions.RandTime = newRandTime;
            Console.WriteLine($"冷卻時間設定介於 {Global.CheckTime} 秒 ~ {Global.CheckTime + Global.GameOptions.RandTime} 秒");
            Console.WriteLine("======================================================================");
            // 更新PVP對手經驗值增量
            Console.WriteLine($"[選填] 設定PVP對手等級增量(預設：{Global.GameOptions.PvpLevel})");
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput) && int.TryParse(newInput, out var newExpPVP))
                Global.GameOptions.PvpLevel = newExpPVP;
            Console.WriteLine($"PVP目前會找高於自身等級 {Global.GameOptions.PvpLevel} 的對手");
            Console.WriteLine("======================================================================");
            // 更新PVP對手暱稱
            Console.WriteLine($"[選填] 設定PVP對手暱稱(預設：{Global.GameOptions.PvpNickName})");
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput))
                Global.GameOptions.PvpNickName = newInput;
            Console.WriteLine($"PVP目前會集中攻擊 {Global.GameOptions.PvpLevel}");
            Console.WriteLine("======================================================================");
            // 更新PVP對手Uid
            Console.WriteLine($"[選填] 設定PVP對手暱稱(預設：{Global.GameOptions.PvpUid})");
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput))
                Global.GameOptions.PvpUid = newInput;
            Console.WriteLine($"PVP目前會集中攻擊 [Uid= {Global.GameOptions.PvpLevel}] 的玩家");
        }        
    }
}
