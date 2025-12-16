using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace Gameplay
{
    public class MouseSelect : MonoBehaviour
    {
        [SerializeField] private Tilemap targetTilemap;

        [ShowInInspector]public Vector3Int CurrentCellPos { get; private set; }

        private void Update()
        {
            if (Camera.main != null)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                Ray ray = Camera.main.ScreenPointToRay(mousePos);
                if (targetTilemap != null)
                {
                    Plane plane = new Plane(Vector3.forward, Vector3.zero);
                    if (plane.Raycast(ray, out float enter))
                    {
                        Vector3 worldPoint = ray.GetPoint(enter);
                        Vector3Int cellPos = targetTilemap.WorldToCell(worldPoint);

                        if (targetTilemap.HasTile(cellPos))
                        {
                            CurrentCellPos = cellPos;
                        }
                    }
                }
            }
        }
    }
}