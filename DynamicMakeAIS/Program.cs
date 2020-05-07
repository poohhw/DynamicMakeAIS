using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private static List<TcpClient> ChangeConnectedClients = new List<TcpClient>();
        static void Main(string[] args)
        {

            Task.Factory.StartNew(CreateAISMessage);
            Task.Factory.StartNew(Send2);
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

            ChangeConnectedClients.Add(tc);

            NetworkStream stream = tc.GetStream();

            byte[] byteInfoMsg = Encoding.Default.GetBytes("데이터 수신을 받으려면 1을 입력하세요. 수신종료 시 Q를 입력하세요." + Environment.NewLine);
            await stream.WriteAsync(byteInfoMsg, 0, byteInfoMsg.Length).ConfigureAwait(false);

            while (tc.Connected)
            {
                if (tc.GetClientState() == TcpState.Established)
                {

                    //await Send(tc, stream);                    
                }
                else
                {
                    Console.WriteLine(ip_point.Address.ToString() + ":" + ip_point.Port.ToString() + " Client 접속 종료");
                    stream.Close();
                    tc.Close();

                    ChangeConnectedClients.Remove(tc);
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
            //송신
            while (true)
            {
                TcpState tcpState = tc.GetClientState();
                if (!tc.Connected) return;
                if (tcpState == TcpState.Established)
                {
                    if (!string.IsNullOrEmpty(SendMessage))
                    {
                        //DataPayload dp = new DataPayload();

                        //string SendMessage = $"!AIVDM,1,1,,B,{dp.CreateDataPayLoadBinary()},0";

                        //SendMessage = SendMessage + "*" + MakeChecksum(SendMessage);


                        byte[] byteMsg = Encoding.Default.GetBytes(SendMessage + Environment.NewLine);
                        await stream.WriteAsync(byteMsg, 0, byteMsg.Length).ConfigureAwait(false);
                        Thread.Sleep(300);
                    }
                }
                else
                {
                    return;
                }
            }
        }

        private static async Task Send2()
        {
            //송신
            while (true)
            {
                DataPayload dp = new DataPayload();
                string SendMessage = $"!AIVDM,1,1,,B,{dp.CreateDataPayLoadBinary()},0";
                SendMessage = SendMessage + "*" + MakeChecksum(SendMessage);

                TcpClient[] tc1 = new TcpClient[ChangeConnectedClients.Count];
                ChangeConnectedClients.CopyTo(tc1);
                foreach (var item in tc1)
                {
                    NetworkStream stream = item.GetStream();
                    TcpState tcpState = item.GetClientState();
                    if (!item.Connected) continue;
                    if (tcpState == TcpState.Established)
                    {
                        byte[] byteMsg = Encoding.Default.GetBytes(SendMessage + Environment.NewLine);
                        await stream.WriteAsync(byteMsg, 0, byteMsg.Length).ConfigureAwait(false);
                        Thread.Sleep(300);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private static void CreateAISMessage()
        {
            while (true)
            {
                Thread.Sleep(300);
                //메세지 생성.
                DataPayload dp = new DataPayload();
                SendMessage = $"!AIVDM,1,1,,B,{dp.CreateDataPayLoadBinary()},0";
                SendMessage = SendMessage + "*" + MakeChecksum(SendMessage);

                //Console.WriteLine(SendMessage);
            }
            //byte[] byteMsg = Encoding.Default.GetBytes(SendMessage + Environment.NewLine);

            //접속된 클라이언트에 메세지 보내기.
            //    foreach (TcpClient client in ConnectedClients)
            //    {
            //        try
            //        {
            //            TcpState tcpState = client.GetClientState();
            //            using (NetworkStream stream = client.GetStream())
            //            {
            //                if (tcpState == TcpState.Established) //연결 유지 상태 확인.
            //                {
            //                    stream.Write(byteMsg, 0, byteMsg.Length);

            //                }
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine(ex.Message);
            //        }
            //    }
            //}
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
