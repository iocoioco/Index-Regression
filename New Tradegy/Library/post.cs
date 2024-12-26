using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

/* 배점 : 대형주 배차 조건 충족시, 수일 과다 움직임 높게(dev 큰 종목 가능성 높음) + 배차
 *  1차 상승시 접근, 2차 이상 자제
 * 그룹 전체도 동일조건 적용
 * 
 * 순간 엄청난 거분 + 프분
 * 누적 프돈, 거돈
 * 
 * 추가조건 : 간단접근 -> 세밀접근
 * 
 * 프돈의 손실종목
 * 
 * 프 매수 가 하락 또는 횡보 -> 가 하락 가능
 * 프 매도 가 상승 -> 프 매수 전환 가능
 * 
 * 기본 : 검둥이 급상 후 매도
 * 분거래액이 일정이상이어야 검토
 * 체강 낮은 종목 제외
 * 돌파 + 배수차 + 분거래액
 * 분거래액이 과대
 * 
 * 돌파 정의 : 강한 돌파 동시 진입 전저조건 -50
 * 
 * 매수 종목 손절 잡고 진입(횟수 줄이고 베팅액 증가)
 * 
 * 자동 매도
 * 
 * 양쪽 지수 프돈 지속 증가 또는 큰 양전
*/
namespace New_Tradegy.Library
{
    public class ps
    {
        public static void ManageChart1Safe()
        {
            Form se = (Form)Application.OpenForms["se"];
            if (se.InvokeRequired)
            {
                se.Invoke(new Action(mm.ManageChart1));
            }
            else
            {
                mm.ManageChart1();
            }
        }
        public static void ManageChart2Safe()
        {
            Form Form_보조_차트 = (Form)Application.OpenForms["Form_보조_차트"];
            if (Form_보조_차트.InvokeRequired)
            {
                Form_보조_차트.Invoke(new Action(mm.ManageChart2));
            }
            else
            {
                mm.ManageChart2();
            }
        }

        public static void post_real(List<g.stock_data> Download_ogl_data_List)
        {
            foreach (var o in Download_ogl_data_List)
            {
                post(o);
            }
                ev.eval_stock();
            ev.eval_group();

            foreach (var o in Download_ogl_data_List)
            {
                if (g.보유종목.Contains(o.stock)) // 보유종목 중 Form_호가 사용하지 않고 있는 경우 대비
                {
                    cn.dgv2_update(); // marketeye_received()
                    marketeye_received_보유종목_푀분의매수매도_소리내기(o); // this is from mip
                }
            }
            ManageChart1Safe();
            ManageChart2Safe();
        }

        public static void post_test()
        {
            ev.eval_stock();
            ev.eval_group();
            mm.ManageChart1();
            mm.ManageChart2();
        }

        public static void marketeye_received_보유종목_푀분의매수매도_소리내기(g.stock_data o)
        {
            if (o.보유량 * o.전일종가 < 200000) // 보유액 20,000 이하이면 무시하고 return
                return;

            double sound_indicater = 0;

            // 소리
            #region
            if (Math.Abs(o.틱프로천[0]) > 0.1 || Math.Abs(o.틱외인천[0]) > 0.1)
            {
                string sound = "";
                int 보유종목_순서번호 = -1;
                for (int i = 0; i < 3; i++)
                {
                    if (g.보유종목[i] == o.stock)
                    {
                        보유종목_순서번호 = i;
                        if (i == 0)
                            sound = "one ";
                        else if (i == 1)
                            sound = "two ";
                        else
                            sound = "three ";
                        break;
                    }
                }
                if (보유종목_순서번호 < 0)
                    return;

                if (o.통계.프분_dev > 0)
                {
                    sound_indicater = (o.틱프로천[0] + o.틱외인천[0]) / o.통계.프분_dev;
                    if (sound_indicater > 2) sound += "buyest";
                    else if (sound_indicater > 1) sound += "buyer";
                    else if (sound_indicater > 0) sound += "buy";
                    else if (sound_indicater > -1) sound += "sell";
                    else if (sound_indicater > -2) sound += "seller";
                    else sound += "sellest";

                }
                mc.Sound("가", sound);
            }
            #endregion

            // 비상매도
            // 푀틱 매도 푀분 매도 가 하락
            // 푀틱 매수 푀분 매수 배차 음, 가 하락
            // 배차 음 가 하락 (누군가 던지고 있다)
            // 피로도
        }

