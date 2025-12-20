using Kingmaker;
using Kingmaker.Blueprints.Base;
using NAudio.Wave;
using Newtonsoft.Json;
using AiVoiceoverMod.Unity;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AiVoiceoverMod.Voice;

public class WindowsSpeech : ISpeech
{
    private static FuzzyResolver s_FuzzyResolver;
    private static string s_ModDirectory;
    private static string SpeakBegin => "<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:mstts=\"http://www.w3.org/2001/mstts\">";
    private static string SpeakEnd => "</speak>";

    private static string NarratorVoice => $"<voice required=\"Name={Main.NarratorVoice}\">";
    private static string NarratorPitch => $"<pitch absmiddle=\"{Main.Settings?.NarratorPitch}\"/>";
    private static string NarratorRate => $"<rate absspeed=\"{Main.Settings?.NarratorRate}\"/>";
    private static string NarratorVolume => $"<volume level=\"{Main.Settings?.NarratorVolume}\"/>";

    private static string FemaleVoice => $"<voice required=\"Name={Main.FemaleVoice}\">";
    private static string FemaleVolume => $"<volume level=\"{Main.Settings?.FemaleVolume}\"/>";
    private static string FemalePitch => $"<pitch absmiddle=\"{Main.Settings?.FemalePitch}\"/>";
    private static string FemaleRate => $"<rate absspeed=\"{Main.Settings?.FemaleRate}\"/>";

    private static string MaleVoice => $"<voice required=\"Name={Main.MaleVoice}\">";
    private static string MaleVolume => $"<volume level=\"{Main.Settings?.MaleVolume}\"/>";
    private static string MalePitch => $"<pitch absmiddle=\"{Main.Settings?.MalePitch}\"/>";
    private static string MaleRate => $"<rate absspeed=\"{Main.Settings?.MaleRate}\"/>";

    private static string ProtagonistVoice => $"<voice required=\"Name={Main.ProtagonistVoice}\">";
    private static string ProtagonistVolume => $"<volume level=\"{Main.Settings?.ProtagonistVolume}\"/>";
    private static string ProtagonistPitch => $"<pitch absmiddle=\"{Main.Settings?.ProtagonistPitch}\"/>";
    private static string ProtagonistRate => $"<rate absspeed=\"{Main.Settings?.ProtagonistRate}\"/>";

    public string CombinedNarratorVoiceStart => $"{NarratorVoice}{NarratorPitch}{NarratorRate}{NarratorVolume}";
    public string CombinedFemaleVoiceStart => $"{FemaleVoice}{FemalePitch}{FemaleRate}{FemaleVolume}";
    public string CombinedMaleVoiceStart => $"{MaleVoice}{MalePitch}{MaleRate}{MaleVolume}";
    public string CombinedProtagonistVoiceStart => $"{ProtagonistVoice}{ProtagonistPitch}{ProtagonistRate}{ProtagonistVolume}";

    public virtual string CombinedDialogVoiceStart
    {
        get
        {
            if (Game.Instance?.DialogController?.CurrentSpeaker == null)
                return CombinedNarratorVoiceStart;

            if (Game.Instance?.DialogController?.CurrentSpeaker.IsMainCharacter == true)
                return CombinedProtagonistVoiceStart;

            return Game.Instance?.DialogController?.CurrentSpeaker.Gender switch
            {
                Gender.Female => CombinedFemaleVoiceStart,
                Gender.Male => CombinedMaleVoiceStart,
                _ => CombinedNarratorVoiceStart
            };
        }
    }

    public static int Length(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        var arr = new[] { "—", "-", "\"" };

        return arr.Aggregate(text, (current, t) => current.Replace(t, "")).Length;
    }

