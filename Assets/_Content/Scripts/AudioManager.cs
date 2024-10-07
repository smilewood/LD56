using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Sound
{ 
   UI_Interact,
   UI_Submit,
   UI_Cancel,
   Warning,
   Transmutite_Call,
   Place_Structure
}

public enum Voice
{ 
   Intro
}

[System.Serializable]
public class SoundAudioClip
{ 
   public AudioClip AudioClip;
   public Sound sound;
}

[System.Serializable]
public class VoiceAudioClip
{
   public AudioClip AudioClip;
   public Voice Voice;
}

/// <summary>
/// Processes audio play requests for the scene
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField]
   private AudioSource musicAudioSource;

   [SerializeField]
   private AudioSource soundsAudioSource;

   [SerializeField]
   private AudioSource ambienceAudioSource;

   [SerializeField]
   private AudioSource voiceAudioSource;

   [SerializeField]
   private List<SoundAudioClip> musicClipList;

   [SerializeField]
   private List<VoiceAudioClip> voiceClipList;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance)
        {

            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

   public void PlayMusic()
   {
      musicAudioSource.Play();
   }

   public void StopMusic()
   {
      musicAudioSource.Stop();
   }

   public void PlayAmbience()
   {
      ambienceAudioSource.Play();
   }

   public void StopAmbience()
   {
      ambienceAudioSource.Stop();
   }

   /// <summary>
   /// Plays the specified voice by locating a clip with the passed enum value
   /// </summary>
   /// <param name="voice">Voice to play</param>
   public void PlayVoice(Voice voice)
   {
      if (!voiceClipList.Any(m => m.Voice == voice))
      {
         Debug.LogError("Voice is not in the audio list! " + voice.ToString());
      }

      var clips = voiceClipList.Where(m => m.Voice == voice);

      AudioClip clip = null;

      if (clips.Count() == 1)
      {
         clip = clips.First().AudioClip;
      }
      else
      {
         var quesadilla = Random.Range(0, clips.Count() - 1);

         clip = clips.ElementAt(quesadilla).AudioClip;
      }

      voiceAudioSource.clip = clip;
      voiceAudioSource.Play();
   }

   public void PlayIntroVoice()
   {
      PlayVoice(Voice.Intro);
   }

   public void StopPlayingVoice()
   {
      voiceAudioSource.Stop();
   }

   /// <summary>
   /// Plays the specified sound by locating a clip with the passed enum value
   /// </summary>
   /// <param name="sound">Sound to play</param>
   public void PlaySound(Sound sound)
   {
      if (!musicClipList.Any(m => m.sound == sound))
      {
         Debug.LogError("Sound is not in the audio list! " + sound.ToString());
      }

      var clips = musicClipList.Where(m => m.sound == sound);

      AudioClip clip = null;

      if (clips.Count() == 1)
      {
         clip = clips.First().AudioClip;
      }
      else
      {
         var quesadilla = Random.Range(0, clips.Count() - 1);

         clip = clips.ElementAt(quesadilla).AudioClip;
      }

      soundsAudioSource.PlayOneShot(clip);
   }

   public void PlayUIInteract()
   {
      PlaySound(Sound.UI_Interact);
   }

   public void PlayUISubmit()
   {
      PlaySound(Sound.UI_Submit);
   }

   public void PlayUICancel()
   {
      PlaySound(Sound.UI_Cancel);
   }

   public void PlayWarning()
   {
      PlaySound(Sound.Warning);
   }

   public void PlayTransmutiteCall()
   {
      PlaySound(Sound.Transmutite_Call);
   }

   public void PlayPlaceStructure()
   {
      PlaySound(Sound.Place_Structure);
   }
}
