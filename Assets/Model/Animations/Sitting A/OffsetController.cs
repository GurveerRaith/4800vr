using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeAnimationOffset : MonoBehaviour
{
    [SerializeField] private string cycleOffsetParam = "Offset";
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        float randomOffset = Random.Range(0f, 1f);

        animator.SetFloat(cycleOffsetParam, randomOffset);

        animator.Update(0f);
    }
}