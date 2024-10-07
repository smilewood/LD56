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

[System.Serializable]
public class SoundAudioClip
{ 
   public AudioClip AudioClip;
   public Sound sound;
}

/// <summary>
/// Processes audio play requests for the scene
/// </summary>
public class AudioManager : MonoBehaviour
{
   [SerializeField]
   private AudioSource musicAudioSource;

   [SerializeField]
   private AudioSource soundsAudioSource;

   [SerializeField]
   private AudioClip musicClip; // clip for music in this scene

   [SerializeField]
   private List<SoundAudioClip> musicClipList;

    // Start is called before the first frame update
    void Start()
    {
      musicAudioSource.clip = musicClip;
      musicAudioSource.Play();
      DontDestroyOnLoad(gameObject);
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
