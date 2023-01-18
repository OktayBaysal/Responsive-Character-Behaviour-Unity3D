using UnityEngine;

public class _PlayerSwordScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyHitbox"))
        {
            other.transform.parent.gameObject.GetComponent<BasicEnemyScript>().Damaged();
        }
    }
}
