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
    
    
    internal class AnimationChannel {
        
        /// <summary>
        /// Backing field for Sampler.
        /// </summary>
        private int m_sampler;
        
        /// <summary>
        /// Backing field for Target.
        /// </summary>
        private AnimationChannelTarget m_target;
        
        /// <summary>
        /// Backing field for Extensions.
        /// </summary>
        private System.Collections.Generic.Dictionary<string, object> m_extensions;
        
        /// <summary>
        /// Backing field for Extras.
        /// </summary>
        private Extras m_extras;
        
        /// <summary>
        /// The index of a sampler in this animation used to compute the value for the target.
        /// </summary>
        [Newtonsoft.Json.JsonRequiredAttribute()]
        [Newtonsoft.Json.JsonPropertyAttribute("sampler")]
        public int Sampler {
            get {
                return this.m_sampler;
            }
            set {
                if ((value < 0)) {
                    throw new System.ArgumentOutOfRangeException("Sampler", value, "Expected value to be greater than or equal to 0");
                }
                this.m_sampler = value;
            }
        }
        
        /// <summary>
        /// The descriptor of the animated property.
        /// </summary>
        [Newtonsoft.Json.JsonRequiredAttribute()]
        [Newtonsoft.Json.JsonPropertyAttribute("target")]
        public AnimationChannelTarget Target {
            get {
                return this.m_target;
            }
            set {
                this.m_target = value;
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
        
        public bool ShouldSerializeTarget() {
            return ((m_target == null) 
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
