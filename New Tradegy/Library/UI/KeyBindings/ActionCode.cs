
using New_Tradegy.Library.Core;
using New_Tradegy.Library.PostProcessing;
using New_Tradegy.Library.Trackers;
using New_Tradegy.Library.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace New_Tradegy.Library.UI.KeyBindings
{
    // usage 
    // var action = ActionCode.New(clear: true, post: true, eval: true, draw: 'm');
    // action.Run();
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
            // no need to used Clear 
            if (Clear)
            {
            }

            if (Post)
            {
                if (g.test)
                    PostProcessor.post_test();
            }


            if (Eval)
                RankLogic.EvalStock();

            if (DrawTarget == 'm' || DrawTarget == 'B')
                g.ChartMainRenderer.RefreshMainChart();


            if (DrawTarget == 's' || DrawTarget == 'B')
            {
                Form_보조_차트 Form_보조_차트 = (Form_보조_차트)Application.OpenForms["Form_보조_차트"];
                if (Form_보조_차트 != null)
                {
                    Form_보조_차트.Form_보조_차트_DRAW();
                }
            }
        }
    }
}


