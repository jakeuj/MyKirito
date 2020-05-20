using System;
using System.Collections.Generic;

namespace MyKirito
{
    public class Global
    {
        // 行動版本號
        public const int DoActionVersion = 2;

        // 遊戲Pvp檢查時間闕值 (預設:410秒)
        public const int PvpTime = 400;

        // 遊戲檢查基本冷卻時間 (預設:100秒)
        public const int CheckTime = 100;

        public static GameOptions GameOptions { get; set; } = new GameOptions();
        public static MyKirito MyKiritoDto { get; set; } = null;

        public static DateTime NextPvpTime = DateTime.Now.AddSeconds(PvpTime);

        public static DateTime NextActionTime = DateTime.Now;

        public static string JsonPath { get; set; }

        public const string JsonFileName = "gamesttings.json";
    }
}