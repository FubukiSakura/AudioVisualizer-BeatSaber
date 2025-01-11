using AudioVisualizer.Views;
using SiraUtil;
using System;
using System.Linq;
using UnityEngine;
using Zenject;
using static AudioSpectrum;

namespace AudioVisualizer.Installers
{
    public class AudioVisualizerGameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<SettingViewController>().FromNewComponentAsViewController().AsSingle().NonLazy();
            foreach (var bandType in Enum.GetValues(typeof(AudioSpectrum.BandType)).OfType<AudioSpectrum.BandType>()) {
                var audio = new GameObject("AudioSpectrumBand", typeof(AudioSpectrum)).GetComponent<AudioSpectrum>();
                audio.Band = bandType;
                this.Container.Bind<AudioSpectrum>().WithId(bandType).FromInstance(audio);
            }
            this.Container.BindInterfacesAndSelfTo<AudioVisualizerController>().AsCached().NonLazy();
        }
    }
}
