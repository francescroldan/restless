using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Generates placeholder WAV files for the MOC loop.
/// Run via: Restless > Generate Placeholder Audio
/// </summary>
public static class GeneratePlaceholderAudio
{
    const int SR = 44100;

    [MenuItem("Restless/Generate Placeholder Audio")]
    public static void Generate()
    {
        string musicDir = Path.Combine(Application.dataPath, "_Project", "Audio", "Music");
        string sfxDir   = Path.Combine(Application.dataPath, "_Project", "Audio", "SFX");
        Directory.CreateDirectory(musicDir);
        Directory.CreateDirectory(sfxDir);

        GenerateFootsteps(sfxDir);
        GenerateMinigameSFX(sfxDir);
        GenerateVigiliaSFX(sfxDir, musicDir);
        WriteWav(Path.Combine(musicDir, "ambient_calm.wav"),     AmbientCalm());
        WriteWav(Path.Combine(musicDir, "ambient_tense.wav"),    AmbientTense());
        WriteWav(Path.Combine(musicDir, "ambient_critical.wav"), AmbientCritical());
        WriteWav(Path.Combine(musicDir, "ambient_overload.wav"), AmbientOverload());
        WriteWav(Path.Combine(sfxDir,   "sfx_zone_enter.wav"),   SfxZoneEnter());
        WriteWav(Path.Combine(sfxDir,   "sfx_ally_encounter.wav"), SfxAllyEncounter());
        WriteWav(Path.Combine(sfxDir,   "sfx_wakeup_voluntary.wav"), SfxWakeupVoluntary());
        WriteWav(Path.Combine(sfxDir,   "sfx_wakeup_abrupt.wav"),    SfxWakeupAbrupt());

        AssetDatabase.Refresh();
        Debug.Log("[AudioGen] 8 placeholder audio files generated.");
    }

    // ── Minigame SFX ─────────────────────────────────────────────────────────

    static void GenerateMinigameSFX(string dir)
    {
        // Hit: bright short tick (correct press)
        WriteWav(Path.Combine(dir, "sfx_minigame_hit.wav"),     MinigameHit());
        // Miss: low dull thud (wrong press)
        WriteWav(Path.Combine(dir, "sfx_minigame_miss.wav"),    MinigameMiss());
        // Success: ascending 3-note chime
        WriteWav(Path.Combine(dir, "sfx_minigame_success.wav"), MinigameSuccess());
        // Fail: descending dissonant tone
        WriteWav(Path.Combine(dir, "sfx_minigame_fail.wav"),    MinigameFail());
        // Memory fragment collected: crystalline tone
        WriteWav(Path.Combine(dir, "sfx_fragment_collect.wav"), FragmentCollect());
        // Memory point activate: low resonant hum
        WriteWav(Path.Combine(dir, "sfx_memory_activate.wav"),  MemoryActivate());
        // Entity proximity: distant low drone pulse
        WriteWav(Path.Combine(dir, "sfx_entity_nearby.wav"),    EntityNearby());
    }

    static float[] MinigameHit()
    {
        int n = (int)(SR * 0.12);
        var s = new float[n];
        for (int i = 0; i < n; i++)
        {
            double t   = (double)i / SR;
            double env = System.Math.Exp(-30.0 * t);
            s[i] = (float)(0.6 * System.Math.Sin(2 * System.Math.PI * 1200 * t) * env);
        }
        return s;
    }

    static float[] MinigameMiss()
    {
        int n = (int)(SR * 0.18);
        var s = new float[n];
        for (int i = 0; i < n; i++)
        {
            double t   = (double)i / SR;
            double env = System.Math.Exp(-14.0 * t);
            s[i] = (float)(0.55 * System.Math.Sin(2 * System.Math.PI * 220 * t) * env);
        }
        return s;
    }

