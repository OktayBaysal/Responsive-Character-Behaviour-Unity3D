using Cinemachine;
using UnityEngine;

public class _PlayerScript : MonoBehaviour
{
    GameObject _playerBody;
    Animator _playerAnimator;
    CharacterController _characterController;
    CinemachineFreeLook _freeCam;
    CinemachineVirtualCamera _lockCam;

    _EnemyScript _lastEnemy;

    private Vector3 _rawInput;
    private Vector3 _rawRawInput;
    private Vector3 _moveInput;
    private Vector3 _rawMoveInput;
    private Vector3 _moveInputNormal;
    private Vector3 _rollDirection;
    private Vector3 _finalDirection;

    private int _velocity;

    private bool _rollTrigger = false;
    private bool _sprintTrigger = false;

    private bool _attackBuffer = false;
    private bool _rollBuffer = false;


    private bool _inIFrame = false;
    private bool _inParryWindow = false;
    private bool _damageTaken = false;
    private bool _heavyDamageTaken = false;

    private bool _isLocked = false;
    private bool _isActing = true;
    private bool _isRolling = false;
    private bool _isDodging = false;
    private bool _isStepping = false;
    private bool _isAttackMoving = false;
    private bool _isAttackTurning = false;


    private bool _canAttack = true;
    private bool _canAttack1 = true;
    private bool _canAttack2 = false;
    private bool _canAttack3 = false;

    private bool _canParry = true;
    private bool _canRoll = true;

    private bool _canTurn = true;
    private bool _canMove = true;


    private bool _attackCancel = false;
    private bool _parryCancel = false;
    private bool _staggerCancel = false;
    private bool _stepCancel = false;

    private float _dynamicSpeed;
    private float _finalSpeed;
    private float _halfMoveSpeed;
    private float _currentAttackMoveSpeed;
    private float _rotationFactorPerFrame;

    private float _bufferTimer;
    private float _sprintIntervalTimer;


    [Space(10)]
    [Header("Movement")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _sprintSpeed;

    [Range(1f, 20f)]
    [SerializeField] private float _moveAccel;
    [Range(1f, 20f)]
    [SerializeField] private float _moveDecel;

    [SerializeField] private float _attackMoveSpeed;
    [SerializeField] private float _attackTurnSpeed;
    [SerializeField] private float _slowMoveSpeed;
    [SerializeField] private float _sprintAccel;
    [SerializeField] private float _sprintDecel;

    [Space(10)]
    [SerializeField] private float _rollSpeed;
    [SerializeField] private float _dodgeSpeed;
    [SerializeField] private float _backStepSpeed;

    [Space(10)]
    [SerializeField] private float _turnSpeed;
    [SerializeField] private float _slowTurnSpeed;
    [SerializeField] private float _lockedTurnSpeed;

    [Space(10)]
    [Header("Input Settings")]
    [SerializeField] private float _bufferTime;
    [SerializeField] private float _sprintInterval;

    [Space(10)]
    [Header("References")]
    [SerializeField] private GameObject _freelookCamera;
    [SerializeField] private GameObject _lockCamera;
    [SerializeField] private GameObject _targetObject;

    void Start()
    {
        _halfMoveSpeed = _moveSpeed / 2f;
        _dynamicSpeed = 0f;
        _currentAttackMoveSpeed = _slowMoveSpeed;
        _rotationFactorPerFrame = _turnSpeed;


        _characterController = GetComponent<CharacterController>();
        _playerBody = transform.Find("PlayerBody").gameObject;
        _playerAnimator = _playerBody.GetComponent<Animator>();
        _freeCam = _freelookCamera.GetComponent<CinemachineFreeLook>();
        _lockCam = _lockCamera.GetComponent<CinemachineVirtualCamera>();

        _velocity = Animator.StringToHash("Velocity");
    }

    void Update()
    {
        InputVectorCalculation();
        RawInputVectorCalculation();
        PlayerTurnLogic();
        SpaceInputHandler();
        MovementInputLerp();
        BufferHandler();
        AnimationHandler();
        AnimationInputAxis();
        RunningCalculation();
    }

    private void AnimationHandler()
    {
        if (_rawRawInput.sqrMagnitude > 0.01f)
        {
            _playerAnimator.SetBool("isMoving", true);
        }
        else
        {
            _playerAnimator.SetBool("isMoving", false);
        }

        if (_rollBuffer && (_canRoll || _attackCancel || _parryCancel || _stepCancel || _staggerCancel))
        {
            if (_rawRawInput.sqrMagnitude > 0.01f)
            {
                if (_isLocked)
                {
                    DodgeStart();
                }
                else
                {
                    RollStart();
                }
            } 
            else
            {
                StepStart();
            }
        }

        if (_attackBuffer && (_canAttack || _attackCancel || _parryCancel || _stepCancel || _staggerCancel))
        {
            AttackLogic();
        }

        if (Input.GetMouseButtonDown(1) && (_canParry || _attackCancel || _stepCancel))
        {
            ParryStart();
        }

        if (_damageTaken)
        {           
            ResetRestrictions();
            StaggerStart();
            
            _damageTaken = false;
        }

        if (_heavyDamageTaken)
        {
            ResetRestrictions();
            StaggerStart();

            _heavyDamageTaken = false;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (_isLocked)
            {
                _isLocked = false;
                _playerAnimator.SetBool("isLocked", false);
                _lockCam.Priority = 8;
            }
            else
            {
                _isLocked = true;
                _playerAnimator.SetBool("isLocked", true);
                _lockCam.Priority = 20;
            }
        }
    }

    private void AnimationInputAxis()
    {
        _playerAnimator.SetFloat(_velocity, _dynamicSpeed);
    }

    private void SpaceInputHandler()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _sprintIntervalTimer = 0f;
        }
        
