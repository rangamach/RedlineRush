using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSO", menuName = "Scriptable Objects/PlayerSO")]
public class PlayerSO : ScriptableObject
{
    public PlayerView1 PlayerView;
    public Vector3 positon;
    public Vector3 rotation;
    public Vector3 scale;
}
