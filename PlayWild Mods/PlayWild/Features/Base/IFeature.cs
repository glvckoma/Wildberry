using UnityEngine;

namespace PlayWild.Features.Base
{
    public interface IFeature
    {
        string Name { get; }
        bool IsEnabled { get; set; }
        void OnUpdate();
        void OnGUI(Rect area);
        void OnEnable();
        void OnDisable();
        void Initialize();
        float GetDynamicHeight();
    }
}