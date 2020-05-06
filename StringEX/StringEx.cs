using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CustomExtension
{
    /*String 확장 메서드*/
    public static class StringEx
    {
        public static int ConvertToInt32(this string value)
        {
            if (value.IsInt())
            {
                return Convert.ToInt32(value);
            }
            else
            {
                throw new Exception("Int32 값으로 변경 할 수 없는 문자입니다.");
            }
        }

        public static bool IsInt(this string value)
        {
            int result = 0;
            return int.TryParse(value, out result);
        }

        public static void Print(this string value)
        {
            Console.WriteLine(value.ToString());
            Debug.WriteLine(value.ToString());
        }

        /// <summary>
        /// 문자열에 지정된 캐릭터로 자리수 만큼 채웁니다.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="digits">자리수</param>
        /// <param name="c">채울 char value</param>
        /// <returns></returns>
        public static string FillBit(this string value, int digits,char c = '0')
        {
            return new string(c, digits - value.Length) + value;
        }
    }
    /*TcpClient 확장 메서드*/
    public static class TcpClientExtend
    {
        public static TcpState GetClientState(this TcpClient tcpClient)
        {
            if (!tcpClient.Connected) return TcpState.Unknown;

            //TcpConnectionInformation[] arrTCP = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections();

            var foo = IPGlobalProperties.GetIPGlobalProperties()
              .GetActiveTcpConnections()
              .SingleOrDefault(x => x.LocalEndPoint.Equals(tcpClient.Client.RemoteEndPoint));
            return foo != null ? foo.State : TcpState.Unknown;
        }

        public static TcpState GetServerState(this TcpClient tcpClient)
        {
            if (!tcpClient.Connected) return TcpState.Unknown;

            var foo = IPGlobalProperties.GetIPGlobalProperties()
              .GetActiveTcpConnections()
              .SingleOrDefault(x => x.LocalEndPoint.Equals(tcpClient.Client.LocalEndPoint));
            return foo != null ? foo.State : TcpState.Unknown;
        }
    }
}
