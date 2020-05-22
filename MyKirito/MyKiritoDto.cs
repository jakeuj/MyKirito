using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MyKirito
{
    // 角色資料傳輸物件
    public class MyKirito
    {
        [JsonPropertyName("empty")] public bool Empty { get; set; }

        [JsonPropertyName("_id")] public string Id { get; set; }

        [JsonPropertyName("actionCount")] public long ActionCount { get; set; }

        [JsonPropertyName("agi")] public long Agi { get; set; }

        [JsonPropertyName("atk")] public long Atk { get; set; }

        [JsonPropertyName("challengeCount")] public long ChallengeCount { get; set; }

        [JsonPropertyName("character")] public string Character { get; set; }

        [JsonPropertyName("color")] public string Color { get; set; }

        [JsonPropertyName("dead")] public bool Dead { get; set; }

        [JsonPropertyName("def")] public long Def { get; set; }

        [JsonPropertyName("defDeath")] public long DefDeath { get; set; }

        [JsonPropertyName("defKill")] public long DefKill { get; set; }

        [JsonPropertyName("exp")] public long Exp { get; set; }

        [JsonPropertyName("floor")] public long Floor { get; set; }

        [JsonPropertyName("hp")] public long Hp { get; set; }

        [JsonPropertyName("int")] public long Int { get; set; }

        [JsonPropertyName("kill")] public long Kill { get; set; }

        [JsonPropertyName("lastAction")] public long LastAction { get; set; }

        [JsonPropertyName("lastBossChallenge")]
        public long LastBossChallenge { get; set; }

        [JsonPropertyName("lastChallenge")] public long LastChallenge { get; set; }

        [JsonPropertyName("lastStatus")] public long LastStatus { get; set; }

        [JsonPropertyName("lck")] public long Lck { get; set; }

        [JsonPropertyName("lose")] public long Lose { get; set; }

        [JsonPropertyName("lv")] public long Lv { get; set; }

        [JsonPropertyName("nickname")] public string Nickname { get; set; }

        [JsonPropertyName("rattrs")] public Rattrs Rattrs { get; set; }

        [JsonPropertyName("reincarnation")] public long Reincarnation { get; set; }

        [JsonPropertyName("reset")] public long Reset { get; set; }

        [JsonPropertyName("resurrect")] public long Resurrect { get; set; }

        [JsonPropertyName("spd")] public long Spd { get; set; }

        [JsonPropertyName("status")] public string Status { get; set; }

        [JsonPropertyName("stm")] public long Stm { get; set; }

        [JsonPropertyName("tec")] public long Tec { get; set; }

        [JsonPropertyName("title")] public string Title { get; set; }

        [JsonPropertyName("totalDeath")] public long TotalDeath { get; set; }

        [JsonPropertyName("totalDefKill")] public long TotalDefKill { get; set; }

        [JsonPropertyName("totalKill")] public long TotalKill { get; set; }

        [JsonPropertyName("totalLose")] public long TotalLose { get; set; }

        [JsonPropertyName("totalWin")] public long TotalWin { get; set; }

        [JsonPropertyName("win")] public long Win { get; set; }

        [JsonPropertyName("avatar")] public string Avatar { get; set; }
    }

    public class ReIncarnationInput
    {
        [JsonPropertyName("character")] public string Character { get; set; }

        [JsonPropertyName("rattrs")] public Rattrs Rattrs { get; set; }

        [JsonPropertyName("useReset")] public bool UseReset { get; set; }
    }

    public class Rattrs
    {
        [JsonPropertyName("hp")] public long Hp { get; set; }

        [JsonPropertyName("atk")] public long Atk { get; set; }

        [JsonPropertyName("def")] public long Def { get; set; }

        [JsonPropertyName("stm")] public long Stm { get; set; }

        [JsonPropertyName("agi")] public long Agi { get; set; }

        [JsonPropertyName("spd")] public long Spd { get; set; }

        [JsonPropertyName("tec")] public long Tec { get; set; }

        [JsonPropertyName("int")] public long Int { get; set; }

        [JsonPropertyName("lck")] public long Lck { get; set; }
    }

    public class ChallengeInput
    {
        [JsonPropertyName("type")] public long Type { get; set; }

        [JsonPropertyName("opponentUID")] public string OpponentUid { get; set; }

        [JsonPropertyName("shout")] public string Shout { get; set; }

        [JsonPropertyName("lv")] public long Lv { get; set; }
    }

    public class ErrorOutput
    {
        [JsonPropertyName("error")] public string Error { get; set; }
    }

    public class ActionInput
    {
        [JsonPropertyName("action")] public string Action { get; set; }
    }

    public class ActionOutput
    {
        public string Message { get; set; }
        public MyKirito MyKirito { get; set; }
        public Gained Gained { get; set; }
    }

    public class UserListDto
    {
        public List<UserList> UserList { get; set; }
    }

    public class UserList
    {
        public string Nickname { get; set; }
        public long Lv { get; set; }
        public string Title { get; set; }
        public long Floor { get; set; }
        public string Status { get; set; }
        public string Color { get; set; }
        public string Uid { get; set; }
        public string Avatar { get; set; }
        public string Character { get; set; }
    }

    public class BattleLog
    {
        [JsonPropertyName("result")] public string Result { get; set; }

        [JsonPropertyName("messages")] public Message[] Messages { get; set; }

        [JsonPropertyName("myKirito")] public MyKirito MyKirito { get; set; }

        [JsonPropertyName("gained")] public Gained Gained { get; set; }
    }

    public class Gained
    {
        [JsonPropertyName("hp")] public object Hp { get; set; }

        [JsonPropertyName("atk")] public object Atk { get; set; }

        [JsonPropertyName("def")] public object Def { get; set; }

        [JsonPropertyName("stm")] public object Stm { get; set; }

        [JsonPropertyName("agi")] public object Agi { get; set; }

        [JsonPropertyName("spd")] public object Spd { get; set; }

        [JsonPropertyName("tec")] public object Tec { get; set; }

        [JsonPropertyName("int")] public object Int { get; set; }

        [JsonPropertyName("lck")] public object Lck { get; set; }

        [JsonPropertyName("prevLV")] public long PrevLv { get; set; }

        [JsonPropertyName("nextLV")] public long NextLv { get; set; }

        [JsonPropertyName("exp")] public long Exp { get; set; }

        [JsonPropertyName("prevTitle")] public string PrevTitle { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("m")] public string M { get; set; }

        public string S { get; set; }
    }


    public class ProfileDto
    {
        public Profile profile { get; set; }
    }

    public class Profile
    {
        public string _id { get; set; }
        public int actionCount { get; set; }
        public int agi { get; set; }
        public int atk { get; set; }
        public int challengeCount { get; set; }
        public string color { get; set; }
        public bool dead { get; set; }
        public int def { get; set; }
        public int defDeath { get; set; }
        public int defKill { get; set; }
        public int floor { get; set; }
        public int hp { get; set; }
        public int _int { get; set; }
        public int kill { get; set; }
        public long lastChallenge { get; set; }
        public long lastStatus { get; set; }
        public int lck { get; set; }
        public int lose { get; set; }
        public int lv { get; set; }
        public string nickname { get; set; }
        public Rattrs rattrs { get; set; }
        public int reincarnation { get; set; }
        public int reset { get; set; }
        public int resurrect { get; set; }
        public int spd { get; set; }
        public string status { get; set; }
        public int stm { get; set; }
        public int tec { get; set; }
        public string title { get; set; }
        public int totalDeath { get; set; }
        public int totalDefKill { get; set; }
        public int totalKill { get; set; }
        public int totalLose { get; set; }
        public int totalWin { get; set; }
        public Unlockedcharacter[] unlockedCharacters { get; set; }
        public int win { get; set; }
        public string character { get; set; }
        public string avatar { get; set; }
    }

    public class Unlockedcharacter
    {
        public string character { get; set; }
        public string name { get; set; }
        public string avatar { get; set; }
    }
}