using UnityEngine;

public class _PlayerHitbox : MonoBehaviour
{
    GameObject _parentObj;
    _PlayerScript _parentScript;

    void Start()
    {
        _parentObj = transform.parent.gameObject;
        _parentScript = _parentObj.GetComponent<_PlayerScript>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyAttack"))
        {
            _parentScript.Damaged(other.transform.parent.parent.gameObject.GetComponent<_EnemyScript>());
        }

        if (other.CompareTag("Enemy Detection Range"))
        {
            other.transform.parent.gameObject.GetComponent<BasicEnemyScript>().DetectionHandler();
        }
    }
}