        public static double post_score(g.stock_data o, int check_row)
        {

            double sending_value;

            // prep 에서 -> 통계.프분_dev, 통계.프분_avr 계산하였으나 푀분에 적용함.
            if (o.통계.프분_dev > g.EPS)
            {
                sending_value = (o.분프로천[0] + o.분외인천[0]) / (o.통계.프분_dev);

                //post_score_interpolation(g.s.푀분, sending_value, ref o.점수.푀분); // 프분, 천만원
                o.점수.푀분 = sending_value;
                if (o.분프로천[0] > 5 && o.분외인천[0] > 5)
                    o.점수.푀분 *= 1.5;
            }
            else
            {
                o.점수.푀분 = 0.0;
            }
            o.점수.푀분 *= g.s.푀분_wgt;

            //if (o.통계.거분_dev > g.EPS)
            //{
            //    sending_value = (o.분거래천[0] - o.통계.거분_avr) / (o.통계.거분_dev * 0.1); // o.통계.거분_dev (백만)
            //    //post_score_interpolation(g.s.거분, sending_value, ref o.점수.거분); // 거분, 천만원
            //    o.점수.거분 = sending_value;

            //}
            //else
            //{
            //    o.점수.거분 = 0.0;
            //}
            //o.점수.거분 *= g.s.거분_wgt;


            if (o.통계.배차_dev > g.EPS)
            {
                sending_value = (o.분배수차[0] - o.통계.배차_avr) / o.통계.배차_dev;
                //post_score_interpolation(g.s.배차, sending_value, ref o.점수.배차); // 배차, 천만원
                o.점수.배차 = sending_value;
            }
            else
            {
                o.점수.배차 = 0.0;
            }
            o.점수.배차 *= g.s.배차_wgt;

            if (o.통계.배합_dev > g.EPS)
            {
                sending_value = (o.분배수합[0] - o.통계.배합_avr) / o.통계.배합_dev;
                //post_score_interpolation(g.s.배합, sending_value, ref o.점수.배합); // 배합, 천만원
                o.점수.배합 = sending_value;
            }
            else
            {
                o.점수.배합 = 0.0;
            }
            o.점수.배합 *= g.s.배합_wgt;

            return o.점수.푀분 + o.점수.배차 + o.점수.배합;





            #region

            // > + 1S.D 16%
            // > + 2S.D. 2.5%
            // > + 3S.D 0.15%

            // post_score_interpolation(g.s.dev, o.dev, ref o.점수.dev, ref o.점수.총점);
            //post_score_interpolation(g.s.avr, o.avr, ref o.점수.avr, ref o.점수.총점);
            //post_score_interpolation(g.s.mkc, o.x[o.nrow - 1, 10], ref o.점수.mkc, ref o.점수.총점);

            //post_score_interpolation(g.s.돌파, o.pass.pass_level, ref o.점수.돌파, ref o.점수.총점);
            //post_score_interpolation(g.s.눌림, o.x[o.nrow - 1, 10], ref o.점수.눌림, ref o.점수.총점);

            // post_score_interpolation(g.s.급락, o.정도.급락, ref o.점수.급락, ref o.점수.총점);
            //post_score_interpolation(g.s.잔잔, o.정도.잔잔, ref o.점수.잔잔, ref o.점수.총점);

            //post_score_interpolation(g.s.가연, o.x[o.nrow - 1, 10], ref o.점수.가연, ref o.점수.총점);
            //post_score_interpolation(g.s.가분, o.x[o.nrow - 1, 10], ref o.점수.가분, ref o.점수.총점);
            //post_score_interpolation(g.s.가틱, o.x[o.nrow - 1, 10], ref o.점수.가틱, ref o.점수.총점);
            //post_score_interpolation(g.s.가반, o.x[o.nrow - 1, 10], ref o.점수.가반, ref o.점수.총점);
            //post_score_interpolation(g.s.가지, o.x[o.nrow - 1, 10], ref o.점수.가지, ref o.점수.총점);
            //post_score_interpolation(g.s.가위, o.x[o.nrow - 1, 10], ref o.점수.가위, ref o.점수.총점);

            //post_score_interpolation(g.s.수연, o.x[o.nrow - 1, 10], ref o.점수.수연, ref o.점수.총점);
            //post_score_interpolation(g.s.수지, o.x[o.nrow - 1, 10], ref o.점수.수지, ref o.점수.총점);
            //post_score_interpolation(g.s.수위, o.x[o.nrow - 1, 10], ref o.점수.수위, ref o.점수.총점);

            //post_score_interpolation(g.s.강연, o.x[o.nrow - 1, 11], ref o.점수.강연, ref o.점수.총점);
            //post_score_interpolation(g.s.강지, o.x[o.nrow - 1, 11], ref o.점수.강지, ref o.점수.총점);
            //post_score_interpolation(g.s.강위, o.x[o.nrow - 1, 11], ref o.점수.강위, ref o.점수.총점);



            //post_score_급락(o, check_row);
            //post_score_급상(o, check_row);


            //if (o.통계.배합_dev > 0.0)
            //{
            //    sending_value = o.분배수차[0] / o.통계.배합_dev;
            //    post_score_interpolation(g.s.배합, sending_value, ref o.점수.배합); // 배합, 천만원
            //}


            //post_score_interpolation(g.s.배반, o.틱거돈천[0], ref o.점수.배반, ref o.점수.총점);

            //if (o.dev < 2.0)
            //    o.점수.총점 *= 0.66;
            //else if (o.dev < 3.0)
            //    o.점수.총점 *= 0.75;
            //else if (o.dev < 4.0)
            //    o.점수.총점 *= 0.80;
            //else
            //{

            //}

            // wr.w(g.date.ToString(), o.점수.프분.ToString("F2"), o.점수.거분.ToString("F2"), o.점수.배차.ToString("F2"), "post"); // TEMP

            //if (o.점수.프분 < 0 || o.점수.배차 < 0)
            //    o.점수.총점 = 0.0;




            //int 분간가격상승 = x[0, 1] - x[1, 1];

            //double 분전배수차평균 = 0;
            //int number_of_data = 3;
            //int count = 0;
            //for (int i = 2; i < number_of_data + 1; i++) // 2분전, 3분전, 4분전의 배수차평균
            //{
            //    if (x[i, 0] < 90000)
            //        break;
            //    분전배수차평균 += (x[i, 8] - x[i, 9]);
            //    count++;
            //}
            //if (count > 0)
            //    분전배수차평균 /= count;



            // 잔잔 급상(특히 낮은 가격 즉, 바닥에서 급상하는 종목)
            //if (분전배수차평균 < 1 && 분간가격상승 > g.v.분간가격상승이상) // 실제는 분간가격상승
            //    ADD = true;
            //if (분전배수차평균 < 1 && 분간가격상승 > 100 && g.add_interest)
            //{
            //    ADD = true;
            //    mc.Sound("","silence added");
            //}

            //if (o.틱프돈천[0] > g.v.틱간프로그램매수이상 && g.add_interest)
            //{
            //    ADD = true;
            //    mc.Sound("","program added");
            //}




            //if (!g.보유종목.Contains(o.stock))
            //{
            //    if (g.호가종목.Contains(o.stock))
            //    {
            //        g.호가종목.Remove(o.stock);
            //    }
            //    while (g.호가종목.Count >= 6)
            //    {
            //        g.호가종목.RemoveAt(g.호가종목.Count - 1);

            //    }
            //    g.호가종목.Insert(0, o.stock); // 첫번째 삽입


            //}



            //public static double eval_score(g.stock_data o, int Exist_Lines, int[,] x)
            //{
            //    {
            //        if (x[0, 0] <= 901)
            //            return -10000;
            //        double 총점 = 0.0;

            //        string stock = o.stock;



            //        double 틱간시간 = ms.total_Seconds(o.틱의시간[1], o.틱의시간[0]); // 0900
            //        double 분간시간 = ms.total_Seconds(o.틱의시간[g.array_size / 2], o.틱의시간[0]); // 0900

            //        // 가격
            //        int 가격 = x[0, 1];
            //        double 틱간가격상승 = 0.0;
            //        double 분간가격상승 = 0.0;
            //        if (!g.test)
            //        {
            //            if (틱간시간 > g.EPS)
            //                틱간가격상승 = (o.틱의가격[0] - o.틱의가격[1]) / 틱간시간 * 60.0; // 0900
            //            if (분간시간 > g.EPS)
            //                분간가격상승 = (o.틱의가격[0] - o.틱의가격[o.틱의시간[g.array_size / 2]]) / 분간시간 * 60.0; // from time to time
            //        }
            //        else
            //        {
            //            분간가격상승 = x[0, 1] - x[1, 1];
            //            분간가격상승 = 0.0;
            //        }





            //        // 가격
            //        int 수급 = x[0, 2];
            //        int 수급연속 = x[0, 10];


            //        //체강
            //        int 체강 = x[0, 3];
            //        int 체강연속 = x[0, 11];


            //        // 프로그램(보완)
            //        double 프로그램 = (x[0, 4] - x[1, 4]) * 100.0 / (x[0, 7] - x[1, 7]);


            //        // 분거천
            //        double 분거천 = o.분거천;




            //        // 배수
            //        double 분전배수차평균 = 0;
            //        int number_of_data = 3;
            //        int count = 0;
            //        for (int i = 1; i < number_of_data + 1; i++)
            //        {
            //            if (x[i, 0] < 90000)
            //                break;
            //            분전배수차평균 += (x[i, 8] - x[i, 9]);
            //            count++;
            //        }
            //        if (count > 0)
            //            분전배수차평균 /= count;

            //        double 배수차 = 0;
            //        double 배수합 = 0;
            //        if (g.test) // testing
            //        {
            //            배수차 = x[0, 8] - x[0, 9];
            //            배수합 = x[0, 8] + x[0, 9];
            //        }
            //        else // real
            //        {
            //            double 매수배 = 0.0;
            //            double 매도배 = 0.0;
            //            if (분간시간 > g.EPS)
            //            {
            //                double multiple_factor = 60.0 / 분간시간 * 380.0 / o.일평균거래량;
            //                매수배 = (o.틱매수량[0] - o.틱매수량[g.array_size / 2 - 1]) * multiple_factor; // 0900
            //                매도배 = (o.틱매도량[0] - o.틱매도량[g.array_size / 2 - 1]) * multiple_factor; // 0900
            //            }
            //            배수차 = (int)((매수배 - 매도배) * g.TEN);
            //            배수합 = (int)((매수배 + 매도배) * g.TEN);
            //        }


            //        // 종가기준추정누적거래액_천만원
            //        double 종가기준추정누적거래액_천만원 = o.종가기준추정누적거래액_천만원;



            //        // 돌파



            //        // 눌림



            //        // 그룹






            //        // NO 0 
            //        // 수의 지속 상승, Regresssion 또는 Deviation 점검으로 ...
            //        // 특히, 시초 가, 수, 강의 급상

            //        // NO1 배수 이상 + 분당거래액이상 + 전 배수 조건, 낮은 배수로 진행되다 갑자기 높은 배수로 전환
            //        // 이런 경우 수, 강의 동시 상승
            //        //if (o.score[1] > 150)

            //        //20201108
            //        //if (o.score[2] >= 4)
            //        //    o.score[2] = 4;
            //        //if (o.score[3] >= 4)
            //        //    o.score[3] = 4;
            //        //if (o.score[2] + o.score[3] >= 7 && o.분거천 > 5 && o.돌파정도 > 1)
            //        //    Add = true;



            //        // DDD
            //        //if (전배수차평균 < 5 && o.score[8] > 100 && o.시총 > 10)
            //        //{
            //        //    Add = true; // 중, 대형주는 가 100 이상 점프는 쉽지 않으나 배수차의 100은 가능
            //        //}
            //        //if (o.score[1] > 100 && saved_average < 15 && o.score[8] > 50  // o.score[8] : 배수차, o.score[9] : 배수합
            //        // ) //대북주, 디지털화폐 등 잠잠하다 단체로 급상
            //        //{
            //        //    Add = true;
            //        //}

            //        // NO2 가격 25 이상, 수급 연속, 분당거래액이상
            //        //if (o.score[1] > 25 && o.score[2] >= g.v.편차이상) // 'M'으로 조정
            //        //{
            //        //    Add = true;
            //        //}

            //        // NO3
            //        // 분간가격상승 > 100 && 시총 > 1조
            //        //if (o.score[1] > 100 && o.시총 > 100)
            //        //    Add = true;

            //        // NO4
            //        // 마젠타 그룹 ... 낮은 배수 ... 다시 마젠타

            //        // NO5
            //        // 외인매수 타고 올라가는 종목, 950 이후 외인과 기관의 양매수 종목

            //        // NO6
            //        // 시초 외인이 베팅하는 섹터가 어디인가 ? 20200624 전자, 차 상승하면서 현차, 삼전, 모비스 등 4 - 5% 상승

            //        // NO7
            //        // 그룹의 강세 ... 일부 종목 제외, 씨젠 그룹의 경우도 진메트릭스, 미코, EDGE 등은 타 종목 대비 미약할 수 있음

            //        // NO 8
            //        // 강의 하락은 가의 하락 전조 가능 ... 즉, 배수차가 일방적에서 동등 또는 반전으로 진행되는 지 확인

            //        // NO 9
            //        // 사이안이 지속되면서 가 상승이 지속되면 중, 대형주의 경우 다음 상승을 기대할 수 있다. 

            //        // NO 10
            //        // 바닥에서 배수 반전(100/600, 100/200, 200/100) 급상, 강의 반전(-, -, -, -, +)도 ... 포함됨

            //        // NO 11
            //        // 시초부터 강세인 종목

            //        // NO 12
            //        // 사이안이 먼저 나타나고 다시 마젠타가 나타나면서 가의 상승

            //        // NO 13
            //        // 하락하다 반전

            //        // NO 14
            //        // Group Buy and Sell Control
            //        // Mask, Device, North, Armament, Bank, Ship etc.

            //        // NO 14 
            //        // Tight group syncronization

            //        // NO 15
            //        // 급상하던 종목 눌림목 하락시 매도 후, 다시 돌파시 매수 ... 이 정도 프로그램을 만들면 아주 고수급

            //        // THE MOST IMPORTANT
            //        // SUDDEN CRASH
            //        // SELF SELL

            //        // NO 0
            //        // Cobra이나 가의 급락 중, 수의 상승은 당연 ... 
            //        // 그러나 강의 상승도 떨어지는 종목을 담는 것이지 가의 상승 담보하지 않음, 그러므로 가의 상승은 필수

            //        // 분거천 일정 이상 조건
            //        //if (g.v.분당거래액이상 > o.분거천)
            //        //    Add = false;


            //        // 
            //        //if (o.분거천 > 1 && 분간가격상승 > 100)
            //        //    Add = true;


            //        //Commented Below is good to apply



            // 가격이 2-3분 횡보하면 좋은 조건에 던져라

            // 프외 손실 종목



            //        // 잔잔 급상 (특히 낮은 가격 즉, 바닥에서 급상하는 종목)
            //        //if (분전배수차평균 < 5 &&  분간가격상승 > g.v.분배수차[0]이상) // 실제는 분간가격상승
            //        //    ADD = true;


            //        //if (o.돌파정도 == 2)
            //        //    if(분간가격상승 > 150)
            //        //        ADD = true;

            //        //if (ADD && g.add_interest)
            //        //{
            //        //    if (!g.보유종목.Contains(stock))
            //        //    {
            //        //        if (g.호가종목.Contains(stock))
            //        //        {
            //        //            g.호가종목.Remove(stock);
            //        //        }
            //        //        //while (g.호가종목.Count >= 15)
            //        //        //{
            //        //        //    //int t = wk.return_index_of_ogldata(g.호가종목.Last());
            //        //        //    //g.ogl_data[t].draw_shrink = false;
            //        //        //    g.호가종목.RemoveAt(g.호가종목.Count - 1);

            //        //        //}
            //        //        g.호가종목.Insert(0, o.stock); // 첫번째 삽입

            //        //        mc.Sound("","호가종목추가");

            //        //        //if (g.test) // 추후 삭제
            //        //        //{
            //        //        //    ex.testing_매수후일정시간경과결과(g.date, o.stock, x[0, 0], 30);
            //        //        //}
            //        //    }
            //        //}


            //        // 총점 계산


            //        // 가격
            //        if (틱간가격상승 > g.s.틱간가격상승저점)
            //            틱간가격상승 = (틱간가격상승 - g.s.틱간가격상승저점) / g.s.틱간가격상승고점 * g.s.틱간가격상승배점;
            //        if (분간가격상승 > g.s.분간가격상승저점)
            //            분간가격상승 = (분간가격상승 - g.s.분간가격상승저점) / g.s.분간가격상승고점 * g.s.분간가격상승배점;

            //        // 수급
            //        //총점 += 수급연속;
            //        // 체강
            //        //총점 += 체강연속;

            //        // 프로그램
            //        if (프로그램 > 100)
            //            프로그램 = 100;
            //        if (프로그램 < -100)
            //            프로그램 = -100;
            //        프로그램 = 프로그램 / 100.0 * g.s.프로그램배점;

            //        // 분거천
            //        분거천 = (분거천 - g.s.분거천저점) / g.s.분거천고점 * g.s.분거천배점;
            //        if (분거천 > g.s.분거천배점)
            //            분거천 = g.s.분거천배점;

            //        // 배수
            //        if (분전배수차평균 < 5 && 배수차 > 50)
            //            배수차 *= 1.5;
            //        배수차 = (배수차 - g.s.분배수차[0]저점) / g.s.분배수차[0]고점 * g.s.분배수차[0]배점;
            //        if (배수차 > g.s.분배수차[0]배점)
            //            배수차 = g.s.분배수차[0]배점;


            //        배수합 = (배수합 - g.s.분배수합[0]저점) / g.s.분배수합[0]고점 * g.s.분배수합[0]배점;
            //        if (배수합 > g.s.분배수합[0]배점)
            //            배수합 = g.s.분배수합[0]배점;

            //        // 종가기준추정누적거래액_천만원
            //        종가기준추정누적거래액_천만원 = (종가기준추정누적거래액_천만원 - g.s.종가기준추정누적거래액_천만원저점) / g.s.종가기준추정누적거래액_천만원고점 * g.s.종가기준추정누적거래액_천만원배점;
            //        if (종가기준추정누적거래액_천만원 > g.s.종가기준추정누적거래액_천만원배점)
            //            종가기준추정누적거래액_천만원 = g.s.종가기준추정누적거래액_천만원배점;

            //        // 돌파
            //        // 눌림
            //        // 그룹

            //        if (분전배수차평균 < 5)
            //        {
            //            if (분간가격상승 < 50)
            //                분전배수차평균 = 0.0;
            //            else if (분간가격상승 < 75)
            //                분전배수차평균 = 10;
            //            else if (분간가격상승 < 100)
            //                분전배수차평균 = 20.0;
            //            else if (분간가격상승 < 150)
            //                분전배수차평균 = 30.0;
            //            else
            //                분전배수차평균 = 40.0;
            //        }
            //        else
            //            분전배수차평균 = 0.0;

            //        if (수급연속 >= 6)
            //            수급연속 = 5;

            //        if (체강연속 >= 6)
            //            체강연속 = 5;

            //        return 총점 = 틱간가격상승
            //            + 분간가격상승
            //            + 수급연속
            //            + 체강연속
            //            + 프로그램
            //            + 분거천
            //            + 배수차
            //            + 배수합
            //            + 종가기준추정누적거래액_천만원
            //            + 분전배수차평균;
            //    }


            //    public static void eval_score_calculation(g.stock_data o, int[,] x, ref int saved_average)
            //    {
            //        //double factor_market_cap;
            //        //if (o.시총 > 300)
            //        //    factor_market_cap = 1.0;
            //        //else if (o.시총 > 100)
            //        //    factor_market_cap = 0.8;
            //        //else if (o.시총 > 30)
            //        //    factor_market_cap = 0.6;
            //        //else
            //        //    factor_market_cap = 0.4;

            //        // Reset of scores
            //        for (int i = 0; i < o.score.Length; i++) // o.score.Length = 12 currently
            //            o.score[i] = 0;



            //        saved_average = 0;
            //        int number_of_data = 3;
            //        for (int i = 2; i < number_of_data + 2; i++)
            //            saved_average += (x[i, 8] + x[i, 9]) / number_of_data;

            //        int value = 0;
            //        if (g.test)
            //        {
            //            value = x[0, 1];
            //        }
            //        else
            //        {
            //            value = o.틱의가격[0];
            //        }





            //        // DDD
            //        if (x[0, 1] > o.전고) // 전고 이상
            //        {
            //            o.돌파정도 = 1;
            //            if (o.전고시간 < o.전저시간) //돌파
            //            {
            //                o.돌파시간 = x[0, 0];
            //                o.돌파정도 = 2;
            //            }
            //            o.전고시간 = x[0, 0]; // 돌파중
            //            o.전고 = x[0, 1];

            //        }
            //        else // 전고 이하
            //        {
            //            o.돌파정도 = 0;
            //            if (x[0, 1] < o.전고 - 100) // 전고보다 100 이하
            //            {
            //                if (o.전저시간 > o.전고시간)
            //                {
            //                    if (o.전저 > x[0, 1]) // 전고 앞의 기존 전저 갱신`
            //                    {
            //                        o.전저 = x[0, 1];
            //                        o.전저시간 = x[0, 0];
            //                    }
            //                }
            //                else // 전고 앞에 새로운 전저 발생
            //                {
            //                    o.전저 = x[0, 1];
            //                    o.전저시간 = x[0, 0];
            //                }
            //            }
            //            else // 전고부터 전고 100 이하 무시
            //            {
            //            }
            //        }

            //        double total_seconds = ms.total_Seconds(o.돌파시간, x[0, 0]);
            //        double difference = total_seconds - 60.0;
            //        if (difference < 0.0) // total seconds from time to time
            //        {
            //            o.돌파정도 = 2; // 돌파 (초기 돌파 후, 60 초간을 돌파로 가정)
            //        }





            //        for (int i = 1; i < o.score.Length - 2; i++)
            //        {
            //            switch (i)
            //            {
            //                case 1:
            //                    // S1 : 가격 : 틱간급상, 분간급상, 지수주점프&시초,
            //                    if (g.test)
            //                        o.score[1] = x[0, 1] - x[1, 1];
            //                    else
            //                        o.score[1] = o.틱의가격[0] - o.틱의가격[g.array_size / 2 - 1]; // 0900
            //                    break;

            //                case 2: // 수급연속점수
            //                    o.score[2] = x[0, 10];
            //                    break;

            //                case 3: // 체강연속점수
            //                    o.score[3] = x[0, 11];
            //                    break;

            //                case 4: // 거래액 대비 기관 매수 %
            //                    if (x[0, 7] > 0)
            //                        o.score[4] = (int)(x[0, 4] * 100.0 / x[0, 7]);
            //                    break;

            //                case 5: // 거래액 대비 당외량 매수 %
            //                    if (x[0, 7] > 0)
            //                        o.score[5] = (int)(x[0, 5] * 100.0 / x[0, 7]);
            //                    break;

            //                case 6: // 거래액 대비 외인 매수 %
            //                    if (x[0, 7] > 0)
            //                        o.score[6] = (int)(x[0, 6] * 100.0 / x[0, 7]);
            //                    break;

            //                case 7: // ms.분거천 cover both ftesitng and trading 
            //                    {
            //                        o.분거천 = ms.분거천(o, g.time[1]);
            //                    }
            //                    break;

            //                case 8:
            //                case 9:
            //                    if (g.test) // testing
            //                    {
            //                        if (i == 8)
            //                            o.score[8] = x[0, 8] - x[0, 9];
            //                        else
            //                            o.score[9] = x[0, 8] + x[0, 9];
            //                    }
            //                    // DDD
            //                    else // real
            //                    {
            //                        double totalSeconds = ms.total_Seconds(o.틱의시간[g.array_size / 2 - 1], o.틱의시간[0]); // 0900
            //                        double 매수배 = 0.0;
            //                        double 매도배 = 0.0;
            //                        if (totalSeconds > g.EPS)
            //                        {
            //                            double multiple_factor = 60.0 / totalSeconds * 380.0 / o.일평균거래량;
            //                            매수배 = (o.틱매수량[0] - o.틱매수량[g.array_size / 2 - 1]) * multiple_factor; // 0900
            //                            매도배 = (o.틱매도량[0] - o.틱매도량[g.array_size / 2 - 1]) * multiple_factor; // 0900
            //                        }
            //                        if (i == 8)
            //                            o.score[8] = (int)((매수배 - 매도배) * g.TEN);
            //                        else
            //                            o.score[9] = (int)((매수배 + 매도배) * g.TEN);
            //                    }
            //                    break;
            //            }
            //        }
            //    }

            #endregion

        }

