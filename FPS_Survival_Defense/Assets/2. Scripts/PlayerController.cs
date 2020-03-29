using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 스피드 조정 변수
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float crouchSpeed;

    private float applySpeed;


    [SerializeField]
    private float jumpForce;

    // 상태 변수
    private bool isRun = false;
    private bool isCrouch = false;
    private bool isGround = true;

    // 앉았을 때 얼마나 앉을지 결정하는 변수
    [SerializeField]
    private float crouchPosY;
    private float originPosY;
    private float applyCrouchPosY;

    // 땅 착지 여부
    private CapsuleCollider capsuleCollider;


    // 민감도
    [SerializeField]
    private float lookSensitivity;                   // 카메라의 민감도

    // 카메라 각도 제한
    [SerializeField]
    private float cameraRotationLimit;              // 카메라의 각도 제한
    private float currentCameraRotationX = 0f;

    // 플레이어의 실제 몸을 뜻하는 Rigidbody
    // 필요한 컴포넌트
    [SerializeField]
    private Camera theCamera;
    private Rigidbody myRigid;
    private GunController theGunController;


    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();

        // theCamera = FindObjectOfType<Camera>(); 모든 객체들을 뒤져서 카메라 컴포넌트를 찾아 넣는 방법 (카메라가 1개 이상이면 X)
        myRigid = GetComponent<Rigidbody>();
        applySpeed = walkSpeed;

        theGunController = FindObjectOfType<GunController>();

        // 초기화
        originPosY = theCamera.transform.localPosition.y;
        applyCrouchPosY = originPosY;

    }




    // 매 프레임마다 호출되는 함수인데 매초에 약 60번, 컴퓨터 사양을 탄다
    void Update()
    {
        IsGround();
        TryJump();
        TryRun();
        TryCrouch();
        Move();
        CameraRotation();
        CharacterRotation();


    }

    // 앉기 시도
    private void TryCrouch()
    {
        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
                
        }

    }

    // 앉기 동작
    private void Crouch()
    {
        isCrouch = !isCrouch;

        if(isCrouch)
        {
            applySpeed = crouchSpeed;
            applyCrouchPosY = crouchPosY;

        }
        else
        {
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;

        }

        // StartCoroutine : 코루틴을 시작하는 함수 
        StartCoroutine(CrouchCoroutine());

    }

    // 부드러운 앉기 동작 실행
    IEnumerator CrouchCoroutine()
    {
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;

        // _posY가 원하는 값이 된다면 while문이 끝난다.
        while(_posY != applyCrouchPosY)
        {
            count++;

            // 보간을 이용한 값의 증가
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.3f);
            theCamera.transform.localPosition = new Vector3(0, _posY, 0);
            if (count > 15) break;

            // 코루틴의 기능 : null은 한프레임 대기를 하게 해준다.
            yield return null;

        }

        theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0);

        // 코루틴의 기능 : WaitForSeconds(1f) 1초 동안 대기한다
        // yield return new WaitForSeconds(1f);

    }

    // 지면 체크
    private void IsGround()
    {
        // Physics.Raycast = 광선을 쏜다.
        // 어느 위치에서, 어느 방향으로, 어느 거리만큼
        // Vector3.down을 이용하는 이유는 -transform.up을 이용한다면 캐릭터가 뒤집힐 경우 레이캐스트가 하늘로 쏘아지기 떄문이다.
        // 그래서 Vector3.down을 이용하여 고정된 값을 이용하는것 (무조건 아래 방향으로 Raycast를 쏴야하기 때문에
        // capsuleCollider.bounds.extents.y 만큼의 거리에 쏜다.
        // bounds는 CapsuleCollider의 영역을 뜻한다.
        // extents는 이 영역의 반 사이즈를 뜻한다.
        // extents.y는 이 영역의 y의 절반을 뜻한다.
        // 너무 딱 맞게 값을 줘버리면 문제가 있는데,
        // 계단이나 대각선 경사면에서 약간의 오차로 땅에 닿은것처럼 보이는데 실제로 안닿게 되는 경우가 있다. 그래서 살짝 여유를 준다. + 0.1f
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);

    }

    // 점프 시도
    private void TryJump()
    {
        if(Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            Jump();
            
        }

    }

    // 점프
    private void Jump()
    {
        // 앉은 상태에서 점프 시 앉은 상태 해제
        if (isCrouch) Crouch();

        myRigid.velocity = transform.up * jumpForce;

    }

    // 걷고 있는지 달리고 있는지 판단 함수
    private void TryRun()
    {
        if(Input.GetKey(KeyCode.LeftShift))
        {
            Running();

        }
        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            RunningCancel();

        }
        
    }

    // 달리기 실행
    private void Running()
    {
        if (isCrouch) Crouch();

        theGunController.CancelFineSight();

        isRun = true;
        applySpeed = runSpeed;

    }

    // 달리기 취소
    private void RunningCancel()
    {
        isRun = false;
        applySpeed = walkSpeed;

    }

    // 움직임 실행
    private void Move()
    {
        // 좌, 우를 누를 시 -1, 1의 값이 반환된다.
        float _moveDirX = Input.GetAxisRaw("Horizontal");
        // 상, 하를 누를 시 -1, 1의 값이 반환된다.
        float _moveDirZ = Input.GetAxisRaw("Vertical");

        // transform.right (1, 0, 0)의 값
        // _moveDirx의 값이 1이라면 그대로 1, -1이라면 -1으로 각기 다른 방향으로 이동한다.
        Vector3 _moveHorizontal = transform.right * _moveDirX;

        // transform.forward (0, 0, 1)의 값
        Vector3 _moveVertical = transform.forward * _moveDirZ;

        // (1, 0, 0) (0, 0, 1) 이 두개를 더해주면
        // (1, 0, 1) = 2의 값이 나온다.
        // (0.5, 0, 0.5) = 1 normalized를 해주면 값이 변한다.
        // 값이 1이 나오는게 연산이 빠르기 때문에
        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);

    }

    // 상하 카메라 회전
    private void CameraRotation()
    {
        // 마우스는 3차원이 아니기 때문에 2차원으로 x, y의 값만 있음
        float _xRotation = Input.GetAxisRaw("Mouse Y");

        // Input.GetAxisRaw를 이용하면 -1이나 1의 값을 얻어 올 수 있는데,
        // 마우스를 움직였을때 확 이동하는것을 방지하기 위해 lookSensitivity를 이용하여 천천히 이동하게 해준다.
        float _cameraRotationX = _xRotation * lookSensitivity;

        // 여기서 +는 FPS 게임류 옵션에 있는 '마우스 Y 반전'과 관련이 있다.
        // +를 하거나 -로 연산을 하여서 마우스 Y 반전 효과를 줄 수 있다.
        //currentCameraRotationX += _cameraRotationX;
        currentCameraRotationX -= _cameraRotationX;
        
        // Mathf에 Clamp 함수를 이용하여, 내가 원하는 각도를 초과하지 못하도록 예외처리를 할 수 있다.
        // 최소값을 넘어간다면 최소값으로, 최대값을 넘어간다면 최대값으로 변경
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);

        // 실제 카메라에 적용
        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);

    }

    // 좌우 캐릭터 회전
    private void CharacterRotation()
    {
        float _yRotation = Input.GetAxisRaw("Mouse X");

        // 민감도를 추가하는것 
        // 위, 아래 회전에서 민감도가 만족스러웠다면 좌, 우에 같은 민간도로 해도 괜찮다.
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;

        // 자기 자신의 회전값(myRigid.rotation)
        // _characterRotationY의 Quaternion값
        // Euler의 값을 Quaternion값으로 바꿔준다.
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));

    }

}
