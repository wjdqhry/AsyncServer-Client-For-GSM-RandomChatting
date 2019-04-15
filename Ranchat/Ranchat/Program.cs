using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ranchat
{
    static class Program
    {
        public static Socket mainSock;
        public static AsyncObject asyncObject;
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            mainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            asyncObject = new AsyncObject(4096);
            asyncObject.WorkingSocket = mainSock;            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form4());            
        }
        public static void Send(Socket handler, string data)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            Socket handler = ar.AsyncState as Socket;

            try
            {
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
            }
            catch { }
        }
        public static string Connect(string Text)
        {
            Thread.Sleep(50);
            try
            {
                mainSock.Connect(Text, 9555);
                mainSock.BeginReceive(asyncObject.Buffer, 0, 4096, 0, DataReceived, asyncObject);
            }catch(Exception e)
            {
                return e.ToString();
            }
            return "성공";
        }

        public static void DataReceived(IAsyncResult ar)
        {
            // BeginReceive에서 추가적으로 넘어온 데이터를 AsyncObject 형식으로 변환한다.
            AsyncObject obj = (AsyncObject)ar.AsyncState;
            try
            {
                int received = obj.WorkingSocket.EndReceive(ar);

                // UTF8 인코더를 사용하여 바이트 배열을 문자열로 변환한다.
                string getstring = Encoding.UTF8.GetString(obj.Buffer, 0, received);
                Console.WriteLine(getstring);

                msgCheck(getstring);

                obj.ClearBuffer();

                // 수신 대기
                obj.WorkingSocket.BeginReceive(obj.Buffer, 0, 4096, 0, DataReceived, obj);
            }
            catch { }
            // 데이터 수신을 끝낸다.
        }
        public static void msgCheck(string getstring)
        {
            if (!Class1.connect)
            {
                if (getstring == "nameno")
                {
                    MessageBox.Show("이름이 일치하지 않습니다.");
                }
                else if (getstring == "hakbunno")
                {
                    MessageBox.Show("학번이 일치하지 않습니다.");
                }
                else if (getstring == "logined")
                {
                    MessageBox.Show("이미 로그인된 아이디!");
                }
                else if (getstring == "cancled")
                {
                    Class1.searching = false;
                }
                if (getstring.Split(':')[0] == "yes")
                {
                    string[] data = getstring.Split(':');
                    Class1.nickname = data[1];

                    Class1.form1_on = false;
                    Class1.change = true;                    
                }
                else if (getstring.Split(':')[0] == "connected")
                {
                    string[] data = getstring.Split(':');
                    Class1.nickname_ = data[1];

                    Class1.connect = true;
                }
                else if (getstring.Split(':')[0] == "num")
                {
                    string[] data = getstring.Split(':');
                    Class1.num = data[1];
                }
            }
            else
            {
                if (getstring == "!@#$exited")
                {
                    Class1.endConnect = true;
                }
                else if (getstring.Split(':')[0] == "num")
                {
                    string[] data = getstring.Split(':');
                    Class1.num = data[1];
                }
                else
                {
                    Form3.msg = getstring;
                }
            }
        }
    }
    public class AsyncObject
    {
        public byte[] Buffer;
        public Socket WorkingSocket;
        public readonly int BufferSize;
        public AsyncObject(int bufferSize)
        {
            BufferSize = bufferSize;
            Buffer = new byte[BufferSize];
        }

        public void ClearBuffer()
        {
            Array.Clear(Buffer, 0, BufferSize);
        }
    }
}