        public static void post_코스닥_코스피_프외_순매수_배차_합산_382()
        {
            int index;

            int kospi_leverage = wk.return_index_of_ogldata(g.KODEX4[0]);
            int kospi_inverse = wk.return_index_of_ogldata(g.KODEX4[1]);
            int kosdaq_leverage = wk.return_index_of_ogldata(g.KODEX4[2]);
            int kosdaq_inverse = wk.return_index_of_ogldata(g.KODEX4[3]);

            for (int i = 0; i < 382; i++)
            {
                double sum = 0.0;
                for (int j = 0; j < g.kospi_mixed.stock.Count; j++)
                {
                    var stock = g.kospi_mixed.stock[j];
                    index = wk.return_index_of_ogldata(stock);
                    if (index < 0) continue;

                    g.stock_data t = g.ogl_data[index];
                    double money_factor = t.전일종가 / g.억원;
                    sum += (int)((t.x[i, 4] + t.x[i, 5]) * money_factor);
                }
                g.ogl_data[kospi_leverage].x[i, 3] = (int)sum;
                g.ogl_data[kospi_inverse].x[i, 3] = (int)sum;

                sum = 0.0;
                for (int j = 0; j < g.kosdaq_mixed.stock.Count; j++)
                {
                    var stock = g.kosdaq_mixed.stock[j];
                    index = wk.return_index_of_ogldata(stock);
                    if (index < 0) continue;

                    g.stock_data t = g.ogl_data[index];
                    double money_factor = t.전일종가 / g.억원;
                    sum += (int)((t.x[i, 4] + t.x[i, 5]) * money_factor);
                }
                g.ogl_data[kosdaq_leverage].x[i, 3] = (int)sum;
                g.ogl_data[kosdaq_inverse].x[i, 3] = (int)sum;
            }
        }

