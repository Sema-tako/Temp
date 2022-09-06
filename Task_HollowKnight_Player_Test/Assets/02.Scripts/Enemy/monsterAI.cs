using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class monsterAI : MonoBehaviour
{
    public enum State
    {
        WALK,//Mantis -> PATROL
       ATTACK,//ATTACK
        DIE,//죽었을 때
        HIT,//맞았을 때
        TRACE//추적할 때
    }

    public State state = State.WALK;//WALK 초기 상태 지정

    Transform playerTr;//플레이어 위치 저장 변수
    Transform enemyTr;//적 위치 저장 변수

    public float damage = 1;//공격력 5

    public float movePower;

    int movementFlag = 0;


    float stopDistance = 1.5f;

    public float attackDist = 5f; //공격 사거리
    public float traceDist = 2f;//추적 사거리
    public bool isDie = false;//사망 여부 판단 변수
    public bool isTracing = false;//추적 상태 판단 변수
    public bool isLeft = true;
    public bool canTrace = true;

    public Vector3 defalut_direction;
    public Vector3 direction;//몬스터가 갈 방향
    public float defult_velocity;
    public float accelaration;//몬스터 가속도
    public float velocity;//몬스터 움직임

    public float speed = 2f;

    public float Hp = 20f;//Mantis Hp = 20

    WaitForSeconds ws;//시간 지연 변수

    Animator animator;
    readonly int Maintis_ATTACK = Animator.StringToHash("ATTACK");//공격 애니메이션
    readonly int Mantis_IDLE = Animator.StringToHash("IDLE");//idle 애니메이션
    readonly int Maintis_WALK = Animator.StringToHash("WALK");//Mantis-> PATROL 애니메이션


    private void Awake()
    {
        var player = GameObject.FindGameObjectWithTag("Player");//player 태그 지정
        if (player != null)
        {
            playerTr = player.GetComponent<Transform>();
        }

        enemyTr = GetComponent<Transform>();

        animator = GetComponent<Animator>();

        ws = new WaitForSeconds(0.1f);//시간 지연 변수 (코루틴 함수에서 사용)

        defalut_direction.x = Random.Range(-1.0f, 1.0f);
        defalut_direction.y = Random.Range(-1.0f, 1.0f);

    }

    private void Update()
    {
        if(canTrace)
        Move_();
    }

    public void OnEnable()//해당 스크립트가 활성화 될 때마다 실행됨
    {
        StartCoroutine(CheckState());
        StartCoroutine(Action());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "EndPoint")
        {
            if (isLeft)
            {
                transform.eulerAngles = new Vector3(0, 180, 0);
                speed = 0f;
                isLeft = false;
            }
            else
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
                speed = 0f;
                isLeft = true;
            }
        }
    }

    public void Move_()//플립 및 추적 
    {
        Vector3 moveVelocity = Vector3.zero;

        if (Vector2.Distance(transform.position, playerTr.position) < stopDistance)
        {
           
            speed = 0f;
        }
        else
        {
            speed = 2f;
        }

        Vector3 flip = transform.localScale;
        if (playerTr.transform.position.x >= this.transform.position.x)
        {
            flip.x = 1f;
            transform.Translate(Vector2.right * speed * Time.deltaTime);
        }
        else
        {
            flip.x = -1f;
            transform.Translate(Vector2.left * speed * Time.deltaTime);
        }

        this.transform.localScale = flip;

        transform.position += moveVelocity * Time.deltaTime;

    }


    public IEnumerator attackDelay()//Attack & Walk파라미터 조절 함수
    {
        yield return new WaitForSeconds(0.06f);//ATTACK 한 번만 실행 시키기 위함

        state = State.WALK;
    }

    public IEnumerator CheckState()//상태체크 코루틴 함수
    {
        yield return new WaitForSeconds(1f);//다른 오브젝트 스크립트 초기화를 위한 대기 시간

        while (!isDie)//몬스터가 살아있는동안 계속 while문으로 실행 시킴
        {
            if (state == State.DIE)
                yield break;//몬스터가 죽으면 코루틴 함수 정지

            float dist = Vector3.Distance(playerTr.position, enemyTr.position);//player와 몬스터 거리 계산 함수

            if (dist <= attackDist)//사정 거리 내일 때 공격으로 변경
            {
                state = State.ATTACK;//공격
                StartCoroutine(attackDelay()); //walk 애니메이션 호출
            }

            yield return new WaitForSeconds(1f);//위에서 설정한 지연시간 0.3초 대기

        }
    }

    public IEnumerator Action()//애니메이션 파라미터
    {
        while (!isDie)
        {
            yield return ws;

            switch (state)
            {
                case State.WALK:
                    animator.SetBool(Maintis_WALK, true);
                    animator.SetBool(Maintis_ATTACK, false);
                    animator.SetBool(Mantis_IDLE, false);
                    break;

                case State.ATTACK:
                    animator.SetBool(Mantis_IDLE, false);
                    animator.SetBool(Maintis_ATTACK, true);
                    animator.SetBool(Maintis_WALK, false);
                    break;

                case State.TRACE:
                    animator.SetBool(Maintis_WALK, false);
                    animator.SetBool(Maintis_ATTACK, false);
                    animator.SetBool(Mantis_IDLE, false);
                    break;

                case State.DIE:
                    animator.SetBool(Mantis_IDLE, true);
                    animator.SetBool(Maintis_ATTACK, false);
                    animator.SetBool(Maintis_WALK, false);

                    gameObject.tag = "Untagged";
                    isDie = true;
                    //애니메이션 종료
                    GetComponent<Collider2D>().enabled = false;//콜라이더 삭제
                    gameObject.SetActive(false);
                    //Destroy(gameObject, 0.5f);//0.5초 뒤 몬스터 삭제 
                    break;

                case State.HIT:
                    Hp -= 5;
                    break;
            }
        }
        yield return ws;
    }

}

