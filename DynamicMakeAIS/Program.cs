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
        private static string SendMessage;
        private static List<TcpClient> ConnectedClients = new List<TcpClient>();
        static void Main(string[] args)
        {
            Task.Factory.StartNew(CreateAISMessage);
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
            ConnectedClients.Add(tc);

            IPEndPoint ip_point = tc.Client.RemoteEndPoint as IPEndPoint;
            Console.WriteLine(ip_point.Address.ToString() + ":" + ip_point.Port.ToString() + " Client 접속");

            int MAX_SIZE = 1024;
            NetworkStream stream = tc.GetStream();


            while (tc.Connected)
            {
                var buff = new byte[MAX_SIZE];

                if (tc.GetClientState() == TcpState.Established)
                {

                    //메세지 전송                    
                    await Send(tc, stream);


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

        private static bool Receive(TcpClient tc, NetworkStream stream)
        {
            Console.WriteLine("Receive 종료 대기");
            int MAX_SIZE = 1024;
            var buff = new byte[MAX_SIZE];
            int nRead = 0;
            string msg = string.Empty;
            if ((nRead = stream.Read(buff, 0, buff.Length)) != 0)
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
            Thread.Sleep(500);
            TcpState tcpState = tc.GetClientState();
            if (!tc.Connected) return;
            if (tcpState == TcpState.Established)
            {
                DataPayload dp = new DataPayload();
                byte[] byteMsg = Encoding.Default.GetBytes(SendMessage + Environment.NewLine);
                await stream.WriteAsync(byteMsg, 0, byteMsg.Length).ConfigureAwait(false);
            }
        }

        private static void CreateAISMessage()
        {
            while (true)
            {
                Thread.Sleep(500);
                //Console.WriteLine("..");
                //메세지 생성.
                DataPayload dp = new DataPayload();
                SendMessage = $"!AIVDM,1,1,,B,{dp.CreateDataPayLoadBinary()},0";
                SendMessage = SendMessage + "*" + MakeChecksum(SendMessage);
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