        public static void post_코스닥_코스피_프외_순매수_배차_합산()
        {
            g.코스피매수배 = 0;
            g.코스피매도배 = 0;
            g.코스피프외순매수 = 0;

            for (int i = 0; i < g.kospi_mixed.stock.Count; i++)
            {
                int index = wk.return_index_of_ogldata(g.kospi_mixed.stock[i]);
                if (index < 0) continue;

                g.stock_data t = g.ogl_data[index];
                double money_factor = t.전일종가 / g.억원;

                g.코스피프외순매수 += (int)((t.x[t.nrow - 1, 3] + t.x[t.nrow - 1, 5]) * money_factor);
                g.코스피매수배 = (int)(t.x[t.nrow - 1, 8] * g.kospi_mixed.weight[i]);
                g.코스피매도배 = (int)(t.x[t.nrow - 1, 9] * g.kospi_mixed.weight[i]);
            }

            g.코스닥매수배 = 0;
            g.코스닥매도배 = 0;
            g.코스닥프외순매수 = 0;

            for (int i = 0; i < g.kosdaq_mixed.stock.Count; i++)
            {
                int index = wk.return_index_of_ogldata(g.kosdaq_mixed.stock[i]);
                if (index < 0) continue;

                g.stock_data t = g.ogl_data[index];
                double money_factor = t.전일종가 / g.억원;

                g.코스닥프외순매수 += (int)((t.x[t.nrow - 1, 3] + t.x[t.nrow - 1, 5]) * money_factor);
                g.코스닥매수배 = (int)(t.x[t.nrow - 1, 8] * g.kosdaq_mixed.weight[i]);
                g.코스닥매도배 = (int)(t.x[t.nrow - 1, 9] * g.kosdaq_mixed.weight[i]);
            }
        }

        public static void post(g.stock_data o)
        {
            if ((o.stock.Contains("KODEX") && !o.stock.Contains("레버리지")) ||
                o.stock.Contains("KODSEF") ||
                o.stock.Contains("TIGER") ||
                o.stock.Contains("KBSTAR") ||
                o.stock.Contains("HANARO"))
            {
                return;
            }

            int check_row = !g.connected ? Math.Min(g.time[1] - 1, o.nrow - 1) : o.nrow - 1;

            post_minute(o, check_row);

            // Maybe no need to do post_minute(o, check_row)
            // for "KODEX 코스닥150레버리지", "KODEX 레버리지"
            if (o.stock == "KODEX 코스닥150레버리지" || o.stock == "KODEX 레버리지")
            {
                if (g.connected) ev.EvalKODEX(o);
                return;
            }
            
            o.점수.총점 = post_score(o, check_row);

          
        }

        public static void PostPassing(g.stock_data o, int checkRow, bool add)
        {
            bool previousPriceReset = false;

            if (o.x[checkRow, 1] < o.pass.previousPriceHigh)
            {
                // 새로운 가격이 previous high보다 작으면 상태 0
                o.pass.priceStatus = 0;
            }
            else if (add && o.x[checkRow, 1] > o.pass.previousPriceHigh && o.pass.previousPriceLow.HasValue)
            {
                // 새로운 가격이 previous high보다 크고 previous low가 있는 경우
                if (o.x[checkRow, 1] > o.pass.previousPriceLow.Value)
                {
                    //mc.Sound("일반", "done");
                    // previous high가 새로운 가격으로 업데이트되고 previous low가 없어짐
                    o.pass.previousPriceHigh = o.x[checkRow, 1]; previousPriceReset = true;
                    o.pass.previousPriceLow = null;
                    o.pass.priceStatus = 1;
                    if (!g.관심종목.Contains(o.stock) &&
                        !o.stock.Contains("KODEX") &&
                        o.분거래천[0] > 50 &&
                        o.분배수차[0] > 100 &&
                        o.분프로천[0] >= 0 &&
                        g.add_interest)
                    {
                        if (g.관심종목.Count > 2)
                        {
                            g.관심종목.RemoveAt(0);
                        }
                        g.관심종목.Add(o.stock);
                    }
                }
                else
                {
                    // 상태 1, logiaclly not here
                    o.pass.priceStatus = 2;
                }
            }
            else if (o.x[checkRow, 1] > o.pass.previousPriceHigh && !o.pass.previousPriceLow.HasValue)
            {
                // 새로운 가격이 previous high보다 크고 previous low가 없는 경우
                o.pass.previousPriceHigh = o.x[checkRow, 1];
                o.pass.priceStatus = 2;
            }

            // 이전 high와 low 업데이트
            if (o.pass.previousPriceHigh - 50 > o.x[checkRow, 1] &&
                !previousPriceReset) // the previousPrice changed in this routine
            {
                o.pass.previousPriceLow = o.x[checkRow, 1];
            }

            if (o.pass.previousPriceHigh > o.pass.month)
                o.pass.monthStatus = 1;
            if (o.pass.previousPriceHigh > o.pass.quarter)
                o.pass.quarterStatus = 1;
            if (o.pass.previousPriceHigh > o.pass.half)
                o.pass.halfStatus = 1;
            if (o.pass.previousPriceHigh > o.pass.year)
                o.pass.yearStatus = 1;
        }
        // 통계 적용
        public static void post_score_급락(g.stock_data o, int check_row)
        {
            if (g.KODEX4.Contains(o.stock))
                return;

            int drop_count = 0;
            for (int i = check_row - 1; i >= 1; i--)
            {
                if (o.x[i, 1] - o.x[i - 1, 1] < 0) // || o.x[i, 8] - o.x[i, 9] < 0)
                    drop_count++;
                else
                    break;
            }
            double drop_value = o.x[check_row - 1, 1] - o.x[check_row - 1 - drop_count, 1];
            string stock = o.stock;
            o.점수.급락 = drop_value;
        }

