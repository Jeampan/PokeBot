using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;

namespace PokemonGo.RocketAPI
{
    public class Narrator
    {
        private SpeechSynthesizer _Synthesizer;

        public Narrator(int p_Volume, int p_Speed)
        {
            var Narrator = new System.Speech.Synthesis.SpeechSynthesizer();
            Narrator.Volume = p_Volume;
            Narrator.Rate = p_Speed;

            _Synthesizer = Narrator;
        }

        public void Speak(string p_Message)
        {
            _Synthesizer.SpeakAsyncCancelAll();
            _Synthesizer.SpeakAsync(p_Message);
        }
    }

    

}
