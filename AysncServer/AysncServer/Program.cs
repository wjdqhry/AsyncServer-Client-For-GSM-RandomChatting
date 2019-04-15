using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AysncServer
{
    public class Program
    { 
        private static List<AsyncObject> connectedClients = new List<AsyncObject>();
        private static List<AsyncObject> chattingClients = new List<AsyncObject>();
        private static Socket mainSock;
        private static MySqlConnection conn;
        private static DataSet data;
        private static int num = 0;

        static void Main(string[] args)                   
        {
            Console.WriteLine("서버 시작");
            dbConnect(); //데이터베이스 연결

            show_Mebers(); //접속인원 띄우기

            get_data(); //db에서 먼저 회원들의 값을 받아온다

            random_connect(); //랜덤 연결해주는 스레드 시작

            ServerOpen(); //서버를 열고 비동기로 클라이언트의 신호를 받기 시작
        }


        private static void show_Mebers()
        {
            new Thread(() =>
            {
                while (true)
                {
                    Console.WriteLine("현재 접속 인원: " + connectedClients.Count);
                    Thread.Sleep(5000);
                }
            }).Start();
        }
        private static void ServerOpen()
        {
            mainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9555);
            mainSock.Bind(ipep);
            mainSock.Listen(10);
            mainSock.BeginAccept(AcceptCallback, null);
        }

        private static void get_data()
        {
            data = new DataSet();
            string sql = "select * from gsm";
            MySqlDataAdapter adpt = new MySqlDataAdapter(sql, conn);
            adpt.Fill(data, "gsm");
        }

        private static void random_connect()
        {
            new Thread(() => //채팅하기 원하는 사람들이 2 이상일 때 2명을 이어주는 쓰레드
            {
                while (true)
                {
                    if (chattingClients.Count >= 2)
                    {
                        try
                        {
                            AsyncObject obj1 = chattingClients[0];
                            AsyncObject obj2 = chattingClients[1];
                            obj1.chatting = true;
                            obj2.chatting = true;
                            obj1.TargetSocket = obj2;
                            obj2.TargetSocket = obj1;
                            

                            Send(obj1.WorkingSocket, "connected:" + obj2.UserName);
                            Send(obj2.WorkingSocket, "connected:" + obj1.UserName);

                            chattingClients.Remove(obj1);
                            chattingClients.Remove(obj2);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
            }).Start();
        }
        private static void dbConnect()
        {
            string connStr = string.Format(@"server=localhost;
                                                  user=root;
                                                  password=1234;
                                                  database=mysql");
            conn = new MySqlConnection(connStr);

            try
            {
                conn.Open();
                Console.WriteLine("MySQL 연결 성공");
            }
            catch
            {
                Console.WriteLine("연결 실패.. 종료");
                Environment.Exit(0);
            }
        }

        static void AcceptCallback(IAsyncResult ar)
        { //클라이언트 요청 수락
            Socket client = mainSock.EndAccept(ar);

            // 클라이언트의 연결 요청을 대기한다. (다른 클라이언트가 또 연결할 수 있으므로)
            mainSock.BeginAccept(AcceptCallback, null);
            AsyncObject obj = new AsyncObject(client);
            connectedClients.Add(obj);
            obj.WorkingSocket.BeginReceive(obj.Buffer, 0, 4096, 0, DataRecieved, obj); 
        }

        private static void DataRecieved(IAsyncResult ar)
        {// BeginReceive에서 추가적으로 넘어온 데이터를 AsyncObject 형식으로 변환한다.
            AsyncObject obj = (AsyncObject)ar.AsyncState;
            try
            {
                int nbyte = obj.WorkingSocket.EndReceive(ar);
                string Text = Encoding.UTF8.GetString(obj.Buffer, 0, nbyte);
                Console.WriteLine(Text);
                msgCheck(obj, Text);
            }
            catch
            {
                connectedClients.Remove(obj);
            }


            obj.ClearBuffer();

            try
            {//접속이 끊길 때
                obj.WorkingSocket.BeginReceive(obj.Buffer, 0, 4096, 0, DataRecieved, obj);
            }catch
            {
                
            }
        }
        private static void Send(Socket handler, string data)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            Socket handler = ar.AsyncState as Socket;

            int bytesSent = handler.EndSend(ar);
            Console.WriteLine("Sent {0} bytes to client.", bytesSent);    
        }

        public static void msgCheck(AsyncObject obj, string Text)
        {
            if (obj.TargetSocket != null)
            {
                if (Text.StartsWith("./"))
                {
                    string newTxt = Text.Substring(2, Text.Length - 2);
                    if (newTxt == "exit")
                    {
                        Send(obj.TargetSocket.WorkingSocket, "!@#$exited");
                        obj.TargetSocket.chatting = false;
                        obj.TargetSocket.TargetSocket = null;
                        obj.TargetSocket = null;
                        obj.chatting = false;
                    }
                    else if (newTxt == "num")
                    {
                        Send(obj.WorkingSocket, "num:" + num.ToString());
                    }
                }
                else
                {
                    Send(obj.TargetSocket.WorkingSocket, Text);
                }
            }
            else if (Text.StartsWith("./"))
            {
                string newTxt = Text.Substring(2, Text.Length - 2);
                if (newTxt == "disconnect")
                {                  
                    connectedClients.Remove(obj);
                    obj.WorkingSocket.Close();
                }
                else if (newTxt == "num")
                {
                    Send(obj.WorkingSocket, "num:" + num.ToString());
                }
                else if (newTxt == "cancle")
                {
                    Send(obj.WorkingSocket, "cancled");
                    chattingClients.Remove(obj);
                    obj.finding = false;
                }
            }
            else if (obj.TargetSocket == null)
            {
                string[] Data = Text.Split(':');
                Console.WriteLine(Text);
                if (Text == "logout")
                {
                    num--;
                    obj.Clear();
                }
                if (Data[0] == "login") //학번 검사
                {
                    bool correct = true;

                    foreach (DataRow r in data.Tables[0].Rows)
                    {
                        if (r["name"].ToString() == Data[1]) //입력한 것과 데이터베이스의 값이 맞는지 검사
                        {

                            if (r["hakbun"].ToString() == Data[2])
                            {
                                for (int i = 0; i < connectedClients.Count; i++)
                                {
                                    if (connectedClients[i].Name == Data[2])
                                    {
                                        Send(obj.WorkingSocket, "logined");
                                        correct = false;
                                        break;
                                    }
                                }
                                if (correct)
                                {
                                    Console.WriteLine("일치");
                                    obj.hakbun = Convert.ToInt32(Data[2]);
                                    obj.Name = Data[2];
                                    obj.UserName = r["nickname"].ToString();
                                    Console.WriteLine(obj.hakbun);
                                    Send(obj.WorkingSocket, "yes:" + obj.UserName);
                                    num++;
                                    correct = false;
                                }
                            }
                            else
                            {
                                Send(obj.WorkingSocket, "hakbunno");
                                correct = false;
                            }
                        }
                    }
                    if (correct)
                    {
                        Send(obj.WorkingSocket, "nameno");
                    }
                    correct = true;
                }
                else if (Data[0] == "search")
                {
                    try
                    {
                        if (!obj.finding)
                        {
                            string sql = "update gsm set    nickname='" + Data[1] + "'where hakbun = " + obj.hakbun;
                            obj.UserName = Data[1];
                            MySqlCommand cmd = new MySqlCommand(sql, conn);
                            cmd.ExecuteNonQuery();
                            Send(obj.WorkingSocket, "searching");
                            chattingClients.Add(obj);
                            obj.finding = true;
                        }
                    }
                    catch
                    {
                        Send(obj.WorkingSocket, "error");
                    }
                }
            }
        }
            
    }

    public class AsyncObject //비동기 클라이언트를 관리하는 클래스
    {
        public bool chatting = false; //현재 누군가와 채팅중인지..
        public bool finding = false;
        public Socket WorkingSocket = null;
        public readonly int BufferSize = 4096;
        public byte[] Buffer;
        public string Name = string.Empty; //gsm 학생의 이름
        public string UserName = string.Empty; //유저가 사용할 닉네임
        public int hakbun = -1; //학번자리
        public AsyncObject TargetSocket = null; //채팅할 소켓

        public AsyncObject(Socket WorkingSocket)
        {
            this.WorkingSocket = WorkingSocket;
            Buffer = new byte[BufferSize];
        }
        public void ClearBuffer()
        {
            Array.Clear(Buffer, 0, BufferSize);
        }

        internal void Clear()
        {
            chatting = false;
            finding = false;
            Name = string.Empty;
            UserName = string.Empty;
            hakbun = -1;
        }
    }
}