        public static void post_score_급상(g.stock_data o, int check_row)
        {
            if (g.KODEX4.Contains(o.stock))
                return;

            int rise_count = 0;
            for (int i = check_row; i >= 1; i--)
            {
                if (i >= g.분_array_size)
                    break;

                if (o.x[i, 1] - o.x[i - 1, 1] > 0) // || o.x[i, 8] - o.x[i, 9] > 0)
                    rise_count++;
                else
                    break;
            }
            double rise_value = o.x[check_row, 1] - o.x[check_row - rise_count, 1];

            o.점수.급상 = rise_value;
        }
        // 통계 적용하지 않은 원본 상태
        public static double post_score_20230824(g.stock_data o)
        {
            // post_score_interpolation(g.s.dev, o.dev, ref o.점수.dev, ref o.점수.총점);
            //post_score_interpolation(g.s.avr, o.avr, ref o.점수.avr, ref o.점수.총점);
            //post_score_interpolation(g.s.mkc, o.x[o.nrow - 1, 10], ref o.점수.mkc, ref o.점수.총점);

            //post_score_interpolation(g.s.돌파, o.pass.pass_level, ref o.점수.돌파, ref o.점수.총점);
            //post_score_interpolation(g.s.눌림, o.x[o.nrow - 1, 10], ref o.점수.눌림, ref o.점수.총점);

            // post_score_interpolation(g.s.급락, o.정도.급락, ref o.점수.급락, ref o.점수.총점);
            //post_score_interpolation(g.s.잔잔, o.정도.잔잔, ref o.점수.잔잔, ref o.점수.총점);

            //post_score_interpolation(g.s.가연, o.x[o.nrow - 1, 10], ref o.점수.가연, ref o.점수.총점);
            //post_score_interpolation(g.s.가분, o.x[o.nrow - 1, 10], ref o.점수.가분, ref o.점수.총점);
            //post_score_interpolation(g.s.가틱, o.x[o.nrow - 1, 10], ref o.점수.가틱, ref o.점수.총점);
            //post_score_interpolation(g.s.가반, o.x[o.nrow - 1, 10], ref o.점수.가반, ref o.점수.총점);
            //post_score_interpolation(g.s.가지, o.x[o.nrow - 1, 10], ref o.점수.가지, ref o.점수.총점);
            //post_score_interpolation(g.s.가위, o.x[o.nrow - 1, 10], ref o.점수.가위, ref o.점수.총점);

            //post_score_interpolation(g.s.수연, o.x[o.nrow - 1, 10], ref o.점수.수연, ref o.점수.총점);
            //post_score_interpolation(g.s.수지, o.x[o.nrow - 1, 10], ref o.점수.수지, ref o.점수.총점);
            //post_score_interpolation(g.s.수위, o.x[o.nrow - 1, 10], ref o.점수.수위, ref o.점수.총점);

            //post_score_interpolation(g.s.강연, o.x[o.nrow - 1, 11], ref o.점수.강연, ref o.점수.총점);
            //post_score_interpolation(g.s.강지, o.x[o.nrow - 1, 11], ref o.점수.강지, ref o.점수.총점);
            //post_score_interpolation(g.s.강위, o.x[o.nrow - 1, 11], ref o.점수.강위, ref o.점수.총점);

            // post_score_interpolation(g.s.3, (o.분프로천[0] + o.분외인천[0]), ref o.점수.프분, ref o.점수.총점); // 프분, 천만원
            //post_score_interpolation(g.s.프틱, o.틱프돈천[0] + o.틱외돈천[0], ref o.점수.프틱, ref o.점수.총점); // 프틱, 천만원 // 20220720
            //post_score_interpolation(g.s.프지, o.틱프돈천[0] + o.틱외돈천[0], ref o.점수.프지, ref o.점수.총점); // 프돈의 지속성
            //post_score_interpolation(g.s.프퍼, o.정도.프퍼, ref o.점수.프퍼, ref o.점수.총점); // 프퍼, 100 percentage
            //post_score_interpolation(g.s.프누, o.프누천, ref o.점수.프누, ref o.점수.총점); // 프돈누적당일
            //                                                                      //post_score_interpolation(10,   g.s.프가,      o.정도.프가,                           ref o.점수.프가, ref o.점수.총점); // 프돈과 가격의 레벨 차이
            // post_score_interpolation(g.s.거분, o.분거래천[0], ref o.점수.거분, ref o.점수.총점);
            //post_score_interpolation(g.s.거틱, o.틱거돈천[0], ref o.점수.거틱, ref o.점수.총점);
            //post_score_interpolation(g.s.거일, o.종거천, ref o.점수.거일, ref o.점수.총점);

            // post_score_interpolation(g.s.배차, o.분배수차[0], ref o.점수.배차, ref o.점수.총점);
            //post_score_interpolation(g.s.배반, o.틱거돈천[0], ref o.점수.배반, ref o.점수.총점);
            // post_score_interpolation(g.s.배합, o.분배수합[0], ref o.점수.배합, ref o.점수.총점);

            return o.점수.총점;


            //int 분간가격상승 = x[0, 1] - x[1, 1];

            //double 분전배수차평균 = 0;
            //int number_of_data = 3;
            //int count = 0;
            //for (int i = 2; i < number_of_data + 1; i++) // 2분전, 3분전, 4분전의 배수차평균
            //{
            //    if (x[i, 0] < 90000)
            //        break;
            //    분전배수차평균 += (x[i, 8] - x[i, 9]);
            //    count++;
            //}
            //if (count > 0)
            //    분전배수차평균 /= count;



            // 잔잔 급상(특히 낮은 가격 즉, 바닥에서 급상하는 종목)
            //if (분전배수차평균 < 1 && 분간가격상승 > g.v.분간가격상승이상) // 실제는 분간가격상승
            //    ADD = true;
            //if (분전배수차평균 < 1 && 분간가격상승 > 100 && g.add_interest)
            //{
            //    ADD = true;
            //    mc.Sound("","silence added");
            //}

            //if (o.틱프돈천[0] > g.v.틱간프로그램매수이상 && g.add_interest)
            //{
            //    ADD = true;
            //    mc.Sound("","program added");
            //}




            //if (!g.보유종목.Contains(o.stock))
            //{
            //    if (g.호가종목.Contains(o.stock))
            //    {
            //        g.호가종목.Remove(o.stock);
            //    }
            //    while (g.호가종목.Count >= 6)
            //    {
            //        g.호가종목.RemoveAt(g.호가종목.Count - 1);

            //    }
            //    g.호가종목.Insert(0, o.stock); // 첫번째 삽입


            //}



            //public static double eval_score(g.stock_data o, int Exist_Lines, int[,] x)
            //{
            //    {
            //        if (x[0, 0] <= 901)
            //            return -10000;
            //        double 총점 = 0.0;

            //        string stock = o.stock;



            //        double 틱간시간 = ms.total_Seconds(o.틱의시간[1], o.틱의시간[0]); // 0900
            //        double 분간시간 = ms.total_Seconds(o.틱의시간[g.array_size / 2], o.틱의시간[0]); // 0900

            //        // 가격
            //        int 가격 = x[0, 1];
            //        double 틱간가격상승 = 0.0;
            //        double 분간가격상승 = 0.0;
            //        if (!g.test)
            //        {
            //            if (틱간시간 > g.EPS)
            //                틱간가격상승 = (o.틱의가격[0] - o.틱의가격[1]) / 틱간시간 * 60.0; // 0900
            //            if (분간시간 > g.EPS)
            //                분간가격상승 = (o.틱의가격[0] - o.틱의가격[o.틱의시간[g.array_size / 2]]) / 분간시간 * 60.0; // from time to time
            //        }
            //        else
            //        {
            //            분간가격상승 = x[0, 1] - x[1, 1];
            //            분간가격상승 = 0.0;
            //        }





            //        // 가격
            //        int 수급 = x[0, 2];
            //        int 수급연속 = x[0, 10];


            //        //체강
            //        int 체강 = x[0, 3];
            //        int 체강연속 = x[0, 11];


            //        // 프로그램(보완)
            //        double 프로그램 = (x[0, 4] - x[1, 4]) * 100.0 / (x[0, 7] - x[1, 7]);


            //        // 분거천
            //        double 분거천 = o.분거천;




            //        // 배수
            //        double 분전배수차평균 = 0;
            //        int number_of_data = 3;
            //        int count = 0;
            //        for (int i = 1; i < number_of_data + 1; i++)
            //        {
            //            if (x[i, 0] < 90000)
            //                break;
            //            분전배수차평균 += (x[i, 8] - x[i, 9]);
            //            count++;
            //        }
            //        if (count > 0)
            //            분전배수차평균 /= count;

            //        double 배수차 = 0;
            //        double 배수합 = 0;
            //        if (g.test) // testing
            //        {
            //            배수차 = x[0, 8] - x[0, 9];
            //            배수합 = x[0, 8] + x[0, 9];
            //        }
            //        else // real
            //        {
            //            double 매수배 = 0.0;
            //            double 매도배 = 0.0;
            //            if (분간시간 > g.EPS)
            //            {
            //                double multiple_factor = 60.0 / 분간시간 * 380.0 / o.일평균거래량;
            //                매수배 = (o.틱매수량[0] - o.틱매수량[g.array_size / 2 - 1]) * multiple_factor; // 0900
            //                매도배 = (o.틱매도량[0] - o.틱매도량[g.array_size / 2 - 1]) * multiple_factor; // 0900
            //            }
            //            배수차 = (int)((매수배 - 매도배) * g.TEN);
            //            배수합 = (int)((매수배 + 매도배) * g.TEN);
            //        }


            //        // 종가기준추정누적거래액_천만원
            //        double 종가기준추정누적거래액_천만원 = o.종가기준추정누적거래액_천만원;



            //        // 돌파



            //        // 눌림



            //        // 그룹






            //        // NO 0 
            //        // 수의 지속 상승, Regresssion 또는 Deviation 점검으로 ...
            //        // 특히, 시초 가, 수, 강의 급상

            //        // NO1 배수 이상 + 분당거래액이상 + 전 배수 조건, 낮은 배수로 진행되다 갑자기 높은 배수로 전환
            //        // 이런 경우 수, 강의 동시 상승
            //        //if (o.score[1] > 150)

            //        //20201108
            //        //if (o.score[2] >= 4)
            //        //    o.score[2] = 4;
            //        //if (o.score[3] >= 4)
            //        //    o.score[3] = 4;
            //        //if (o.score[2] + o.score[3] >= 7 && o.분거천 > 5 && o.돌파정도 > 1)
            //        //    Add = true;



            //        // DDD
            //        //if (전배수차평균 < 5 && o.score[8] > 100 && o.시총 > 10)
            //        //{
            //        //    Add = true; // 중, 대형주는 가 100 이상 점프는 쉽지 않으나 배수차의 100은 가능
            //        //}
            //        //if (o.score[1] > 100 && saved_average < 15 && o.score[8] > 50  // o.score[8] : 배수차, o.score[9] : 배수합
            //        // ) //대북주, 디지털화폐 등 잠잠하다 단체로 급상
            //        //{
            //        //    Add = true;
            //        //}

            //        // NO2 가격 25 이상, 수급 연속, 분당거래액이상
            //        //if (o.score[1] > 25 && o.score[2] >= g.v.편차이상) // 'M'으로 조정
            //        //{
            //        //    Add = true;
            //        //}

            //        // NO3
            //        // 분간가격상승 > 100 && 시총 > 1조
            //        //if (o.score[1] > 100 && o.시총 > 100)
            //        //    Add = true;

            //        // NO4
            //        // 마젠타 그룹 ... 낮은 배수 ... 다시 마젠타

            //        // NO5
            //        // 외인매수 타고 올라가는 종목, 950 이후 외인과 기관의 양매수 종목

            //        // NO6
            //        // 시초 외인이 베팅하는 섹터가 어디인가 ? 20200624 전자, 차 상승하면서 현차, 삼전, 모비스 등 4 - 5% 상승

            //        // NO7
            //        // 그룹의 강세 ... 일부 종목 제외, 씨젠 그룹의 경우도 진메트릭스, 미코, EDGE 등은 타 종목 대비 미약할 수 있음

            //        // NO 8
            //        // 강의 하락은 가의 하락 전조 가능 ... 즉, 배수차가 일방적에서 동등 또는 반전으로 진행되는 지 확인

            //        // NO 9
            //        // 사이안이 지속되면서 가 상승이 지속되면 중, 대형주의 경우 다음 상승을 기대할 수 있다. 

            //        // NO 10
            //        // 바닥에서 배수 반전(100/600, 100/200, 200/100) 급상, 강의 반전(-, -, -, -, +)도 ... 포함됨

            //        // NO 11
            //        // 시초부터 강세인 종목

            //        // NO 12
            //        // 사이안이 먼저 나타나고 다시 마젠타가 나타나면서 가의 상승

            //        // NO 13
            //        // 하락하다 반전

            //        // NO 14
            //        // Group Buy and Sell Control
            //        // Mask, Device, North, Armament, Bank, Ship etc.

            //        // NO 14 
            //        // Tight group syncronization

            //        // NO 15
            //        // 급상하던 종목 눌림목 하락시 매도 후, 다시 돌파시 매수 ... 이 정도 프로그램을 만들면 아주 고수급

            //        // THE MOST IMPORTANT
            //        // SUDDEN CRASH
            //        // SELF SELL

            //        // NO 0
            //        // Cobra이나 가의 급락 중, 수의 상승은 당연 ... 
            //        // 그러나 강의 상승도 떨어지는 종목을 담는 것이지 가의 상승 담보하지 않음, 그러므로 가의 상승은 필수

            //        // 분거천 일정 이상 조건
            //        //if (g.v.분당거래액이상 > o.분거천)
            //        //    Add = false;


            //        // 
            //        //if (o.분거천 > 1 && 분간가격상승 > 100)
            //        //    Add = true;


            //        //Commented Below is good to apply



            // 가격이 2-3분 횡보하면 좋은 조건에 던져라

            // 프외 손실 종목



            //        // 잔잔 급상 (특히 낮은 가격 즉, 바닥에서 급상하는 종목)
            //        //if (분전배수차평균 < 5 &&  분간가격상승 > g.v.분배수차[0]이상) // 실제는 분간가격상승
            //        //    ADD = true;


            //        //if (o.돌파정도 == 2)
            //        //    if(분간가격상승 > 150)
            //        //        ADD = true;

            //        //if (ADD && g.add_interest)
            //        //{
            //        //    if (!g.보유종목.Contains(stock))
            //        //    {
            //        //        if (g.호가종목.Contains(stock))
            //        //        {
            //        //            g.호가종목.Remove(stock);
            //        //        }
            //        //        //while (g.호가종목.Count >= 15)
            //        //        //{
            //        //        //    //int t = wk.return_index_of_ogldata(g.호가종목.Last());
            //        //        //    //g.ogl_data[t].draw_shrink = false;
            //        //        //    g.호가종목.RemoveAt(g.호가종목.Count - 1);

            //        //        //}
            //        //        g.호가종목.Insert(0, o.stock); // 첫번째 삽입

            //        //        mc.Sound("","호가종목추가");

            //        //        //if (g.test) // 추후 삭제
            //        //        //{
            //        //        //    ex.testing_매수후일정시간경과결과(g.date, o.stock, x[0, 0], 30);
            //        //        //}
            //        //    }
            //        //}


            //        // 총점 계산


            //        // 가격
            //        if (틱간가격상승 > g.s.틱간가격상승저점)
            //            틱간가격상승 = (틱간가격상승 - g.s.틱간가격상승저점) / g.s.틱간가격상승고점 * g.s.틱간가격상승배점;
            //        if (분간가격상승 > g.s.분간가격상승저점)
            //            분간가격상승 = (분간가격상승 - g.s.분간가격상승저점) / g.s.분간가격상승고점 * g.s.분간가격상승배점;

            //        // 수급
            //        //총점 += 수급연속;
            //        // 체강
            //        //총점 += 체강연속;

            //        // 프로그램
            //        if (프로그램 > 100)
            //            프로그램 = 100;
            //        if (프로그램 < -100)
            //            프로그램 = -100;
            //        프로그램 = 프로그램 / 100.0 * g.s.프로그램배점;

            //        // 분거천
            //        분거천 = (분거천 - g.s.분거천저점) / g.s.분거천고점 * g.s.분거천배점;
            //        if (분거천 > g.s.분거천배점)
            //            분거천 = g.s.분거천배점;

            //        // 배수
            //        if (분전배수차평균 < 5 && 배수차 > 50)
            //            배수차 *= 1.5;
            //        배수차 = (배수차 - g.s.분배수차[0]저점) / g.s.분배수차[0]고점 * g.s.분배수차[0]배점;
            //        if (배수차 > g.s.분배수차[0]배점)
            //            배수차 = g.s.분배수차[0]배점;


            //        배수합 = (배수합 - g.s.분배수합[0]저점) / g.s.분배수합[0]고점 * g.s.분배수합[0]배점;
            //        if (배수합 > g.s.분배수합[0]배점)
            //            배수합 = g.s.분배수합[0]배점;

            //        // 종가기준추정누적거래액_천만원
            //        종가기준추정누적거래액_천만원 = (종가기준추정누적거래액_천만원 - g.s.종가기준추정누적거래액_천만원저점) / g.s.종가기준추정누적거래액_천만원고점 * g.s.종가기준추정누적거래액_천만원배점;
            //        if (종가기준추정누적거래액_천만원 > g.s.종가기준추정누적거래액_천만원배점)
            //            종가기준추정누적거래액_천만원 = g.s.종가기준추정누적거래액_천만원배점;

            //        // 돌파
            //        // 눌림
            //        // 그룹

            //        if (분전배수차평균 < 5)
            //        {
            //            if (분간가격상승 < 50)
            //                분전배수차평균 = 0.0;
            //            else if (분간가격상승 < 75)
            //                분전배수차평균 = 10;
            //            else if (분간가격상승 < 100)
            //                분전배수차평균 = 20.0;
            //            else if (분간가격상승 < 150)
            //                분전배수차평균 = 30.0;
            //            else
            //                분전배수차평균 = 40.0;
            //        }
            //        else
            //            분전배수차평균 = 0.0;

            //        if (수급연속 >= 6)
            //            수급연속 = 5;

            //        if (체강연속 >= 6)
            //            체강연속 = 5;

            //        return 총점 = 틱간가격상승
            //            + 분간가격상승
            //            + 수급연속
            //            + 체강연속
            //            + 프로그램
            //            + 분거천
            //            + 배수차
            //            + 배수합
            //            + 종가기준추정누적거래액_천만원
            //            + 분전배수차평균;
            //    }


            //    public static void eval_score_calculation(g.stock_data o, int[,] x, ref int saved_average)
            //    {
            //        //double factor_market_cap;
            //        //if (o.시총 > 300)
            //        //    factor_market_cap = 1.0;
            //        //else if (o.시총 > 100)
            //        //    factor_market_cap = 0.8;
            //        //else if (o.시총 > 30)
            //        //    factor_market_cap = 0.6;
            //        //else
            //        //    factor_market_cap = 0.4;

            //        // Reset of scores
            //        for (int i = 0; i < o.score.Length; i++) // o.score.Length = 12 currently
            //            o.score[i] = 0;



            //        saved_average = 0;
            //        int number_of_data = 3;
            //        for (int i = 2; i < number_of_data + 2; i++)
            //            saved_average += (x[i, 8] + x[i, 9]) / number_of_data;

            //        int value = 0;
            //        if (g.test)
            //        {
            //            value = x[0, 1];
            //        }
            //        else
            //        {
            //            value = o.틱의가격[0];
            //        }





            //        // DDD
            //        if (x[0, 1] > o.전고) // 전고 이상
            //        {
            //            o.돌파정도 = 1;
            //            if (o.전고시간 < o.전저시간) //돌파
            //            {
            //                o.돌파시간 = x[0, 0];
            //                o.돌파정도 = 2;
            //            }
            //            o.전고시간 = x[0, 0]; // 돌파중
            //            o.전고 = x[0, 1];

            //        }
            //        else // 전고 이하
            //        {
            //            o.돌파정도 = 0;
            //            if (x[0, 1] < o.전고 - 100) // 전고보다 100 이하
            //            {
            //                if (o.전저시간 > o.전고시간)
            //                {
            //                    if (o.전저 > x[0, 1]) // 전고 앞의 기존 전저 갱신`
            //                    {
            //                        o.전저 = x[0, 1];
            //                        o.전저시간 = x[0, 0];
            //                    }
            //                }
            //                else // 전고 앞에 새로운 전저 발생
            //                {
            //                    o.전저 = x[0, 1];
            //                    o.전저시간 = x[0, 0];
            //                }
            //            }
            //            else // 전고부터 전고 100 이하 무시
            //            {
            //            }
            //        }

            //        double total_seconds = ms.total_Seconds(o.돌파시간, x[0, 0]);
            //        double difference = total_seconds - 60.0;
            //        if (difference < 0.0) // total seconds from time to time
            //        {
            //            o.돌파정도 = 2; // 돌파 (초기 돌파 후, 60 초간을 돌파로 가정)
            //        }





            //        for (int i = 1; i < o.score.Length - 2; i++)
            //        {
            //            switch (i)
            //            {
            //                case 1:
            //                    // S1 : 가격 : 틱간급상, 분간급상, 지수주점프&시초,
            //                    if (g.test)
            //                        o.score[1] = x[0, 1] - x[1, 1];
            //                    else
            //                        o.score[1] = o.틱의가격[0] - o.틱의가격[g.array_size / 2 - 1]; // 0900
            //                    break;

            //                case 2: // 수급연속점수
            //                    o.score[2] = x[0, 10];
            //                    break;

            //                case 3: // 체강연속점수
            //                    o.score[3] = x[0, 11];
            //                    break;

            //                case 4: // 거래액 대비 기관 매수 %
            //                    if (x[0, 7] > 0)
            //                        o.score[4] = (int)(x[0, 4] * 100.0 / x[0, 7]);
            //                    break;

            //                case 5: // 거래액 대비 당외량 매수 %
            //                    if (x[0, 7] > 0)
            //                        o.score[5] = (int)(x[0, 5] * 100.0 / x[0, 7]);
            //                    break;

            //                case 6: // 거래액 대비 외인 매수 %
            //                    if (x[0, 7] > 0)
            //                        o.score[6] = (int)(x[0, 6] * 100.0 / x[0, 7]);
            //                    break;

            //                case 7: // ms.분거천 cover both ftesitng and trading 
            //                    {
            //                        o.분거천 = ms.분거천(o, g.time[1]);
            //                    }
            //                    break;

            //                case 8:
            //                case 9:
            //                    if (g.test) // testing
            //                    {
            //                        if (i == 8)
            //                            o.score[8] = x[0, 8] - x[0, 9];
            //                        else
            //                            o.score[9] = x[0, 8] + x[0, 9];
            //                    }
            //                    // DDD
            //                    else // real
            //                    {
            //                        double totalSeconds = ms.total_Seconds(o.틱의시간[g.array_size / 2 - 1], o.틱의시간[0]); // 0900
            //                        double 매수배 = 0.0;
            //                        double 매도배 = 0.0;
            //                        if (totalSeconds > g.EPS)
            //                        {
            //                            double multiple_factor = 60.0 / totalSeconds * 380.0 / o.일평균거래량;
            //                            매수배 = (o.틱매수량[0] - o.틱매수량[g.array_size / 2 - 1]) * multiple_factor; // 0900
            //                            매도배 = (o.틱매도량[0] - o.틱매도량[g.array_size / 2 - 1]) * multiple_factor; // 0900
            //                        }
            //                        if (i == 8)
            //                            o.score[8] = (int)((매수배 - 매도배) * g.TEN);
            //                        else
            //                            o.score[9] = (int)((매수배 + 매도배) * g.TEN);
            //                    }
            //                    break;
            //            }
            //        }
            //    }



        }
        // 배점 기준 보간으로 배점 계산
        public static void post_score_interpolation(List<List<double>> s,
            double 성적, ref double 획득점수)
        {
            획득점수 = 0; // 초기화

            if (s.Count < 2 || s[0][0] == 0) // less than 3 ... can not interpolate
                return;

            double 배점 = s[0][0];

            double u_ratio = 0;
            double l_ratio = 0;

            int position = s.Count; // the number of array does exceed this number
            for (int i = 1; i < s.Count; i++)
            {
                if (성적 < s[i][0])
                {
                    position = i;
                    break;
                }
            }

            if (position == 1)
                획득점수 = 배점 * s[1][1];
            else if (position == s.Count)
                획득점수 = 배점 * s[s.Count - 1][1];
            else
            {
                double divider = s[position][0] - s[position - 1][0];
                if (divider < g.EPS)
                    획득점수 = 0;
                else
                {
                    l_ratio = (s[position][0] - 성적) / divider;
                    u_ratio = (성적 - s[position - 1][0]) / divider;

                    획득점수 = 배점 * (s[position - 1][1] * l_ratio + s[position][1] * u_ratio);
                }
            }
        }
        // not used
        // 특정조건을 만족하면 관심에 추가
        //public static void marketeye_received_틱프로천_ebb_tide(g.stock_data o)
        //{
        //    if (o.stock.Contains("혼합") || o.stock.Contains("KODEX"))
        //        return;

