using AudioVisualizer.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

using static AudioSpectrum;

namespace AudioVisualizer
{
    public class AudioVisualizerController : IInitializable, IDisposable
    {
        public List<GameObject> cubes = new List<GameObject>();
        public GameObject AudioVisualizerParent01;
        public GameObject audiospectrum;
        public float scale = 30.0f;
        public float fallSpeed = 0.3f;
        public float sensibility = 10.0f;
        public int SpectrumSize = 31;
        public BandType type = BandType.ThirtyEXOneBand;
        public float radius = 2.0f;
        public float SpectrumShif = 0.0f;
        private GameObject AVParent;
        public GameObject BasePrefub_Cube;

        public void Initialize()
        {
            // For this particular MonoBehaviour, we only want one instance to exist at any time, so store a reference to it in a static property
            //   and destroy any that are created while one already exists.

            this.AVParent = new GameObject("AudioVisualizerParent");
            this.VisualizerSetup();

            PluginConfig.Instance.OnConfigChanged += this.OnConfigChanged;
            SceneManager.activeSceneChanged += this.OnActiveSceneChanged;
            _audiospectrum.UpdatedRawSpectrums += this.OnUpdatedRawSpectrums;
        }

        private bool _disposedValue;
        private AudioSpectrum _audiospectrum;
        private DiContainer _container;

        [Inject]
        public void Construct([Inject(Id = AudioSpectrum.BandType.ThirtyOneBand)] AudioSpectrum audioSpectrum, DiContainer diContainer)
        {
            this._audiospectrum = audioSpectrum;
            this._container = diContainer;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    PluginConfig.Instance.OnConfigChanged -= this.OnConfigChanged;
                    SceneManager.activeSceneChanged -= this.OnActiveSceneChanged;
                    _audiospectrum.UpdatedRawSpectrums -= this.OnUpdatedRawSpectrums;
                }
                this._disposedValue = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void VisualizerSetup()
        {
            audiospectrum = new GameObject("audiospectrum");

            _audiospectrum.bandType = type;
            _audiospectrum.fallSpeed = fallSpeed;
            _audiospectrum.sensibility = sensibility;

            AudioVisualizerParent01 = new GameObject("AudioVisualizerParent01");
            AudioVisualizerParent01.transform.SetParent(AVParent.transform);

            //GameObject BasePrefub_Cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Shader _shader = GetShader("Custom/Glowing");
            Shader _shaderStandard = GetShader("Standard");


            audiospectrum.transform.SetParent(AVParent.transform);

            var oneCycle = 2.0f * Mathf.PI;

            for (int i = 0; i < SpectrumSize; i++)
            {
                //GameObject obj = _container.InstantiatePrefab(BasePrefub_Cube, Vector3.zero, Quaternion.identity, AVParent.transform);
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);

                GameObject cubeParent = new GameObject("cubeParent");

                //　虹色生成
                float H = (float)i / (float)SpectrumSize;
                Color color = Color.HSVToRGB(H, 1.0f, 1.0f);
                MeshRenderer renderer2 = obj.GetComponent<MeshRenderer>();
                renderer2.material = new Material(_shader);
                renderer2.material.SetColor("_Color", color);

                //　生成したキューブを子にして、キューブの中心点をずらす
                cubeParent.transform.SetParent(AVParent.transform);
                obj.transform.SetParent(cubeParent.transform);
                obj.transform.localPosition = new Vector3(0, 0.1f, 0);
                //　キューブを細長くする
                obj.transform.localScale = new Vector3(0.1f, 0.4f, 0.1f);

                //　円形に配置
                var point = ((float)i / SpectrumSize) * oneCycle; // 周期の位置 (1.0 = 100% の時 2π となる)
                var repeatPoint = point * 1.0f; // 繰り返し位置
                var x = Mathf.Cos(repeatPoint) * radius;
                var y = Mathf.Sin(repeatPoint) * radius;
                var position = new Vector3(x, 0, y);

                cubeParent.transform.localPosition = position;
                cubeParent.transform.localEulerAngles = new Vector3(0.0f, 360.0f / SpectrumSize * -i, -80.0f);

                cubes.Add(cubeParent);

            }

