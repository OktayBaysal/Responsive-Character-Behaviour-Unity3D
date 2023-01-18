using UnityEngine;

public class _PlayerAnimation : MonoBehaviour
{
    [SerializeField] GameObject _sword;
    [SerializeField] private float _animationAccel;

    Collider _swordCollider;

    Animator _anim;
    GameObject _parentObj;
    _PlayerScript _parentScript;

    private int _vectorX;
    private int _vectorZ;


    void Start()
    { 
        _anim = GetComponent<Animator>();
        _anim.SetFloat("Velocity", 1f);
        _swordCollider = _sword.GetComponent<Collider>();
        _parentObj = transform.parent.gameObject;
        _parentScript = _parentObj.GetComponent<_PlayerScript>();
        _swordCollider.enabled = false;
        _vectorX = Animator.StringToHash("VectorX");
        _vectorZ = Animator.StringToHash("VectorZ");
    }

    void Update()
    {
        InputAxis();
    }

    void InputAxis()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");

        _anim.SetFloat(_vectorX, inputX);
        _anim.SetFloat(_vectorZ, inputZ);
    }
    
    void ColliderStart()
    {
        _swordCollider.enabled = true;

    }

    void ColliderEnd()
    {
        _swordCollider.enabled = false;

    }

    void MidAttack()
    {
        _parentScript.MoveSpeedHandler(1);
        _parentScript.AttackCancel();
    }

    void MidParry()
    {
        _parentScript.ParryCancel();
    }

    void MidStagger()
    {
        _parentScript.StaggerCancel();
    }

    void MidStep()
    {
        _parentScript.StepCancel();
    }

    void IFrameEnded()
    {
        _parentScript.IFrameEnd();
    }

    void ParryWindowEnded()
    {
        _parentScript.ParryWindowEnd();
    }

    void RollEnded()
    {
        _parentScript.RollEnd();
    }

    void DodgeEnded()
    {
        _parentScript.DodgeEnd();
    }

    void StepEnded()
    {
        _parentScript.StepEnd();
    }

    void RunNormal()
    {
        _parentScript.MoveSpeedHandler(3);
    }

    void RunSlow()
    {
        _parentScript.MoveSpeedHandler(1);
    }

    void RunStop()
    {
        _parentScript.MoveSpeedHandler(0);
    }

    void TurnSlow()
    {
        _parentScript.TurnSpeedHandler(1);
    }

    void TurnStop()
    {
        _parentScript.TurnSpeedHandler(0);
    }

    void AttackMoveStart()
    {
        _parentScript.AttackMoveHandler(true);
    }

    void AttackMoveEnd()
    {
        _parentScript.AttackMoveHandler(false);
    }

    void AttackTurnStart()
    {
        _parentScript.AttackTurnHandler(true);
    }

    void AttackTurnEnd()
    {
        _parentScript.AttackTurnHandler(false);
    }

    void ResetAll()
    {
        _parentScript.ResetRestrictions();
    }
}
