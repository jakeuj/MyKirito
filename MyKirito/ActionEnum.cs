using System.ComponentModel;

namespace MyKirito
{
    // 行動列舉值
    public enum ActionEnum
    {
        [Description("不做事")] None = -1,
        [Description("坐下休息")] Sit,
        [Description("做善事")] Good,
        [Description("自主訓練")] Train,
        [Description("外出野餐")] Eat,
        [Description("汁妹")] Girl,
        [Description("狩獵兔肉")] Hunt,
        [Description("釣魚")] Fish
    }

    public enum CharEnum
    {
        [Description("不轉生")] None = -1,
        [Description("桐人")] Kirito,
        [Description("初見泉")] Hatsumi,
        [Description("尤吉歐")] Eugeo,
        [Description("茶渡泰虎")] Sado,
        [Description("克萊因")] Klein,
        [Description("牙王")] Kibaou,
        [Description("日番谷冬獅郎")] Hitsugaya,
        [Description("六車拳西")] Muguruma,
        [Description("超級噴火龍X")] CharizardX,
        [Description("獨行玩家")] Reiner,
        [Description("詩乃")] Sinon,
        [Description("莉茲貝特")] Lisbeth,
        [Description("西莉卡")] Silica,
        [Description("羅莎莉雅")] Rosalia,
        [Description("雛森桃")] Hinamori,
        [Description("藍染惣右介")] Aizen,
        [Description("亞絲娜")] Asuna,
        [Description("天野陽菜")] Hina,
        [Description("艾基爾")] Agil,
        [Description("雜燴兔")] Rabbit,
        [Description("克拉帝爾")] Kuradeel,
        [Description("金·布拉德雷")] Bradley,
        [Description("哥德夫利")] Godfree,
        [Description("努西")] Fish,
        [Description("星爆小拳石")] Geodude,
        [Description("愛麗絲")] Alice,
    }

    // 戰鬥列舉值
    public enum FightEnum
    {
        [Description("不主動出戰")] None = -1,
        [Description("友好切磋")] Friend,
        [Description("認真對決")] Hard,
        [Description("決一死戰")] Duo,
        [Description("我要殺死你")] Kill
    }
}