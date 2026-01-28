using Architecture;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Generated;
using R3;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace UI
{
    /// <summary>
    /// 语言选择页面
    /// 切换语言会有延迟，所以这里要监听语言切换事件来手动更新UI
    /// </summary>
    [RequireComponent(typeof(UIBinder), typeof(CanvasGroup), typeof(GraphicRaycaster))]
    public class LanguagePage : MonoBehaviour, IBasePage
    {
        [SerializeField] private TextMeshProUGUI languageText;
        [SerializeField] private float fadeDuration = 0.5f;

        [Inject] private EventBus _eventBus;
        [Inject] private UIManager _uiManager;
        
        private UIBinder _uiBinder;
        private CanvasGroup _canvasGroup;
        private GraphicRaycaster _raycaster;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _raycaster = GetComponent<GraphicRaycaster>();
            _canvasGroup.alpha = 0;
            
            // 订阅语言变更事件
            _eventBus.Receive<LanguageChangeEvent>().Subscribe((type) =>
            {
                switch (type.NewLanguage)
                {
                    case GameLanguageType.Chinese:
                        languageText.text = "这是正确的语言吗？";
                        break;
                    case GameLanguageType.English:
                        languageText.text = "Is this the correct language?";
                        break;
                    case GameLanguageType.Japanese:
                        languageText.text = "これは正しい言語ですか？";
                        break;
                }
            }).AddTo(this);

            // 绑定按钮事件
            _uiBinder = GetComponent<UIBinder>();
            _uiBinder.Get<Button>("Button_Chinese").OnClickAsObservable().Subscribe((_) =>
            {
                OnLanguageSelected(GameLanguageType.Chinese);
            }).AddTo(this);
            _uiBinder.Get<Button>("Button_English").OnClickAsObservable().Subscribe((_) =>
            {
                OnLanguageSelected(GameLanguageType.English);
            }).AddTo(this);
            _uiBinder.Get<Button>("Button_Japanese").OnClickAsObservable().Subscribe((_) =>
            {
                OnLanguageSelected(GameLanguageType.Japanese);
            }).AddTo(this);
        }

        private void OnLanguageSelected(GameLanguageType language)
        {
            _eventBus.Publish(new LanguageConfirmEvent(language));
            _uiManager.PopPage().ContinueWith(() =>
            {
                _uiManager.PushPage<MainScenePage>(AddressableKeys.Assets.MainScenePrefab).Forget();
            }).Forget();
        }

        #region IBasePage 实现

        public async UniTask OnEnter()
        {
            if (_raycaster != null) _raycaster.enabled = true;
            await _canvasGroup.FadeIn(fadeDuration).AsyncWaitForCompletion();
        }

        public async UniTask OnPause()
        {
            if (_raycaster != null) _raycaster.enabled = false;
            await UniTask.CompletedTask;
        }

        public async UniTask OnResume()
        {
            if (_raycaster != null) _raycaster.enabled = true;
            await UniTask.CompletedTask;
        }

        public async UniTask OnExit()
        {
            await _canvasGroup.FadeOut(fadeDuration).AsyncWaitForCompletion();
        }

        #endregion
    }
}
