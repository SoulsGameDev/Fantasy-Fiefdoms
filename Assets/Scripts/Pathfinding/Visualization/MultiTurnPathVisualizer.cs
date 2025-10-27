using System.Collections.Generic;
using UnityEngine;
using Pathfinding.Core;

namespace Pathfinding.Visualization
{
    /// <summary>
    /// Handles visualization of multi-turn paths.
    /// Provides color-coding and markers for different turn segments.
    /// </summary>
    public class MultiTurnPathVisualizer : MonoBehaviour
    {
        [Header("Turn Colors")]
        [SerializeField]
        [Tooltip("Colors for each turn (cycles if more turns than colors)")]
        private Color[] turnColors = new Color[]
        {
            new Color(0.2f, 0.8f, 0.2f, 0.7f),  // Turn 1: Green
            new Color(0.2f, 0.2f, 0.8f, 0.7f),  // Turn 2: Blue
            new Color(0.8f, 0.8f, 0.2f, 0.7f),  // Turn 3: Yellow
            new Color(0.8f, 0.2f, 0.8f, 0.7f),  // Turn 4: Magenta
            new Color(0.2f, 0.8f, 0.8f, 0.7f),  // Turn 5: Cyan
        };

        [Header("Waypoint Markers")]
        [SerializeField]
        private GameObject waypointMarkerPrefab;

        [SerializeField]
        private Color waypointColor = new Color(1f, 0.5f, 0f, 1f);

        [SerializeField]
        private float waypointMarkerScale = 1.5f;

        private List<GameObject> activeMarkers = new List<GameObject>();
        private Dictionary<HexCell, Color> cellColors = new Dictionary<HexCell, Color>();

        /// <summary>
        /// Visualizes a multi-turn path with color-coded segments
        /// </summary>
        public void VisualizePath(MultiTurnPathResult path)
        {
            if (path == null || !path.Success)
            {
                Debug.LogWarning("Cannot visualize invalid path");
                return;
            }

            ClearVisualization();

            // Color each turn segment
            for (int turnIndex = 0; turnIndex < path.TurnsRequired; turnIndex++)
            {
                Color turnColor = GetTurnColor(turnIndex);
                var turnCells = path.GetTurnPath(turnIndex);

                foreach (var cell in turnCells)
                {
                    VisualizeTurnCell(cell, turnColor, turnIndex);
                    cellColors[cell] = turnColor;
                }
            }

            // Mark turn endpoints (waypoints)
            for (int i = 0; i < path.TurnEndpoints.Count; i++)
            {
                var endpoint = path.TurnEndpoints[i];
                CreateWaypointMarker(endpoint, i + 1);
            }

            Debug.Log($"Visualized {path.TurnsRequired}-turn path");
        }

        /// <summary>
        /// Visualizes multi-turn reachable cells grouped by turn
        /// </summary>
        public void VisualizeReachability(Dictionary<int, List<HexCell>> cellsByTurn)
        {
            if (cellsByTurn == null)
            {
                Debug.LogWarning("Cannot visualize null reachability data");
                return;
            }

            ClearVisualization();

            foreach (var kvp in cellsByTurn)
            {
                int turnIndex = kvp.Key;
                Color turnColor = GetTurnColor(turnIndex);

                foreach (var cell in kvp.Value)
                {
                    VisualizeTurnCell(cell, turnColor, turnIndex);
                    cellColors[cell] = turnColor;
                }
            }

            Debug.Log($"Visualized {cellsByTurn.Count}-turn reachability");
        }

        /// <summary>
        /// Clears all visualization elements
        /// </summary>
        public void ClearVisualization()
        {
            // Remove waypoint markers
            foreach (var marker in activeMarkers)
            {
                if (marker != null)
                {
                    Destroy(marker);
                }
            }
            activeMarkers.Clear();

            // Clear cell colors (restore default)
            foreach (var kvp in cellColors)
            {
                RestoreCellVisuals(kvp.Key);
            }
            cellColors.Clear();
        }

        /// <summary>
        /// Gets the color for a specific turn index
        /// </summary>
        private Color GetTurnColor(int turnIndex)
        {
            if (turnColors.Length == 0)
                return Color.white;

            return turnColors[turnIndex % turnColors.Length];
        }

        /// <summary>
        /// Visualizes a single cell as part of a turn segment
        /// </summary>
        private void VisualizeTurnCell(HexCell cell, Color color, int turnIndex)
        {
            if (cell == null)
                return;

            // TODO: Implement your visual effect here
            // This depends on your rendering system
            // Examples:
            // - Change cell material color
            // - Add overlay quad with turn color
            // - Modify emission
            // - Add particle effect

            // Example implementation (requires renderer on cell):
            // var renderer = cell.GetComponent<Renderer>();
            // if (renderer != null)
            // {
            //     renderer.material.SetColor("_TurnColor", color);
            // }

            // Mark cell for pathfinding visualization
            cell.PathfindingState.IsPath = true;
        }

