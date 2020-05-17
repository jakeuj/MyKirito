using System.ComponentModel;

namespace MyKirito
{
    // 行動列舉值
    public enum ActionEnum
    {
        [Description("坐下休息")] Sit,
        [Description("做善事")] Good,
        [Description("自主訓練")] Train,
        [Description("外出野餐")] Eat,
        [Description("汁妹")] Girl,
        [Description("狩獵兔肉")] Hunt
    }

    public enum CharEnum
    {
        Kirito,
        Hatsumi,
        Eugeo,
        Sado,
        Klein,
        Kibaou,
        Hitsugaya,
        Muguruma,
        CharizardX,
        Reiner,
        Sinon,
        Lisbeth,
        Silica,
        Rosalia,
        Hinamori,
        Aizen,
        Asuna,
        Hina,
        Agil,
        Rabbit,
        Kuradeel,
        Bradley
    }

    // 戰鬥列舉值
    public enum FightEnum
    {
        [Description("深蹲中...")] None = -1,
        [Description("友好切磋")] Friend,
        [Description("認真對決")] Hard,
        [Description("決一死戰")] Duo,
        [Description("我要殺死你")] Kill
    }
}