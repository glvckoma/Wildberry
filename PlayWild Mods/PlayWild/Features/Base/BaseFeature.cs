using UnityEngine;
using MelonLoader;
using PlayWild.Utils;

namespace PlayWild.Features.Base
{
    public abstract class BaseFeature : IFeature
    {
        public abstract string Name { get; }
        
        private bool _isEnabled = false;
        private bool _isInitialized = false;
        
        public virtual bool IsEnabled 
        { 
            get => _isEnabled;
            set 
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    if (value)
                        OnEnable();
                    else
                        OnDisable();
                    
                    // Save to persistence
                    if (_isInitialized)
                    {
                        PersistenceManager.SetFeatureEnabled(GetPersistenceKey(), value);
                    }
                        
                    MelonLogger.Msg($"[WildBerry] {Name}: {(value ? "ENABLED" : "DISABLED")}");
                }
            }
        }

        public virtual void Initialize() 
        {
            // Load persisted state
            _isEnabled = PersistenceManager.GetFeatureEnabled(GetPersistenceKey(), false);
            _isInitialized = true;
            
            // Call OnEnable if the feature was persisted as enabled
            if (_isEnabled)
            {
                OnEnable();
            }
        }
        public virtual void OnUpdate() { }
        public abstract void OnGUI(Rect area);
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }

        protected void DrawCheckbox(Rect position, bool currentValue, string label, System.Action<bool> onValueChanged)
        {
            // Checkbox border
            GUI.color = Color.white;
            GUI.DrawTexture(position, Texture2D.whiteTexture);
            
            // Checkbox inner area
            Rect innerRect = new Rect(position.x + 2, position.y + 2, 12, 12);
            
            // Set fill color based on state
            if (currentValue)
            {
                GUI.color = Color.white; // White fill when enabled
            }
            else
            {
                GUI.color = Color.black; // Black fill when disabled  
            }
            
            // Draw the inner fill
            GUI.DrawTexture(innerRect, Texture2D.whiteTexture);
            
            // Reset color for button
            GUI.color = Color.white;
            
            // Invisible clickable button over entire checkbox
            if (GUI.Button(position, "", GUIStyle.none))
            {
                onValueChanged(!currentValue);
            }
            
            // Label next to checkbox
            GUI.color = Color.white;
            GUI.Label(new Rect(position.x + 25, position.y - 2, 200, 20), label);
        }

        protected string GetGameObjectPath(GameObject obj)
        {
            try
            {
                string path = obj.name;
                Transform current = obj.transform.parent;
                while (current != null)
                {
                    path = current.name + "/" + path;
                    current = current.parent;
                }
                return path;
            }
            catch
            {
                return obj.name;
            }
        }

        // Persistence helpers
        protected virtual string GetPersistenceKey()
        {
            return Name.Replace(" ", "_").Replace("+", "_");
        }

        protected T GetPersistedValue<T>(string valueName, T defaultValue = default(T))
        {
            return PersistenceManager.GetFeatureValue(GetPersistenceKey(), valueName, defaultValue);
        }

        protected void SetPersistedValue<T>(string valueName, T value)
        {
            if (_isInitialized)
            {
                PersistenceManager.SetFeatureValue(GetPersistenceKey(), valueName, value);
            }
        }

        // Default dynamic height implementation
        public virtual float GetDynamicHeight()
        {
            // Standard height: checkbox (25) + content when enabled
            if (!IsEnabled)
            {
                return 25f; // Just checkbox height when disabled
            }
            
            // Default enabled height for most simple features
            return 50f; // Checkbox + basic content
        }
    }
}