    static float[] MinigameSuccess()
    {
        // Three ascending notes: C5(523) E5(659) G5(784), each 0.18s
        double[] freqs = { 523.25, 659.25, 783.99 };
        int noteLen = (int)(SR * 0.2);
        int n = noteLen * 3;
        var s = new float[n];
        for (int note = 0; note < 3; note++)
        {
            int offset = note * noteLen;
            for (int i = 0; i < noteLen; i++)
            {
                double t   = (double)i / SR;
                double env = System.Math.Min(t / 0.02, 1.0) * System.Math.Max(0, (0.2 - t) / 0.12);
                s[offset + i] = (float)(0.5 * System.Math.Sin(2 * System.Math.PI * freqs[note] * t) * env);
            }
        }
        return s;
    }

    static float[] MinigameFail()
    {
        // Descending dissonant: 440→311 Hz glide over 0.6s
        int n = (int)(SR * 0.6);
        var s = new float[n];
        double phase = 0;
        for (int i = 0; i < n; i++)
        {
            double t    = (double)i / SR;
            double freq = 440.0 * System.Math.Pow(311.0 / 440.0, t / 0.6);
            phase += 2 * System.Math.PI * freq / SR;
            double env  = (0.6 - t) / 0.6;
            s[i] = (float)(0.45 * System.Math.Sin(phase) * env);
        }
        return s;
    }

    static float[] FragmentCollect()
    {
        // Crystalline: 880 Hz + 1320 Hz + 1760 Hz, 1.0s decay
        int n = (int)(SR * 1.0);
        var s = new float[n];
        for (int i = 0; i < n; i++)
        {
            double t   = (double)i / SR;
            double env = System.Math.Exp(-3.5 * t);
            s[i] = (float)(env * (
                0.35 * System.Math.Sin(2 * System.Math.PI * 880  * t) +
                0.22 * System.Math.Sin(2 * System.Math.PI * 1320 * t) +
                0.12 * System.Math.Sin(2 * System.Math.PI * 1760 * t)));
        }
        return s;
    }

    static float[] MemoryActivate()
    {
        // Low resonant hum: 80 Hz + 120 Hz, fade in then sustain, 0.8s
        int n = (int)(SR * 0.8);
        var s = new float[n];
        for (int i = 0; i < n; i++)
        {
            double t      = (double)i / SR;
            double attack  = System.Math.Min(t / 0.15, 1.0);
            double release = System.Math.Max(0.0, (0.8 - t) / 0.2);
            double env     = attack * System.Math.Min(release, 1.0);
            s[i] = (float)(env * (
                0.42 * System.Math.Sin(2 * System.Math.PI * 80  * t) +
                0.20 * System.Math.Sin(2 * System.Math.PI * 120 * t)));
        }
        return s;
    }

    static float[] EntityNearby()
    {
        // Distant low pulse: 40 Hz, single slow throb 1.2s
        int n = (int)(SR * 1.2);
        var s = new float[n];
        for (int i = 0; i < n; i++)
        {
            double t   = (double)i / SR;
            double lfo = System.Math.Sin(System.Math.PI * t / 1.2); // half-sine envelope
            s[i] = (float)(lfo * (
                0.38 * System.Math.Sin(2 * System.Math.PI * 40 * t) +
                0.15 * System.Math.Sin(2 * System.Math.PI * 80 * t)));
        }
        return s;
    }

    // ── Vigilia SFX ──────────────────────────────────────────────────────────

    static void GenerateVigiliaSFX(string sfxDir, string musicDir)
    {
        // Ambient room: very quiet hiss + 60 Hz hum (electrical), 8s loop
        WriteWav(Path.Combine(musicDir, "ambient_vigil.wav"),         AmbientVigil());
        // Tranquil return: soft breath-like whoosh
        WriteWav(Path.Combine(sfxDir, "sfx_vigil_return_tranquil.wav"), VigilReturnTranquil());
        // Abrupt return: jarring noise sting
        WriteWav(Path.Combine(sfxDir, "sfx_vigil_return_abrupt.wav"),   VigilReturnAbrupt());
        // Sleep: slow descending tone (going to sleep)
        WriteWav(Path.Combine(sfxDir, "sfx_vigil_sleep.wav"),           VigilSleep());
    }

