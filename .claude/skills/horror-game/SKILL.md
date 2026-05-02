---
name: horror-game
description: "Expert blueprint for horror games in Unity: tension pacing (sawtooth wave: buildup/peak/relief), Director system (macro AI controlling pacing), sensory AI (vision/sound detection), sanity/stress systems (camera shake, audio distortion), lighting atmosphere (volumetric fog URP, dynamic shadows), and dual-brain AI (cheating director + honest senses). Use for psychological horror, survival horror, or atmospheric games. Trigger keywords: horror_game, tension_pacing, director_system, sensory_perception, sanity_system, volumetric_fog, AI_reaction_time."
---

# Horror Game — Expert Blueprint (Unity 6 / C#)

Expert blueprint for horror games balancing tension, atmosphere, and player agency.
Adapted from Godot patterns to Unity 6 URP + C#.

---

## NEVER Do (Expert Anti-Patterns)

### Atmósfera y tensión
- **NEVER** mantener tensión al 100% todo el tiempo — usa el modelo **Sawtooth Pacing** (buildup → peak/susto → alivio) para evitar que el jugador se insensibilice.
- **NEVER** depender de jump-scares como fuente primaria de horror — construye horror real con atmósfera, audio espacial y anticipación de amenazas.
- **NEVER** hacer los entornos completamente oscuros hasta frustrar la navegación — la oscuridad debe ocultar amenazas (detalles), no el suelo. Usa rim lighting o linterna con batería limitada.
- **NEVER** dar recursos ilimitados al jugador — el horror de supervivencia requiere escasez. Batería limitada, animaciones lentas y recursos raros son obligatorios.

### IA y sentidos
- **NEVER** permitir que la IA detecte al jugador instantáneamente — implementa un `SuspicionMeter` o ventana de reacción de 1-3s antes de pasar a agresión total.
- **NEVER** usar paths de IA predecibles — un enemigo en loop perfecto es un puzzle, no un depredador. El Director debe sugerir periódicamente nuevas posiciones cerca del jugador.
- **NEVER** usar `OverlapSphere` o triggers para line-of-sight instantáneo — usa `Physics.Raycast()` en `FixedUpdate` para sincronía física correcta.
- **NEVER** calcular visión o pathfinding de monstruos fuera del frustum — deshabilita la lógica AI con `Renderer.isVisible` o `OnBecameInvisible()`.
- **NEVER** dejar `NavMeshObstacle` sin configurar en monstruos que persiguen — asigna capas de avoidance explícitas para evitar "apilamiento" en corredores estrechos.

### Técnico y escasez
- **NEVER** usar el árbol de UI como fuente de verdad del inventario — mantén una estructura tipada en memoria (`Dictionary<string, ItemData>`).
- **NEVER** usar MonoBehaviours para stats base de items — usa `ScriptableObject` para reducir overhead de memoria y permitir edición en Inspector.
- **NEVER** olvidar hacer `Instantiate` de un `ItemData` al añadir al inventario si tiene estado mutable (munición/durabilidad) — de lo contrario sobreescribes el asset global.
- **NEVER** parsear ficheros de guardado grandes en el hilo principal — usa `Task.Run()` o `Thread` para parsing pesado.
- **NEVER** comparar floats con `==` en hot paths (sanidad, estrés) — usa `Mathf.Approximately()` o comparaciones con threshold.
- **NEVER** cargar escenas de susto o texturas 4K síncronamente — usa `Resources.LoadAsync()` o `Addressables.LoadAssetAsync()`.
- **NEVER** escalar `Collider` de forma no uniforme — ajusta los parámetros internos (radius, height) para evitar física errática.
- **NEVER** usar `Animator` para parpadeo de luces — usa DOTween o coroutines para manipulación programática limpia.

---

## Componentes principales

### 1. Director System (Macro AI)
Controla el pacing global para evitar fatiga constante.

```csharp
public class HorrorDirector : MonoBehaviour
{
    public enum TensionState { Quiet, Buildup, Peak, Relief }

    [SerializeField] private MonsterAI m_Monster;
    [SerializeField] private Transform m_Player;

    private TensionState m_State = TensionState.Quiet;
    private float m_Tension;

    private void Update()
    {
        switch (m_State)
        {
            case TensionState.Buildup:
                m_Tension += 0.5f * Time.deltaTime;
                if (m_Tension > 75f) TriggerEvent();
                break;
            case TensionState.Relief:
                m_Tension -= 2f * Time.deltaTime;
                if (m_Tension <= 0f) m_State = TensionState.Quiet;
                break;
        }
    }

    private void TriggerEvent()
    {
        // Sugiere al monstruo un área CERCA del jugador, no ENCIMA
        Vector3 offset = new Vector3(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f));
        m_Monster.InvestigateArea(m_Player.position + offset);
        m_State = TensionState.Peak;
    }

    public void NotifyPlayerSafe() => m_State = TensionState.Relief;
}
```

### 2. Sensory Perception (Micro AI)
Los sentidos reales del monstruo — honesto, sin trampa.

