using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private NetworkCharacterController _cc;
    public float moveSpeed = 5.0f;

    [SerializeField] private NetworkPrefabRef _prefabBall;
    [Networked] private TickTimer delay { get; set; }
    [Networked] private NetworkButtons _networkButtons { get; set; }
    
    // 카메라 관련 선언
    public ThirdPersonCamera thirdPersonCamera;
    [Networked] private Vector3 _networkCameraForward { get; set; }
    [Networked] private Vector3 _networkCameraRight { get; set; }
    [Networked] private Vector3 _networkMoveDirection { get; set; }
    
    public float rotationSpeed = 720.0f;

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            _networkButtons = data.buttons;

            _networkMoveDirection = data.direction;

            if (Object.HasInputAuthority)
            {
                UpdateCameraDirection();
            }

            MovePlayer(data.direction);            
        }

        CheckAndFireProjectile();
    }
    
    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            SetupCamera();
        }
    }

    private void CheckAndFireProjectile()                           //üũ�ϰ� ��� �Լ�
    {
        if (delay.ExpiredOrNotRunning(Runner))
        {
            if (_networkButtons.IsSet(NetworkInputData.MOUSEBUTTON0))    //��ư �����Ѱ� �����ͼ� �����Ѵ�. 
            {
                delay = TickTimer.CreateFromSeconds(Runner, 0.5f);      //0.5�� �������� ���. 
                FireProjectile();
            }
        }
    }

    private void FireProjectile()                                   //�߻�ü ���� �Լ� 
    {
        if (Object.HasStateAuthority)
        {
            Vector3 forward = transform.forward;
            Runner.Spawn(_prefabBall,
                transform.position + forward,
                Quaternion.LookRotation(forward),
                Object.InputAuthority,
                (runner, o) => o.GetComponent<Ball>().Init());
        }
    }

    private void SetupCamera()
    {
        ThirdPersonCamera camera = FindObjectOfType<ThirdPersonCamera>();
        if (camera != null)
        {
            camera.target = transform;
            thirdPersonCamera = camera;
        }
        else
        {
            Debug.LogError("ThirdPersonCamera not found in the scene!");
        }
    }

    private void UpdateCameraDirection()
    {
        if (thirdPersonCamera != null)
        {
            _networkCameraForward = thirdPersonCamera.transform.forward;
            _networkCameraRight = thirdPersonCamera.transform.right;
        }
    }

    private void MovePlayer(Vector3 moveDirection)
    {
        // 중력을 포함한 이동 백터 계산
        Vector3 movement = moveDirection * moveSpeed;

        if (moveDirection != Vector3.zero)
        {
            // NetworkCharacterController를 사용하여 이동 (중력 포함)
            _cc.Move(movement);
            
            // 이동 방향으로 회전
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Runner.DeltaTime);
            
            // 애니메이션 파라미터 설정
            float currentMoveSpeed = moveDirection.magnitude * moveSpeed;

            if (Object.HasInputAuthority)
            {
                Debug.unityLogger.Log(movement);
            }
        }
    }
}