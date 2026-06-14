using UnityEngine;

public sealed class SceneUIAccess : MonoBehaviour
{
    [SerializeField] private bool allowGlobalWindows = true;

    public bool AllowGlobalWindows => allowGlobalWindows;
}
