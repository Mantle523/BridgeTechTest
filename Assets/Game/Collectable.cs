using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Collectable : MonoBehaviour, IGridObject
{
    [SerializeField] GameObject capsule;
    [SerializeField] GameObject sphere;

    public UnityEvent<Vector2Int, bool> OnCollect;

    bool state; //true == capsule, false == sphere    

    public bool GetState()
    {
        return state;
    }

    public void SetState(bool newState)
    {
        capsule.SetActive(newState);
        sphere.SetActive(!newState);

        state = newState;
    }

    Vector2Int _gridPosition;
    public Vector2Int gridPosition { get => _gridPosition; set => _gridPosition = value; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            OnCollect.Invoke(gridPosition, state);
            Destroy(gameObject);
        }        
    }
}
