using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UI
{
    /// <summary>
    /// UI 页面栈管理器
    /// 负责管理页面的 Push/Pop/Clear 操作，协调页面生命周期调用
    /// </summary>
    public class UIPageStack
    {
        private class PageInfo
        {
            public GameObject GameObject;
            public IBasePage Page;
            public string AddressKey;
        }

        [ShowInInspector, ReadOnly]
        private readonly Stack<PageInfo> _pageStack = new();
        
        private readonly IObjectResolver _resolver;
        private readonly Transform _uiRoot;

        public UIPageStack(IObjectResolver resolver, Transform uiRoot)
        {
            _resolver = resolver;
            _uiRoot = uiRoot;
        }

        /// <summary>
        /// Push 新页面到栈顶
        /// </summary>
        /// <typeparam name="T">页面类型，必须实现 IBasePage 接口</typeparam>
        /// <param name="prefab">页面预制体</param>
        /// <param name="addressKey">Addressable 资源 Key</param>
        /// <returns>新创建的页面实例</returns>
        public async UniTask<T> PushPage<T>(GameObject prefab, string addressKey) where T : MonoBehaviour, IBasePage
        {
            // 暂停当前栈顶页面
            if (_pageStack.Count > 0)
            {
                var currentTop = _pageStack.Peek();
                await currentTop.Page.OnPause();
            }

            // 实例化并入栈
            var go = _resolver.Instantiate(prefab, _uiRoot);
            var page = go.GetComponent<T>();

            if (page == null)
            {
                Debug.LogError($"[UIPageStack] Prefab {prefab.name} 没有实现 IBasePage 接口的组件");
                Object.Destroy(go);
                return null;
            }

            _pageStack.Push(new PageInfo
            {
                GameObject = go,
                Page = page,
                AddressKey = addressKey
            });

            // 调用新页面的 OnEnter
            await page.OnEnter();

            Debug.Log($"[UIPageStack] Push 页面: {typeof(T).Name}, 当前栈深度: {_pageStack.Count}");
            return page;
        }

        /// <summary>
        /// Pop 栈顶页面
        /// </summary>
        public async UniTask PopPage()
        {
            if (_pageStack.Count == 0)
            {
                Debug.LogWarning("[UIPageStack] 栈为空，无法 Pop");
                return;
            }

            var topPage = _pageStack.Pop();

            // 调用 OnExit 并销毁
            await topPage.Page.OnExit();
            Object.Destroy(topPage.GameObject);

            Debug.Log($"[UIPageStack] Pop 页面，当前栈深度: {_pageStack.Count}");

            // 恢复下层页面
            if (_pageStack.Count > 0)
            {
                var newTop = _pageStack.Peek();
                await newTop.Page.OnResume();
            }
        }

        /// <summary>
        /// 清空栈中所有页面
        /// </summary>
        public async UniTask ClearStack()
        {
            Debug.Log($"[UIPageStack] 开始清空栈，当前深度: {_pageStack.Count}");
            
            while (_pageStack.Count > 0)
            {
                var topPage = _pageStack.Pop();
                await topPage.Page.OnExit();
                Object.Destroy(topPage.GameObject);
            }
            
            Debug.Log("[UIPageStack] 栈已清空");
        }

        /// <summary>
        /// 获取栈顶页面
        /// </summary>
        public IBasePage GetTopPage()
        {
            return _pageStack.Count > 0 ? _pageStack.Peek().Page : null;
        }

        /// <summary>
        /// 获取栈深度
        /// </summary>
        public int Count => _pageStack.Count;

        /// <summary>
        /// 检查栈是否为空
        /// </summary>
        public bool IsEmpty => _pageStack.Count == 0;
    }
}