    private string FormatGenderSpecificVoices(string text)
    {
        text = text.Replace($"<i><color=#{Constants.NARRATOR_COLOR_CODE}>", $"</voice>{CombinedNarratorVoiceStart}");
        text = text.Replace("</color></i>", $"</voice>{CombinedDialogVoiceStart}");

        if (text.StartsWith("</voice>"))
            text = text.Remove(0, 8);
        else
            text = CombinedDialogVoiceStart + text;

        if (text.EndsWith(CombinedDialogVoiceStart!))
            text = text.Remove(text.Length - CombinedDialogVoiceStart.Length, CombinedDialogVoiceStart.Length);

        if (!text.EndsWith("</voice>"))
            text += "</voice>";
        return text;
    }

    

    private static IWavePlayer _output;
    private static WaveStream _waveStream;

    public static void PlayOgg(string path)
    {
        StopVorbis();

        // VorbisWaveReader from NAudio.Vorbis
        var vorbisStream = new NAudio.Vorbis.VorbisWaveReader(path);
        _output = new WaveOutEvent();
        _output.Init(vorbisStream);
        _output.Play();

        _waveStream = vorbisStream;
    }

    public static void StopVorbis()
    {
        _output?.Stop();
        _output?.Dispose();
        _output = null;

        _waveStream?.Dispose();
        _waveStream = null;
    }

