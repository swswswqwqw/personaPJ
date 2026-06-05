using System.Collections.Generic;

namespace AstralEchoes.Audio
{
    public sealed class AudioManager
    {
        static AudioManager _instance;
        public static AudioManager Instance => _instance ??= new AudioManager();

        public float BGMVolume { get; private set; } = 0.8f;
        public float SEVolume { get; private set; } = 1.0f;
        public float VoiceVolume { get; private set; } = 1.0f;
        public string CurrentBGM { get; private set; }

        AudioManager() { }

        public void SetBGMVolume(float volume) => BGMVolume = System.Math.Clamp(volume, 0f, 1f);
        public void SetSEVolume(float volume) => SEVolume = System.Math.Clamp(volume, 0f, 1f);
        public void SetVoiceVolume(float volume) => VoiceVolume = System.Math.Clamp(volume, 0f, 1f);

        public void PlayBGM(string bgmId, float fadeTime = 1.0f)
        {
            if (CurrentBGM == bgmId) return;
            // In Unity: Use AudioSource with DOTween fade
            // StopBGM(fadeTime);
            // Load and play new BGM
            CurrentBGM = bgmId;
        }

        public void StopBGM(float fadeTime = 1.0f)
        {
            // In Unity: DOTween fade to 0, then stop
            CurrentBGM = null;
        }

        public void PlaySE(string seId)
        {
            // In Unity: AudioSource.PlayOneShot with volume
        }

        public void PlayVoice(string voiceId)
        {
            // In Unity: Dedicated AudioSource for voice
        }
    }

    public static class BGMIds
    {
        public const string Title = "bgm_title";
        public const string SchoolDay = "bgm_school_day";
        public const string AfterSchool = "bgm_after_school";
        public const string Evening = "bgm_evening";
        public const string LateNight = "bgm_late_night";
        public const string AstralExplore = "bgm_astral_explore";
        public const string BattleNormal = "bgm_battle_normal";
        public const string BattleBoss = "bgm_battle_boss";
        public const string Attunement = "bgm_attunement";
        public const string Tension = "bgm_tension";
        public const string Victory = "bgm_victory";
        public const string Sadness = "bgm_sadness";
        public const string FullMoon = "bgm_full_moon";
    }

    public static class SEIds
    {
        public const string MenuSelect = "se_menu_select";
        public const string MenuConfirm = "se_menu_confirm";
        public const string MenuCancel = "se_menu_cancel";
        public const string WeaknessHit = "se_weakness_hit";
        public const string CriticalHit = "se_critical_hit";
        public const string FullResonance = "se_full_resonance";
        public const string AttunementUp = "se_attunement_up";
        public const string TimeAdvance = "se_time_advance";
        public const string EchoAwaken = "se_echo_awaken";
        public const string EchoShift = "se_echo_shift";
        public const string DamageNormal = "se_damage_normal";
        public const string Heal = "se_heal";
    }
}