        if (Input.GetKey(KeyCode.Space) && !_sprintTrigger)
        {
            _sprintIntervalTimer += Time.deltaTime;

            if (_sprintIntervalTimer >= _sprintInterval)
            {
                _dynamicSpeed = _moveSpeed;
                _sprintTrigger = true;
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (_sprintIntervalTimer < _sprintInterval)
            {
                _rollTrigger = true;
            }

            _sprintTrigger = false;
        }
    }

    private void MovementInputLerp()
    {
        if (!_isActing)
        {
            if (_sprintTrigger)
            {
                _dynamicSpeed = Mathf.MoveTowards(_dynamicSpeed, _sprintSpeed, _sprintAccel * Time.deltaTime);
                return;
            }
            else if (_dynamicSpeed > _moveSpeed)
            {
                _dynamicSpeed = Mathf.MoveTowards(_dynamicSpeed, _moveSpeed, _sprintDecel * Time.deltaTime);
                return;
            }

            if (_rawRawInput != Vector3.zero)
            {
                if (_dynamicSpeed < _moveSpeed)
                {
                    _dynamicSpeed = Mathf.Lerp(_dynamicSpeed, _moveSpeed, _moveAccel * Time.deltaTime);

                    if (_dynamicSpeed > _moveSpeed - 0.1f)
                    {
                        _dynamicSpeed = _moveSpeed;
                    }
                }
                else
                {
                    _dynamicSpeed = _moveSpeed;
                }
            }
            else if (_dynamicSpeed != 0f)
            {
                _dynamicSpeed = Mathf.Lerp(_dynamicSpeed, 0f, _moveDecel * Time.deltaTime);

                if (_dynamicSpeed < 0.1f)
                {
                    _dynamicSpeed = 0f;
                }
            }
        }
    }

    private void RunningCalculation()
    {
        if (_isRolling)
        {
            _finalSpeed = _rollSpeed;
            _finalDirection = _rollDirection;
        }
        else if (_isDodging)
        {
            _finalSpeed = _dodgeSpeed;
            _finalDirection = _rollDirection;
        }
        else if (_isStepping)
        {
            _finalSpeed = _backStepSpeed;
            _finalDirection = -transform.forward;
        }
        else if (!_isActing)
        {
            _finalSpeed = _dynamicSpeed;
            _finalDirection = _moveInputNormal;
        }
        else if (_isAttackMoving)
        {
            _finalSpeed = _currentAttackMoveSpeed;

            if (_rawRawInput == Vector3.zero || _isLocked)
            {
                _finalDirection = transform.forward;
            }
            else
            {
                _finalDirection = _moveInputNormal;
            }
        }
        else
        {
            _finalSpeed = 0f;
        }

        _characterController.SimpleMove(_finalSpeed * _finalDirection);
    }

    private void InputVectorCalculation()
    {
        _rawInput.z = Input.GetAxis("Vertical");
        _rawInput.x = Input.GetAxis("Horizontal");

        if (_canMove)
        {
            _moveInput = _rawInput.normalized;
        }
        else
        {
            _moveInput = Vector3.zero;
        }

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        Vector3 cameraForwardProduct = _moveInput.z * cameraForward;
        Vector3 cameraRightProduct = _moveInput.x * cameraRight;

        _moveInputNormal = cameraForwardProduct + cameraRightProduct;
    }

