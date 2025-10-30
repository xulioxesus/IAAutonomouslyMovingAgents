using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//=============================================================================
// Controla o comportamento dun axente autónomo que se move polo NavMesh
// Implementa o comportamento de steering Wander (deambular aleatoriamente)
//=============================================================================
public class WanderAI : MonoBehaviour
{
    NavMeshAgent agent; // Compoñente NavMeshAgent para movemento polo NavMesh
    Vector3 wanderTarget = Vector3.zero; // Punto de deambulación no círculo, almacenado entre frames

    //=============================================================================
    // Inicializa as referencias aos compoñentes necesarios
    //=============================================================================
    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
    }

    //=============================================================================
    // Envía o axente a unha localización no NavMesh
    //=============================================================================
    void Seek(Vector3 location)
    {
        agent.SetDestination(location);
    }

    //=============================================================================
    // Deambula polo mapa de forma aleatoria usando o algoritmo de círculo de deambulación
    //=============================================================================
    void Wander()
    {
        float wanderRadius = 10; // Radio do círculo de deambulación
        float wanderDistance = 10; // Distancia do círculo á fronte do axente
        float wanderJitter = 1; // Cantidade de aleatoriedade aplicada cada frame

        //determinar unha localización nun círculo 
        wanderTarget += new Vector3(Random.Range(-1.0f, 1.0f) * wanderJitter,
                                        0,
                                        Random.Range(-1.0f, 1.0f) * wanderJitter);
        wanderTarget.Normalize();
        //proxectar o punto ao radio do círculo
        wanderTarget *= wanderRadius;

        //mover o círculo cara adiante do axente á distancia de deambulación
        Vector3 targetLocal = wanderTarget + new Vector3(0, 0, wanderDistance);
        //calcular a localización mundial do punto no círculo
        Vector3 targetWorld = this.gameObject.transform.InverseTransformVector(targetLocal);

        Seek(targetWorld);
    }

    //=============================================================================
    // Actualiza o comportamento do axente cada frame, deambulando aleatoriamente polo mapa
    //=============================================================================
    void Update()
    {
        Wander();
    }
}
