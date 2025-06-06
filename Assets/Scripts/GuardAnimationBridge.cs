using UnityEngine;

public class GuardAnimationBridge : MonoBehaviour
{
    EnemyController enemy;
    void Awake() => enemy = GetComponentInParent<EnemyController>();
    public void HitPlayer()
    {
        enemy.HitPlayer();
    }
}
