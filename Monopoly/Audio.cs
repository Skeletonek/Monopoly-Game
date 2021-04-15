using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Monopoly
{
    public class Audio
    {
        public bool active = true;
        public MediaPlayer music = new MediaPlayer();
        public string musicfile = @"Resources\music_wait.mp3";
        public MediaPlayer sfx = new MediaPlayer();
        public string sfxfile = @"Resources\music_wait.mp3";
    }
}
