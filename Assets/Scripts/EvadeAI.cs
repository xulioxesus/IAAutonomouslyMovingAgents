using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//=============================================================================
// Controla o comportamento dun axente autónomo que se move polo NavMesh
// Implementa o comportamento de steering Evade (fuxir predicindo a posición futura do obxectivo)
//=============================================================================
public class EvadeAI : MonoBehaviour
{
    NavMeshAgent agent; // Compoñente NavMeshAgent para movemento polo NavMesh
    public GameObject target; // Obxectivo do que o axente foxe

    public int targetInRange = 50; // Distancia máxima para considerar o obxectivo unha ameaza e activar a evasión

    PlayerController playerController; // Referencia ao script PlayerController do obxectivo

    //=============================================================================
    // Inicializa as referencias aos compoñentes necesarios
    //=============================================================================
    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        playerController = target.GetComponent<PlayerController>();
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
    // Predí a localización futura do obxectivo e despois foxe dela
    //=============================================================================
    void Evade()
    {
        //o vector desde o axente ata o obxectivo
        Vector3 targetDir = target.transform.position - this.transform.position;
        //calcular canto mirar cara adiante baseándose nas velocidades do axente e do obxectivo
        float lookAhead = targetDir.magnitude / (agent.speed + playerController.currentSpeed);

        //igual que Pursue pero en lugar de Seek estamos fuxindo coa predicción
        Flee(target.transform.position + target.transform.forward * lookAhead);
    }


    //=============================================================================
    // Determina se o obxectivo está preto dabondo para ser considerado unha ameaza
    //=============================================================================
    bool TargetInRange()
    {
        // Se o obxectivo está a menos da distancia definida en targetInRange, considérao dentro do rango
        // e o axente activará o comportamento de evasión
        if (Vector3.Distance(transform.position, target.transform.position) < targetInRange)
            return true;
        return false;
    }

    //=============================================================================
    // Actualiza o comportamento do axente cada frame, evadindo se o obxectivo está en rango
    //=============================================================================
    void Update()
    {
        if (TargetInRange())
            Evade(); // Se o obxectivo está dentro do rango, evádeo predicindo a súa posición futura
        else
            agent.ResetPath(); // Se o obxectivo está fóra do rango, detén o movemento
    }
}
