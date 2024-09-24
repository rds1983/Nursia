//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace glTFLoader.Schema {
    using System.Linq;
    using System.Runtime.Serialization;
    
    
    internal class Gltf {
        
        /// <summary>
        /// Backing field for ExtensionsUsed.
        /// </summary>
        private string[] m_extensionsUsed;
        
        /// <summary>
        /// Backing field for ExtensionsRequired.
        /// </summary>
        private string[] m_extensionsRequired;
        
        /// <summary>
        /// Backing field for Accessors.
        /// </summary>
        private Accessor[] m_accessors;
        
        /// <summary>
        /// Backing field for Animations.
        /// </summary>
        private Animation[] m_animations;
        
        /// <summary>
        /// Backing field for Asset.
        /// </summary>
        private Asset m_asset;
        
        /// <summary>
        /// Backing field for Buffers.
        /// </summary>
        private Buffer[] m_buffers;
        
        /// <summary>
        /// Backing field for BufferViews.
        /// </summary>
        private BufferView[] m_bufferViews;
        
        /// <summary>
        /// Backing field for Cameras.
        /// </summary>
        private Camera[] m_cameras;
        
        /// <summary>
        /// Backing field for Images.
        /// </summary>
        private Image[] m_images;
        
        /// <summary>
        /// Backing field for Materials.
        /// </summary>
        private Material[] m_materials;
        
        /// <summary>
        /// Backing field for Meshes.
        /// </summary>
        private Mesh[] m_meshes;
        
        /// <summary>
        /// Backing field for Nodes.
        /// </summary>
        private Node[] m_nodes;
        
        /// <summary>
        /// Backing field for Samplers.
        /// </summary>
        private Sampler[] m_samplers;
        
        /// <summary>
        /// Backing field for Scene.
        /// </summary>
        private System.Nullable<int> m_scene;
        
        /// <summary>
        /// Backing field for Scenes.
        /// </summary>
        private Scene[] m_scenes;
        
        /// <summary>
        /// Backing field for Skins.
        /// </summary>
        private Skin[] m_skins;
        
        /// <summary>
        /// Backing field for Textures.
        /// </summary>
        private Texture[] m_textures;
        
        /// <summary>
        /// Backing field for Extensions.
        /// </summary>
        private System.Collections.Generic.Dictionary<string, object> m_extensions;
        
        /// <summary>
        /// Backing field for Extras.
        /// </summary>
        private Extras m_extras;
        
        /// <summary>
        /// Names of glTF extensions used in this asset.
        /// </summary>
        [Newtonsoft.Json.JsonConverterAttribute(typeof(glTFLoader.Shared.ArrayConverter))]
        [Newtonsoft.Json.JsonPropertyAttribute("extensionsUsed")]
        public string[] ExtensionsUsed {
            get {
                return this.m_extensionsUsed;
            }
            set {
                if ((value == null)) {
                    this.m_extensionsUsed = value;
                    return;
                }
                if ((value.Length < 1u)) {
                    throw new System.ArgumentException("Array not long enough");
                }
                this.m_extensionsUsed = value;
            }
        }
        
        /// <summary>
        /// Names of glTF extensions required to properly load this asset.
        /// </summary>
        [Newtonsoft.Json.JsonConverterAttribute(typeof(glTFLoader.Shared.ArrayConverter))]
        [Newtonsoft.Json.JsonPropertyAttribute("extensionsRequired")]
        public string[] ExtensionsRequired {
            get {
                return this.m_extensionsRequired;
            }
            set {
                if ((value == null)) {
                    this.m_extensionsRequired = value;
                    return;
                }
                if ((value.Length < 1u)) {
                    throw new System.ArgumentException("Array not long enough");
                }
                this.m_extensionsRequired = value;
            }
        }
        
        /// <summary>
        /// An array of accessors.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("accessors")]
        public Accessor[] Accessors {
            get {
                return this.m_accessors;
            }
            set {
                if ((value == null)) {
                    this.m_accessors = value;
                    return;
                }
                if ((value.Length < 1u)) {
                    throw new System.ArgumentException("Array not long enough");
                }
                this.m_accessors = value;
            }
        }
        
        /// <summary>
        /// An array of keyframe animations.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("animations")]
        public Animation[] Animations {
            get {
                return this.m_animations;
            }
            set {
                if ((value == null)) {
                    this.m_animations = value;
                    return;
                }
                if ((value.Length < 1u)) {
                    throw new System.ArgumentException("Array not long enough");
                }
                this.m_animations = value;
            }
        }
        
        /// <summary>
        /// Metadata about the glTF asset.
        /// </summary>
        [Newtonsoft.Json.JsonRequiredAttribute()]
        [Newtonsoft.Json.JsonPropertyAttribute("asset")]
        public Asset Asset {
            get {
                return this.m_asset;
            }
            set {
                this.m_asset = value;
            }
        }
        
        /// <summary>
        /// An array of buffers.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("buffers")]
        public Buffer[] Buffers {
            get {
                return this.m_buffers;
            }
            set {
                if ((value == null)) {
                    this.m_buffers = value;
                    return;
                }
                if ((value.Length < 1u)) {
                    throw new System.ArgumentException("Array not long enough");
                }
                this.m_buffers = value;
            }
        }
        
        /// <summary>
        /// An array of bufferViews.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("bufferViews")]
        public BufferView[] BufferViews {
            get {
                return this.m_bufferViews;
            }
            set {
                if ((value == null)) {
                    this.m_bufferViews = value;
                    return;
                }
                if ((value.Length < 1u)) {
                    throw new System.ArgumentException("Array not long enough");
                }
                this.m_bufferViews = value;
            }
        }
        
        /// <summary>
        /// An array of cameras.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("cameras")]
        public Camera[] Cameras {
            get {
                return this.m_cameras;
            }
            set {
                if ((value == null)) {
                    this.m_cameras = value;
                    return;
                }
                if ((value.Length < 1u)) {
                    throw new System.ArgumentException("Array not long enough");
                }
                this.m_cameras = value;
            }
        }
        
        /// <summary>
        /// An array of images.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("images")]
        public Image[] Images {
            get {
                return this.m_images;
            }
            set {
                if ((value == null)) {
                    this.m_images = value;
                    return;
                }
                if ((value.Length < 1u)) {
                    throw new System.ArgumentException("Array not long enough");
                }
                this.m_images = value;
            }
        }
        
        /// <summary>
        /// An array of materials.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("materials")]
        public Material[] Materials {
            get {
                return this.m_materials;
            }
            set {
                if ((value == null)) {
                    this.m_materials = value;
                    return;
                }
                if ((value.Length < 1u)) {
                    throw new System.ArgumentException("Array not long enough");
                }
                this.m_materials = value;
            }
        }
        
        /// <summary>
        /// An array of meshes.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("meshes")]
        public Mesh[] Meshes {
            get {
                return this.m_meshes;
            }
            set {
                if ((value == null)) {
                    this.m_meshes = value;
                    return;
                }
                if ((value.Length < 1u)) {
                    throw new System.ArgumentException("Array not long enough");
                }
                this.m_meshes = value;
            }
        }
        
        /// <summary>
        /// An array of nodes.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("nodes")]
        public Node[] Nodes {
            get {
                return this.m_nodes;
            }
            set {
                if ((value == null)) {
                    this.m_nodes = value;
                    return;
                }
                if ((value.Length < 1u)) {
                    throw new System.ArgumentException("Array not long enough");
                }
                this.m_nodes = value;
            }
        }
        
        /// <summary>
        /// An array of samplers.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("samplers")]
        public Sampler[] Samplers {
            get {
                return this.m_samplers;
            }
            set {
                if ((value == null)) {
                    this.m_samplers = value;
                    return;
                }
                if ((value.Length < 1u)) {
                    throw new System.ArgumentException("Array not long enough");
                }
                this.m_samplers = value;
            }
        }
        
        /// <summary>
        /// The index of the default scene.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("scene")]
        public System.Nullable<int> Scene {
            get {
                return this.m_scene;
            }
            set {
                if ((value < 0)) {
                    throw new System.ArgumentOutOfRangeException("Scene", value, "Expected value to be greater than or equal to 0");
                }
                this.m_scene = value;
            }
        }
        
        /// <summary>
        /// An array of scenes.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("scenes")]
        public Scene[] Scenes {
            get {
                return this.m_scenes;
            }
            set {
                if ((value == null)) {
                    this.m_scenes = value;
                    return;
                }
                if ((value.Length < 1u)) {
                    throw new System.ArgumentException("Array not long enough");
                }
                this.m_scenes = value;
            }
        }
        
        /// <summary>
        /// An array of skins.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("skins")]
        public Skin[] Skins {
            get {
                return this.m_skins;
            }
            set {
                if ((value == null)) {
                    this.m_skins = value;
                    return;
                }
                if ((value.Length < 1u)) {
                    throw new System.ArgumentException("Array not long enough");
                }
                this.m_skins = value;
            }
        }
        
        /// <summary>
        /// An array of textures.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("textures")]
        public Texture[] Textures {
            get {
                return this.m_textures;
            }
            set {
                if ((value == null)) {
                    this.m_textures = value;
                    return;
                }
                if ((value.Length < 1u)) {
                    throw new System.ArgumentException("Array not long enough");
                }
                this.m_textures = value;
            }
        }
        
        /// <summary>
        /// JSON object with extension-specific objects.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("extensions")]
        public System.Collections.Generic.Dictionary<string, object> Extensions {
            get {
                return this.m_extensions;
            }
            set {
                this.m_extensions = value;
            }
        }
        
        /// <summary>
        /// Application-specific data.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("extras")]
        public Extras Extras {
            get {
                return this.m_extras;
            }
            set {
                this.m_extras = value;
            }
        }
        
        public bool ShouldSerializeExtensionsUsed() {
            return ((m_extensionsUsed == null) 
                        == false);
        }
        
        public bool ShouldSerializeExtensionsRequired() {
            return ((m_extensionsRequired == null) 
                        == false);
        }
        
        public bool ShouldSerializeAccessors() {
            return ((m_accessors == null) 
                        == false);
        }
        
        public bool ShouldSerializeAnimations() {
            return ((m_animations == null) 
                        == false);
        }
        
        public bool ShouldSerializeAsset() {
            return ((m_asset == null) 
                        == false);
        }
        
        public bool ShouldSerializeBuffers() {
            return ((m_buffers == null) 
                        == false);
        }
        
        public bool ShouldSerializeBufferViews() {
            return ((m_bufferViews == null) 
                        == false);
        }
        
        public bool ShouldSerializeCameras() {
            return ((m_cameras == null) 
                        == false);
        }
        
        public bool ShouldSerializeImages() {
            return ((m_images == null) 
                        == false);
        }
        
        public bool ShouldSerializeMaterials() {
            return ((m_materials == null) 
                        == false);
        }
        
        public bool ShouldSerializeMeshes() {
            return ((m_meshes == null) 
                        == false);
        }
        
        public bool ShouldSerializeNodes() {
            return ((m_nodes == null) 
                        == false);
        }
        
        public bool ShouldSerializeSamplers() {
            return ((m_samplers == null) 
                        == false);
        }
        
        public bool ShouldSerializeScene() {
            return ((m_scene == null) 
                        == false);
        }
        
        public bool ShouldSerializeScenes() {
            return ((m_scenes == null) 
                        == false);
        }
        
        public bool ShouldSerializeSkins() {
            return ((m_skins == null) 
                        == false);
        }
        
        public bool ShouldSerializeTextures() {
            return ((m_textures == null) 
                        == false);
        }
        
        public bool ShouldSerializeExtensions() {
            return ((m_extensions == null) 
                        == false);
        }
        
        public bool ShouldSerializeExtras() {
            return ((m_extras == null) 
                        == false);
        }
    }
}
