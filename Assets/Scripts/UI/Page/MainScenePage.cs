using Architecture;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Generated;
using R3;
using Sirenix.OdinInspector;
using Tools;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;
using VContainer;

namespace UI
{
    /// <summary>
    /// 主界面 Page
    /// </summary>
    [RequireComponent(typeof(UIBinder), typeof(GraphicRaycaster))]
    public class MainScenePage : MonoBehaviour, IBasePage
    {
        [BoxGroup("按任意键"), Header("按任意键"), SerializeField]
        private CanvasGroup pressAnyButton;

        [BoxGroup("按任意键"), Header("偏移量"), SerializeField]
        private Vector2 slideOffset;

        [BoxGroup("按任意键"), Header("滑动时间"), SerializeField]
        private float slideDuration;

        [BoxGroup("主页面"), Header("主页面"), SerializeField]
        private CanvasGroup mainSceneContent;

        [BoxGroup("主页面"), Header("主页面出现时间"), SerializeField]
        private float mainSceneDuration;

        [BoxGroup("主页面"), Header("默认选中按钮"), SerializeField]
        private Button defaultSelectedButton;

        [Inject] private IAudioService _audioService;
        [Inject] private UIManager _uiManager;

        private UIBinder _uiBinder;
        private GraphicRaycaster _raycaster;

        private void Awake()
        {
            _raycaster = GetComponent<GraphicRaycaster>();
            _uiBinder = GetComponent<UIBinder>();

            // 绑定设置按钮事件
            _uiBinder.Get<Button>("Button_Settings").OnClickAsObservable().Subscribe((_) =>
            {
                _audioService.PlaySfxAsync(AudioClipName.SFX.ClickSound);
                // 使用新的页面栈 API 推入设置页面
                _uiManager.PushPage<SettingsPage>(AddressableKeys.Assets.SettingsPagePrefab).Forget();
            }).AddTo(this);
        }

        #region IBasePage 实现

        public async UniTask OnEnter()
        {
            if (_raycaster != null) _raycaster.enabled = true;

            // 播放"按任意键"入场动画
            pressAnyButton.gameObject.SetActive(true);
            var canvasGroup = pressAnyButton;
            var pos = pressAnyButton.transform.localPosition;
            pressAnyButton.transform.localPosition -= (Vector3)slideOffset;
            canvasGroup.alpha = 0;

            var seq = DOTween.Sequence();
            seq.Append(pressAnyButton.transform.LocalMoveTo(pos, slideDuration));
            seq.Join(canvasGroup.FadeIn(slideDuration));
            await seq.AsyncWaitForCompletion();

            // 等待任意按键
            InputSystem.onAnyButtonPress.CallOnce((_) =>
            {
                _audioService.PlaySfxAsync(AudioClipName.SFX.ClickSound);
                var cg = pressAnyButton;
                var currentPos = pressAnyButton.transform.localPosition;
                var hideSeq = DOTween.Sequence();
                hideSeq.Append(pressAnyButton.transform.LocalMoveTo(currentPos + (Vector3)slideOffset, slideDuration));
                hideSeq.Join(cg.FadeOut(slideDuration));
                hideSeq.OnComplete(() =>
                {
                    pressAnyButton.gameObject.SetActive(false);
                    ShowMainScene().Forget();
                });
            });
        }

        public async UniTask OnPause()
        {
            if (_raycaster != null) _raycaster.enabled = false;
            gameObject.SetActive(false);
            await UniTask.CompletedTask;
        }

        public async UniTask OnResume()
        {
            gameObject.SetActive(true);
            if (_raycaster != null) _raycaster.enabled = true;
            defaultSelectedButton.Select();
            await UniTask.CompletedTask;
        }

        public async UniTask OnExit()
        {
            await UniTask.CompletedTask;
        }

        #endregion

        /// <summary>
        /// 按任意键之后，显示主页面的全部内容
        /// </summary>
        private async UniTask ShowMainScene()
        {
            mainSceneContent.gameObject.SetActive(true);
            mainSceneContent.alpha = 0;
            await mainSceneContent.FadeIn(mainSceneDuration).AsyncWaitForCompletion();
            defaultSelectedButton.Select();
        }

        /// <summary>
        /// 隐藏主页面内容
        /// </summary>
        private async UniTask HideMainScene()
        {
            await mainSceneContent.FadeOut(mainSceneDuration).AsyncWaitForCompletion();
            mainSceneContent.gameObject.SetActive(false);
        }
    }
}
