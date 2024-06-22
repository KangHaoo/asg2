using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class healthBar : MonoBehaviour
{
    public Slider healthSlider;
    public Slider easeHealthSlide;
    public float maxHealth = 100f;
    public float health;
    private float lerpSpeed = 0.05f;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (healthSlider.value != health)
        {
            healthSlider.value = health;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            takeDamage(10);
        }

        if (healthSlider.value != easeHealthSlide.value)
        {
            easeHealthSlide.value = Mathf.Lerp(easeHealthSlide.value, health, lerpSpeed);
        }
    }

    void takeDamage(float damage)
    {
        health -= damage;
    }
}
