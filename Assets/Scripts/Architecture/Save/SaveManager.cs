using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Architecture.GameSound;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace Architecture
{
    public class SaveManager:ManagerNeedInitializeBase
    {
        [Inject] private IAudioService _audioService;
        
        private const string SettingsSaveName = "GameSettings.sav";
        private readonly string _settingsSavePath = Path.Combine(Application.persistentDataPath, SettingsSaveName);

        public bool IsFirstLaunch;
        public GameSettings CurrentSettingsSave;
        public GameSave CurrentGameSave;

        /// <summary>
        /// 初始化，会设置IsFirstLaunch和CurrentSettingsSave
        /// </summary>
        public override async UniTask Init()
        {
            await base.Init();
            IsFirstLaunch = !File.Exists(_settingsSavePath);
            if (IsFirstLaunch)
            {
                CurrentSettingsSave = new GameSettings();
                SaveSettings();
            }
            else
            {
                LoadSettings();
                
                _audioService.SetBgmVolume(CurrentSettingsSave.bgmVolume/100f);
                _audioService.SetSfxVolume(CurrentSettingsSave.sfxVolume/100f);

                var resParts = CurrentSettingsSave.gameResolution.ToString().Replace("Res_", "").Split('x');
                if (resParts.Length == 2 && int.TryParse(resParts[0], out int width) && int.TryParse(resParts[1], out int height))
                {
                    Screen.SetResolution(width, height,
                        CurrentSettingsSave.gameWindow == GameWindow.FullScreenWindow
                            ? FullScreenMode.FullScreenWindow
                            : FullScreenMode.Windowed);
                }
            }
        }

        private void LoadSettings()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(_settingsSavePath, FileMode.Open);
            CurrentSettingsSave = (GameSettings)formatter.Deserialize(stream);
            stream.Close();
            Debug.Log("设置数据已从: " + _settingsSavePath + " 加载");
        }

        public void SaveSettings()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(_settingsSavePath, FileMode.Create);
            formatter.Serialize(stream, CurrentSettingsSave);
            stream.Close();
            Debug.Log("设置数据已保存至: " + _settingsSavePath);
        }
        
        public string GetDefaultSavePath(int saveSlot)
        {
            return Path.Combine(Application.persistentDataPath, "SaveSlot" + saveSlot + ".sav");
        }
        
        public void LoadGame(string savePath)
        {
            if (!File.Exists(savePath))
            {
                Debug.LogError("存档文件不存在: " + savePath);
                return;
            }

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(savePath, FileMode.Open);
            CurrentGameSave = (GameSave)formatter.Deserialize(stream);
            stream.Close();
            Debug.Log("游戏存档已从: " + savePath + " 加载");
        }
        
        public void SaveGame(string savePath)
        {
            CurrentGameSave.LastSaveTime = DateTime.Now;
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(savePath, FileMode.Create);
            formatter.Serialize(stream, CurrentGameSave);
            stream.Close();
            Debug.Log("游戏存档已保存至: " + savePath);
        }
    }
}