using System.Collections.Generic;
using UnityEngine;

namespace CustomUnityHierarchy
{
    [CreateAssetMenu(fileName = "CustomUnityHierarchyData", menuName = "CustomUnityHierarchyData")]
    public class CustomUnityHierarchyData : ScriptableObject
    {
        // Components data
        public bool componentIconsEnabled = true;
        public bool compactScriptIcons = true;

        // Tree data
        public bool treeEnabled = true;
        public Color mainBranchColor = Color.green;
        public Color subBranchColor = Color.blue;

        // Header data
        public List<PrefixAndColor> prefixAndColor = new();
        public bool headersEnabled = true;

        // Layers and tags combined data
        public Color textColor = new(0.769f, 0.769f, 0.769f, 1);

        // Layers data
        public bool layersEnabled = true;

        // Tags data
        public bool tagsEnabled = true;

        // Hidden caches
        [HideInInspector] public bool firstTimeLoading = true;
        [HideInInspector] public List<GameObjectCache> gameObjectCache = new();
        [HideInInspector] public List<ComponentsAndTextures> componentsAndTextures = new();
        [HideInInspector] public HashSet<string> activeTags = new();
        [HideInInspector] public HashSet<string> activeLayers = new();
        [HideInInspector] public List<ComponentsAndTextures> inspectorIcons = new();
        [HideInInspector] public Texture tagTexture, layerTexture;
        [System.Serializable]
        public class GameObjectCache
        {
            public int instanceID;
            public int componentCount;
            public List<string> componentTypes;
            public string tag;
            public int layer;
            public bool isGameObjectActive;

            public GameObjectCache(int instanceID, int componentCount, List<string> componentTypes, string tag, int layer, bool isGameObjectActive)
            {
                this.instanceID = instanceID;
                this.componentCount = componentCount;
                this.componentTypes = componentTypes;
                this.tag = tag;
                this.layer = layer;
                this.isGameObjectActive = isGameObjectActive;
            }
        }
        [System.Serializable]
        public class ComponentsAndTextures
        {
            public string componentName;
            public Texture componentTexture;

            public ComponentsAndTextures(string componentName, Texture componentTexture)
            {
                this.componentName = componentName;
                this.componentTexture = componentTexture;
            }
        }
        [System.Serializable]
        public class PrefixAndColor
        {
            public string headerPrefix;
            public Color headerColor;

            public PrefixAndColor(string headerPrefix, Color headerColor)
            {
                this.headerPrefix = headerPrefix;
                this.headerColor = headerColor;
            }
        }
    }
}