    public static void LoadPreprocessedDatabase()
    {
        UnityEngine.Debug.Log("Loading preprocessed database...");
        try
        {
            s_ModDirectory = Path.Combine(Constants.LOCAL_LOW_PATH!,
                "Owlcat Games",
                "Warhammer 40000 Rogue Trader",
                "UnityModManager",
                Constants.MOD_NAME);

            var dbFile = Path.Combine(s_ModDirectory, "enGB-preprocessed.json");

            if (!File.Exists(dbFile))
            {
                UnityEngine.Debug.LogWarning($"Preprocessed database not found at: {dbFile}");
                return;
            }

            var json = File.ReadAllText(dbFile, Encoding.UTF8);
            var db = JsonConvert.DeserializeObject<PrecompiledDb>(json);

            if (db != null)
            {
                s_FuzzyResolver = new FuzzyResolver(db);
                UnityEngine.Debug.Log($"Loaded {db.entries.Count} entries from preprocessed database.");
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
            UnityEngine.Debug.LogWarning("Failed to load preprocessed database!");
        }
    }

    private void SpeakInternal(string origText, float delay = 0f)
    {
        var mcName = Game.Instance.Player.MainCharacterEntity?.CharacterName;
        var text = origText;
        if (mcName != null)
        {
            text = origText.Replace(mcName, "{name}");
        }

        string localizationKey = null;

        // Try to play prerecorded audio if available
        if (s_FuzzyResolver != null)
        {
            try
            {
                // Strip XML tags and normalize text for querying
                var cleanText = new Regex("<[^>]+>").Replace(text, "");
                cleanText = cleanText.Trim();

                if (!string.IsNullOrEmpty(cleanText))
                {
                    // Resolve GUID from text
                    var result = s_FuzzyResolver.Query(cleanText, topK: 1, refine: true);
                    UnityEngine.Debug.Log($"Playing prerecorded audio: {localizationKey} (score: {result.Best.Score:0.000})");
                    localizationKey = result.Best.Id;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
                UnityEngine.Debug.LogWarning("Failed to resolve/play prerecorded audio, falling back to TTS");
            }
        } else {
            UnityEngine.Debug.Log($"Fuzzy matcher is not initialized!");
        }

        SpeakByKey(localizationKey, origText, delay);
    }

    public bool IsSpeaking()
    {
        return WindowsVoiceUnity.IsSpeaking;
    }

    public void SpeakPreview(string text, VoiceType voiceType)
    {
        if (string.IsNullOrEmpty(text))
        {
            UnityEngine.Debug.LogWarning("No text to speak!");
            return;
        }

        text = text.PrepareText();
        text = new Regex("<[^>]+>").Replace(text, "");

        text = voiceType switch
        {
            VoiceType.Narrator => $"{CombinedNarratorVoiceStart}{text}</voice>",
            VoiceType.Female => $"{CombinedFemaleVoiceStart}{text}</voice>",
            VoiceType.Male => $"{CombinedMaleVoiceStart}{text}</voice>",
            VoiceType.Protagonist => $"{CombinedProtagonistVoiceStart}{text}</voice>",
            _ => throw new ArgumentOutOfRangeException(nameof(voiceType), voiceType, null)
        };

        SpeakInternal(text);
    }

    public string PrepareSpeechText(string text)
    {
        text = new Regex("<[^>]+>").Replace(text, "");
        text = text.PrepareText();
        text = $"{CombinedNarratorVoiceStart}{text}</voice>";
        return text;
    }

    public string PrepareDialogText(string text)
    {
        text = text.PrepareText();
        text = new Regex("<b><color[^>]+><link([^>]+)?>([^<>]*)</link></color></b>").Replace(text, "$2");
        text = FormatGenderSpecificVoices(text);
        return text;
    }

    public void SpeakDialog(string text, float delay = 0f)
    {
        if (string.IsNullOrEmpty(text))
        {
            UnityEngine.Debug.LogWarning("No text to speak!");
            return;
        }

        if (!Main.Settings.UseGenderSpecificVoices)
        {
            Speak(text, delay);
            return;
        }

        text = PrepareDialogText(text);

        SpeakInternal(text, delay);
    }

    public void SpeakAs(string text, VoiceType voiceType, float delay = 0f)
    {
        if (string.IsNullOrEmpty(text))
        {
            UnityEngine.Debug.LogWarning("No text to speak!");
            return;
        }

        if (Main.Settings!.UseProtagonistSpecificVoice && voiceType == VoiceType.Protagonist)
        {
            text = $"{CombinedProtagonistVoiceStart}{text}</voice>";
            SpeakInternal(text, delay);
            return;
        }

        if (!Main.Settings!.UseGenderSpecificVoices)
        {
            Speak(text, delay);
            return;
        }

        text = voiceType switch
        {
            VoiceType.Narrator => $"{CombinedNarratorVoiceStart}{text}</voice>",
            VoiceType.Female => $"{CombinedFemaleVoiceStart}{text}</voice>",
            VoiceType.Male => $"{CombinedMaleVoiceStart}{text}</voice>",
            _ => throw new ArgumentOutOfRangeException(nameof(voiceType), voiceType, null)
        };

        SpeakInternal(text, delay);
    }

    public void Speak(string text, float delay = 0f)
    {
        if (string.IsNullOrEmpty(text))
        {
            UnityEngine.Debug.LogWarning("No text to speak!");
            return;
        }

        text = PrepareSpeechText(text);

        SpeakInternal(text, delay);
    }

    public void Stop()
    {
        StopVorbis();
        WindowsVoiceUnity.Stop();
    }

    public string[] GetAvailableVoices()
    {
        return WindowsVoiceUnity.GetAvailableVoices();
    }

    public string GetStatusMessage()
    {
        return WindowsVoiceUnity.GetStatusMessage();
    }

    public void SpeakByKey(string localizationKey, string fallbackText, float delay = 0)
    {
        if (!string.IsNullOrEmpty(localizationKey) && localizationKey.Length >= 2)
        {
            // Build path: mod_dir/tts/{first two letters}/{GUID}.ogg
            var prefix = localizationKey.Substring(0, 2);
            var audioPath = Path.Combine(s_ModDirectory, "tts", prefix, $"{localizationKey}.ogg");
            
            if (File.Exists(audioPath))
            {
                PlayOgg(audioPath);
                return; // Skip TTS if we played the audio
            }
            else
            {
                UnityEngine.Debug.Log($"Audio file not found: {audioPath}");
            }
        }

        var text = SpeakBegin + fallbackText + SpeakEnd;
        if (Main.Settings?.LogVoicedLines == true)
            UnityEngine.Debug.Log(text);
        WindowsVoiceUnity.Speak(text, Length(text), delay);
    }
}