        //    if (g.add_interest) // g.add_insterest default -> false in glbl.c
        //    {
        //        Form_보조_차트 Form_보조_차트 = (Form_보조_차트)Application.OpenForms["Form_보조_차트"];
        //        if (Form_보조_차트 != null)
        //        {
        //            if (!ev.eval_inclusion(o) || o.틱프로천[0] <= 0) // not included or o.틱프로천[0] <= 0, return
        //                return;

        //            bool add_stock = false;
        //            for (int i = 1; i < g.틱_array_size; i++)
        //            {
        //                if (o.틱프로천[i] < 0)
        //                {
        //                    add_stock = true;
        //                    break;
        //                }
        //            }

        //            if (add_stock)
        //                Form_보조_차트.Form_보조_차트_DRAW(o.stock);
        //        }

        //        // bool add = false;

        //        //if (o.틱프로천[0] + o.틱외인천[0] > 100)
        //        //    add = true;

        //        //if (o.분프로천[0] + o.분외인천[0] > 300)
        //        //    add = true;

        //        //if (o.분거래천[0] > 1000)
        //        //    add = true;

        //        //if (o.pass.pass_level == 2 && o.분배수차[0] > 100 && o.분거래천[0] > 50)
        //        //    add = true;
        //    }
        //}

        public static void post_minute(g.stock_data o, int check_row) // 
        {
            if (o.nrow < 2) // not started download
                return;

            //post_minute_급락(o);
            //post_minute_잔잔(o);

            double money_factor = o.전일종가 / g.천만원;

            // 일 데이터
            o.프누천 = o.x[check_row, 4] * money_factor;
            o.외누천 = o.x[check_row, 5] * money_factor;
            o.기누천 = o.x[check_row, 6] * money_factor;
            o.거누천 = o.x[check_row, 7] * money_factor;
            o.종거천 = o.x[check_row, 7] * money_factor * wk.누적거래액환산율(o.x[check_row, 0]); // 혼합, 계산은 하되 사용하지 않음
            o.매도호가거래액_백만원 = (int)(o.최우선매도호가잔량 * money_factor * 10); 
            o.매수호가거래액_백만원 = (int)(o.최우선매수호가잔량 * money_factor * 10); 

            // 혼합, 아래 계산은 하되 사용하지 않음
            if (g.test)
            {
                double multiple_factor = 0.0;
                if (o.일평균거래량 > 0)
                    multiple_factor = 380.0 / o.일평균거래량;
                else
                    return;

                for (int i = 0; i < g.분_array_size - 1; i++)
                {
                    if (check_row - i - 1 < 0)
                        break;
                    o.분프로천[i] = (int)((o.x[check_row - i, 4] - o.x[check_row - i - 1, 4]) * money_factor); // unit : 천만원
                    o.분외인천[i] = (int)((o.x[check_row - i, 5] - o.x[check_row - i - 1, 5]) * money_factor); // unit : 천만원
                    o.분거래천[i] = (int)((o.x[check_row - i, 7] - o.x[check_row - i - 1, 7]) * money_factor); // unit : 천만원
                    o.분매수배[i] = (int)((o.x[check_row, 8] + o.x[check_row - 1, 8]) * multiple_factor);
                    o.분매도배[i] = (int)((o.x[check_row, 9] + o.x[check_row - 1, 9]) * multiple_factor);
                    o.분배수차[i] = o.x[check_row, 8] - o.x[check_row, 9];
                    o.분배수합[i] = o.x[check_row, 8] + o.x[check_row, 9];
                }
                if (check_row > 1)
                    o.분당가격차 = o.x[check_row, 1] - o.x[check_row - 1, 1];
            }

            else
            {
                if (o.틱의시간[1] == 0)
                    return;
                if (o.일평균거래량 == 0 || o.전일종가 == 0)
                    return;

                int selected_time = g.틱_array_size - 1; // if not found, use the last time 

                double elapsed_seconds = 0;
                for (int i = 1; i < g.틱_array_size; i++) // MDF 2023 0301 g.array_size = 30
                {
                    if (o.틱의시간[i] == 0) // no data for the tick, i.e. not downloaded yet
                    {
                        selected_time = i - 1;
                        break;
                    }

                    elapsed_seconds = mc.total_Seconds(o.틱의시간[i], o.틱의시간[0]); // from time to time
                    if (elapsed_seconds > g.postInterval) // if over 30 seconds elapsed
                    {
                        selected_time = i;
                        break;
                    }
                }

                if (selected_time == 0)
                    selected_time = g.틱_array_size - 1;


                if (selected_time > 0) // more than one tick
                {
                    double total_seconds = mc.total_Seconds(o.틱의시간[selected_time], o.틱의시간[0]); // from time to time
                    if (total_seconds == 0)
                        return;

                    int amount_dealt = o.틱매수량[0] - o.틱매수량[selected_time] +
                                                                o.틱매도량[0] - o.틱매도량[selected_time];
                    if (amount_dealt == 0)
                    {
                        if (!g.관심종목.Contains(o.stock))
                        {
                            //g.관심종목.Add(o.stock);
                            //mc.Sound("일반", "v i");
                        }
                    }
                    int amount_dealt_program = o.틱프로량[0] - o.틱프로량[selected_time];
                    int amount_dealt_foreign = o.틱외인량[0] - o.틱외인량[selected_time]; // 20220720
                    double time_money_factor = o.전일종가 / g.천만원 / total_seconds * 60; // unit : 천만원/분
                    double multiple_factor = 60.0 / total_seconds * 380.0 / o.일평균거래량; // checke o.일평균거래량 != 0

                    o.분프로천[0] = (int)(amount_dealt_program * time_money_factor);
                    o.분외인천[0] = (int)(amount_dealt_foreign * time_money_factor);
                    o.분거래천[0] = (int)(amount_dealt * time_money_factor);
                    o.분매수배[0] = (int)((o.틱매수량[0] - o.틱매수량[selected_time]) * multiple_factor);
                    o.분매도배[0] = (int)((o.틱매도량[0] - o.틱매도량[selected_time]) * multiple_factor);
                    o.분배수차[0] = o.분매수배[0] - o.분매도배[0];
                    o.분배수합[0] = o.분매수배[0] + o.분매도배[0];

                    o.분당가격차 = (int)((o.틱의가격[0] - o.틱의가격[selected_time]) / total_seconds * 60);
                }
            }

            if (o.분거래천[0] > 0)
            {
                o.정도.프퍼 = 100.0 * o.분프로천[0] / o.분거래천[0];
                o.정도.푀퍼 = 100.0 * (o.분프로천[0] + o.분외인천[0]) / o.분거래천[0];
            }
            else
            {
                o.정도.프퍼 = 0.0;
                o.정도.푀퍼 = 0.0;
            }
        }

