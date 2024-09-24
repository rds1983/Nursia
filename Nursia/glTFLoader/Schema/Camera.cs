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
    
    
    internal class Camera {
        
        /// <summary>
        /// Backing field for Orthographic.
        /// </summary>
        private CameraOrthographic m_orthographic;
        
        /// <summary>
        /// Backing field for Perspective.
        /// </summary>
        private CameraPerspective m_perspective;
        
        /// <summary>
        /// Backing field for Type.
        /// </summary>
        private TypeEnum m_type;
        
        /// <summary>
        /// Backing field for Name.
        /// </summary>
        private string m_name;
        
        /// <summary>
        /// Backing field for Extensions.
        /// </summary>
        private System.Collections.Generic.Dictionary<string, object> m_extensions;
        
        /// <summary>
        /// Backing field for Extras.
        /// </summary>
        private Extras m_extras;
        
        /// <summary>
        /// An orthographic camera containing properties to create an orthographic projection matrix. This property **MUST NOT** be defined when `perspective` is defined.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("orthographic")]
        public CameraOrthographic Orthographic {
            get {
                return this.m_orthographic;
            }
            set {
                this.m_orthographic = value;
            }
        }
        
        /// <summary>
        /// A perspective camera containing properties to create a perspective projection matrix. This property **MUST NOT** be defined when `orthographic` is defined.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("perspective")]
        public CameraPerspective Perspective {
            get {
                return this.m_perspective;
            }
            set {
                this.m_perspective = value;
            }
        }
        
        /// <summary>
        /// Specifies if the camera uses a perspective or orthographic projection.
        /// </summary>
        [Newtonsoft.Json.JsonConverterAttribute(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        [Newtonsoft.Json.JsonRequiredAttribute()]
        [Newtonsoft.Json.JsonPropertyAttribute("type")]
        public TypeEnum Type {
            get {
                return this.m_type;
            }
            set {
                this.m_type = value;
            }
        }
        
        /// <summary>
        /// The user-defined name of this object.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("name")]
        public string Name {
            get {
                return this.m_name;
            }
            set {
                this.m_name = value;
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
        
        public bool ShouldSerializeOrthographic() {
            return ((m_orthographic == null) 
                        == false);
        }
        
        public bool ShouldSerializePerspective() {
            return ((m_perspective == null) 
                        == false);
        }
        
        public bool ShouldSerializeName() {
            return ((m_name == null) 
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
        
        public enum TypeEnum {
            
            perspective,
            
            orthographic,
        }
    }
}
