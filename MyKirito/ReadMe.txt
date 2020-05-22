======================================================================================
[提示] 升級時會自動保存升級點數資訊到 lv.csv
[格式] Action,Win,HP,攻擊,防禦,體力,敏捷,反應速度,技巧,智力,幸運,原等級,下一個等級,經驗值,原稱號
[參數]  Action：-1=PVP,0=坐下休息,1.做善事,2.自主訓練,3.外出野餐,4.汁妹,5.狩獵兔肉,6.釣魚
        Win：True=贏，False=輸 (Action 非 PVP 時恆 False)
======================================================================================
[提示] 執行時可以給具名參數設定啟動參數
[格式] dotnet MyKirito.dll {參數名稱1=值} {參數名稱2=值}
[參數]  權杖：Token 
        安靜：Quiet (不詢問參數)
        對手識別碼：TargetUid 
        戰鬥：PVP (-1=不主動出戰,0=友好切磋,1.認真對決,2.決一死戰,3.我要殺死你)
        行為：Action (-1不動作,0=坐下休息,1.做善事,2.自主訓練,3.外出野餐,4.汁妹,5.狩獵兔肉,6.釣魚)
        角色：Char (-1=不復活,0以上會自動復活全點智力，清單參照最下方角色Id對照表)
        路徑：Path (Json存放位置)
[範例] MyKirito.exe Token=ABC123.456 TargetUid=a1b2c3d4 PVP=0 Action=0 Char=2 Quiet=1 Path=D:\Asuna1
======================================================================================
[提示] 多帳號區分各自Json設定檔
[範例] 1.直接執行：複製多份主程式, 2.批次檔：加上Path參數
======================================================================================
[提示] 如果是批次檔或命令列執行請指定Path參數，否則Json路徑會抓不到
[範例] dotnet D:\MyKirito\MyKirito.dll Path=D:\Asuna
[提示] 多開帳號可以設定不同Path分別儲存各帳號Json
[範例] SrartAccount1.bat 開 Asuna1 帳號, SrartAccount2.bat 開 Asuna2 帳號
SrartAccount1.bat
```
dotnet D:\MyKirito\MyKirito.dll Path=D:\Asuna1
```
SrartAccount2.bat
```
dotnet D:\MyKirito\MyKirito.dll Path=D:\Asuna2
```
======================================================================================
[提示] 批次檔如未指定Path 需要先切到目標磁碟 再切到目標目錄 最後再執行程式
[範例] Start.bat
```
D:
cd \Asuna\MyKiritoV2
dotnet MyKirito.dll Token=ABC123.456
```
======================================================================================
[提示] 批次檔利上上面問題可以達到同一份執行檔分別儲存各帳號Json
[範例] 主程式放 D:\MyKirito , Json分別存放 D:\Asuna1 與 D:\Asuna2
SrartAccount1.bat
```
D:
cd \Asuna1
dotnet \MyKirito\MyKirito.dll Token=ABC123.456
```
SrartAccount2.bat
```
D:
cd \Asuna2
dotnet \MyKirito\MyKirito.dll Token=ABC123.456
```
======================================================================================
[提示] 首次執行並關閉後會自動生成gamesettings.json
[範例] JSON設定 (僅供參考)
```
{
  //======================================================================基本設定
  //[必填]認證 Token
  "token": "",
  //詢問設定開關：都設定好以後不想再被問問題改true
  "isAsk": true,    
  //自動行為：Action (-1不動作,0=坐下休息,1.做善事,2.自主訓練,3.外出野餐,4.汁妹,5.狩獵兔肉,6.釣魚)
  "defaultAct": 4,    
  //自動重生角色：Char (-1=不復活,0以上會自動復活全點智力，清單參照最下方角色Id對照表)
  "defaultChar": 0,
  //自動重生等級,0=不自殺
  "defaultReIncarnationLevel": 0,
  //======================================================================PVP一般搜尋
  //自動戰鬥：PVP (-1=不主動出戰,0=友好切磋,1.認真對決,2.決一死戰,3.我要殺死你)
  "defaultFight": 0,
  //PVP搜尋高於自己多少等級的對手
  "pvpLevel": 1,
  //PVP指定特定玩家暱稱
  "pvpNickName": "",
  //PVP指定特定玩家Id
  "pvpUid": "",
  //PVP優先打的顏色
  "colorPVP": [
    "red",
    "orange"
  ],  
  //PVP優先打特定角色
  "characterPVP": [
    "莉茲貝特",
  ],
  //PVP最後打特定角色
  "notWantCharacterPVP": [
    "希茲克利夫",
  ],
  //======================================================================PVP進階搜尋
  //PVP進階搜尋-總開關 (從第一名(或從currentSearchLv指定等級)開始打，直到都打不贏比自己等級高的對手，等級夠高再開)
  "MustIsModeEnable": true,
  //PVP進階搜尋-忽略角色篩選 (mustIsCharacterPVP)
  "MustIsModeIgnore": true,  
  //PVP進階模式-目前打的等級 (等級低於自己會停止，需要重置時手動改你要打等級，999從第一名開始打)
  "currentSearchLv": 999,
  //PVP進階模式-目前打的頁數 (跟上方一起使用，需要重置時手動改回1)
  "currentPage": 1,
  //PVP進階模式-目前正在打的人 (不需設定，打贏會自動存)
  "currentPvpUser": null,
  //PVP強制搜尋-角色篩選條件 (不填或MustIsModeIgnore=true就全打)
  "mustIsCharacterPVP": [
    "莉茲貝特"
  ],
  //PVP強制搜尋-排除特定對手 (打輸時會自動新增，需要重置時自己刪除)
  "lostUidListPVP": [
    "5ec3529f431b010405263676"
  ],
  //======================================================================遊戲設定
  //每次行動的額外冷卻秒數 (0=固定冷卻100秒,100=冷卻100~200秒)
  "randTime": 0,
  //獲得點數的闕值
  "addPointLevel": [
    15,
    20,
    23,
    25
  ],
}
```
======================================================================================
// 角色Id對照
0	[Description("桐人")] Kirito,
1	[Description("初見泉")] Hatsumi,
2	[Description("尤吉歐")] Eugeo,
3	[Description("茶渡泰虎")] Sado,
4	[Description("克萊因")] Klein,
5	[Description("牙王")] Kibaou,
6	[Description("日番谷冬獅郎")] Hitsugaya,
7	[Description("六車拳西")] Muguruma,
8	[Description("超級噴火龍X")] CharizardX,
9	[Description("獨行玩家")] Reiner,
10	[Description("詩乃")] Sinon,
11	[Description("莉茲貝特")] Lisbeth,
12	[Description("西莉卡")] Silica,
13	[Description("羅莎莉雅")] Rosalia,
14	[Description("雛森桃")] Hinamori,
15	[Description("藍染惣右介")] Aizen,
16	[Description("亞絲娜")] Asuna,
17	[Description("天野陽菜")] Hina,
18	[Description("艾基爾")] Agil,
19	[Description("雜燴兔")] Rabbit,
20	[Description("克拉帝爾")] Kuradeel,
21	[Description("金·布拉德雷")] Bradley,
22	[Description("哥德夫利")] Godfree,
23	[Description("努西")] Fish,
24	[Description("星爆小拳石")] Geodude,
25	[Description("愛麗絲")] Alice,