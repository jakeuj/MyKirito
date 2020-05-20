[提示] 執行時可以給具名參數設定啟動參數
[格式] dotnet MyKirito.dll {參數名稱1=值} {參數名稱2=值}
[參數]  權杖：Token 
        安靜：Quiet (不詢問參數)
        對手識別碼：TargetUid 
        戰鬥：PVP (-1=不主動出戰,0=友好切磋,1.認真對決,2.決一死戰,3.我要殺死你)
        行為：Action (-1不動作,0=坐下休息,1.做善事,2.自主訓練,3.外出野餐,4.汁妹,5.狩獵兔肉,6.釣魚)
        角色：Char (-1=不重生,0之後會復活，清單參照最下方角色Id對照表)
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
[提示] 可以到gamesettings.json裡面設定啟動參數
[範例] JSON設定
```
{
  //詢問設定開關：不想再被問問題改true
  "isAsk": false,
  //[必填]認證 Token
  "token": "",
  //獲得點數的門檻
  "addPointLevel": [
    15,
    20,
    23,
    25
  ],
  //0=坐下休息,做善事,自主訓練,外出野餐,汁妹,狩獵兔肉,釣魚
  "defaultAct": 4,
  //-1=不主動出戰,友好切磋,認真對決,決一死戰,我要殺死你
  "defaultFight": 0,
  //PVP搜尋等級相對自身增加量
  "pvpLevel": 1,
  //PVP指名
  "pvpNickName": null,
  //PVP指定Id
  "pvpUid": "",
  //PVP優先打的顏色
  "colorPVP": [
    "red",
    "orange"
  ],
  //[測試中]PVP強制搜尋
  "mustIsCharacterPVP": [
    "莉茲貝特"
  ],
  //PVP高優先角色
  "characterPVP": [
    "莉茲貝特",
  ],
  //PVP低優先角色
  "notWantCharacterPVP": [
    "希茲克利夫",
  ],
  //額外冷卻秒數
  "randTime": 0,
  //重生角色(CharEnum)：0=桐人
  "defaultChar": 0,
  //自動重生等級,0=不自殺
  "defaultReIncarnationLevel": 0,
  //強制搜尋排除對手Uid
  "lostUidListPVP": [
    "5ec3529f431b010405263676"
  ],
  //[測試中]強制搜尋用
  "currentPvpUser": null,
  "currentPage": 1,
  "currentSearchLv": 999
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