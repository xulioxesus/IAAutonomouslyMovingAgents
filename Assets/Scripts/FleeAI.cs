using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//=============================================================================
// Controla o comportamento dun axente autónomo que se move polo NavMesh
// Implementa o comportamento de steering Flee (fuxir do obxectivo)
//=============================================================================
public class FleeAI : MonoBehaviour
{
    NavMeshAgent agent; // Compoñente NavMeshAgent para movemento polo NavMesh
    public GameObject target; // Obxectivo do que o axente foxe

    public int targetInRange = 50; // Distancia mínima para considerar o obxectivo dentro do rango e activar a fuga

    //=============================================================================
    // Inicializa as referencias aos compoñentes necesarios
    //=============================================================================
    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
    }

    //=============================================================================
    // Envía o axente na dirección oposta a unha localización no NavMesh
    //=============================================================================
    void Flee(Vector3 location)
    {
        //calcular o vector na dirección contraria á localización
        //este é 180 graos ao vector cara á localización
        Vector3 fleeVector = location - this.transform.position;

        //restar este vector da posición do axente e 
        //establecer isto como a nova localización no NavMesh
        agent.SetDestination(this.transform.position - fleeVector);
    }

    //=============================================================================
    // Determina o lonxe que está o obxectivo do axente
    //=============================================================================
    bool TargetInRange()
    {
        // Se o obxectivo está a menos da distancia definida en targetInRange, considérao dentro do rango
        // para que o axente active o comportamento de fuxir
        if (Vector3.Distance(transform.position, target.transform.position) < targetInRange)
            return true;
        return false;
    }

    //=============================================================================
    // Actualiza o comportamento do axente cada frame, afastándoo do obxectivo se está en rango
    //=============================================================================
    void Update()
    {
        if (TargetInRange())
            Flee(target.transform.position); // Se o obxectivo está dentro do rango, foxe del
        else
            agent.ResetPath(); // Se o obxectivo está fóra do rango, detén o movemento
    }
}
