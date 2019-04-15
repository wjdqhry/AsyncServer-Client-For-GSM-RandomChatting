using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ranchat
{
    public class Class1
    {
        public static string Address = string.Empty;
        public static Point location = new Point();

        public static bool form1_on = true;
        public static bool endConnect = false;
        public static bool go_Form3 = false;

        public static bool change = false;

        public static string nickname;  //본인 닉네임
        public static bool searching = false; //상대를 찾는 중인지
        public static string num; //접속인원 받아오기

        public static string nickname_; //상대방 닉네임
        public static bool connect = false; //상대방과 연결 상태
    }
}
