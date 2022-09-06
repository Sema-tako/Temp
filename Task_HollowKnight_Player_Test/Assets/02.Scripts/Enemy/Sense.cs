using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sense : MonoBehaviour
{
    Boss boss;
    public CanvasGroup challenge;
    bool startActive = false;
    bool startActiveZ = false;
    public bool isBossStart = false;

    private void Awake()
    {
        boss = GameObject.Find("MantisLords").GetComponent<Boss>();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && !boss.canStart)
        {
            if (!startActive && !isBossStart)
            {
                StartCoroutine(ChallengeActive());
            }
            boss.BossHead_Up();
            boss.canStart = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!startActiveZ && !isBossStart)
            {
                StartCoroutine(ChallengeActiveZ());
            }
            boss.canStart = false;
        }
    }

    IEnumerator ChallengeActive()
    {
        if (startActiveZ)
        {
            startActiveZ = false;
            StopCoroutine(ChallengeActiveZ());
        }

        startActive = true;
        for (float a = challenge.alpha; a <= 1;)
        {
            challenge.alpha = a;
            a += 0.01f;
            yield return null;
        }
    }

    IEnumerator ChallengeActiveZ()
    {
        if (startActive)
        {
            startActive = false;
            StopCoroutine(ChallengeActive());
        }

        startActiveZ = true;
        for (float a = challenge.alpha; a >= 0;)
        {
            challenge.alpha = a;
            a -= 0.05f;
            yield return null;
        }
    }

}
