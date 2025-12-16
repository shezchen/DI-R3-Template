using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

namespace Gameplay
{
    /// <summary>
    /// 阅读同一游戏物体上的Tilemap，并以testGameObjectRoot为根对象创建调试信息（TMP预制体）
    /// </summary>
    [RequireComponent(typeof(Tilemap))]
    public class TilemapReader : MonoBehaviour
    {
        [SerializeField] private Transform testGameObjectRoot;
        [SerializeField] private bool showTester;
        [SerializeField] private bool showQRS;
        [SerializeField] private bool showCube;
        [SerializeField] private bool showOffset;

        private bool _preDisplayTester;
        private bool _preShowQRS;
        private bool _preShowCube;
        private bool _preShowOffset;
        
        private List<TestTMPController> _controllerList = new List<TestTMPController>();
        
        private Tilemap _tilemap;
        private List<Vector3Int> _cacheResult;
        private bool _cached;

        private UniTask _initTask;

        private void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
            Assert.IsNotNull(_tilemap);
            _initTask = Init();
        }

        private async UniTask Init()
        {
            var allOffset = GetTilesPosition();
            foreach (var offset in allOffset)
            {
                var g = await Addressables.InstantiateAsync("Demo_TestTMP");
                g.transform.position = _tilemap.CellToWorld(offset);
                g.transform.SetParent(testGameObjectRoot);
                var control = g.GetComponent<TestTMPController>();
                Assert.IsNotNull(control);
                control.SetSelfCoordinate(Tools.HexMath.OffsetToCube(offset)
                    , new Vector2Int(offset.x, offset.y));
                _controllerList.Add(control);
            }
        }

        private void Update()
        {
            if (showTester != _preDisplayTester || (showTester && showQRS != _preShowQRS) ||
                (showTester && showCube != _preShowCube) || (showTester && showOffset != _preShowOffset))
            {
                RefreshDisplayTester();
            }
            _preDisplayTester = showTester;
            _preShowQRS = showQRS;
            _preShowCube = showCube;
            _preShowOffset = showOffset;
        }

        public List<Vector3Int> GetTilesPosition()
        {
            if (_cached)
            {
                return _cacheResult;
            }
            var result = new List<Vector3Int>();

            if (_tilemap == null)
            {
                Debug.LogWarning("TilemapReader: tilemap 未设置");
                return result;
            }

            // cellBounds 是“包含所有已用格子”的包围盒
            var bounds = _tilemap.cellBounds;
            foreach (var pos in bounds.allPositionsWithin)
            {
                if (_tilemap.HasTile(pos))
                {
                    result.Add(pos);
                }
            }

            _cached = true;
            _cacheResult = result;
            return result;
        }

        private async void RefreshDisplayTester()
        {
            try
            {
                if (!_initTask.Status.IsCompleted())
                {
                    await _initTask;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            foreach (var controller in _controllerList)
            {
                controller.SetQSR(showTester && showQRS);
                controller.SetCube(showTester && showCube);
                controller.SetOffset(showTester && showOffset);
            }
        }
    }
}
