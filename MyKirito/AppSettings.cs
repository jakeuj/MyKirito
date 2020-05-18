namespace MyKirito
{
    public class AppSettings
    {
        // 詢問設定開關
        public static bool IsAsk = true;

        // Token
        public static string Token = string.Empty;

        // 額外屬性等級闕值
        public static readonly int[] AddPointLevel = {15, 20, 23, 25};

        // 預設動作
        public static ActionEnum DefaultAct = ActionEnum.Girl;

        // 預設Pvp
        public static FightEnum DefaultFight = FightEnum.None;

        // PVP對手經驗值增量
        public static int PvpLevel = 1;

        // PVP對手暱稱
        public static string PvpNickName;

        // 遊戲檢查額外冷卻時間浮動上限 (預設:10秒)
        public static int RandTime = 10;

        // 預設角色
        public static CharEnum DefaultChar = CharEnum.Kirito;

        //自動投胎等級 (0為不自動投胎，預設:0級)
        public static int DefaultReIncarnationLevel;

        public static MyKirito MyKiritoDto { get; set; } = null;
    }
}