```csharp
public class SensoryComponent : MonoBehaviour
{
    [SerializeField] private float m_VisionRange = 20f;
    [SerializeField] private float m_VisionAngle = 90f;
    [SerializeField] private float m_SuspicionBuildRate = 0.5f;
    [SerializeField] private LayerMask m_ObstacleMask;

    private float m_Suspicion;
    public event Action<Vector3> OnPlayerSpotted;
    public event Action<Vector3, float> OnSoundHeard;

    private void FixedUpdate()
    {
        if (CheckVision(out Vector3 playerPos))
        {
            m_Suspicion += m_SuspicionBuildRate * Time.fixedDeltaTime;
            if (m_Suspicion >= 1f) OnPlayerSpotted?.Invoke(playerPos);
        }
        else
        {
            m_Suspicion = Mathf.Max(0f, m_Suspicion - Time.fixedDeltaTime);
        }
    }

    private bool CheckVision(out Vector3 targetPos)
    {
        targetPos = Vector3.zero;
        // Buscar jugador en rango
        Collider[] hits = Physics.OverlapSphere(transform.position, m_VisionRange);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;
            Vector3 dir = (hit.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dir);
            if (angle > m_VisionAngle * 0.5f) continue;
            // Raycast para LoS real — en FixedUpdate para sincronía física
            if (!Physics.Raycast(transform.position, dir, out RaycastHit rayHit, m_VisionRange, m_ObstacleMask))
            {
                targetPos = hit.transform.position;
                return true;
            }
        }
        return false;
    }

    // Llamado por SoundEmitter cuando el jugador hace ruido
    public void HearSound(Vector3 origin, float volume)
    {
        float dist = Vector3.Distance(transform.position, origin);
        if (dist < volume * 10f) OnSoundHeard?.Invoke(origin, volume);
    }
}
```

### 3. Sanity / Stress System
Distorsiona el mundo según el nivel de miedo.

```csharp
public class SanityManager : MonoBehaviour
{
    [SerializeField] private Camera m_Camera;
    [SerializeField] private AudioMixer m_AudioMixer;
    [SerializeField] private Volume m_PostProcessVolume;

    private float m_Sanity = 100f;
    private ChromaticAberration m_ChromaAberration;
    private LensDistortion m_LensDistortion;

    private void Awake()
    {
        m_PostProcessVolume.profile.TryGet(out m_ChromaAberration);
        m_PostProcessVolume.profile.TryGet(out m_LensDistortion);
    }

    public void ModifySanity(float amount)
    {
        m_Sanity = Mathf.Clamp(m_Sanity + amount, 0f, 100f);
        float insanity = (100f - m_Sanity) / 100f;

        // Camera shake — via DOTween o Cinemachine noise
        float shakeIntensity = insanity * 0.3f;
        CinemachineCore.Instance?.GetActiveBrain(0)
            ?.ActiveVirtualCamera?.VirtualCameraGameObject
            ?.GetComponent<CinemachineBasicMultiChannelPerlin>()
            ?.SetAmplitude(shakeIntensity);

        // Audio distortion via Audio Mixer
        m_AudioMixer.SetFloat("Distortion", insanity * 0.8f);
        m_AudioMixer.SetFloat("LowPassCutoff", Mathf.Lerp(22000f, 800f, insanity));

        // Post-processing
        if (m_ChromaAberration != null) m_ChromaAberration.intensity.value = insanity;
        if (m_LensDistortion != null) m_LensDistortion.intensity.value = insanity * -0.3f;
    }
}
```

### 4. Dual-Brain AI
Director (omnisciente, hace trampa para mantener la amenaza relevante) + Senses (honesto, solo ataca si ve/escucha al jugador).

```csharp
public class MonsterAI : MonoBehaviour
{
    private NavMeshAgent m_Agent;
    private SensoryComponent m_Senses;
    private Transform m_Player;

    private enum State { Patrol, Investigate, Chase, Search }
    private State m_State = State.Patrol;

    private void Awake()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        m_Senses = GetComponent<SensoryComponent>();
        m_Senses.OnPlayerSpotted += pos => SetState(State.Chase);
        m_Senses.OnSoundHeard += (pos, _) => InvestigateArea(pos);
    }

    // Llamado por el Director (hace trampa — conoce posición exacta)
    public void InvestigateArea(Vector3 position)
    {
        if (m_State == State.Chase) return; // Senses tienen prioridad
        InvestigateArea(position);
        SetState(State.Investigate);
    }

    private void SetState(State newState) => m_State = newState;

    // Deshabilitar IA pesada fuera del frustum
    private void OnBecameInvisible() => enabled = false;
    private void OnBecameVisible() => enabled = true;
}
```

---

## Core Loop

```
Explorar → Percibir (sonido/visión) → Reaccionar (esconderse/huir) → Sobrevivir → Alivio → Explorar
```

## Pacing — La Ola Sawtooth

| Fase | Estado | Qué ocurre |
|------|--------|------------|
| Calma | `Quiet` | Sala segura, sin amenaza visible |
| Incomodidad | `Buildup` | Sonidos extraños, luces que parpadean |
| Terror | `Peak` | Monstruo cerca, chase activo |
| Alivio | `Relief` | Escape, momento de calma forzado |

## Cadena de skills relacionadas

| Fase | Skill | Propósito |
|------|-------|-----------|
| Atmósfera | `unity-lighting-vfx` | Fog volumétrico URP, sombras dinámicas |
| Audio | `unity-audio` | Reverb, low-pass filter, audio espacial 3D |
| IA | `unity-state-machines` + `unity-ai-navigation` | Hunter AI, percepción sensorial |
| Pacing | `a5c-ai-babysitter-director-ai` | Sistema Director |
| Escasez | `inventory-system` | Batería limitada, recursos de supervivencia |
| Estrés visual | `unity-lighting-vfx` | Post-processing por estado mental |
| Animación | `unity-animation` + `dotween` | Transiciones suaves, feedback visual |

## Errores comunes

| Error | Solución |
|-------|----------|
| Tensión constante | El jugador se insensibiliza — forzar períodos de `Relief` |
| IA injusta | Susto instantáneo — implementar `SuspicionMeter` con ventana de 1-3s |
| Oscuridad frustrante | El jugador no puede navegar — oscuridad debe ocultar amenazas, no el suelo |
| Jump-scares baratos | Construir anticipación primero — el Director guía, los Senses atacan |
