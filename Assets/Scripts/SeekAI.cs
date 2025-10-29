using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//=============================================================================
// Controla o comportamento dun axente autónomo que se move polo NavMesh
// Implementa o comportamento de steering Seek (perseguir o obxectivo)
//=============================================================================
public class SeekAI : MonoBehaviour
{
    NavMeshAgent agent; // Compoñente NavMeshAgent para movemento polo NavMesh
    public GameObject target; // Obxectivo ao que o axente persegue

    public int targetInRange = 3; // Distancia mínima para considerar o obxectivo dentro do rango

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
    // Determina o lonxe que está o obxectivo do axente
    //=============================================================================
    bool TargetInRange()
    {
        // Se o obxectivo está a menos da distancia definida en targetInRange, considérao dentro do rango
        // para afectar o comportamento do axente
        if (Vector3.Distance(transform.position, target.transform.position) < targetInRange)
            return true;
        return false;
    }

    //=============================================================================
    // Actualiza o comportamento do axente cada frame, movéndoo cara á posición do obxectivo
    //=============================================================================
    void Update()
    {
        if (TargetInRange())
            agent.ResetPath(); // Se o obxectivo está dentro do rango, detén o movemento
        else
            Seek(target.transform.position);
    }
}
