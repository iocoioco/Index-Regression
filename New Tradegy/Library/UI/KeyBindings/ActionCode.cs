
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Tradegy.Library.UI.KeyBindings
{
    public class ActionCode
    {
        public bool Clear { get; }
        public bool Post { get; }
        public bool Eval { get; }
        public char DrawTarget { get; }

        public ActionCode(bool clear, bool post, bool eval, char draw)
        {
            Clear = clear;
            Post = post;
            Eval = eval;
            DrawTarget = draw;
        }

        public static ActionCode New(bool clear = false, bool post = false, bool eval = false, char draw = ' ')
        {
            return new ActionCode(clear, post, eval, draw);
        }

        public void Run()
        {
            if (Clear) mm.ClearChartAreaAndAnnotations(g.ChartManager.Chart1, g.clickedStock);
            if (Post) ps.post_test();
            if (Eval) ev.eval_stock();

            if (DrawTarget == 'm' || DrawTarget == 'B')
                mm.ManageChart1();
            if (DrawTarget == 's' || DrawTarget == 'B')
                mm.ManageChart2();
        }
    }

}


// usage 
// var action = ActionCode.New(clear: true, post: true, eval: true, draw: 'm');
// action.Run();