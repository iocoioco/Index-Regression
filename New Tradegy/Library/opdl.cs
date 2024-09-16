using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Tradegy.Library
{
    internal class op
    {
        private static CPTRADELib.CpTd0311 _cptd0311; //주문(현금 주문) 데이터를 요청
        //public static void dgv2_updated_trade_exec()
        //{
        //    for (int i = g.매매대기.Count - 1; i >= 0; i--)
        //    {
        //        // 실행조건

        //        _cptd0311.SetInputValue(1, g.Account);  // 계좌번호
        //        _cptd0311.SetInputValue(2, "01");     // 상품구분코드

        //        if (g.매매대기[i].buy_or_sell == "매수")
        //            _cptd0311.SetInputValue(0, "2");    // 2:매수
        //        else
        //            _cptd0311.SetInputValue(0, "1");    // 1:매도

        //        _cptd0311.SetInputValue(3, g.매매대기[i].code);  // 종목코드
        //        _cptd0311.SetInputValue(4, g.매매대기[i].number);  // 주문수량 
        //        _cptd0311.SetInputValue(5, g.매매대기[i].price);    // 매수단가
        //        _cptd0311.SetInputValue(7, "0'");  // 주문조건구분("0":없음,"1":IOC, "2":FOK)
        //        _cptd0311.SetInputValue(8, "01");    // 주문호가구분("01":보통,"02":임의,"03":시장가,"05":조건부지정가 etc)

        //        int check = _cptd0311.BlockRequest();

        //        g.매매대기.RemoveAt(i);
        //    }
        //}

    }
}
