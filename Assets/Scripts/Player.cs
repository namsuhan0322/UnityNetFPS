using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private NetworkCharacterController _cc;
    public float moveSpeed = 5.0f;

    [SerializeField] private NetworkPrefabRef _prefabBall;
    [Networked] private TickTimer delay { get; set; }
    [Networked] private NetworkButtons _networkButtons { get; set; }

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            _networkButtons = data.buttons;

            Vector3 moveDirection = data.direction;
            _cc.Move(moveDirection * moveSpeed * Runner.DeltaTime);

            if (moveDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection);
            }
        }

        CheckAndFireProjectile();
    }

    private void CheckAndFireProjectile()                           //체크하고 쏘는 함수
    {
        if (delay.ExpiredOrNotRunning(Runner))
        {
            if (_networkButtons.IsSet(NetworkInputData.MOUSEBUTTON0))    //버튼 선언한것 가져와서 진행한다. 
            {
                delay = TickTimer.CreateFromSeconds(Runner, 0.5f);      //0.5초 간격으로 쏜다. 
                FireProjectile();
            }
        }
    }

    private void FireProjectile()                                   //발사체 생성 함수 
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
}