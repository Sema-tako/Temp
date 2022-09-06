using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill : MonoBehaviour
{
    public int power = 10;

    List<Collider2D> enemies = new List<Collider2D>();

    private void Awake()
    {
        GameObject[] tmp = GameObject.FindGameObjectsWithTag("Enemy");
        for (int a = 0; a < tmp.Length; a++)
        {
            Collider2D tmp2 = tmp[a].GetComponent<Collider2D>();
            enemies.Add(tmp2);
        }

        for (int a = 0; a < enemies.Count; a++)
        {
            enemies[a].isTrigger = false;
        }
    }

    private void OnEnable()
    {
        GameObject[] tmp = GameObject.FindGameObjectsWithTag("Enemy");
        for (int a = 0; a < tmp.Length; a++)
        {
            Collider2D tmp2 = tmp[a].GetComponent<Collider2D>();
            enemies.Add(tmp2);
        }

        for (int a=0; a < enemies.Count; a++)
        {
            enemies[a].isTrigger = false;
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyDamageScr>().Damage(power);
            if (other.GetComponent<EnemyDamageScr>().hp <= 0)
            {
                for(int a=0; a<enemies.Count; a++)
                {
                    if (enemies[a].gameObject.name == other.gameObject.name)
                        enemies.RemoveAt(a);
                }
            }
        }
    }

    private void OnDisable()
    {
        for (int a = 0; a < enemies.Count; a++)
        {
            enemies[a].isTrigger = true;
        }
        enemies.Clear();
    }
}
