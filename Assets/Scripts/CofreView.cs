using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CofreView : MonoBehaviour
{
    private Animator _animator;
    private Animator _gemaAnimator;
    [SerializeField] private GameObject _gema;
    private bool _opened = false;

    // Chest waipoint 
    [SerializeField] public Transform _waypoint;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _gemaAnimator = _gema.GetComponent<Animator>(); 
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Human")
        {
            _animator.Play("Open");
            if(_opened == false)
                _gemaAnimator.Play("Show");
            _opened = true;
        }
    }

}
