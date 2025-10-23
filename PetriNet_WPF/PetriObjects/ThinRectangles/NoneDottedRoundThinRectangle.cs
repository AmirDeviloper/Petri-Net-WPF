using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.TextFormatting;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;

namespace AppGUI_WPF.PetriObjects
{
    public class NoneDottedRoundThinRectangle : ThinRectangle
    {
        
        public NoneDottedRoundThinRectangle(ref int last_id, Canvas canvas) : base(ref last_id, 0.5D, canvas)
        {

        }

    }
}
