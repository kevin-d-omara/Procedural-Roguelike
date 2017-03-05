using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    // This file contains all the debug methods which extend CaveManager.
    public partial class CaveManager : BoardManager
    {
        /// <summary>
        /// Set the seed to a random or specific value.
        /// </summary>
        private void SetRandomState()
        {
            //var random = (int)System.DateTime.Now.Ticks;
            var random = -1212239271;
            Debug.Log(random); Random.InitState(random);

        }

        public GameObject plotPrefab;
        private enum Feature { Bottleneck, Fork, Chamber }
        private enum Entity { Chest, Obstacle, Enemy }

        /// <summary>
        /// Type of feature to plot.
        /// </summary>
        private Dictionary<Vector2, Feature> featurePlots = new Dictionary<Vector2, Feature>();

        /// <summary>
        /// Type of entity to plot.
        /// </summary>
        private Dictionary<Vector2, Entity> entityPlots = new Dictionary<Vector2, Entity>();

        /// <summary>
        /// Display a simple debug version of the cave.
        /// </summary>
        private void PlotPaintedCave(bool shouldPlotEssentialPath)
        {
            // Plot feature points.
            foreach (KeyValuePair<Vector2, Feature> feature in featurePlots)
            {
                switch (feature.Value)
                {
                    case Feature.Bottleneck:
                        PlotPoint(feature.Key, Color.magenta);
                        break;
                    case Feature.Fork:
                        PlotPoint(feature.Key, Color.red);
                        break;
                    case Feature.Chamber:
                        PlotPoint(feature.Key, Color.blue);
                        break;
                    default:
                        break;
                }
            }

            // Plot entity points.
            foreach (KeyValuePair<Vector2, Entity> entity in entityPlots)
            {
                switch (entity.Value)
                {
                    case Entity.Chest:
                        PlotPoint(entity.Key, Color.green);
                        break;
                    case Entity.Obstacle:
                        PlotPoint(entity.Key, Color.grey);
                        break;
                    case Entity.Enemy:
                        PlotPoint(entity.Key, Color.yellow);
                        break;
                    default:
                        break;
                }
            }

            if (shouldPlotEssentialPath)
            {
                // Plot essential path.
                foreach (Vector2 pt in caveEssentialPath)
                {
                    PlotPoint(pt, Color.white);
                }
            }
        }

        /// <summary>
        /// Plot a single point on the screne.
        /// </summary>
        private void PlotPoint(Vector2 position, Color color)
        {
            var plotPt = Instantiate(plotPrefab, position, Quaternion.identity);
            color.a = .1f;
            plotPt.GetComponent<SpriteRenderer>().color = color;
            plotPt.transform.SetParent(holders["Floor"]);
        }
    }
}
