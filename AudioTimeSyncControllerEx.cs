using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AudioVisualizer
{
    internal class AudioTimeSyncControllerEx : AudioTimeSyncController
    {
        public AudioSource GetAudioSource()
        {
            return _audioSource;
        }
    }
}
