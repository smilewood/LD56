using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuFunctions : MonoBehaviour
{
   public Slider masterSlider;
   public Slider musicSlider;
   public Slider soundSlider;
   public Slider voiceSlider;
   public Slider ambienceSlider;

   [SerializeField] private InputActionAsset _input;

   private InputAction _cancelAction;
   private InputAction _openMenuAction;

   public List<GameObject> Menus;
   public AudioMixer mixer;

   public bool CurrentSceneIsGameplay = false;

   public bool InGameMenuIsOpen = false;

   private bool MainMenuOpen = true;

   private bool AnyMenuOpen = true;

   private bool isquitting = false;

   private void Start()
   {
      DontDestroyOnLoad(gameObject);

      mixer.GetFloat("Master", out float masterVol);
      masterSlider.value = Mathf.Pow(10, masterVol / 20);

      mixer.GetFloat("Voice", out float voiceVol);
      voiceSlider.value = Mathf.Pow(10, voiceVol / 20);

      mixer.GetFloat("Ambience", out float ambienceVol);
      ambienceSlider.value = Mathf.Pow(10, ambienceVol / 20);

      mixer.GetFloat("Sounds", out float soundsVol);
      soundSlider.value = Mathf.Pow(10, soundsVol / 20);

      mixer.GetFloat("Music", out float musicVol);
      musicSlider.value = Mathf.Pow(10, musicVol / 20);

      _cancelAction = _input.FindAction("Cancel");
      _cancelAction.Enable();
      _openMenuAction = _input.FindAction("OpenMenu");
      _openMenuAction.Enable();
   }

   public void ExitGame()
   {
      if (!isquitting)
      {
         isquitting = true;
         StartCoroutine(ExplodeGame());
      }
   }

   private IEnumerator ExplodeGame()
   {
      yield return new WaitForSeconds(0.5f);
#if UNITY_EDITOR
      UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
   }

   public void StartLevel(string level)
   {
      if (level.Contains("Gameplay"))
      {
         CurrentSceneIsGameplay = true;
         ShowMenu("None");
         InGameMenuIsOpen = false;
         MainMenuOpen = false;
      }
      else
      {
         CurrentSceneIsGameplay = false;
         MainMenuOpen = true;
      }
      SceneManager.LoadSceneAsync(level);
   }

   public void SetMasterVolume()
   {
      mixer.SetFloat("Master", Mathf.Log10(masterSlider.value) * 20);
   }

   public void SetMusicVolume()
   {
      mixer.SetFloat("Music", Mathf.Log10(musicSlider.value) * 20);
   }

   public void SetEffectVolume()
   {
      mixer.SetFloat("Sounds", Mathf.Log10(soundSlider.value) * 20);
   }

   public void SetAmbienceVolume()
   {
      mixer.SetFloat("Ambience", Mathf.Log10(ambienceSlider.value) * 20);
   }

   public void SetVoiceVolume()
   {
      mixer.SetFloat("Voice", Mathf.Log10(voiceSlider.value) * 20);
   }

   public void ShowMenu(string menu)
   {
      foreach(var yurt in Menus.Where(g => g.activeSelf))
      {
         yurt.SetActive(false);
      }

      MainMenuOpen = false;
      InGameMenuIsOpen = false;
      AnyMenuOpen = true;

      if (menu.Contains("None"))
      {
         AnyMenuOpen = false;
         _input.Enable();
      }

      if (menu.Contains("MainMenu"))
      {
         MainMenuOpen = true;

         if (CurrentSceneIsGameplay)
         {
            menu = "InGameMenu";
            InGameMenuIsOpen = true;
            MainMenuOpen = false;
         }
      }
      else if (menu.Contains("InGameMenu"))
      {
         InGameMenuIsOpen = true;
      }

      Debug.Log("Opening Menu: " + menu);

      if (Menus.Any(m => m.name == menu))
      {
         Menus.Find(m => m.name == menu).SetActive(true);
         Debug.Log("Success");
      }
   }

   void Update()
   {
      AudioManager mgr = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();

      if (CurrentSceneIsGameplay)
      {
         if (AnyMenuOpen)
         {
            if (InGameMenuIsOpen)
            {
               if (_cancelAction.triggered)
               {
                  ShowMenu("None");
                  mgr.PlayUICancel();
               }
            }
            else
            {
               if (_cancelAction.triggered)
               {
                  ShowMenu("InGameMenu");
                  mgr.PlayUICancel();
               }
            }
         }
         else
         {
            if (_openMenuAction.triggered)
            {
               mgr.PlayUIInteract();
               ShowMenu("MainMenu");
            }
         }
      }
      else
      {
         if (_cancelAction.triggered)
         {
            if (!MainMenuOpen && !CurrentSceneIsGameplay)
            {
               mgr.PlayUICancel();
            }
            ShowMenu("MainMenu");
         }
      }
   }
}