    static float[] AmbientVigil()
    {
        int n = SR * 8;
        var s = new float[n];
        var rng = new System.Random(55);
        float prev = 0f;
        for (int i = 0; i < n; i++)
        {
            double t     = (double)i / SR;
            // 60 Hz electrical hum (very quiet)
            double hum   = 0.04 * System.Math.Sin(2 * System.Math.PI * 60 * t);
            // Filtered noise (room hiss)
            float noise  = (float)(rng.NextDouble() * 2 - 1) * 0.03f;
            float lp     = prev * 0.9f + noise * 0.1f; // low-pass
            prev = lp;
            s[i] = (float)(hum + lp);
            s[i] *= LoopFade(i, n, SR / 3);
        }
        return s;
    }

    static float[] VigilReturnTranquil()
    {
        // Soft breath-in whoosh: filtered noise sweep upward, 1.5s
        int n = (int)(SR * 1.5);
        var s = new float[n];
        var rng = new System.Random(77);
        float prev = 0f;
        for (int i = 0; i < n; i++)
        {
            double t   = (double)i / SR;
            double env = System.Math.Sin(System.Math.PI * t / 1.5); // half-sine
            float  raw = (float)(rng.NextDouble() * 2 - 1);
            float  lp  = prev * 0.88f + raw * 0.12f;
            prev = lp;
            s[i] = (float)(lp * env * 0.38);
        }
        return s;
    }

    static float[] VigilReturnAbrupt()
    {
        // Sharp noise burst + low impact, 0.7s
        int n = (int)(SR * 0.7);
        var s = new float[n];
        var rng = new System.Random(13);
        for (int i = 0; i < n; i++)
        {
            double t     = (double)i / SR;
            double env   = System.Math.Exp(-7.0 * t);
            double noise = (rng.NextDouble() * 2 - 1) * 0.65;
            double thump = System.Math.Sin(2 * System.Math.PI * 45 * t) * 0.4;
            s[i] = (float)((noise + thump) * env);
        }
        return s;
    }

    static float[] VigilSleep()
    {
        // Slow descending tone 300→120 Hz, 2s, fades out
        int n = (int)(SR * 2.0);
        var s = new float[n];
        double phase = 0;
        for (int i = 0; i < n; i++)
        {
            double t    = (double)i / SR;
            double freq = 300.0 * System.Math.Pow(120.0 / 300.0, t / 2.0);
            phase += 2 * System.Math.PI * freq / SR;
            double env  = System.Math.Max(0, (2.0 - t) / 2.0);
            s[i] = (float)(0.4 * System.Math.Sin(phase) * env);
        }
        return s;
    }

    // ── Footsteps ────────────────────────────────────────────────────────────

    static void GenerateFootsteps(string dir)
    {
        // 3 variations: low thump + click transient, slightly different pitch/body
        (float baseHz, float clickAmp, int seed)[] variants =
        {
            (95f,  0.55f, 11),
            (86f,  0.42f, 22),
            (104f, 0.50f, 33),
        };
        for (int v = 0; v < variants.Length; v++)
        {
            var (hz, clickAmp, seed) = variants[v];
            WriteWav(Path.Combine(dir, $"sfx_footstep_{v + 1}.wav"), Footstep(hz, clickAmp, seed));
        }
    }

    // Footstep: fast-decay thump at baseHz + short noise click
    static float[] Footstep(float baseHz, float clickAmp, int seed)
    {
        int n   = (int)(SR * 0.18f);
        var s   = new float[n];
        var rng = new System.Random(seed);
        for (int i = 0; i < n; i++)
        {
            double t    = (double)i / SR;
            // Woody thump: sine with fast exponential decay
            double thump = System.Math.Sin(2 * System.Math.PI * baseHz * t)
                           * System.Math.Exp(-28.0 * t) * 0.72;
            // Click transient: noise burst in first 8ms only
            double click = (t < 0.008)
                ? (rng.NextDouble() * 2 - 1) * clickAmp * System.Math.Exp(-180.0 * t)
                : 0.0;
            s[i] = (float)(thump + click);
        }
        return s;
    }

