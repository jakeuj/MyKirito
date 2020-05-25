using System.Collections.Generic;

namespace MyKirito
{
    public class GameOptions
    {
        // 詢問設定開關
        public bool IsAsk { get; set; } = true;

        // Token
        public string Token { get; set; }

        // 額外屬性等級闕值
        public int[] AddPointLevel { get; set; } = {15, 20, 23, 25};

        // 預設動作
        public ActionEnum DefaultAct { get; set; } = ActionEnum.Eat;

        // 預設Pvp
        public FightEnum DefaultFight { get; set; } = FightEnum.Friend;

        // PVP對手經驗值增量
        public int PvpLevel { get; set; }

        // PVP對手暱稱
        public string PvpNickName { get; set; }

        // PVP對手UID
        public string PvpUid { get; set; }

        public string[] ColorPVP { get; set; } = {"red", "orange"};

        public List<string> CharacterPVP { get; set; } = new List<string> {"莉茲貝特"};
        public List<string> NotWantCharacterPVP { get; set; } = new List<string> {"希茲克利夫","尤吉歐"};

        // 遊戲檢查額外冷卻時間浮動上限 (預設:10秒)
        public int RandTime { get; set; }

        // 預設角色
        public CharEnum DefaultChar { get; set; } = CharEnum.Kirito;

        //自動投胎等級 (0為不自動投胎，預設:0級)
        public int DefaultReIncarnationLevel { get; set; }

        //打不贏的人
        public HashSet<string> LostUidListPVP { get; set; } = new HashSet<string> {"5ec3529f431b010405263676"};

        public bool MustIsModeEnable { get; set; }

        public bool MustIsModeIgnore { get; set; }
        public List<string> MustIsCharacterPVP { get; set; } = new List<string> {"莉茲貝特"};

        //目前正在打的人
        public UserList CurrentPvpUser { get; set; }

        //目前打到第幾頁
        public int CurrentPage { get; set; } = 1;

        //目前在打的等級
        public long CurrentSearchLv { get; set; } = 999;

        public bool FloorBonusEnable { get; set; } = true;
    }
}