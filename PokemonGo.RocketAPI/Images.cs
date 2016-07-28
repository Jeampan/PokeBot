using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PokemonGo.RocketAPI
{
    class Images
    {
        public static Bitmap GetPokemonImage(int pokemonId)
        {
            var Sprites = AppDomain.CurrentDomain.BaseDirectory + "Sprites\\";
            string location = Sprites + pokemonId + ".png";
            if (!Directory.Exists(Sprites))
                Directory.CreateDirectory(Sprites);
            if (!File.Exists(location))
            {
                WebClient wc = new WebClient();
                wc.DownloadFile("http://pokeapi.co/media/sprites/pokemon/" + pokemonId + ".png", @location);
            }

            Bitmap original = (Bitmap)Image.FromFile(location);

            if(original.Size.Height > 48)
            {
                original = new Bitmap(original, new Size((int)Math.Round(original.Width / 1.5), (int)Math.Round(original.Height / 1.5)));

            }

            return original;

        }
    }
}
