using System;
using System.Collections.Generic;
using System.Text;

namespace MyKirito
{
    // 角色資料傳輸物件
    public class MyKiritoDto
    {
        public string NickName { get; set; }
        public int Lv { get; set; }
        public int Exp { get; set; }
        public bool Dead { get; set; }
    }
}
