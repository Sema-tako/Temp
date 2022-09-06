using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTrail : MonoBehaviour
{
    public ParticleSystem hitEffect;
    public Rigidbody2D player;
    PlayerCtrl playerCtrl;
    public int power=5;

    void Awake()
    {
        player = player.GetComponent<Rigidbody2D>();
        playerCtrl = player.gameObject.GetComponent<PlayerCtrl>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!collision.CompareTag("Wall"))
            playerCtrl.attackOffset = true;

        if (collision.CompareTag("Enemy"))
        {
            playerCtrl.curMp++;

            if (!playerCtrl.isCollectShade && playerCtrl.curMp > 6)
                playerCtrl.curMp = 6;

            if (playerCtrl.curMp > playerCtrl.maxMp)
                playerCtrl.curMp = playerCtrl.maxMp;
        }

        hitEffect.transform.position = collision.bounds.ClosestPoint(transform.position)-new Vector3(0,0,1.5f);
        hitEffect.gameObject.SetActive(true);
        hitEffect.Play();
    }

}
