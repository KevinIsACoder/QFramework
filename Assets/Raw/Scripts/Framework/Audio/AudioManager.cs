using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using XLua;

namespace CCM
{
    [XLua.LuaCallCSharp]
    public class AudioManager : MonoBehaviour
    {
        public static float soundVolumeFactor = 0.6f;
        public bool forceMute;

        private bool m_enableMusic = true;
        private bool m_enableSound = true;
        private bool m_isPaused;

        private float m_musicVolume = 1f;
        private float m_soundVolume = 1f;
        private float m_bgVolume = 1f;

        private AudioSource m_bgAS;
        private AudioSource m_effectAS;
        private List<AudioSource> m_slotAS;

        private struct CrossFadeSoundParam
        {
            public AudioSource source;
            public AudioClip clip;
            public float volume;
            public float fadeLength;
        }

        //=========================================================================
        // 成员函数
        //=========================================================================
        private static AudioManager m_instance = null;
        public static AudioManager GetInstance()
        {
            //m_instance = Common.AddTSRGameObject("AudioManager", "CCM.AudioManager") as AudioManager;

            return m_instance;
        }

        public bool Music
        {
            get { return m_enableMusic; }
            set { m_enableMusic = value; m_bgAS.mute = !value; }
        }

        public float MusicVolume
        {
            get { return m_musicVolume; }
            set { m_musicVolume = value; m_bgAS.volume = m_musicVolume * m_bgVolume; }
        }

        public bool Sound
        {
            get { return m_enableSound; }
            set { m_enableSound = value; }
        }

        public float SoundVolume
        {
            get { return m_soundVolume; }
            set { m_soundVolume = value; }
        }

        public bool Mute
        {
            get { return (AudioListener.volume == 0f); }
            set { AudioListener.volume = (!value && !forceMute) ? 1f : 0f; }
        }

        private void Awake()
        {
            Mute = false;
            m_effectAS = gameObject.AddComponent<AudioSource>();
            m_effectAS.loop = false;
            m_bgAS = gameObject.AddComponent<AudioSource>();
            m_bgAS.loop = true;
            m_slotAS = new List<AudioSource>();

            gameObject.AddComponent<AudioListener>();

            m_instance = this;
        }

        private void OnDestroy()
        {
            m_instance = null;
        }

        //播放=====================================================================
        public void Play(string name, float volume)
        {
            if (m_enableSound)
            {
                StartCoroutine(DoPlay(name, m_effectAS, true, (volume * SoundVolume) * soundVolumeFactor, 0f, false));
            }
        }

        public void Play(string name, int count, float volume)
        {
            string str = RandomAudioClip(name, count);
            Play(str, volume);
        }

        public void Play(string name, int count, AudioSource source, float volume = 1f)
        {
            string str = RandomAudioClip(name, count);
            StartCoroutine(DoPlay(str, source, true, (volume * SoundVolume) * soundVolumeFactor, 0f, false));
        }

        //播放完需要手动回收
        public AudioSource PlayInSlot(string name, bool isLoop = true, float volume = 1f)
        {
            if (!m_enableSound)
            {
                return null;
            }
            int num = -1;
            for (int i = 0; i < m_slotAS.Count; i++)
            {
                if (m_slotAS[i].clip == null)
                {
                    num = i;
                    break;
                }
            }
            if (num == -1)
            {
                m_slotAS.Add(gameObject.AddComponent<AudioSource>());
                num = m_slotAS.Count - 1;
            }
            m_slotAS[num].loop = isLoop;
            StartCoroutine(DoPlay(name, m_slotAS[num], false, volume * SoundVolume, 0f, true));
            return m_slotAS[num];
        }

        //只播放一次，自动回收
        public void PlayOneShot(string name, float volume = 1f)
        {

            if (!m_enableSound)
            {
                return;
            }
            int num = -1;
            for (int i = 0; i < m_slotAS.Count; i++)
            {
                if (m_slotAS[i].clip == null)
                {
                    num = i;
                    break;
                }
            }
            if (num == -1)
            {
                m_slotAS.Add(gameObject.AddComponent<AudioSource>());
                num = m_slotAS.Count - 1;
            }
            StartCoroutine(DoOncePlay(name, m_slotAS[num], volume * SoundVolume));
        }

        //播放背景音乐
        public void PlayBackground(string name, float volume = 0.35f, float fadeLength = 6)
        {
            m_bgVolume = volume;
            StartCoroutine(DoPlay(name, m_bgAS, false, m_bgVolume * MusicVolume, fadeLength, false));
        }

        //暂停播放=================================================================
        public void PauseAllSlot()
        {
            foreach (AudioSource source in m_slotAS)
            {
                source.Pause();
            }
        }

        public void PauseAll()
        {
            m_isPaused = true;
            PauseBackground();
            PauseAllSlot();
        }

        public void PauseBackground()
        {
            m_bgAS.Pause();
        }

        //恢复播放=================================================================
        public void ResumeBackground()
        {
            m_bgAS.Play();
        }

        public void ResumeAllSlot()
        {
            foreach (AudioSource source in m_slotAS)
            {
                if ((source.clip != null) && !source.isPlaying)
                {
                    source.Play();
                }
            }
        }

