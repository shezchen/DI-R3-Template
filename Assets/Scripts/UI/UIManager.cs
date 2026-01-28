using System;
using System.Collections.Generic;
using System.Linq;
using Architecture;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;

namespace UI
{
    /// <summary>
    /// UI 管理器
    /// 负责 UI 资源的预加载和页面栈的管理
    /// </summary>
    public class UIManager : IDisposable
    {
        private readonly UIRoot _uiRoot;
        private readonly EventBus _eventBus;
        private readonly IObjectResolver _resolver;
        private readonly UIPageStack _pageStack;

        [Title("Resource Management")]
        [ShowInInspector, ReadOnly]
        private Dictionary<string, AsyncOperationHandle<GameObject>> _uiHandles = new();

        public UIManager(UIRoot root, EventBus eventBus, IObjectResolver resolver)
        {
            _uiRoot = root;
            _eventBus = eventBus;
            _resolver = resolver;
            _pageStack = new UIPageStack(resolver, root.transform);
        }

        public async UniTask Init()
        {
            // 初始化为空，后续根据流程控制器调用 RefreshPreloadedUI
            await UniTask.CompletedTask;
        }

        #region 页面栈操作

        /// <summary>
        /// Push 新页面到栈顶
        /// </summary>
        /// <typeparam name="T">页面类型，必须实现 IBasePage 接口</typeparam>
        /// <param name="addressableKey">Addressable 资源 Key</param>
        /// <returns>新创建的页面实例</returns>
        public async UniTask<T> PushPage<T>(string addressableKey) where T : MonoBehaviour, IBasePage
        {
            var prefab = await GetPreloadedPrefab(addressableKey);
            return await _pageStack.PushPage<T>(prefab, addressableKey);
        }

        /// <summary>
        /// Pop 栈顶页面
        /// </summary>
        public async UniTask PopPage()
        {
            await _pageStack.PopPage();
        }

        /// <summary>
        /// 清空栈中所有页面
        /// </summary>
        public async UniTask ClearAllPages()
        {
            await _pageStack.ClearStack();
        }

        /// <summary>
        /// 获取栈顶页面
        /// </summary>
        public IBasePage GetTopPage()
        {
            return _pageStack.GetTopPage();
        }

        /// <summary>
        /// 获取当前页面栈深度
        /// </summary>
        public int PageCount => _pageStack.Count;

        #endregion

        #region 资源管理

        /// <summary>
        /// 动态刷新预加载状态：加载列表中的资源，释放不在列表中的资源
        /// </summary>
        /// <param name="targetKeys">当前阶段需要的 UI 资源 Key 列表</param>
        public void RefreshPreloadedUI(List<string> targetKeys)
        {
            if (targetKeys == null) return;

            // 释放不再需要的资源
            var keysToRemove = _uiHandles.Keys
                .Where(key => !targetKeys.Contains(key))
                .ToList();

            foreach (var key in keysToRemove)
            {
                if (_uiHandles[key].IsValid())
                {
                    Addressables.Release(_uiHandles[key]);
                }
                _uiHandles.Remove(key);
                Debug.Log($"[UIManager] 已释放不需要的 UI 资源: {key}");
            }

            // 加载新增的资源
            foreach (var key in targetKeys)
            {
                if (!_uiHandles.ContainsKey(key))
                {
                    var handle = Addressables.LoadAssetAsync<GameObject>(key);
                    _uiHandles.Add(key, handle);
                    Debug.Log($"[UIManager] 开始预加载新增 UI 资源: {key}");
                }
            }
        }

        /// <summary>
        /// 获取已预加载的 Prefab，若不存在则进行强制加载
        /// </summary>
        private async UniTask<GameObject> GetPreloadedPrefab(string key)
        {
            if (_uiHandles.TryGetValue(key, out var handle))
            {
                return await handle;
            }

            var newHandle = Addressables.LoadAssetAsync<GameObject>(key);
            _uiHandles.Add(key, newHandle);
            return await newHandle;
        }

        #endregion

        public void Dispose()
        {
            // 清空页面栈（同步方式，因为 Dispose 不支持 async）
            // 注意：这里不调用生命周期方法，直接释放资源
            foreach (var handle in _uiHandles.Values)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            _uiHandles.Clear();
            Debug.Log("[UIManager] 资源句柄已全部释放");
        }
    }
}
