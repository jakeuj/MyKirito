using System;
using System.Collections.Generic;
using System.Text;

namespace MyKirito
{
    public class AppSettings
    {
        // 額外屬性等級闕值
        public static readonly int[] AddPointLevel = { 15, 20, 23, 25 };

        // 預設動作
        public static ActionEnum _defaultAct = ActionEnum.Girl;

        // 預設Pvp
        public static FightEnum _defaultFight = FightEnum.None;

        // 遊戲檢查額外冷卻時間浮動上限 (預設:100秒)
        public static int _randTime = 100;

        // 預設角色
        public static CharEnum _defaultChar = CharEnum.Kirito;

        //自動投胎等級 (0為不自動投胎，預設:0級)
        public static int _defaultReIncarnationLevel;
    }
}
