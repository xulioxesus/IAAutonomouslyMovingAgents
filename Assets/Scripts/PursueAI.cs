using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//=============================================================================
// Controla o comportamento dun axente autónomo que se move polo NavMesh
// Implementa o comportamento de steering Pursue (perseguir predicindo a posición futura do obxectivo)
//=============================================================================
public class PursueAI : MonoBehaviour
{
    NavMeshAgent agent; // Compoñente NavMeshAgent para movemento polo NavMesh
    public GameObject target; // Obxectivo ao que o axente persegue

    public int targetInRange = 5; // Distancia mínima á que o axente deixa de moverse (para evitar colisión)

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
    // Envía o axente a unha localización no NavMesh
    //=============================================================================
    void Seek(Vector3 location)
    {
        agent.SetDestination(location);
    }

    //=============================================================================
    // Persegue o obxectivo predicindo onde estará no futuro
    //=============================================================================
    void Pursue()
    {
        //o vector desde o axente ata o obxectivo
        Vector3 targetDir = target.transform.position - this.transform.position;

        //o ángulo entre a dirección frontal do axente e a dirección frontal do obxectivo
        float relativeHeading = Vector3.Angle(this.transform.forward, this.transform.TransformVector(target.transform.forward));

        //o ángulo entre a dirección frontal do axente e a posición do obxectivo
        float toTarget = Vector3.Angle(this.transform.forward, this.transform.TransformVector(targetDir));

        //se o axente está detrás e indo na mesma dirección ou o obxectivo se detivo entón só buscar
        if ((toTarget > 90 && relativeHeading < 20) || playerController.currentSpeed < 0.01f)
        {
            Seek(target.transform.position);
            return;
        }

        //calcular canto mirar cara adiante e engadilo á localización de busca
        float lookAhead = targetDir.magnitude / (agent.speed + playerController.currentSpeed);
        Seek(target.transform.position + target.transform.forward * lookAhead);
    }

    //=============================================================================
    // Determina se o obxectivo está preto dabondo para deter o movemento
    //=============================================================================
    bool TargetInRange()
    {
        // Se o obxectivo está a menos da distancia definida en targetInRange, considérao dentro do rango
        // e o axente deterase para evitar unha colisión ou sobrepasar o obxectivo
        if (Vector3.Distance(transform.position, target.transform.position) < targetInRange)
            return true;
        return false;
    }

    //=============================================================================
    // Actualiza o comportamento do axente cada frame, perseguindo ata alcanzar a distancia mínima
    //=============================================================================
    void Update()
    {
        if (TargetInRange())
            agent.ResetPath(); // Se o obxectivo está dentro do rango, detén o movemento
        else
            Pursue(); // Se o obxectivo está fóra do rango, persegue predicindo a súa posición futura
    }
}