        public void ResumeAll()
        {
            m_isPaused = false;
            ResumeBackground();
            ResumeAllSlot();
        }

        //临时修改背景音乐播放音量
        public void ModifyBackgroundVolume(float volume)
        {
            m_bgAS.volume = volume * MusicVolume;
        }

        //恢复播放音量
        public void RestoreBackgroundVolume()
        {
            m_bgAS.volume = m_bgVolume * MusicVolume;
        }


        //停止播放=====================================================================
        public void StopSoundEffect()
        {
            m_effectAS.Stop();
        }

        public void StopBySlot(int slot)
        {
            if ((slot >= 0) && (slot < m_slotAS.Count))
            {
                m_slotAS[slot].Stop();
                m_slotAS[slot].clip = null;
            }
        }

        public void StopAllSlot()
        {
            foreach (AudioSource source in m_slotAS)
            {
                source.Stop();
                source.clip = null;
            }
        }

        public void StopAll()
        {
            StopBackground();
            StopAllSlot();
        }

        public void StopBackground()
        {
            m_bgAS.Stop();
            m_bgAS.clip = null;
        }

        public void StopBackground(float fadeLength)
        {
            if (fadeLength > 0f)
            {
                CrossFadeSoundParam param = new CrossFadeSoundParam
                {
                    source = m_bgAS,
                    clip = null,
                    volume = 0f,
                    fadeLength = fadeLength
                };
                StopCoroutine("CrossFadeSound");
                StartCoroutine("CrossFadeSound", param);
            }
            else
            {
                StopBackground();
            }
        }

        //静音游戏对像下的音源
        public void MuteSound(GameObject obj)
        {
            AudioSource[] componentsInChildren = obj.GetComponentsInChildren<AudioSource>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                componentsInChildren[i].mute = true;
            }
        }

        public float GetSlotAudioLength(int slot)
        {
            if ((slot < 0) || (slot >= m_slotAS.Count))
            {
                return -1f;
            }
            if (m_slotAS[slot].clip == null)
            {
                return 0f;
            }
            return m_slotAS[slot].clip.length;
        }

        //=========================================================================
        string RandomAudioClip(string name, int count)
        {
            if (count < 2)
            {
                return name;
            }
            return string.Format("{0}{1}", name.Substring(0, name.Length - 1), UnityEngine.Random.Range(1, count + 1));
        }

        [DebuggerHidden]
        IEnumerator CrossFadeSound(CrossFadeSoundParam param)
        {
            AudioSource source = param.source;
            if(source == null) yield return 0;
            AudioClip clip = param.clip;
            float volume = param.volume;
            float fadeLength = param.fadeLength;
            float origVolumn = source.volume;
            float fadeDuration = fadeLength / 2f;
            float curLength = fadeDuration;

            while (curLength > 0)
            {
                curLength -= Time.deltaTime;
                source.volume = (curLength / fadeDuration) * origVolumn;
                yield return 0;
            }

            source.volume = 0f;
            source.clip = clip;
            if (!m_isPaused && clip != null)
            {
                source.Play();
            }
            curLength = 0f;

            while (curLength < fadeDuration)
            {
                curLength += Time.deltaTime;
                source.volume = (curLength / fadeDuration) * volume;
                yield return 0;
            }

            source.volume = volume;
        }

        IEnumerator DoPlay(string name, AudioSource source, bool isOneShot, float volume, float fadeLength, bool forceImmediate = false)
        {
            AudioClip clip = null;
            bool isLoading = false;
            CrossFadeSoundParam param;

            if (!string.IsNullOrEmpty(name))
            {
                clip = Spark.Assets.LoadAsset<AudioClip>(name);
                //clip = ResManager.GetInstance().Load(ResType.Audio, name) as AudioClip;
                isLoading = true;
            }

            //yield return isLoading;


            if ((clip != null) && (source != null))
            {
                if (isOneShot)
                {
                    source.PlayOneShot(clip, volume);
                }
                else if (fadeLength > 0f)
                {
                    if ((source.clip == null) || (source.clip.name != clip.name))
                    {
                        param = new CrossFadeSoundParam();
                        param.source = source;
                        param.clip = clip;
                        param.volume = volume;
                        param.fadeLength = fadeLength;
                        StopCoroutine("CrossFadeSound");
                        StartCoroutine("CrossFadeSound", param);
                    }
                }
                else
                {
                    source.clip = clip;
                    if (!m_isPaused)
                    {
                        source.Play();
                    }
                }
            }

            yield break;
        }

        IEnumerator DoOncePlay(string name, AudioSource source, float volume)
        {
            //AudioClip clip = ResManager.GetInstance().Load(ResType.Audio, name) as AudioClip;
            AudioClip clip = Spark.Assets.LoadAsset<AudioClip>(name);
            float audioTime = 0;
            if ((clip != null) && (source != null))
            {
                audioTime = clip.length;
                source.clip = clip;
                source.PlayOneShot(clip, volume);
            }
            yield return new WaitForSeconds(audioTime);
            if (source != null)
            {
                source.Stop();
                source.clip = null;
            }
        }
    }
}