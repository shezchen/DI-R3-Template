using TMPro;
using UnityEngine;

namespace Gameplay
{
    public class TestTMPController : MonoBehaviour
    {
        [SerializeField] private TextMeshPro q;
        [SerializeField] private TextMeshPro s;
        [SerializeField] private TextMeshPro r;
        [SerializeField] private TextMeshPro center;
        [SerializeField] private TextMeshPro rawOffset;
        private CubeCoordinate _selfCoordinate;
        private Vector2Int _selfOffset;

        private void Awake()
        {
            SetAll(false);
        }

        public void SetSelfCoordinate(CubeCoordinate coordinate,Vector2Int offset)
        {
            _selfCoordinate = coordinate;
            _selfOffset = offset;
        }

        public void SetQSR(bool active)
        {
            q.gameObject.SetActive(active);
            s.gameObject.SetActive(active);
            r.gameObject.SetActive(active);
            q.text = _selfCoordinate.Q > 0 ? "+" + _selfCoordinate.Q.ToString() : _selfCoordinate.Q.ToString();
            s.text = _selfCoordinate.S > 0 ? "+" + _selfCoordinate.S.ToString() : _selfCoordinate.S.ToString();
            r.text = _selfCoordinate.R > 0 ? "+" + _selfCoordinate.R.ToString() : _selfCoordinate.R.ToString();
        }

        public void SetCube(bool active)
        {
            center.gameObject.SetActive(active);
            center.text = _selfCoordinate.ToString();
        }

        public void SetOffset(bool active)
        {
            rawOffset.gameObject.SetActive(active);
            rawOffset.text = _selfOffset.ToString();
        }

        public void SetAll(bool active)
        {
            q.gameObject.SetActive(active);
            s.gameObject.SetActive(active);
            r.gameObject.SetActive(active);
            center.gameObject.SetActive(active);
            rawOffset.gameObject.SetActive(active);
        }
    }
}