    // ── Ambient loops ────────────────────────────────────────────────────────

    // Calm: 55 Hz drone + harmonics, slow tremolo — sombre but stable
    static float[] AmbientCalm()
    {
        int n = SR * 10;
        var s = new float[n];
        for (int i = 0; i < n; i++)
        {
            double t   = (double)i / SR;
            double lfo = 0.85 + 0.15 * System.Math.Sin(2 * System.Math.PI * 0.18 * t);
            s[i] = (float)(lfo * (
                0.45 * System.Math.Sin(2 * System.Math.PI * 55  * t) +
                0.18 * System.Math.Sin(2 * System.Math.PI * 110 * t) +
                0.07 * System.Math.Sin(2 * System.Math.PI * 165 * t)));
            s[i] *= LoopFade(i, n, SR / 4);
        }
        return s;
    }

    // Tense: 80 Hz + detuned copy (beating at 3 Hz) — unsettling, pulsing
    static float[] AmbientTense()
    {
        int n = SR * 10;
        var s = new float[n];
        for (int i = 0; i < n; i++)
        {
            double t = (double)i / SR;
            s[i] = (float)(0.7 * (
                0.38 * System.Math.Sin(2 * System.Math.PI * 80  * t) +
                0.32 * System.Math.Sin(2 * System.Math.PI * 83  * t) +  // 3 Hz beat
                0.14 * System.Math.Sin(2 * System.Math.PI * 160 * t) +
                0.06 * System.Math.Sin(2 * System.Math.PI * 240 * t)));
            s[i] *= LoopFade(i, n, SR / 4);
        }
        return s;
    }

    // Critical: tritone (110 + 155 Hz) + noise — overtly dissonant
    static float[] AmbientCritical()
    {
        int n = SR * 10;
        var s = new float[n];
        var rng = new System.Random(42);
        for (int i = 0; i < n; i++)
        {
            double t     = (double)i / SR;
            double noise = (rng.NextDouble() * 2 - 1) * 0.05;
            s[i] = (float)(
                0.36 * System.Math.Sin(2 * System.Math.PI * 110 * t) +
                0.28 * System.Math.Sin(2 * System.Math.PI * 155 * t) +
                0.12 * System.Math.Sin(2 * System.Math.PI * 330 * t) +
                noise);
            s[i] *= LoopFade(i, n, SR / 4);
        }
        return s;
    }

    // Overload: clipped distortion + noise — oppressive, hostile
    static float[] AmbientOverload()
    {
        int n = SR * 10;
        var s = new float[n];
        var rng = new System.Random(99);
        float prev = 0f;
        for (int i = 0; i < n; i++)
        {
            double t    = (double)i / SR;
            float  tone = (float)(System.Math.Sin(2 * System.Math.PI * 220 * t) * 0.4 +
                                  System.Math.Sin(2 * System.Math.PI * 311 * t) * 0.25); // tritone
            float dist  = Mathf.Clamp(tone * 4f, -0.78f, 0.78f);
            float noise = (float)(rng.NextDouble() * 2 - 1) * 0.28f;
            // simple one-pole low-pass for noise (reduces harshness)
            float lp = prev * 0.55f + (dist * 0.55f + noise * 0.45f) * 0.45f;
            prev = lp;
            s[i] = lp * LoopFade(i, n, SR / 4);
        }
        return s;
    }

    // ── SFX ─────────────────────────────────────────────────────────────────

    // Zone enter: ascending frequency sweep 200→900 Hz, 0.45s
    static float[] SfxZoneEnter()
    {
        int n = (int)(SR * 0.45);
        var s = new float[n];
        double phase = 0;
        for (int i = 0; i < n; i++)
        {
            double t    = (double)i / SR;
            double pct  = t / 0.45;
            double freq = 200 + (900 - 200) * pct;
            phase += 2 * System.Math.PI * freq / SR;
            double env  = 1.0 - pct;
            s[i] = (float)(0.52 * System.Math.Sin(phase) * env);
        }
        return s;
    }

