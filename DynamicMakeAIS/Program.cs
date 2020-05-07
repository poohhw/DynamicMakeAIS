using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CustomExtension;

namespace DynamicMakeAIS
{

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("비동기 TCP 서버 시작");
            AysncEchoServer().Wait();
        }

        async static Task AysncEchoServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 7000);
            listener.Start();
            while (true)
            {
                //비동기 Accept
                TcpClient tc = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                // 새 쓰레드에서 처리
                await Task.Factory.StartNew(AsyncTcpProcess, tc);
            }
        }

        async static void AsyncTcpProcess(object obj)
        {

            TcpClient tc = obj as TcpClient;

            IPEndPoint ip_point = tc.Client.RemoteEndPoint as IPEndPoint;
            Console.WriteLine(ip_point.Address.ToString() + ":" + ip_point.Port.ToString() + " Client 접속");

            int MAX_SIZE = 1024;
            NetworkStream stream = tc.GetStream();

            byte[] byteInfoMsg = Encoding.Default.GetBytes("데이터 수신을 받으려면 1을 입력하세요!. 수신종료 시 Q를 입력하세요." + Environment.NewLine);
            await stream.WriteAsync(byteInfoMsg, 0, byteInfoMsg.Length).ConfigureAwait(false);

            while (tc.Connected)
            {
                var buff = new byte[MAX_SIZE];
                //var nbytes = await stream.ReadAsync(buff, 0, buff.Length).ConfigureAwait(false);

                int nRead = 0;   
                //string msg = string.Empty;
                if (tc.GetClientState() == TcpState.Established)
                {

                    Console.WriteLine($"{msg} at {DateTime.Now}");
                    //메세지 전송
                    Send(tc, stream);

                    //클라이언트 다음 메세지 대기.
                    bool bQuit = await Receive(tc, stream);
                    if (!bQuit)
                    {
                        Console.WriteLine("보내기 종료");

                        break;
                    }

                    while ((nRead = await stream.ReadAsync(buff, 0, buff.Length)) != 0)
                    {
                        string msg  = Encoding.ASCII.GetString(buff, 0, nRead);

                        if (msg.Equals("1"))
                        {

                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    Console.WriteLine(ip_point.Address.ToString() + ":" + ip_point.Port.ToString() + " Client 접속 종료");
                    stream.Close();
                    tc.Close();
                    break;
                }
            }

        }

        private static async Task<bool> Receive(TcpClient tc, NetworkStream stream)
        {
            Console.WriteLine("Receive 종료 대기");
            int MAX_SIZE = 1024;
            var buff = new byte[MAX_SIZE];
            int nRead = 0;
            string msg = string.Empty;
            if((nRead = stream.Read(buff, 0, buff.Length)) != 0)
            {
                msg = Encoding.ASCII.GetString(buff, 0, nRead);

                if (msg.ToUpper().Equals("Q"))
                {
                    tc.Close();
                    return false;
                }

            }
            return true;
        }

        private static async Task Send(TcpClient tc, NetworkStream stream)
        {
            Random random = new Random();
            //송신
            while (true)
            {
                TcpState tcpState = tc.GetClientState();
                if (!tc.Connected) break;
                if (tcpState == TcpState.Established)
                {
                    DataPayload dp = new DataPayload();

                    string SendMessage = $"!AIVDM,1,1,,B,{dp.CreateDataPayLoadBinary()},0";
                   
                    SendMessage = SendMessage + "*" + MakeChecksum(SendMessage);


                    byte[] byteMsg = Encoding.Default.GetBytes(SendMessage + Environment.NewLine);
                    await stream.WriteAsync(byteMsg, 0, byteMsg.Length).ConfigureAwait(false);
                    Thread.Sleep(300);
                }
                else
                {
                    return;
                }
            }
        }

        public static string MakeChecksum(string pData)
        {          
            int nCheckSum = 0;

            pData = pData.Replace("!", string.Empty);

            foreach (char c in pData)
            {
                nCheckSum ^= Convert.ToByte(c);
            }
            string hex = Convert.ToString(nCheckSum, 16);

            return hex.FillBit(2).ToUpper();
        }
    }
}