            //　すべてのキューブの親（AVParent）の子にする。
            AVParent.transform.SetParent(AVParent.transform);

            for (int i = 0; i < cubes.Count; i++)
            {
                cubes[i].transform.SetParent(AVParent.transform);
            }

        }

        private void OnUpdatedRawSpectrums(AudioSpectrum obj)
        {

           this.UpdateAudioSpectrums(obj);
        }


        private void UpdateAudioSpectrums(AudioSpectrum audio)
        {

            //　オーディオビジュアライザーを動かす
            for (int i = 0; i < cubes.Count; i++)
            {
                var cube = cubes[i];
                var localScale = cube.transform.localScale;
                //float value = audiospectrum.GetComponent<AudioSpectrum>().MeanLevels[i] * scale;
                float value = audio.PeakLevels[i] * scale;
                //float value = audiospectrum.GetComponent<AudioSpectrum>().Levels[i] * scale;

                localScale.y = value + 0.1f;
                cube.transform.localScale = localScale;

            }

            //　オーディオビジュアライザー全体を動かす
            //　9番目（PeakLevels[8]）のスペクトラムを使う
            //　PeakLevels[8]はBASS付近の音。
            var AVP_Scale = AVParent.transform.localScale;
            var AVP_Angle = AVParent.transform.localEulerAngles;
            float AVP_Value = audio.PeakLevels[8] * scale;

            //　サイズ
            AVP_Scale.x = 1.0f + AVP_Value * 0.1f;
            AVP_Scale.y = 1.0f + AVP_Value * 0.1f;
            AVP_Scale.z = 1.0f + AVP_Value * 0.1f;
            AVParent.transform.localScale = AVP_Scale;

            //　回転
            AVP_Angle.y = AVP_Angle.y - AVP_Value * 0.1f;
            if(AVP_Angle.y < -360.0f)
            {
                AVP_Angle.y -= 360.0f;
            }
            AVParent.transform.localEulerAngles = AVP_Angle;

            //　360.0f / SpectrumSize分回転したら一つずらす
            SpectrumShif = SpectrumShif + AVP_Value * 0.1f;
            if(SpectrumShif >= 360.0f / SpectrumSize)
            {
                SpectrumShif = SpectrumShif - 360.0f / SpectrumSize;
                cubes.Insert(0, cubes[cubes.Count - 1]);
                cubes.RemoveAt(cubes.Count - 1);
            }

        }

        public static Shader GetShader(string name)
        {
            var shaderes = Resources.FindObjectsOfTypeAll<Shader>();
            Shader shader = shaderes.FirstOrDefault(x => x.name == name);
            return shader;
        }

        private void OnConfigChanged()
        {
            if (SceneManager.GetActiveScene().name == "GameCore") {
                this.AVParent.SetActive(PluginConfig.Instance.ShowGame);
            }
            else {
                this.AVParent.SetActive(PluginConfig.Instance.ShowMenu);
            }
        }

        private void OnActiveSceneChanged(Scene arg0, Scene arg1)
        {
            if (arg1.name == "GameCore") {
                this.AVParent.SetActive(PluginConfig.Instance.ShowGame);
            }
            else {
                this.AVParent.SetActive(PluginConfig.Instance.ShowMenu);
            }
        }

        public static T GetValue<T>(object obj, string name)
        {

            FieldInfo field = obj.GetType().GetField(
                name,
                BindingFlags.NonPublic |
                BindingFlags.Public |
                BindingFlags.Instance
            );

            return (T)field.GetValue(obj);

        }
        public static void SetValue<T>(object obj, string name, T value)
        {

            FieldInfo field = obj.GetType().GetField(
                name,
                BindingFlags.NonPublic |
                BindingFlags.Public |
                BindingFlags.Instance
            );

            field.SetValue(obj, value);

        }
    }
}
