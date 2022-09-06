using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Boss : MonoBehaviour
{
    public int BossHP;

    public EnemyDamageScr enemyDamageScr;
    PlayerCtrl pCtrl;
    int Php;

    public Animator anim;
    SpriteRenderer rend;
    BoxCollider2D collider;
    public GameObject sense;

    Vector3 nextPos;
    float moveSpeed;

    public GameObject target;

    IEnumerator currCoroutine;
    IEnumerator bossPatternCoroutine;
    IEnumerator dieCoroutine = null;

    public GameObject boomerang;

    public bool canStart;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rend = GetComponent<SpriteRenderer>();
        target = GameObject.FindWithTag("Player");
        collider = GetComponent<BoxCollider2D>();
        collider.enabled = false;
        pCtrl = target.GetComponent<PlayerCtrl>();
        BossHP = enemyDamageScr.hp;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow) && canStart && BossHP>0)
        {
            collider.enabled = true;
            sense.GetComponent<Sense>().isBossStart = true;
            sense.SetActive(false);
            sense.GetComponent<Sense>().challenge.gameObject.SetActive(false);
            BossStageStart();
        }


        transform.Translate((nextPos - transform.position).normalized * Time.deltaTime * moveSpeed);

        DamageDie();
        BossReset();
    }

    public void BossReset()
    {
        if (pCtrl.hp <= 0&&!canStart)
        {
            //bossHP = 210;
            enemyDamageScr.hp = 210;
            collider.enabled = false;
            sense.SetActive(true);
            if (bossPatternCoroutine != null)
            StopCoroutine(bossPatternCoroutine);
            if(currCoroutine!=null)
            StopCoroutine(currCoroutine);
            BossIdle();

        }
    }

    public void BossIdle()
    {
        rend.enabled = true;
        anim.SetTrigger("0_Idle");
        transform.position = new Vector3(0.07f, 0.397f, 0);
        nextPos = transform.position;
    }

    public void BossHead_Up()
    {
        anim.SetTrigger("1_Head_Up");
    } // �÷��̾ ���� �Ÿ��� ������ �Ӹ����� ���ø�

    public void BossStageStart()
    {
        canStart = false;
        anim.SetTrigger("2_1Stand_Start");
        currCoroutine = BossStageDeley();
        StartCoroutine(currCoroutine);
    } // Ű �Է� �� ���ڿ��� �Ͼ

    public void BossStageStart2()
    {
        anim.SetTrigger("2_2Stand_End");
        nextPos = new Vector3(17.2f, 11.38f, 0);
        //transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime*10);
        moveSpeed = 16f;
        // Start �Լ�>>������������ �ű��
        // nextPos = ��ǥ ���� 
        // �ִϸ��̼� ���̿� �°� moveSpeed ����.
        // transform.position = Vector3.Lerp(transform.position, nextPos, Time.deltaTime*moveSpeed); >> update �Լ��� ���� �κ�.
        bossPatternCoroutine = BossPattern();
        StartCoroutine(bossPatternCoroutine);

    } // �� ���ִٰ� �پ� ������ ��������

    IEnumerator BossStageDeley()
    {
        yield return new WaitForSeconds(2f);
        BossStageStart2();
    } // �ڷ�ƾ �Լ��� �ð� ����

    public void BossRushStart()
    {
        transform.localScale = new Vector3(1f, 1, 1);
        anim.SetTrigger("3_1Rush_Start");

        transform.position = new Vector3(-5.38f, -1.87f, 0);
        nextPos = new Vector3(-4.64f, -2.94f, 0f);
        moveSpeed = 16f;

    } // ���� ���� ���� ����

    public void BossRushpreparation()
    {
        anim.SetTrigger("3_2Rush_preparation");
        transform.position = new Vector3(-4.8f, -3.9f, 0);
        nextPos = transform.position;

    } // ���� ���� ���� �غ�

    public void BossRushing()
    {
        anim.SetTrigger("3_3Rushing");
        nextPos = new Vector3(3.1f, -3.9f, 0);
        moveSpeed = 17f;
    } // ���� ���� ����

    public void BossRushEnd1()
    {
        anim.SetTrigger("3_4RushEnd1");
        transform.position = new Vector3(3.1f, -3.9f, 0);
        nextPos = transform.position;
    } // ���� ���� ���� ��1

    public void BossRushEnd2()
    {
        anim.SetTrigger("3_5Rush_End2");
        transform.position = new Vector3(3.1f, -3.9f, 0);
        nextPos = new Vector3(11.5f, -0.3f, 0f);
        moveSpeed = 16f;

        currCoroutine = RushDeley();
        StartCoroutine(currCoroutine);

    } // ���� ���� �� ���󰡱�


    public void BossRushStart_L()
    {
        transform.localScale = new Vector3(-1f, 1, 1);

        //transform.localScale = new Vector3(-1, 1, 1);

        anim.SetTrigger("3_1Rush_Start");
        transform.position = new Vector3(5.38f, -1.87f, 0);
        nextPos = new Vector3(4.64f, -2.94f, 0f);
        moveSpeed = 16f;

    }

    public void BossRushpreparation_L()
    {
        anim.SetTrigger("3_2Rush_preparation");
        transform.position = new Vector3(4.8f, -3.9f, 0);
        nextPos = transform.position;
    }

    public void BossRushing_L()
    {
        anim.SetTrigger("3_3Rushing");
        nextPos = new Vector3(-3.1f, -3.9f, 0);
        moveSpeed = 17f;
    }

    public void BossRushEnd1_L()
    {
        anim.SetTrigger("3_4RushEnd1");
        transform.position = new Vector3(-3.1f, -3.9f, 0);
        nextPos = transform.position;
    }

    public void BossRushEnd2_L()
    {
        anim.SetTrigger("3_5Rush_End2");
        transform.position = new Vector3(-3.1f, -3.9f, 0);
        nextPos = new Vector3(-11.5f, -0.3f, 0f);
        moveSpeed = 16f;

        currCoroutine = RushDeley();
        StartCoroutine(currCoroutine);
    }

    public void RushPull()
    {
        currCoroutine = BossRushPull();
        StartCoroutine(currCoroutine);
    } // ���� Ǯ 

    public void RushPull_L()
    {
        currCoroutine = BossRushPull_L();
        StartCoroutine(currCoroutine);
    } // ����(��) Ǯ

    IEnumerator BossRushPull()
    {
        rend.enabled = true;
        BossRushStart();
        yield return new WaitForSeconds(0.3f);
        BossRushpreparation();
        yield return new WaitForSeconds(0.3f);
        BossRushing();
        yield return new WaitForSeconds(0.3f);
        BossRushEnd1();
        yield return new WaitForSeconds(0.3f);
        BossRushEnd2();
        yield return new WaitForSeconds(0.3f);
    } // ���� Ǯ �ڷ�ƾ

    IEnumerator BossRushPull_L()
    {
        rend.enabled = true;
        BossRushStart_L();
        yield return new WaitForSeconds(0.3f);
        BossRushpreparation_L();
        yield return new WaitForSeconds(0.3f);
        BossRushing_L();
        yield return new WaitForSeconds(0.3f);
        BossRushEnd1_L();
        yield return new WaitForSeconds(0.3f);
        BossRushEnd2_L();
        yield return new WaitForSeconds(0.3f);
    } // ����(��) Ǯ �ڷ�ƾ

    IEnumerator RushDeley()
    {
        yield return new WaitForSeconds(0.3f);
        rend.enabled = false;
    }

    public void FlyingStart()
    {
        transform.localScale = new Vector3(1f, 1, 1);
        anim.SetTrigger("4_1Flying_Start");

        transform.position = new Vector3(target.transform.position.x, 2.88f, 0);
        nextPos = new Vector3(target.transform.position.x, 2.08f, 0);
        moveSpeed = 10f;

    } // ������� ����

    public void Flyingpreparation()
    {
        anim.SetTrigger("4_2Flying_preparation");

        transform.position = new Vector3(target.transform.position.x, 2.88f, 0);
        nextPos = transform.position;
    } // ������� �غ�

    public void Flying()
    {
        anim.SetTrigger("4_3Flying");
        nextPos = new Vector3(target.transform.position.x, -3.5f, 0);
        moveSpeed = 18f;
    } // �������

    public void FlyingEnd1()
    {
        anim.SetTrigger("4_4Flying_End1");
        transform.position = new Vector3(this.transform.position.x, -3.9f, 0);
        nextPos = transform.position;
    } // ������� ��1

    public void FlyingEnd2()
    {
        anim.SetTrigger("4_5Flying_End2");
        transform.position = new Vector3(this.transform.position.x, -3.9f, 0);
        nextPos = (new Vector3(2.33f, 1.8f, 0)) + target.transform.position;
        moveSpeed = 16f;

        currCoroutine = FlyEndDelay();
        StartCoroutine(currCoroutine);

    } // ������� ��2

    IEnumerator FlyEndDelay()
    {
        yield return new WaitForSeconds(0.3f);
        rend.enabled = false;
    } // �ڷ�ƾ �Լ��� �ð� ���� �� ���� ȭ�� ������ ��Ż


    public void BossFlyingPull()
    {
        currCoroutine = FlyingPull();
        StartCoroutine(currCoroutine);
    } // ���� ������� Ǯ

    IEnumerator FlyingPull()
    {
        rend.enabled = true;
        FlyingStart();
        yield return new WaitForSeconds(0.2f);
        Flyingpreparation();
        yield return new WaitForSeconds(0.3f);
        Flying();
        yield return new WaitForSeconds(0.3f);
        FlyingEnd1();
        yield return new WaitForSeconds(0.3f);
        FlyingEnd2();
    } // Flying Pull ����

    public void WallStart()
    {
        transform.localScale = new Vector3(1f, 1, 1);
        anim.SetTrigger("5_1Wall_Start");

        transform.position = new Vector3(4.45f, -0.2f, 0);
        nextPos = new Vector3(6.36f, -0.57f, 0f);
        moveSpeed = 16f;

    } // �� ���� ����

    public void WallAttack()
    {
        anim.SetTrigger("5_2Wall_Attack");

        transform.position = new Vector3(7.8f, -0.3f, 0);
        nextPos = transform.position;
    } // �� ���� 

    public void WallEnd()
    {
        anim.SetTrigger("5_3Wall_End");

        transform.position = new Vector3(7.8f, -0.3f, 0);
        nextPos = new Vector3(3.31f, 1.1f, 0);
        moveSpeed = 16f;

        currCoroutine = WallEndDeley();
        StartCoroutine(currCoroutine);
    } // �� ���� ��

    IEnumerator WallEndDeley()
    {
        yield return new WaitForSeconds(0.3f);
        rend.enabled = false;
    } // WallEnd �ڷ�ƾ

    public void BossWallPull()
    {
        currCoroutine = WallPull();
        StartCoroutine(currCoroutine);
    } // �� ���� Ǯ

    IEnumerator WallPull()
    {
        rend.enabled = true;
        WallStart();
        yield return new WaitForSeconds(0.3f);
        WallAttack();
        yield return new WaitForSeconds(2.5f);
        WallEnd();
    } // ������ Ǯ �ڷ�ƾ

    public void WallStart_L()
    {
        transform.localScale = new Vector3(-1f, 1, 1);
        anim.SetTrigger("5_1Wall_Start");

        transform.position = new Vector3(-4.45f, -0.2f, 0);
        nextPos = new Vector3(-6.36f, -0.57f, 0f);
        moveSpeed = 16f;

    }

    public void WallAttack_L()
    {
        anim.SetTrigger("5_2Wall_Attack");

        transform.position = new Vector3(-7.8f, -0.3f, 0);
        nextPos = transform.position;
    }

    public void WallEnd_L()
    {
        anim.SetTrigger("5_3Wall_End");

        transform.position = new Vector3(-7.8f, -0.3f, 0);
        nextPos = new Vector3(-3.31f, 1.1f, 0);
        moveSpeed = 16f;

        currCoroutine = WallEndDeley();
        StartCoroutine(currCoroutine);
    }
    public void BossWallPull_L()
    {
        currCoroutine = WallPull_L();
        StartCoroutine(currCoroutine);
    } // ������ Ǯ(L)

    IEnumerator WallPull_L()
    {
        rend.enabled = true;
        WallStart_L();
        yield return new WaitForSeconds(0.3f);
        WallAttack_L();
        yield return new WaitForSeconds(2.5f);
        WallEnd_L();
    } // ������ Ǯ �ڷ�ƾ(L)

    public void BossDie1()
    {
        //StopCoroutine(bossPatternCoroutine);
        //StopCoroutine(currCoroutine);
        collider.enabled = false;
        anim.SetTrigger("6_1Dying_Start");
        //StopCoroutine(currCoroutine);

        transform.position = new Vector3(this.transform.position.x, -3.5f, 0);
        nextPos = transform.position;
    } // ���� ���� ����

    public void BossDie2()
    {
        anim.SetTrigger("6_2Dying_End");

        transform.position = new Vector3(this.transform.position.x, -3.5f, 0);
        nextPos = new Vector3(this.transform.position.x, -0.53f, 0);

        currCoroutine = BossDieDeley();
        StartCoroutine(currCoroutine);
    } // ���� ���� ��
    IEnumerator BossDieDeley()
    {
        yield return new WaitForSeconds(0.5f);
        rend.enabled = false;
    } // ���� Die �����

    public void DiePull()
    {
        StartCoroutine("BossDiePull");
    } // ���� ���� Ǯ

    IEnumerator BossDiePull()
    {
        rend.enabled = true;
        BossDie1();
        yield return new WaitForSeconds(1f);
        BossDie2();
        yield return new WaitForSeconds(1.5f);
    } // �������� Ǯ �ڷ�ƾ

    public void LoserStart()
    {
        transform.position = new Vector3(-0.1f, 1.16f, 0);
        nextPos = transform.position;

        rend.enabled = true;
        anim.SetTrigger("7_1Loser_Start");

        /*currCoroutine = BossLoserDeley();
        StartCoroutine(currCoroutine);*/
    } // ���� �й��� ����

    public void LoserEnd()
    {
        anim.SetTrigger("7_2Loser_End");

        transform.position = new Vector3(-0.1f, 1.162f, 0);
        nextPos = transform.position;
    } // ���� �й��� ��

    public void LoserPull()
    {
        StopCoroutine(currCoroutine);
        currCoroutine = BossLoserPull();
        StartCoroutine(currCoroutine);
    } // ���� �й��� Ǯ

    IEnumerator BossLoserPull()
    {
        LoserStart();
        yield return new WaitForSeconds(0.5f);
        LoserEnd();
    } // ���� �й��� Ǯ �ڷ�ƾ

    IEnumerator BossLoserDie()
    {
        //DiePull();

        yield return StartCoroutine(BossDiePull());
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(BossLoserPull());
        //LoserPull();

    } // ���� ���� �й��� ����

    public void Boomerang_Right()
    {
        float moveX = 2.5f;

        if (transform.localScale.x == 1f)
        {
            GameObject _boomerang = Instantiate(boomerang, new Vector3(transform.position.x - moveX, transform.position.y, transform.position.z), Quaternion.identity);
            _boomerang.GetComponent<Boomerang>().moveSpeed *= -1f;
        }
        else if (transform.localScale.x == -1f)
        {
            GameObject _boomerang = Instantiate(boomerang, new Vector3(transform.position.x + moveX, transform.position.y, transform.position.z), Quaternion.identity);

        }
    } //�θ޶� (���� �ǵ帮�� �� ��!!)

    IEnumerator BossPattern()
    {
        yield return new WaitForSeconds(3f);

        while (true)
        {
            int ranAction = Random.Range(0, 5);
            switch (ranAction)
            {
                case 0:

                    currCoroutine = BossRushPull();
                    yield return StartCoroutine(currCoroutine);

                    break;
                case 1:

                    currCoroutine = BossRushPull_L();
                    yield return StartCoroutine(currCoroutine);
                    break;
                case 2:

                    currCoroutine = FlyingPull();
                    yield return StartCoroutine(currCoroutine);
                    break;
                case 3:

                    currCoroutine = WallPull();
                    yield return StartCoroutine(currCoroutine);
                    break;
                case 4:

                    currCoroutine = WallPull_L();
                    yield return StartCoroutine(currCoroutine);
                    break;
            }

            yield return new WaitForSeconds(2f);
        }

    } // ���ϸ��� ����

    public void DamageDie()
    {
        if (BossHP <= 0 && dieCoroutine == null)
        {
            StopAllCoroutines();
            dieCoroutine = BossLoserDie();
            StartCoroutine(dieCoroutine);
        }
    }


}
