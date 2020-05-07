using CustomExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicMakeAIS
{
    public enum MID
    {
        Korea = 440,
        Japan = 432

    }
    public class DataPayload
    {
        public int _MessageType { get; set; }
        public int _RepeatIndicator { get; set; }
        public int _MMSI { get; set; }
        public int _NavigationStatus { get; set; }
        public int _ROT { get; set; }
        public int _SOG { get; set; }
        public int _PositionAccuracy { get; set; }
        public double[] _LongLat { get; set; }
        public double _COG { get; set; }
        public int _HDG { get; set; }
        public int _TimeStamp { get; set; }
        public int _ManeuverIndicator { get; set; }
        public int _Spare { get; set; }
        public int _RAIMflag { get; set; }
        public int _Radiostatus { get; set; }

        public string _BinaryData { get; set; }
        public string _DataPayLoad { get; set; }

        private Random rnMMSI = new Random();

        private Random rMessage = new Random();
        //private Random rn;
        public DataPayload()
        {

        }

        public string CreateDataPayLoadBinary()
        {
            int Seed = (int)DateTime.Now.Ticks;
            StringBuilder sb = new StringBuilder();
            //0-5, 6, Message Type
            //6-7, 2, RepeatIndicator
            //8-37, 30, MMSI
            //38-41, 4, Navigation Status
            //42-49, 8, Rate of Turn (ROT)
            //50-59, 10, Speed Over Ground(SOG)
            //60-60, 1, Position Accuracy
            //61-88, 28, Longitude
            //89-115, 27, Latitude
            //116-127, 12, Course Over Ground(COG)
            //128-136, 9, TrueHeading(HDG)
            //137-142, 6, TimeStamp
            //143-144, 2, ManeuverIndicator
            //145-147, 3, Spare
            //148-148, 1, RAIM flag
            //149-167, 19, Radio status
            this._MessageType = rMessage.Next(1, 4);
            this._RepeatIndicator = 1;
            this._MMSI = this.CreateMMSI();
            this._NavigationStatus = 0;
            this._ROT = 0;
            this._SOG = Convert.ToInt32(this.CreateSOG() * 10.0d);
            this._PositionAccuracy = 0;
            this._LongLat = this.CreateCoodinate();
            this._COG = this.CreateCOG(Seed);
            this._HDG = this.CreateHDG(Seed);
            this._TimeStamp = 1;
            this._ManeuverIndicator = 0;
            this._Spare = 0;
            this._RAIMflag = 0;
            this._Radiostatus = 0;


            sb.Append(Convert.ToString(_MessageType, 2).FillBit(6));
            sb.Append(Convert.ToString(_RepeatIndicator, 2).FillBit(2));
            sb.Append(Convert.ToString(_MMSI, 2).FillBit(30));
            sb.Append(Convert.ToString(_NavigationStatus, 2).FillBit(4));
            sb.Append(Convert.ToString(_ROT, 2).FillBit(8));
            sb.Append(Convert.ToString(_SOG, 2).FillBit(10));
            sb.Append(Convert.ToString(_PositionAccuracy, 2).FillBit(1));
            sb.Append(Convert.ToString(Convert.ToInt32((_LongLat[1] * 600000d)), 2).FillBit(28));
            sb.Append(Convert.ToString(Convert.ToInt32((_LongLat[0] * 600000d)), 2).FillBit(27));
            sb.Append(Convert.ToString(Convert.ToInt32(_COG * 10.0d), 2).FillBit(12));
            sb.Append(Convert.ToString(_HDG, 2).FillBit(9));
            sb.Append(Convert.ToString(_TimeStamp, 2).FillBit(6));
            sb.Append(Convert.ToString(_ManeuverIndicator, 2).FillBit(2));
            sb.Append(Convert.ToString(_Spare, 2).FillBit(3));
            sb.Append(Convert.ToString(_RAIMflag, 2).FillBit(1));
            sb.Append(Convert.ToString(_Radiostatus, 2).FillBit(19));

            this._BinaryData = sb.ToString();
            return CreateDataPayLoad(_BinaryData);
        }

        private string CreateDataPayLoad(string DataBinary)
        {
            char[] arrData = new char[DataBinary.Length / 6];
            int idx = 0;

            for (int i = 0; i < arrData.Length; i++)
            {
                int num = 0;
                char c;
                if (Convert.ToInt32(DataBinary.Substring(idx, 6), 2) >= 40)
                {
                    num = 8;
                }

                c = Convert.ToChar(Convert.ToInt32(DataBinary.Substring(idx, 6), 2) + 48 + num);

                //Console.WriteLine($"이진수: {DataBinary.Substring(idx, 6)}, 십진수: {Convert.ToInt32(DataBinary.Substring(idx, 6), 2)},문자: {c}");
                arrData[i] = c;
                idx += 6;
            }
            _DataPayLoad = new string(arrData);

            return _DataPayLoad;

        }

        /// <summary>
        /// 경위도 좌표.
        /// </summary>
        /// <returns>[0]:위도,[1]경도</returns>
        public double[] CreateCoodinate()
        {
            double[] arrCoodinate = new double[2];

            Random rLat = new Random();
            //Random rLong = new Random();

            int nLat = rLat.Next(34, 39);
            int nLong = rLat.Next(125, 131);

            double dLat = rLat.NextDouble();
            double dLong = rLat.NextDouble();

            string strLat = Math.Round((nLat + dLat), 5).ToString();
            string strLong = Math.Round((nLong + dLong), 5).ToString();

            arrCoodinate[0] = Convert.ToDouble(strLat);
            arrCoodinate[1] = Convert.ToDouble(strLong);

            return arrCoodinate;
        }

        /// <summary>
        /// Course Over Ground 데이터 페이로드 생성, 부호없는 정수, 소수점 1.
        /// </summary>
        /// <returns></returns>
        public double CreateCOG(int pSeed)
        {
            Random rn = new Random(pSeed);
            string vCOG = string.Empty;
            int nCOG = rn.Next(0, 360);

            Random rnd = new Random();
            float fCOG = (float)rnd.NextDouble();

            vCOG = (nCOG + Math.Round(fCOG, 1)).ToString();

            return Convert.ToDouble(vCOG);
        }

        /// <summary>
        /// True Heading 데이터 페이로드 생성, 부호없는 정수.
        /// </summary>
        /// <returns></returns>
        public int CreateHDG(int pSeed)
        {
            Random rn = new Random(pSeed);
            string vHDG = string.Empty;
            vHDG = rn.Next(0, 360).ToString();
            return vHDG.ConvertToInt32();
        }

        /// <summary>
        /// MMSI 데이터 페이로드 생성. 랜덤 9자리 숫자.
        /// </summary>
        /// <param name="mid">국가코드</param>
        /// <returns></returns>
        public int CreateMMSI(MID mid = MID.Korea)
        {
            string vMMSI = string.Empty;

            int rnValue = rnMMSI.Next(100000, 999999);

            vMMSI = ((int)mid).ToString() + rnValue.ToString();

            return vMMSI.ConvertToInt32();
        }

        /// <summary>
        /// SOG 데이터 페이로드 생성, 0~26 랜덤 숫자.
        /// </summary>
        /// <returns></returns>
        public double CreateSOG()
        {
            string vSOG = string.Empty;

            Random rn = new Random();

            vSOG = NextFloat(rn, 0, 100).ToString();

            return Convert.ToDouble(vSOG);
        }

        public float NextFloat(Random random, int MinValue, int MaxValue)
        {
            double val = random.NextDouble(); // range 0.0 to 1.0
            val -= 0.5; // expected range now -0.5 to +0.5
            val *= 2; // expected range now -1.0 to +1.0
            return (float)Math.Round(random.Next(MinValue, MaxValue) * Math.Abs(val), 1);
        }
    }
}