        public static void post_minute_급락(g.stock_data o) // 
        {
            o.정도.급락 = 0;

            double price_down = 0;
            for (int i = o.nrow - 2; i > 0; i--)
            {
                int price_differ = o.x[o.nrow - 1, 1] - o.x[i, 1];
                if (price_differ < price_down)
                    price_down = price_differ;
                else
                    break;
            }

            double current_price = o.x[o.nrow - 1, 1];

            //if(g.MM < )
            //if (o.정도.급락 > 10)
            //    o.정도.급락 = 10;
        }

        public static void post_minute_잔잔(g.stock_data o) // 
        {
            o.정도.잔잔 = 0;
            int rangeofcalculation = 5;
            int total = 0;
            for (int i = 1; i < rangeofcalculation + 1; i++)
            {
                total += o.분배수차[i];
            }
            if (rangeofcalculation != 0)
                o.정도.잔잔 = total / (double)rangeofcalculation;
        }

        public static int post_minute_직선(g.stock_data o)
        {
            if (o.nrow < 5)
                return -1;

            int numberMinutes = 5;
            if (o.nrow < numberMinutes + 1)
                return 0;

            double[] xVals = new double[5];
            double[] yVals = new double[5];
            double rSquared;
            double yIntercept;
            double slope;

            for (int i = o.nrow - 1 - numberMinutes; i < o.nrow - 1; i++)
            {
                xVals[i] = i - (o.nrow - 1 - numberMinutes);
                yVals[i] = o.x[o.nrow - i, 1];
            }

            ma.LinearRegression(xVals, yVals, out rSquared, out yIntercept, out slope);

            return 1;
        }
    }
}