        /// <summary>
        /// Restores cell to default visual state
        /// </summary>
        private void RestoreCellVisuals(HexCell cell)
        {
            if (cell == null)
                return;

            // TODO: Restore default visuals
            // var renderer = cell.GetComponent<Renderer>();
            // if (renderer != null)
            // {
            //     renderer.material.SetColor("_TurnColor", Color.clear);
            // }

            cell.PathfindingState.ClearPath();
        }

        /// <summary>
        /// Creates a waypoint marker at a turn endpoint
        /// </summary>
        private void CreateWaypointMarker(HexCell cell, int turnNumber)
        {
            if (cell == null)
                return;

            GameObject marker;

            if (waypointMarkerPrefab != null)
            {
                // Use custom prefab
                Vector3 position = GetCellCenterPosition(cell);
                marker = Instantiate(waypointMarkerPrefab, position, Quaternion.identity, transform);
                marker.transform.localScale = Vector3.one * waypointMarkerScale;
            }
            else
            {
                // Create simple primitive marker
                marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.transform.position = GetCellCenterPosition(cell);
                marker.transform.localScale = Vector3.one * waypointMarkerScale;
                marker.transform.SetParent(transform);

                var renderer = marker.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = waypointColor;
                }
            }

            marker.name = $"TurnEndpoint_{turnNumber}";
            activeMarkers.Add(marker);

            // Optionally add a text label showing turn number
            // AddTurnNumberLabel(marker, turnNumber);
        }

        /// <summary>
        /// Gets the world position of a cell's center
        /// </summary>
        private Vector3 GetCellCenterPosition(HexCell cell)
        {
            if (cell == null || cell.Grid == null)
                return Vector3.zero;

            Vector3 center = HexMetrics.Center(
                cell.HexSize,
                (int)cell.OffsetCoordinates.x,
                (int)cell.OffsetCoordinates.y,
                HexOrientation.PointyTop // Adjust based on your orientation
            );

            return center + cell.Grid.transform.position + Vector3.up * 0.5f; // Offset above terrain
        }

        /// <summary>
        /// Highlights a specific turn segment
        /// </summary>
        public void HighlightTurn(MultiTurnPathResult path, int turnIndex)
        {
            if (path == null || !path.Success || turnIndex < 0 || turnIndex >= path.TurnsRequired)
                return;

            // Dim all cells
            foreach (var kvp in cellColors)
            {
                Color dimmedColor = kvp.Value;
                dimmedColor.a *= 0.3f;
                VisualizeTurnCell(kvp.Key, dimmedColor, -1);
            }

            // Highlight selected turn
            Color highlightColor = GetTurnColor(turnIndex);
            var turnCells = path.GetTurnPath(turnIndex);
            foreach (var cell in turnCells)
            {
                VisualizeTurnCell(cell, highlightColor, turnIndex);
            }
        }

        /// <summary>
        /// Shows the path up to a specific turn
        /// </summary>
        public void ShowPathUpToTurn(MultiTurnPathResult path, int completedTurns)
        {
            if (path == null || !path.Success)
                return;

            ClearVisualization();

            // Show only completed turns
            for (int turnIndex = 0; turnIndex < completedTurns && turnIndex < path.TurnsRequired; turnIndex++)
            {
                Color turnColor = GetTurnColor(turnIndex);
                turnColor.a *= 0.5f; // Dim completed turns

                var turnCells = path.GetTurnPath(turnIndex);
                foreach (var cell in turnCells)
                {
                    VisualizeTurnCell(cell, turnColor, turnIndex);
                    cellColors[cell] = turnColor;
                }
            }

            // Highlight current turn
            if (completedTurns < path.TurnsRequired)
            {
                Color currentColor = GetTurnColor(completedTurns);
                var currentCells = path.GetTurnPath(completedTurns);
                foreach (var cell in currentCells)
                {
                    VisualizeTurnCell(cell, currentColor, completedTurns);
                    cellColors[cell] = currentColor;
                }
            }
        }

        /// <summary>
        /// Creates a legend showing turn colors
        /// </summary>
        public void CreateColorLegend(MultiTurnPathResult path)
        {
            // TODO: Implement UI legend
            // This would typically be done with Unity UI
            Debug.Log("Turn Color Legend:");
            for (int i = 0; i < path.TurnsRequired; i++)
            {
                Color color = GetTurnColor(i);
                Debug.Log($"  Turn {i + 1}: RGB({color.r:F2}, {color.g:F2}, {color.b:F2})");
            }
        }

        private void OnDestroy()
        {
            ClearVisualization();
        }
    }
}
