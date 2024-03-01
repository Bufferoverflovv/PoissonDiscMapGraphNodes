using System;
using System.Collections.Generic;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.MapGraph;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace Buffer.MapGraphExtensions
{
    /// <summary>
    /// A node that generates a uniform poisson disc sample
    /// </summary>
    [ScriptNode("Uniform Poisson Disc Sampling", "Drawing"), Serializable]
    public class UniformPoissonDiscSampleNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference]
        private InPort textureIn = null;

        [InPort("Mask", typeof(Mask)), SerializeReference]
        private InPort maskIn = null;

        [InPort("Draw Color", typeof(Color32)), SerializeReference]
        private InPort fillColorIn = null;

        [InPort("Points Per Iteration", typeof(int)), SerializeReference]
        private InPort pointsPerIterationIn = null;

        [InPort("Min Distance", typeof(float)), SerializeReference]
        private InPort minimumDistanceIn = null;

        [OutPort("Texture", typeof(TextureData)), SerializeReference]
        private OutPort textureOut = null;

        [OutPort("Mask", typeof(Mask)), SerializeReference]
        private OutPort maskOut = null;

        [OutPort("Placements", typeof(Vector2Int[])), SerializeReference]
        private OutPort placementsOut = null;

        private TextureData textureData;

#if UNITY_EDITOR
        /// <summary>
        /// Gets the latest generated texture data. Only available in the editor.
        /// </summary>
        public TextureData TextureData => textureData;
#endif

        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();

            var rng = Get<Rng>();

            var fillColor = fillColorIn.Get<Color32>();

            var minimumDistance = minimumDistanceIn.Get<float>();
            var pointsPerIteration = pointsPerIterationIn.Get<int>();
            var mask = maskIn.Get<Mask>();

            textureData = instanceProvider.Get<TextureData>();
            textureIn.Get<TextureData>().Clone(textureData);

            var width = textureData.Width;
            var height = textureData.Height;

            Mask outputMask = null;

            var availableTiles = instanceProvider.Get<List<int>>();

            if (mask != null)
            {
                outputMask = instanceProvider.Get<Mask>();
                mask.Clone(outputMask);
                availableTiles.AddRange(mask.UnmaskedPoints);
            }
            else 
            {
                List<int> unmaskedPoints = null;

                if (maskOut.IsConnected)
                {
                    unmaskedPoints = instanceProvider.Get<List<int>>();
                }

                availableTiles.EnsureCapacity(width * height);

                for (var i = 0; i < width * height; i++)
                {
                    availableTiles.Add(i);
                    unmaskedPoints?.Add(i);
                }

                if (maskOut.IsConnected)
                {
                    outputMask = instanceProvider.Get<Mask>();
                    outputMask.Set(unmaskedPoints);
                }
            }

            var placementCoords = instanceProvider.Get<List<Vector2Int>>();

            var topLeft = new Vector2(0, 0);
            var lowerRight = new Vector2(width, height);
            var sample = UniformPoissonDiskSampler.SampleRectangle(topLeft, lowerRight, minimumDistance, pointsPerIteration, rng.Next());

            placementCoords.EnsureCapacity(sample.Count);

            foreach (var point in sample)
            {
                var x = (int)point.x;
                var y = (int)point.y;

                if (x < 0 || x >= width || y < 0 || y >= height)
                {
                    continue;
                }

                var index = y * width + x;

                if (mask != null && mask.IsPointMasked(index))
                {
                    continue;
                }

                textureData[index] = fillColor;
                outputMask?.MaskPoint(index);
                placementCoords.Add(new Vector2Int(x, y));
            }

            textureOut.Set(() => textureData);
            maskOut.Set(() => outputMask);
            placementsOut.Set(() => placementCoords.ToArray());
        }
    }
}
