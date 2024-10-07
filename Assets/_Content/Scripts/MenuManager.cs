using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuFunctions : MonoBehaviour
{
   public GameObject myBoy;

   public bool PlayIntro = true;
   private bool IsIntroPlaying = false;
   public AudioManager AudioManager;

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

   public bool GameOver = false;

   private bool MainMenuOpen = true;

   private bool AnyMenuOpen = true;

   private bool isquitting = false;

   public GameObject TitleSplash;

   public TextMeshProUGUI subtitle;

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

      if (PlayIntro)
      {
         PlayIntroVoiceAndHideMenu();
      }
   }

   /// <summary>
   /// Show the victory screen/end of game
   /// </summary>
   public static void ShowWinScreen()
   {
      MenuFunctions menuFunctions = GameObject.FindGameObjectWithTag("Menu").GetComponent<MenuFunctions>();
      menuFunctions.ShowMenu("Win");
      menuFunctions.AudioManager.PlayUISubmit();
      menuFunctions.GameOver = true;
      Time.timeScale = 0f;
   }

   public void PlayIntroVoiceAndHideMenu()
   {
      TitleSplash.SetActive(false);
      IsIntroPlaying = true;
      ShowMenu("Intro");
      Cursor.visible = false;

      StartCoroutine(IntroCoroutine());
   }

   private IEnumerator IntroCoroutine()
   {
      subtitle.text = "";

      yield return new WaitForSeconds(3); //3

      if (IsIntroPlaying)
      {
         AudioManager.PlayIntroVoice();
         string subtitle1 = "Many centuries ago, right here in our little corner of Exaria, expert alchemists produced all of the technological and material products across the land. Using various minerals, plants, and machinery, alchemy was an early science of sorts, with thousands aiming to top the latest big discovery.";
         subtitle.text = subtitle1;
      }

      yield return new WaitForSeconds(20); //23

      if (IsIntroPlaying)
      {
         string subtitle2 = "One particular day, though, a young alchemist made the revolutionary discovery of Transmutation: creating short-lived, semi-intelligent elemental creatures using rather cheap alchemical processes. Their discovery shocked the entire alchemical world and brought us to the future we know today.";
         subtitle.text = subtitle2;
      }

      yield return new WaitForSeconds(20); //43

      if (IsIntroPlaying)
      {
         string subtitle3 = "It was so influential, in fact, that you may see and feel the turning point unfold with your own eyes using this Creature Box. Come, make yourself comfortable, and experience the legendary tale of Valenor the Wisp…";
         subtitle.text = subtitle3;
      }

      yield return new WaitForSeconds(8); //51
      if (IsIntroPlaying)
      {
         TitleSplash.SetActive(true);
      }

      yield return new WaitForSeconds(8); //59
      if (IsIntroPlaying)
      {
         ShowMenu("MainMenu");
         IsIntroPlaying = false;
         Cursor.visible = true;
      }
   }

   public void ExitGame()
   {
      if (!isquitting)
      {
         Time.timeScale = 1f;
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

      if (Menus.Any(m => m.name == menu))
      {
         Menus.Find(m => m.name == menu).SetActive(true);
      }
   }

   void Update()
   {
      if (GameOver)
      {
         return;
      }

      if (IsIntroPlaying)
      {
         if (_openMenuAction.triggered)
         {
            ShowMenu("MainMenu");

            AudioManager.StopPlayingVoice();
            IsIntroPlaying = false;
            Cursor.visible = true;
         }
      }
      else if (CurrentSceneIsGameplay)
      {
         AudioManager.StopPlayingVoice();

         if (AnyMenuOpen)
         {
            myBoy.SetActive(true);

            if (InGameMenuIsOpen)
            {
               if (_cancelAction.triggered)
               {
                  ShowMenu("None");
                  AudioManager.PlayUICancel();
               }
            }
            else
            {
               if (_cancelAction.triggered)
               {
                  ShowMenu("InGameMenu");
                  AudioManager.PlayUICancel();
               }
            }
         }
         else
         {
            myBoy.SetActive(false);

            if (_openMenuAction.triggered)
            {
               AudioManager.PlayUIInteract();
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
               AudioManager.PlayUICancel();
            }
            ShowMenu("MainMenu");
         }
      }
   }
}
