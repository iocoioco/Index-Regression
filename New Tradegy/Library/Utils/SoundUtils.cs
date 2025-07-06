using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Tradegy.Library.Utils
{
    internal class SoundUtils
    {

        public static void Sound_돈(int total_amount)
        {
            if (g.optimumTrading)
            {
                switch (total_amount)
                {
                    case 0:
                        Sound("돈", "single stock opt");
                        break;
                    case 100:
                        Sound("돈", "one hundred opt");
                        break;
                    case 500:
                        Sound("돈", "five hundred opt");
                        break;
                    case 1000:
                        Sound("돈", "one thousand opt");
                        break;
                    case 2000:
                        Sound("돈", "two thousand opt");
                        break;
                    case 4000:
                        Sound("돈", "four thousand opt");
                        break;
                    case 8000:
                        Sound("돈", "eight thousand opt");
                        break;
                    case 16000:
                        Sound("돈", "sixteen thousand opt");
                        break;
                    case 32000:
                        Sound("돈", "thirty two thousand opt");
                        break;
                    case 64000:
                        Sound("돈", "sixty four thousand opt");
                        break;
                    default:
                        Sound("돈", "limit exceed opt");
                        break;
                }
            }
            else
            {
                switch (total_amount)
                {
                    case 0:
                        Sound("돈", "single stock");
                        break;
                    case 100:
                        Sound("돈", "one hundred");
                        break;
                    case 500:
                        Sound("돈", "five hundred");
                        break;
                    case 1000:
                        Sound("돈", "one thousand");
                        break;
                    case 2000:
                        Sound("돈", "two thousand");
                        break;
                    case 4000:
                        Sound("돈", "four thousand");
                        break;
                    case 8000:
                        Sound("돈", "eight thousand");
                        break;
                    case 16000:
                        Sound("돈", "sixteen thousand");
                        break;
                    case 32000:
                        Sound("돈", "thirty two thousand");
                        break;
                    case 64000:
                        Sound("돈", "sixty four thousand");
                        break;
                    default:
                        Sound("돈", "limit exceed");
                        break;
                }
            }
        }

        public static async Task MarketTimeAlarmsAsync(int HHmm)
        {
            int[] alarm_HHmm = { 1000, 1030, 1450, 1455, 1500, 1505, 1510, 1515, 1518, 1519 };

            for (int i = 0; i < alarm_HHmm.Length; i++)
            {
                if (HHmm == alarm_HHmm[i] && HHmm != g.AlarmedHHmm)
                {
                    g.AlarmedHHmm = HHmm;
                    if (HHmm == 1000)
                    {
                        Sound("time", "taiwan open");
                    }
                    else if (HHmm == 1030)
                        Sound("time", "china open");
                    else if (HHmm == 1450)
                        Sound("time", "30");
                    else if (HHmm == 1455)
                        Sound("time", "25");
                    else if (HHmm == 1500)
                        Sound("time", "20");
                    else if (HHmm == 1505)
                        Sound("time", "15");
                    else if (HHmm == 1510)
                        Sound("time", "10");
                    else if (HHmm == 1515)
                        Sound("time", "5");
                    else if (HHmm == 1518)
                        Sound("time", "2");
                    else if (HHmm == 1519)
                        Sound("time", "1");
                }
            }
        }


        public static void Sound(string sub_directory, string sound)
        {
            if (sound == "")
                return;

            string sound_file;
            if (sub_directory == "")
                sound_file = @"C:\병신\data work\소\" + sound + ".wav";
            else
                sound_file = @"C:\병신\data work\소\" + sub_directory + "\\" + sound + ".wav";

            if (!File.Exists(sound_file))
            {
                return;
            }

            System.Media.SoundPlayer player = new System.Media.SoundPlayer();
            player.SoundLocation = sound_file;

            player.PlaySync(); // instead of Play()
        }

    }
}
