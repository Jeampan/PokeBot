using System.Drawing;
using System.Windows.Forms;


namespace PokemonGo.RocketAPI.Logic
{
    public partial class PokemonSummary : UserControl
    {
        public PokemonSummary(Bitmap image, string cp, string percent)
        {
            InitializeComponent();
           
            this.image.Image = image;
            this.lblCP.Text = cp;
            this.lblPercent.Text = percent;
        }
    }
}
