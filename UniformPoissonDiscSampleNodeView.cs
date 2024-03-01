using InsaneScatterbrain.MapGraph.Editor;
using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace Buffer.MapGraphExtensions
{
    [ScriptNodeView(typeof(UniformPoissonDiscSampleNode))]
    public class UniformPoissonDiscSampleNodeView : ScriptNodeView
    {
        public UniformPoissonDiscSampleNodeView(UniformPoissonDiscSampleNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<UniformPoissonDiscSampleNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(UniformPoissonDiscSampleNode node) => node.TextureData.ToTexture2D();
    }
}
