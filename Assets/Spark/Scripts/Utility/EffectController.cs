using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using DG.Tweening;

#if !MOON_ART
using Spark;
[XLua.LuaCallCSharp]
[ExecuteInEditMode]
public class EffectController : MonoBehaviour, IResettable
#else
[ExecuteInEditMode]
public class EffectController : MonoBehaviour
#endif
{
    [SerializeField]
    private bool m_FullScreen;
	[SerializeField]
	private bool m_Loop = false;

	[SerializeField]
	private float m_Duration = 0f;

	[SerializeField]
	private Animator[] m_Animators;
	[SerializeField]
	private List<int> m_AnimatorStateNameHashList = new List<int>();
	[SerializeField]
	private Animation[] m_Animations;
	[SerializeField]
	private TrailRenderer[] m_TrailRenderers;
	[SerializeField]
	private ParticleSystem[] m_ParticleSystems;
	[SerializeField]
	private Particle2DUGUI[] m_Particle2DUGUIs;
	[SerializeField]
	private List<DOTweenAnimation> m_DOTweenAnimations = new List<DOTweenAnimation>();

    private void Awake()
    {
        if (m_FullScreen && Application.isPlaying)
        {
            var scale = transform.localScale;
            float s = Screen.width / (float)Screen.height;
            float n = 576.0f / 1024.0f;
            if (s > n)
            {
                scale.x *= s / n;
            }
            else
            {
                scale.z *= n / s;
            }
            transform.localScale = scale;
        }
		if (Application.isPlaying) {
			foreach (DOTweenAnimation animation in m_DOTweenAnimations) {
				if (animation != null) {
					animation.autoKill = false;
					animation.autoPlay = false;
				}
			}
		}
    }
    public bool loop
	{
		get
		{
			return m_Loop;
		}
	}

	public float duration
	{
		get
		{
			return m_Duration;
		}
	}

	public void Play()
	{
		for (int i = 0, count = m_Animators.Length; i < count; i++) {
			var animator = m_Animators[i];
			if (animator != null) {
				animator.Play(m_AnimatorStateNameHashList[i], -1, 0f);
			}
		}
		foreach (Animation animation in m_Animations) {
			if (animation != null) {
				animation.Play();
			}
		}
		foreach (TrailRenderer renderer in m_TrailRenderers) {
			if (renderer != null) {
				renderer.Clear();
			}
		}
		foreach (ParticleSystem particle in m_ParticleSystems) {
			if (particle != null) {
				particle.Simulate(0, false);
				particle.Play();
			}
		}
		foreach (Particle2DUGUI particle2D in m_Particle2DUGUIs) {
			if (particle2D != null) {
				particle2D.ResetParticle();
				if (particle2D.playOnAwake && particle2D.delayPlay > 0f) {
					if (particle2D.IsInvoking()) {
						particle2D.CancelInvoke();
					}
					particle2D.Invoke("Play", particle2D.delayPlay);
				} else {
					particle2D.Play();
				}
			}
		}
		foreach (DOTweenAnimation animation in m_DOTweenAnimations) {
			if (animation != null) {
				if (animation.tween != null) {
					animation.tween.Kill(true);
					animation.tween = null;
				}
				animation.CreateTween();
				if (animation.tween != null) {
					animation.tween.SetAutoKill(true).Play();
				}
			}
		}
	}

	private void Stop()
	{
		foreach (Animator animator in m_Animators) {
			if (animator != null) {
				animator.speed = 0;
			}
		}
		foreach (Animation animation in m_Animations) {
			if (animation != null) {
				animation.Stop();
			}
		}
		foreach (TrailRenderer renderer in m_TrailRenderers) {
			if (renderer != null) {
				renderer.Clear();
			}
		}
		foreach (ParticleSystem particle in m_ParticleSystems) {
			if (particle != null) {
				particle.Simulate(0, false);
			}
		}
		foreach (Particle2DUGUI particle2D in m_Particle2DUGUIs) {
			if (particle2D != null) {
				if (particle2D.IsInvoking()) {
					particle2D.CancelInvoke();
				}
				particle2D.Stop();
			}
		}
		foreach (DOTweenAnimation animation in m_DOTweenAnimations) {
			if (animation != null) {
				if (animation.tween != null) {
					animation.tween.Kill(true);
				}
			}
		}
	}
	public void ShowParticle()
	{
		gameObject.SetActive(true);
	}
	public void HideParticle()
	{
		gameObject.SetActive(false);
	}
	void OnDestroy()
	{
		foreach (DOTweenAnimation animation in m_DOTweenAnimations) {
			if (animation != null) {
				if (animation.tween != null) {
					animation.tween.Kill(true);
					animation.tween = null;
				}
			}
		}
	}

#if !MOON_ART
	void IResettable.Reset()
	{
#if UNITY_EDITOR
		foreach (DOTweenAnimation animation in m_DOTweenAnimations) {
			if (animation != null) {
				animation.autoKill = false;
				animation.autoPlay = false;
			}
		}
#endif
	}
#endif

#if UNITY_EDITOR
	void Update()
	{
		if (Application.isPlaying)
			return;

		m_Loop = false;
		m_Duration = 0;
		m_Animators = GetComponentsInChildren<Animator>(true);
		m_Animations = GetComponentsInChildren<Animation>(true);
		m_TrailRenderers = GetComponentsInChildren<TrailRenderer>(true);
		m_ParticleSystems = GetComponentsInChildren<ParticleSystem>(true);
		m_Particle2DUGUIs = GetComponentsInChildren<Particle2DUGUI>(true);
		m_DOTweenAnimations.Clear();
		var tweenAnimations = GetComponentsInChildren<DOTweenAnimation>(true);
		for (int i = 0; i < tweenAnimations.Length; i ++) {
			var tween = tweenAnimations[i];
			if (tween != null) {
				#if !MOON_ART
					if (!(tween is UIViewOpenAnimation || tween is UIViewCloseAnimation)) {
						m_DOTweenAnimations.Add(tween);
					}
				#endif
			}
		}
		foreach (ParticleSystem particleSystem in m_ParticleSystems) {
			Renderer renderer = particleSystem.GetComponent<Renderer>();
			if (renderer == null) {
				continue;
			}
			Material material = renderer.sharedMaterial;
			if (material == null) {
				continue;
			}
			if (particleSystem.emission.enabled) {
				var rateOverTime = particleSystem.emission.rateOverTime;
				float rateCount = 0;
				if (rateOverTime.mode == ParticleSystemCurveMode.Constant || rateOverTime.mode == ParticleSystemCurveMode.TwoConstants) {
					rateCount = rateOverTime.constantMax;
				}

				float duration = 0;
				if (rateCount > 0) {
					duration = particleSystem.main.duration;
				} else {
					float burstTime = 0;
					ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[particleSystem.emission.burstCount];
					int count = particleSystem.emission.GetBursts(bursts);
					if (count > 0) {
						for (int i = 0; i < count; i++) {
							if (burstTime < bursts[i].time) {
								burstTime = bursts[i].time;
							}
						}
						duration = burstTime;
					}
				}
				if (duration > 0) {
					var startLifetime = particleSystem.main.startLifetime;
					if (startLifetime.mode == ParticleSystemCurveMode.Constant || startLifetime.mode == ParticleSystemCurveMode.TwoConstants) {
						duration = duration + particleSystem.main.startDelay.constantMax + startLifetime.constantMax;
					} else {
						duration = duration + particleSystem.main.startDelay.constantMax;
					}
				}

				if (m_Duration < duration) {
					m_Duration = duration;
				}
			}
			if (particleSystem.main.loop) {
				if (!m_Loop) {
					m_Loop = true;
				}
			}
		}

		foreach (Animation aniamtion in m_Animations) {
			AnimationClip clip = aniamtion.clip;
			if (clip != null) {
				float duration = clip.length;
				if (m_Duration < duration) {
					m_Duration = duration;
				}
				if (clip.wrapMode == WrapMode.Loop || clip.wrapMode == WrapMode.PingPong) {
					if (!m_Loop) {
						m_Loop = true;
					}
				}
			}
		}

		m_AnimatorStateNameHashList.Clear();
		foreach (Animator animator in m_Animators) {
			int layerCount = animator.layerCount;
			int nameHash = 0;
			for (int i = 0; i < layerCount; i++) {
				AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(i);
				nameHash = stateInfo.fullPathHash;
				if (m_Duration < stateInfo.length) {
					m_Duration = stateInfo.length;
				}
				if (stateInfo.loop) {
					if (!m_Loop) {
						m_Loop = true;
					}
				}
			}
			m_AnimatorStateNameHashList.Add(nameHash);
		}

		if (m_TrailRenderers.Length > 0) {
			if (!m_Loop) {
				m_Loop = true;
			}
		}

		foreach (Particle2DUGUI particle2D in m_Particle2DUGUIs) {
			float duration = particle2D.configValues.defaultDuration + particle2D.configValues.lifespan + particle2D.configValues.lifespanVariance;
			if (m_Duration < duration) {
				m_Duration = duration;
			}
			if (particle2D.configValues.isLooop) {
				if (!m_Loop)
					m_Loop = true;
			}
		}
		
		foreach (DOTweenAnimation animation in m_DOTweenAnimations) {
			if (animation != null) {
				float duration = animation.delay + animation.duration;
				if (m_Duration < animation.duration) {
					m_Duration = animation.duration;
				}
				if (animation.loops == -1) {
					if (!m_Loop)
						m_Loop = true;
				}
			}
		}

		foreach (var graphic in GetComponentsInChildren<UnityEngine.UI.Graphic>(true)) {
			graphic.raycastTarget = false;
		}
	}
#endif
}