    private void RawInputVectorCalculation()
    {
        _rawRawInput.z = Input.GetAxisRaw("Vertical");
        _rawRawInput.x = Input.GetAxisRaw("Horizontal");

        Vector3 rawInputNormal = _rawRawInput.normalized;

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        Vector3 cameraForwardProduct = rawInputNormal.z * cameraForward;
        Vector3 cameraRightProduct = rawInputNormal.x * cameraRight;

        _rawMoveInput = cameraForwardProduct + cameraRightProduct;
    }

    private void PlayerTurnLogic()
    {
        if (_isLocked)
        {
            LockedBodyRotation();
        }
        else
        {
            BodyRotation();
        }
    }

    private void BodyRotation()
    {
        Vector3 positionToLookAt;

        positionToLookAt.x = _moveInputNormal.x;
        positionToLookAt.y = 0f;
        positionToLookAt.z = _moveInputNormal.z;

        Quaternion currentRotation = transform.rotation;

        if (_rawRawInput.sqrMagnitude > 0.01f && positionToLookAt != Vector3.zero)
        {
            if (_canTurn)
            {
                Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
                transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, _rotationFactorPerFrame * Time.deltaTime);
            }
            else if (_isAttackTurning)
            {
                Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
                transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, _attackTurnSpeed * Time.deltaTime);
            }
        }
    }

    private void LockedBodyRotation()
    {
        Vector3 positionToLookAt = _targetObject.transform.position;

        Vector3 vectorToLook = positionToLookAt - transform.position;

        vectorToLook.y = 0f;

        Quaternion currentRotation = transform.rotation;

        Quaternion targetRotation = Quaternion.LookRotation(vectorToLook);
        transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, _lockedTurnSpeed * Time.deltaTime);
    }

    private void AttackLogic()
    {
        AttackStart();

        if (_canAttack1)
        {
            _canAttack1 = false;
            _canAttack2 = true;
            _playerAnimator.SetTrigger("Attack1");
        }
        else if (_canAttack2)
        {
            _canAttack2 = false;
            _canAttack3 = true;
            _playerAnimator.SetTrigger("Attack2");
        }
        else if (_canAttack3)
        {
            _canAttack3 = false;
            _canAttack1 = true;
            _playerAnimator.SetTrigger("Attack3");
        }
        else
        {
            _canAttack1 = true;
            Debug.Log("There is a problem with Attack Logic Handle.");
        }
    }

    private void BufferHandler()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _attackBuffer = true;
            _rollBuffer = false;
            _bufferTimer = 0f;
        }
        
        if (_rollTrigger)
        {
            _rollBuffer = true;
            _rollTrigger = false;
            _attackBuffer = false;
            _bufferTimer = 0f;
        }

        if (_rollBuffer || _attackBuffer)
        {
            _bufferTimer += Time.deltaTime;

            if (_bufferTimer > _bufferTime)
            {
                _attackBuffer = false;
                _rollBuffer = false;
                _bufferTimer = 0f;
            }
        }
    }



    private void AttackStart()
    {
        _rotationFactorPerFrame = _turnSpeed;

        _attackCancel = false;
        _parryCancel = false;
        _staggerCancel = false;
        _stepCancel = false;

        _canAttack = false;
        _canParry = false;

        _canRoll = false;

        _canTurn = false;
        _canMove = true;

        _isActing = true;
        _isRolling = false;
        _isDodging = false;
        _isStepping = false;
        _isAttackMoving = false;
        _isAttackTurning = false;
    }

    private void RollStart()
    {
        ActionStart();

        //--------------------------------------------------------------------------------------

        _rollDirection = _rawMoveInput;

        _isActing = true;
        _inIFrame = true;
        _isRolling = true;

        _playerAnimator.SetTrigger("Roll");
    }

    private void DodgeStart()
    {
        ActionStart();

        //--------------------------------------------------------------------------------------

        _playerAnimator.SetFloat("DodgeX", _rawRawInput.x);
        _playerAnimator.SetFloat("DodgeZ", _rawRawInput.z);

        _rollDirection = _rawMoveInput;

        _isActing = true;
        _inIFrame = true;
        _isDodging = true;

        _playerAnimator.SetTrigger("Dodge");
    }

    private void StepStart()
    {
        ActionStart();

        //--------------------------------------------------------------------------------------

        _isActing = true;
        _isStepping = true;

        _playerAnimator.SetTrigger("Step");
    }

    private void ParryStart()
    {
        ActionStart();

        //--------------------------------------------------------------------------------------

        _isActing = true;
        _inIFrame = true;
        _inParryWindow = true;

        _playerAnimator.SetTrigger("Parry");
    }

    private void StaggerStart()
    {
        ActionStart();

        //--------------------------------------------------------------------------------------

        _isActing = true;
        _inIFrame = true;

        _playerAnimator.SetTrigger("Stagger");

    }



    public void AttackCancel()
    {
        _attackCancel = true;
    }

    public void ParryCancel()
    {
        _parryCancel = true;
    }

    public void StaggerCancel()
    {
        _staggerCancel= true;
    }

    public void StepCancel()
    {
        _stepCancel = true;
    }

    public void ResetRestrictions()
    {
        _rotationFactorPerFrame = _turnSpeed;
        _currentAttackMoveSpeed = _slowMoveSpeed;

        _dynamicSpeed = _moveSpeed;

        _inIFrame = false;
        _inParryWindow = false;

        _attackCancel = false;
        _parryCancel = false;
        _staggerCancel = false;
        _stepCancel = false;

        _canAttack = true;
        _canAttack1 = true;
        _canAttack2 = false;
        _canAttack3 = false;
        _canParry = true;

        _canRoll = true;

        _canTurn = true;
        _canMove = true;

        _isActing = false;
        _isRolling = false;
        _isDodging = false;
        _isStepping = false;
        _isAttackMoving = false;
        _isAttackTurning = false;
}

    public void ActionStart()
    {
        _inIFrame = false;
        _inParryWindow = false;

        _dynamicSpeed = _moveSpeed;
        _currentAttackMoveSpeed = _slowMoveSpeed;

        _attackCancel = false;
        _parryCancel = false;
        _staggerCancel = false;
        _stepCancel = false;

        _canAttack = false;
        _canAttack1 = true;
        _canAttack2 = false;
        _canAttack3 = false;
        _canParry = false;

        _canRoll = false;

        _canTurn = false;
        _canMove = false;

        _isActing = true;
        _isRolling = false;
        _isDodging = false;
        _isStepping = false;
        _isAttackMoving = false;
        _isAttackTurning = false;
    }

    public void IFrameEnd()
    {
        _inIFrame = false;
    }

    public void ParryWindowEnd()
    {
        _inParryWindow = false;
    }

    public void RollEnd()
    {
        _isRolling = false;
        _canMove = true;
    }

    public void DodgeEnd()
    {
        _isDodging = false;
        _canMove = true;
    }

    public void StepEnd()
    {
        _isStepping = false;
    }



    public void Damaged(_EnemyScript enemy)
    {
        if (!_inIFrame)
        {
            _lastEnemy = enemy;
            _damageTaken = true;
        }

        if (_inParryWindow)
        {
            _lastEnemy = enemy;
            _lastEnemy.Parried();
        }
    }

    public void HeavyDamaged(_EnemyScript enemy)
    {
        if (!_inIFrame)
        {
            _lastEnemy = enemy;
            _heavyDamageTaken = true;
        }
    }



    public void TurnSpeedHandler(int num)
    {
        switch (num)
        {
            case 0:
                _rotationFactorPerFrame = 0f;
                break;

            case 1:
                _rotationFactorPerFrame = _slowTurnSpeed;
                break;

            case 2:
                _rotationFactorPerFrame = _turnSpeed;
                break;
        }
    }

    public void MoveSpeedHandler(int num)
    {
        switch (num)
        {
            case 0:
                _currentAttackMoveSpeed = 0f;
                break;

            case 1:
                _currentAttackMoveSpeed = _slowMoveSpeed;
                break;

            case 2:
                _currentAttackMoveSpeed = _halfMoveSpeed;
                break;

            case 3:
                _currentAttackMoveSpeed = _attackMoveSpeed;
                break;
        }
    }

    public void AttackMoveHandler(bool what)
    {
        if (what)
        {
            _isAttackMoving = true;
        }
        else
        {
            _isAttackMoving = false;
        }
    }

    public void AttackTurnHandler (bool what)
    {
        if (what)
        {
            _isAttackTurning = true;
        }
        else
        {
            _isAttackTurning = false;
        }
    }
}