    // Ally encounter: warm major chord C4-E4-G4, 0.9s with soft attack/release
    static float[] SfxAllyEncounter()
    {
        int n = (int)(SR * 0.9);
        var s = new float[n];
        for (int i = 0; i < n; i++)
        {
            double t      = (double)i / SR;
            double attack  = System.Math.Min(t / 0.12, 1.0);
            double release = System.Math.Max(0, (0.9 - t) / 0.35);
            release        = System.Math.Min(release, 1.0);
            double env     = attack * release;
            s[i] = (float)(env * 0.26 * (
                System.Math.Sin(2 * System.Math.PI * 261.63 * t) +
                System.Math.Sin(2 * System.Math.PI * 329.63 * t) +
                System.Math.Sin(2 * System.Math.PI * 392.00 * t)));
        }
        return s;
    }

    // Voluntary wake-up: 440→220 Hz exponential slide, 1.3s fade-out
    static float[] SfxWakeupVoluntary()
    {
        int n = (int)(SR * 1.3);
        var s = new float[n];
        double phase = 0;
        for (int i = 0; i < n; i++)
        {
            double t    = (double)i / SR;
            double freq = 440.0 * System.Math.Pow(220.0 / 440.0, t / 1.3);
            phase += 2 * System.Math.PI * freq / SR;
            double env  = (1.3 - t) / 1.3;
            s[i] = (float)(0.45 * System.Math.Sin(phase) * env);
        }
        return s;
    }

    // Abrupt wake-up: noise burst + low thump, 0.55s fast exponential decay
    static float[] SfxWakeupAbrupt()
    {
        int n = (int)(SR * 0.55);
        var s = new float[n];
        var rng = new System.Random(7);
        for (int i = 0; i < n; i++)
        {
            double t     = (double)i / SR;
            double env   = System.Math.Exp(-9.0 * t);
            double noise = (rng.NextDouble() * 2 - 1) * 0.72;
            double thump = System.Math.Sin(2 * System.Math.PI * 55 * t) * 0.35;
            s[i] = (float)((noise + thump) * env);
        }
        return s;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    // Cosine fade-in/out at loop boundaries to avoid clicks
    static float LoopFade(int i, int total, int fadeSamples)
    {
        if (i < fadeSamples)
            return (float)(0.5 - 0.5 * System.Math.Cos(System.Math.PI * i / fadeSamples));
        if (i > total - fadeSamples)
            return (float)(0.5 - 0.5 * System.Math.Cos(System.Math.PI * (total - i) / fadeSamples));
        return 1f;
    }

    static void WriteWav(string path, float[] samples)
    {
        const int channels   = 1;
        const int bitsPerSmp = 16;
        int byteRate  = SR * channels * bitsPerSmp / 8;
        int blockAlign= channels * bitsPerSmp / 8;
        int dataSize  = samples.Length * blockAlign;

        using var w = new BinaryWriter(File.Open(path, FileMode.Create));
        // RIFF header
        w.Write(new[]{'R','I','F','F'});
        w.Write(36 + dataSize);
        w.Write(new[]{'W','A','V','E'});
        // fmt chunk
        w.Write(new[]{'f','m','t',' '});
        w.Write(16);
        w.Write((short)1);          // PCM
        w.Write((short)channels);
        w.Write(SR);
        w.Write(byteRate);
        w.Write((short)blockAlign);
        w.Write((short)bitsPerSmp);
        // data chunk
        w.Write(new[]{'d','a','t','a'});
        w.Write(dataSize);
        foreach (float sample in samples)
            w.Write((short)(Mathf.Clamp(sample, -1f, 1f) * 32767f));

        Debug.Log("[AudioGen] " + Path.GetFileName(path));
    }
}
