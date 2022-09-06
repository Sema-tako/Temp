using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCtrl : MonoBehaviour
{
    //==변수 목록==============================================
    #region

    public enum State
    {
        IDLE,
        WALK,
        DASH,
        ATTACK,
        UPATTACK,
        DOWNATTACK,
        JUMP,
        LANDED,
        SLIDE,
        WALLJUMP,
        DAMAGE,
        SPINE_DAMAGE,
        SKILL,
        PINTCH,
        DEATH,
    }

    public State state = State.IDLE;

    [Header("스탯 관련 변수")]
    public int hp = 5;
    public int curMp = 0;
    public int maxMp = 9;
    public bool isCollectShade = true;
    bool isStartHeal = false;

    Coroutine co_Heal;

    [Header("이동 관련 변수")]
    #region 이동 관련 변수
    float h;
    float moveSpeed = 4f;
    bool isTouchingCollider;
    bool isMoving;
    #endregion

    [Header("점프 관련 변수")]
    #region

    public float minJumpPower = 5;
    public float maxJumpPower = 3;
    public Transform feetPos;
    public LayerMask Ground;

    Rigidbody2D rb;
    float jumpTimer;
    bool canJump = true;
    bool isGrounded;
    bool isJumping = false;
    bool isLanded = false;

    #endregion

    [Header("벽 타기 관련 변수")]
    #region
    public Transform front;
    public Transform slideFront;
    public LayerMask Wall;
    bool isTouchingFront;
    bool WallSliding;
    public float WallSlidingSpeed = 1.5f;

    bool wallJumping;
    bool canSetDash = true;
    public float wallJumpPower;
    public float wallJumpTime;
    #endregion

    [Header("대시 관련 변수")]
    #region
    bool canDash = true;
    bool isDashing = false;
    public float dashSpeed = 5;
    public float dashDelay = 0.05f;
    float dashTimer;
    #endregion

    [Header("공격 관련 변수")]
    #region

    public GameObject attackEffect;
    public bool attackOffset = false;
    bool canAttack = true;
    bool normalAttacking = false;
    bool upAttacking = false;
    bool downAttacking = false;
    bool normalAttackOffset = false;
    bool downAttackOffset = false;

    float attacktimer;
    public float attackDelay;

    #endregion

    [Header("스킬 관련 변수")]
    #region

    public Vector3 skillOffset;
    public GameObject[] spirit;
    bool isUsingSkill = false;
    bool canUseSkill = true;
    bool isManaCanUseSkill = false;

    public float skillDelay;
    float skillTimer;

    #endregion

    [Header("데미지 관련 변수")]
    #region 데미지 관련
    public Image fade;
    public ParticleSystem damageVoid;
    public Shade shade;
    bool isSpineDamage;
    bool isEnemyDamage;
    bool canDamage = true;
    public bool isDie = false;
    SpriteRenderer spr;
    Shader defaultSd;
    Shader damageSd;

    Vector3 respawnPos;
    Vector3 shadePos;
    #endregion

    [Header("애니메이션 관련 변수")]
    #region 애니메이션 관련

    Animator animator;

    readonly int hashWalk = Animator.StringToHash("WALK");
    readonly int hashJump = Animator.StringToHash("JUMP");
    readonly int hashLand = Animator.StringToHash("LANDED");
    readonly int hashAttack = Animator.StringToHash("ATTACK");
    readonly int hashUpAttack = Animator.StringToHash("UPATTACK");
    readonly int hashDownAttack = Animator.StringToHash("DOWNATTACK");
    readonly int hashDash = Animator.StringToHash("DASH");
    readonly int hashSkill = Animator.StringToHash("SKILL");
    readonly int hashWallSlide = Animator.StringToHash("SLIDE");
    readonly int hashWallJump = Animator.StringToHash("WALLJUMP");
    readonly int hashSpineDamage = Animator.StringToHash("SPINE_DAMAGE");
    readonly int hashDamage = Animator.StringToHash("DAMAGE");
    readonly int hashDeath = Animator.StringToHash("DEATH");

    #endregion 

    [Header("기타")]
    FollowCam cam;

    #endregion

    #region 유니티 함수

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FollowCam>();
        spr = GetComponent<SpriteRenderer>();
        defaultSd = spr.material.shader;
        damageSd = Shader.Find("GUI/Text Shader");
        dashTimer = dashDelay;
        jumpTimer = 0.2f;
    }

    private void Start()
    {
        StartCoroutine(SetState());
        StartCoroutine(Animation());
    }

    void Update()
    {
        if (!isDie)
        {
            Move();
            DashCommand();
            Jump();
            Skill();
            isCanWallSlide();
            Attack();
            AttackOffset();
            if (curMp >= 3)
            {
                isManaCanUseSkill = true;
            }
            else
            {
                isManaCanUseSkill = false;
            }
        }
    }

    private void FixedUpdate()
    {
        if (rb.velocity.y > 6.7f)
            rb.velocity = new Vector2(rb.velocity.x, 6.7f);
        else if(rb.velocity.y<-12f)
            rb.velocity = new Vector2(rb.velocity.x, -12f);

        Dash();
        WallSlide();
        WallJump();
        StartCoroutine(SkillOffset());
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Enemy") && !isDie && canDamage)
        {
            canAttack = false;
            canDash = false;
            canJump = false;
            isJumping = false;
            isGrounded = false;
            isLanded = false;
            isUsingSkill = false;
            isMoving = false;
            canUseSkill = false;
            canSetDash = false;
            isEnemyDamage = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("RespawnArea"))
        {
            respawnPos = collision.transform.position;
            shadePos = collision.GetComponent<SetShadePos>().shadePos;
        }
        else if (collision.CompareTag("HealArea"))
        {
            if (!isStartHeal)
            {
                co_Heal = StartCoroutine(Heal());
            }
        }

    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Camera_y"))
        {
            cam.setCameraY = true;
            cam.y = collision.gameObject.transform.position.y;
        }

        if (collision.gameObject.CompareTag("Camera_x"))
        {
            cam.setCameraX = true;
            cam.x = collision.gameObject.transform.position.x;
        }

        if (collision.CompareTag("Spine") && canDamage)
        {
            if (rb.velocity.y > 0)
                rb.velocity = Vector2.zero;
            canDamage = false;
            canAttack = false;
            canDash = false;
            canJump = false;
            isJumping = false;
            isGrounded = false;
            isLanded = false;
            isUsingSkill = false;
            isMoving = false;
            canUseSkill = false;
            canSetDash = false;
            isSpineDamage = true;
            StartCoroutine(SpineDamage());
        }
        else if (collision.CompareTag("Enemy") && canDamage)
        {
            if (rb.velocity.y > 0)
                rb.velocity = Vector2.zero;
            canDamage = false;
            canAttack = false;
            canDash = false;
            canJump = false;
            isJumping = false;
            isGrounded = false;
            isLanded = false;
            isUsingSkill = false;
            isMoving = false;
            canUseSkill = false;
            canSetDash = false;
            isEnemyDamage = true;
            StartCoroutine(EnemyDamage());
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Camera_y") && !isDie)
            cam.setCameraY = false;

        if (collision.gameObject.CompareTag("Camera_x") && !isDie)
            cam.setCameraX = false;

        if (collision.CompareTag("HealArea"))
        {
            StopCoroutine(co_Heal);
            isStartHeal = false;
        }
    }

    #endregion

    //==작성한 함수들==============================================
    #region 작성한 함수들

    /// <summary>
    /// 이동 함수<br/>
    /// </summary>
    private void Move()
    {
        h = Input.GetAxisRaw("Horizontal");
        isTouchingCollider = Physics2D.OverlapCircle(front.position, 0.01f, Wall) || Physics2D.OverlapCircle(front.position, 0.01f, Ground);
        if (h > 0)
        {
            transform.localRotation = Quaternion.Euler(0, 0, 0);

            if (!isTouchingCollider && !isDashing && !isUsingSkill && !isSpineDamage && !isEnemyDamage)
            {
                if (isGrounded)
                    isMoving = true;
                else
                    isMoving = false;

                if (wallJumping)
                    transform.Translate(new Vector3(h * 0.1f, 0, 0) * moveSpeed * Time.deltaTime);
                else
                    transform.Translate(new Vector3(h, 0, 0) * moveSpeed * Time.deltaTime);
            }
        }
        else if (h < 0)
        {
            transform.localRotation = Quaternion.Euler(0, 180, 0);
            if (!isTouchingCollider && !isDashing && !isUsingSkill && !isSpineDamage && !isEnemyDamage)
            {
                if (isGrounded)
                    isMoving = true;
                else
                    isMoving = false;

                if (wallJumping)
                    transform.Translate(new Vector3(-h * 0.1f, 0, 0) * moveSpeed * Time.deltaTime);
                else
                    transform.Translate(new Vector3(-h, 0, 0) * moveSpeed * Time.deltaTime);
            }
        }
        else
        {
            isMoving = false;
        }
    }

    /// <summary>
    /// 점프 함수
    /// </summary>
    void Jump()
    {
        isGrounded = Physics2D.OverlapCircle(feetPos.position, 0.05f, Ground);
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded && canJump && !isUsingSkill && !isSpineDamage && !isEnemyDamage)
            {   //일반 점프
                isJumping = true;
                wallJumping = false;
                isMoving = false;
                isLanded = false;
                jumpTimer = 0.2f;
                rb.velocity = Vector2.up * minJumpPower;
            }
            else if (WallSliding && !isSpineDamage && !isEnemyDamage)
            {   //벽 점프
                wallJumping = true;
                isJumping = false;
                isMoving = false;
                isLanded = false;

                jumpTimer = wallJumpTime;
            }

        }
        else if (isJumping && Input.GetButton("Jump"))
        {
            if (jumpTimer > 0.00015)
            {
                rb.AddForce(Vector2.up * maxJumpPower * Time.deltaTime * 500);
                canJump = false;
                isLanded = false;
                isMoving = false;
                jumpTimer -= Time.deltaTime;
            }
            else if (jumpTimer <= 0.0001)
            {
                jumpTimer = 0.0001f;
                canJump = false;
                wallJumping = false;
                if (isGrounded && !isDashing)
                {
                    isJumping = false;
                    StartCoroutine(Landed());
                }
            }
        }
        else if (wallJumping)
        {
            if (jumpTimer > 0.0003)
            {
                jumpTimer -= Time.deltaTime;
            }
            else if (jumpTimer <= 0.0003)
            {
                jumpTimer = 0.0003f;
                wallJumping = false;
                isJumping = true;
                isMoving = false;
                if (isGrounded && !isDashing)
                {
                    StartCoroutine(Landed());
                }
            }
        }

        if (!Input.GetButton("Jump") && !Input.GetKey(KeyCode.Z) && !isUsingSkill && !isSpineDamage && !isEnemyDamage)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            if (isGrounded && !isDashing)
            {
                if (isJumping)
                    StartCoroutine(Landed());
                canJump = true;
                isJumping = false;
            }
        }

    }

    /// <summary>
    /// 벽 점프 함수
    /// </summary>
    void WallJump()
    {
        if (wallJumping)
        {
            rb.velocity = -transform.right * 3.5f + new Vector3(0, wallJumpPower);
        }

        if (jumpTimer <= 0 && !isUsingSkill)
        {
            jumpTimer = 0.0001f;
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    /// <summary>
    /// 착지 여부 변수 제어용 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator Landed()
    {
        isLanded = true;
        yield return new WaitForSeconds(0.2f);
        isLanded = false;
    }

    /// <summary>
    /// 벽 슬라이딩 여부 판단 함수
    /// </summary>
    void isCanWallSlide()
    {
        isTouchingFront = Physics2D.OverlapCircle(slideFront.position, 0.15f, Wall);
        if (isTouchingFront && !isGrounded)
        {
            WallSliding = true;
            isJumping = false;
            isMoving = false;
            isTouchingCollider = false;
            if (isDashing && canSetDash)
            {
                isDashing = false;
            }
        }
        else
        {
            canSetDash = false;
            WallSliding = false;
        }
    }

    /// <summary>
    /// 벽 슬라이딩 함수 (FixedUpdate)
    /// </summary>
    void WallSlide()
    {
        if (WallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -WallSlidingSpeed, float.MaxValue));
        }
        else
        {
            if (isDashing)
                return;
            if (!isUsingSkill)
                rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    /// <summary>
    /// 대시 조작 받아오기
    /// </summary>
    void DashCommand()
    {
        if (Input.GetKeyDown(KeyCode.Z) && canDash && !isSpineDamage)
        {
            canDash = false;
            WallSliding = false;
            isDashing = true;
            isLanded = false;
        }
        if (!canDash)
        {
            dashTimer -= Time.deltaTime;
        }
        if (dashTimer <= 0)
        {
            if (WallSliding || isGrounded)
            {
                canDash = true;
            }
            else
            {
                canDash = false;
            }
            isDashing = false;
            dashTimer = dashDelay;
        }
    }

    /// <summary>
    /// 대시 함수(FixedUpdate)
    /// </summary>
    void Dash()
    {
        if (isDashing)
        {
            isLanded = false;
            isMoving = false;
            if (!isGrounded)
            {
                if (WallSliding)
                {
                    if (transform.localRotation.y >= 0)
                    {
                        transform.localRotation = Quaternion.Euler(transform.rotation.x, 180, transform.rotation.z);
                    }
                    else if (transform.localRotation.y < 0)
                    {
                        transform.localRotation = Quaternion.Euler(transform.rotation.x, 0, transform.rotation.z);
                    }
                }
                rb.velocity = transform.right * dashSpeed + new Vector3(0, 0.5f, 0) * Time.deltaTime * 50;
                StartCoroutine(isJump(2));
            }
            else
                rb.velocity = transform.right * dashSpeed * Time.deltaTime * 50;

            if (dashTimer <= 0 && !isUsingSkill && !isSpineDamage)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
                isDashing = false;
                if (!isGrounded && !wallJumping && !isSpineDamage && !isEnemyDamage)
                {
                    isJumping = true;
                }
            }
        }
        else
        {
            if (!isUsingSkill)
                rb.velocity = new Vector2(0, rb.velocity.y);
            isDashing = false;
            if (!isGrounded && !wallJumping && !isSpineDamage && !isEnemyDamage)
            {
                isJumping = true;
            }
        }
    }

    /// <summary>
    /// 공격 함수
    /// </summary>
    void Attack()
    {
        if (Input.GetKeyDown(KeyCode.X)&& canAttack)
        {
            attacktimer = attackDelay;
            isLanded = false;
            isMoving = false;
            canAttack = false;
            wallJumping = false;

            if (Input.GetAxisRaw("Vertical") > 0)
            {
                attackEffect.transform.localPosition = new Vector3(0, 1f, -0.5f);
                attackEffect.transform.localRotation = Quaternion.Euler(0, 0, -90);
                StartCoroutine(setAttackEffect());
                StartCoroutine(isJump(1));
                upAttacking = true;

            }
            else if (Input.GetAxisRaw("Vertical") < 0 && !isGrounded)
            {
                attackEffect.transform.localPosition = new Vector3(0, -1f, -0.5f);
                attackEffect.transform.localRotation = Quaternion.Euler(0, 180, 90);
                StartCoroutine(setAttackEffect());
                StartCoroutine(isJump(1));
                downAttacking = true;
                downAttackOffset = true;
            }
            else
            {
                attackEffect.transform.localPosition = new Vector3(1, -0.1f, -0.5f);
                attackEffect.transform.localRotation = Quaternion.Euler(0, 180, 0);
                StartCoroutine(setAttackEffect());
                normalAttacking = true;
                normalAttackOffset = true;
                if (isJumping)
                    StartCoroutine(isJump(0));
            }
        }

        if (attacktimer > 0)
        {
            attacktimer -= Time.deltaTime;
        }
        else 
        {
            attacktimer = 0;
            upAttacking = false;
            downAttacking = false;
            normalAttacking = false;
        }

        if (!Input.GetKey(KeyCode.X))
        {
            upAttacking = false;
            downAttacking = false;
            normalAttacking = false;
            if (attacktimer <= 0.02f && !isSpineDamage)
            {
                canAttack = true;
            }
        }


    }

    /// <summary>
    /// 공중에서 공격 시 점프 중 여부 변수 제어를 위한 코루틴
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    IEnumerator isJump(int a)
    {
        isJumping = false;
        switch (a)
        {
            case 0:
                yield return new WaitForSeconds(0.08f);
                break;

            case 1:
                yield return new WaitForSeconds(0.03f);
                break;
            case 2:
                yield return new WaitForSeconds(0.2f);
                break;
        }

        isJumping = true;
    }

    /// <summary>
    /// 공격 이펙트 On
    /// </summary>
    /// <returns></returns>
    IEnumerator setAttackEffect()
    {
        yield return new WaitForSeconds(0.08f);
        attackEffect.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        attackEffect.SetActive(false);
    }

    /// <summary>
    /// 공격 시 콜라이더에 맞았을 때 밀려나도록 하는 함수
    /// </summary>
    void AttackOffset()
    {
        if (attackOffset)
        {
            canDash = true;
            if (normalAttackOffset)
            {
                if (gameObject.transform.localRotation.y == 0)
                {
                    rb.AddForce(new Vector3(-1000f, 0, 0));
                }
                else if (gameObject.transform.localRotation.y != 0)
                {
                    rb.AddForce(new Vector3(1000f, 0, 0));
                }

                attackOffset = false;
                normalAttackOffset = false;
            }
            else if (downAttackOffset)
            {
                rb.AddForce(new Vector3(0, 2000f, 0));
                rb.velocity = new Vector3(0, 4f, 0);
                attackOffset = false;
                downAttackOffset = false;
            }
            else
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
                attackOffset = false;
            }
        }
    }

    /// <summary>
    /// 스킬 조작 함수
    /// </summary>
    void Skill()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canUseSkill && isManaCanUseSkill)
        {
            curMp -= 3;
            StartCoroutine(setSkillActive());
            skillTimer = skillDelay;
            canUseSkill = false;
            isUsingSkill = true;
            isMoving = false;
        }
        if (isUsingSkill)
        {
            isJumping = false;
            if (skillTimer > 0)
            {
                skillTimer -= Time.deltaTime;
            }
            else if (skillTimer <= 0.001f)
            {
                skillTimer = 0.001f;
                isUsingSkill = false;
                if (!isGrounded && !isEnemyDamage && !isSpineDamage)
                {
                    isJumping = true;
                }

                if (!Input.GetKey(KeyCode.A))
                {
                    canUseSkill = true;
                }
            }
        }
    }

    /// <summary>
    /// 스킬 사용 시 뒤로 밀려나도록
    /// </summary>
    /// <returns></returns>
    IEnumerator SkillOffset()
    {
        if (isUsingSkill)
        {
            yield return new WaitForSeconds(0.03f);
            rb.velocity = (-transform.right * skillOffset.x + transform.up * skillOffset.y);
        }
    }

    /// <summary>
    /// 스킬 파티클 위치 조정 및 활성화
    /// </summary>
    /// <returns></returns>
    IEnumerator setSkillActive()
    {
        for (int a = 0; a < 3; a++)
        {
            if (spirit[a].activeSelf == false)
            {
                ParticleSystem skill = spirit[a].GetComponent<ParticleSystem>();

                if (transform.localRotation.y == 0)
                {
                    spirit[a].transform.localPosition = transform.position + new Vector3(0.7f, 0, 0);
                    spirit[a].transform.rotation = Quaternion.Euler(0, 0, 0);
                    skill.startRotation3D = new Vector3(0, 0, 0);
                    ParticleSystem[] subs = skill.GetComponentsInChildren<ParticleSystem>();
                    for (int b = 1; b < subs.Length; b++)
                    {
                        subs[b].startRotation3D = new Vector3(0, 0, 0);
                    }
                }
                else if (transform.localRotation.y != 0)
                {
                    spirit[a].transform.localPosition = transform.position - new Vector3(0.7f, 0, 0);
                    spirit[a].transform.rotation = Quaternion.Euler(0, -180, 0);
                    skill.startRotation3D = new Vector3(0, -180 * Mathf.Deg2Rad, 0);
                    ParticleSystem[] subs = skill.GetComponentsInChildren<ParticleSystem>();
                    for (int b = 0; b < subs.Length; b++)
                    {
                        subs[b].startRotation3D = new Vector3(0, -180 * Mathf.Deg2Rad, 0);
                    }
                }

                spirit[a].SetActive(true);
                yield return new WaitForSeconds(1.3f);
                spirit[a].SetActive(false);
                break;
            }
        }
        yield return null;
    }

    IEnumerator Heal()
    {
        isStartHeal = true;
        for (int a = 0; a < 5; a++)
        {
            yield return new WaitForSeconds(1.5f);
            if (hp < 5)
                hp++;
        }
    }


    /// <summary>
    /// 가시 콜라이더에 닿았을 시 데미지 함수
    /// </summary>
    /// <returns></returns>
    IEnumerator SpineDamage()
    {
        if (hp > 1)
        {
            rb.AddForce(new Vector2(300, 800));
            yield return StartCoroutine(FadeOut());
            yield return transform.position = Vector3.Lerp(transform.position, respawnPos, 1f);
            isSpineDamage = false;
            StartCoroutine(FadeIN());
            yield return StartCoroutine(DamageEffect());
        }
        else if (hp == 1)
        {
            hp--;
            StartCoroutine(Death());
        }
    }

    /// <summary>
    /// 일반 에너미와 충돌했을 시 데미지 함수
    /// </summary>
    /// <returns></returns>
    IEnumerator EnemyDamage()
    {
        if (hp > 1)
        {
            rb.velocity = Vector2.zero;
            rb.AddForce(-transform.right * new Vector2(2000, 0f) + new Vector2(0, 500f));
            StartCoroutine(DamageEffect());
            yield return new WaitForSeconds(0.06f);
            isEnemyDamage = false;
        }
        else if (hp == 1)
        {
            hp--;
            StartCoroutine(Death());
        }
    }


    IEnumerator FadeOut()
    {
        for (float a = 0; a <= 1; a += 0.01f)
        {
            fade.color = new Color(0, 0, 0, a);
            yield return new WaitForSeconds(0.005f);
        }
        yield return new WaitForSeconds(1f);
    }

    IEnumerator FadeIN()
    {
        for (float a = 1; a >= 0; a -= 0.1f)
        {
            fade.color = new Color(0, 0, 0, a);
            yield return new WaitForSeconds(0.1f);
        }
    }


    /// <summary>
    /// 데미지 시 깜박이는 이펙트
    /// </summary>
    /// <returns></returns>
    IEnumerator DamageEffect()
    {
        if (hp > 1)
        {
            hp--;
            canDamage = false;
            damageVoid.Play();
            for (int a = 0; a < 6; a++)
            {
                if (a % 2 == 0)
                {
                    spr.material.color = Color.black;
                    spr.material.shader = damageSd;
                }
                else
                {
                    spr.material.color = Color.white;
                    spr.material.shader = defaultSd;
                }
                yield return new WaitForSeconds(0.3f);
            }
            canDamage = true;
            canAttack = true;
            canDash = true;
            canJump = true;
            canSetDash = true;
            canUseSkill = true;
        }
        else if (hp == 1)
        {
            hp--;
            isDie = true;
        }
    }


    IEnumerator Death()
    {
        isDie = true;
        isCollectShade = false;
        curMp = 0;
        rb.gravityScale = 0;
        rb.simulated = false;
        yield return new WaitForSeconds(1f);
        rb.gravityScale = 1.5f;
        rb.simulated = true;
        shade.transform.position = transform.position;
        shade.gameObject.SetActive(true);
        yield return StartCoroutine(FadeOut());
        transform.position = new Vector3(3.5f, 69.3f, -0.27f);
        state = State.IDLE;
        hp = 5;
        shade.transform.position = shadePos;
        shade.defaultPos = shadePos;
        isDie = false;
        isEnemyDamage = false;
        isSpineDamage = false;
        canDamage = true;
        canUseSkill = true;
        yield return StartCoroutine(FadeIN());
    }


    IEnumerator SetState()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.01f);
            if (isDie)
            {
                state = State.DEATH;
            }
            else
            {
                if (isLanded)
                {
                    state = State.LANDED;
                }
                else if (isSpineDamage)
                {
                    state = State.SPINE_DAMAGE;
                }
                else if (isEnemyDamage)
                {
                    state = State.DAMAGE;
                }
                else if (normalAttacking)
                {
                    state = State.ATTACK;
                }
                else if (upAttacking)
                {
                    state = State.UPATTACK;
                }
                else if (downAttacking)
                {
                    state = State.DOWNATTACK;
                }
                else if (isMoving)
                {
                    state = State.WALK;
                }
                else if (isTouchingFront)
                {
                    state = State.SLIDE;
                }
                else if (wallJumping)
                {
                    state = State.WALLJUMP;
                }
                else if (isJumping)
                {
                    state = State.JUMP;
                }
                else if (isDashing)
                {
                    state = State.DASH;
                }
                else if (isUsingSkill)
                {
                    state = State.SKILL;
                }
                else
                {
                    state = State.IDLE;
                }
            }
        }
    }   //스탯 변경 함수

    IEnumerator Animation()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.01f);
            switch (state)
            {

                case State.IDLE:
                    animator.SetBool(hashDash, false);
                    animator.SetBool(hashWalk, false);
                    animator.SetBool(hashLand, false);
                    animator.SetBool(hashJump, false);
                    animator.SetBool(hashAttack, false);
                    animator.SetBool(hashDownAttack, false);
                    animator.SetBool(hashUpAttack, false);
                    animator.SetBool(hashSkill, false);
                    animator.SetBool(hashWallJump, false);
                    animator.SetBool(hashWallSlide, false);
                    animator.SetBool(hashSpineDamage, false);
                    animator.SetBool(hashDeath, false);
                    break;

                case State.WALK:
                    animator.SetBool(hashWalk, true);   //
                    animator.SetBool(hashDash, false);
                    animator.SetBool(hashJump, false);
                    animator.SetBool(hashAttack, false);
                    animator.SetBool(hashDownAttack, false);
                    animator.SetBool(hashUpAttack, false);
                    animator.SetBool(hashSkill, false);
                    animator.SetBool(hashWallJump, false);
                    animator.SetBool(hashWallSlide, false);
                    animator.SetBool(hashSpineDamage, false);
                    animator.SetBool(hashDeath, false);
                    break;

                case State.JUMP:
                    animator.SetBool(hashJump, true);   //
                    animator.SetBool(hashWalk, false);
                    animator.SetBool(hashLand, false);
                    animator.SetBool(hashDash, false);
                    animator.SetBool(hashSkill, false);
                    animator.SetBool(hashAttack, false);
                    animator.SetBool(hashDownAttack, false);
                    animator.SetBool(hashUpAttack, false);
                    animator.SetBool(hashWallJump, false);
                    animator.SetBool(hashWallSlide, false);
                    animator.SetBool(hashSpineDamage, false);
                    animator.SetBool(hashDeath, false);
                    break;

                case State.LANDED:
                    animator.SetTrigger(hashLand);   //
                    animator.SetBool(hashWalk, false);
                    animator.SetBool(hashJump, false);
                    animator.SetBool(hashDash, false);
                    animator.SetBool(hashSkill, false);
                    animator.SetBool(hashAttack, false);
                    animator.SetBool(hashDownAttack, false);
                    animator.SetBool(hashUpAttack, false);
                    animator.SetBool(hashWallJump, false);
                    animator.SetBool(hashWallSlide, false);
                    animator.SetBool(hashSpineDamage, false);
                    animator.SetBool(hashDeath, false);
                    break;


                case State.SLIDE:
                    animator.SetBool(hashWallSlide, true);  //
                    animator.SetBool(hashWalk, false);
                    animator.SetBool(hashWallJump, false);
                    animator.SetBool(hashJump, false);
                    animator.SetBool(hashDash, false);
                    animator.SetBool(hashAttack, false);
                    animator.SetBool(hashDownAttack, false);
                    animator.SetBool(hashUpAttack, false);
                    animator.SetBool(hashSkill, false);
                    animator.SetBool(hashSpineDamage, false);
                    animator.SetBool(hashDeath, false);
                    break;

                case State.WALLJUMP:
                    animator.SetBool(hashWallJump, true);  //
                    animator.SetBool(hashWalk, false);
                    animator.SetBool(hashDash, false);
                    animator.SetBool(hashJump, false);
                    animator.SetBool(hashSkill, false);
                    animator.SetBool(hashAttack, false);
                    animator.SetBool(hashDownAttack, false);
                    animator.SetBool(hashUpAttack, false);
                    animator.SetBool(hashWallSlide, false);
                    animator.SetBool(hashSpineDamage, false);
                    animator.SetBool(hashDeath, false);
                    break;

                case State.ATTACK:
                    animator.SetTrigger(hashAttack);    //
                    animator.SetBool(hashWalk, false);
                    animator.SetBool(hashDash, false);
                    animator.SetBool(hashJump, false);
                    animator.SetBool(hashSkill, false);
                    animator.SetBool(hashWallJump, false);
                    animator.SetBool(hashWallSlide, false);
                    animator.SetBool(hashSpineDamage, false);
                    animator.SetBool(hashDeath, false);
                    break;

                case State.UPATTACK:
                    animator.SetTrigger(hashUpAttack);  //
                    animator.SetBool(hashWalk, false);
                    animator.SetBool(hashDash, false);
                    animator.SetBool(hashJump, false);
                    animator.SetBool(hashSkill, false);
                    animator.SetBool(hashWallJump, false);
                    animator.SetBool(hashWallSlide, false);
                    animator.SetBool(hashSpineDamage, false);
                    animator.SetBool(hashDeath, false);
                    break;

                case State.DOWNATTACK:
                    animator.SetTrigger(hashDownAttack);
                    animator.SetBool(hashWalk, false);
                    animator.SetBool(hashDash, false);
                    animator.SetBool(hashJump, false);
                    animator.SetBool(hashSkill, false);
                    animator.SetBool(hashWallJump, false);
                    animator.SetBool(hashWallSlide, false);
                    animator.SetBool(hashSpineDamage, false);
                    animator.SetBool(hashDeath, false);
                    break;

                case State.DASH:
                    animator.SetBool(hashDash, true);
                    animator.SetBool(hashSkill, false);
                    animator.SetBool(hashJump, false);
                    animator.SetBool(hashLand, false);
                    animator.SetBool(hashWalk, false);
                    animator.SetBool(hashAttack, false);
                    animator.SetBool(hashDownAttack, false);
                    animator.SetBool(hashUpAttack, false);
                    animator.SetBool(hashWallJump, false);
                    animator.SetBool(hashWallSlide, false);
                    animator.SetBool(hashSpineDamage, false);
                    animator.SetBool(hashDeath, false);
                    break;

                case State.DAMAGE:
                    animator.SetTrigger(hashDamage);
                    animator.SetBool(hashDash, false);
                    animator.SetBool(hashSkill, false);
                    animator.SetBool(hashJump, false);
                    animator.SetBool(hashLand, false);
                    animator.SetBool(hashWalk, false);
                    animator.SetBool(hashAttack, false);
                    animator.SetBool(hashDownAttack, false);
                    animator.SetBool(hashUpAttack, false);
                    animator.SetBool(hashWallJump, false);
                    animator.SetBool(hashWallSlide, false);
                    animator.SetBool(hashDeath, false);
                    break;

                case State.SPINE_DAMAGE:
                    animator.SetBool(hashSpineDamage, true);
                    animator.SetBool(hashDash, false);
                    animator.SetBool(hashSkill, false);
                    animator.SetBool(hashAttack, false);
                    animator.SetBool(hashDownAttack, false);
                    animator.SetBool(hashUpAttack, false);
                    animator.SetBool(hashJump, false);
                    animator.SetBool(hashLand, false);
                    animator.SetBool(hashWalk, false);
                    animator.SetBool(hashWallJump, false);
                    animator.SetBool(hashWallSlide, false);
                    animator.SetBool(hashDeath, false);
                    break;

                case State.SKILL:
                    animator.SetBool(hashSkill, true);
                    animator.SetBool(hashDash, false);
                    animator.SetBool(hashJump, false);
                    animator.SetBool(hashAttack, false);
                    animator.SetBool(hashDownAttack, false);
                    animator.SetBool(hashUpAttack, false);
                    animator.SetBool(hashLand, false);
                    animator.SetBool(hashWalk, false);
                    animator.SetBool(hashWallJump, false);
                    animator.SetBool(hashWallSlide, false);
                    animator.SetBool(hashSpineDamage, false);
                    animator.SetBool(hashDeath, false);
                    break;

                case State.DEATH:
                    animator.SetBool(hashDeath, true);
                    animator.SetBool(hashSkill, false);
                    animator.SetBool(hashDash, false);
                    animator.SetBool(hashJump, false);
                    animator.SetBool(hashLand, false);
                    animator.SetBool(hashAttack, false);
                    animator.SetBool(hashDownAttack, false);
                    animator.SetBool(hashUpAttack, false);
                    animator.SetBool(hashWalk, false);
                    animator.SetBool(hashWallJump, false);
                    animator.SetBool(hashWallSlide, false);
                    animator.SetBool(hashSpineDamage, false);
                    break;

            }
        }   //스탯에 따른 애니메이션 변경 함수
    }

    #endregion

}
