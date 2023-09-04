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
    
    
    public class TextureInfo {
        
        /// <summary>
        /// Backing field for Index.
        /// </summary>
        private int m_index;
        
        /// <summary>
        /// Backing field for TexCoord.
        /// </summary>
        private int m_texCoord = 0;
        
        /// <summary>
        /// Backing field for Extensions.
        /// </summary>
        private System.Collections.Generic.Dictionary<string, object> m_extensions;
        
        /// <summary>
        /// Backing field for Extras.
        /// </summary>
        private Extras m_extras;
        
        /// <summary>
        /// The index of the texture.
        /// </summary>
        [Newtonsoft.Json.JsonRequiredAttribute()]
        [Newtonsoft.Json.JsonPropertyAttribute("index")]
        public int Index {
            get {
                return this.m_index;
            }
            set {
                if ((value < 0)) {
                    throw new System.ArgumentOutOfRangeException("Index", value, "Expected value to be greater than or equal to 0");
                }
                this.m_index = value;
            }
        }
        
        /// <summary>
        /// The set index of texture's TEXCOORD attribute used for texture coordinate mapping.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("texCoord")]
        public int TexCoord {
            get {
                return this.m_texCoord;
            }
            set {
                if ((value < 0)) {
                    throw new System.ArgumentOutOfRangeException("TexCoord", value, "Expected value to be greater than or equal to 0");
                }
                this.m_texCoord = value;
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
        
        public bool ShouldSerializeTexCoord() {
            return ((m_texCoord == 0) 
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