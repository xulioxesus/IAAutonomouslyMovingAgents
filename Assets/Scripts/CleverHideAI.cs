using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//=============================================================================
// Controla o comportamento dun axente autónomo que se move polo NavMesh
// Implementa o comportamento de steering Hide (agocharse detrás dun obxecto para evitar o obxectivo)
//=============================================================================
public class CleverHideAI : MonoBehaviour
{
    NavMeshAgent agent; // Compoñente NavMeshAgent para movemento polo NavMesh
    public GameObject target; // Obxectivo do que o axente se agocha

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
    // Busca un lugar de agocho pero determina onde debe estar o axente
    // Baseándose no límite do obxecto determinado por un box collider
    //=============================================================================
    void CleverHide()
    {
        float dist = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;
        Vector3 chosenDir = Vector3.zero;
        GameObject chosenGO = World.Instance.GetHidingSpots()[0];

        // Mesma lóxica que Hide() para atopar o lugar de agocho máis próximo
        for (int i = 0; i < World.Instance.GetHidingSpots().Length; i++)
        {
            Vector3 hideDir = World.Instance.GetHidingSpots()[i].transform.position - target.transform.position;
            Vector3 hidePos = World.Instance.GetHidingSpots()[i].transform.position + hideDir.normalized * 100;

            if (Vector3.Distance(this.transform.position, hidePos) < dist)
            {
                chosenSpot = hidePos;
                chosenDir = hideDir;
                chosenGO = World.Instance.GetHidingSpots()[i];
                dist = Vector3.Distance(this.transform.position, hidePos);
            }
        }

        // Obter o collider do lugar de agocho elixido
        Collider hideCol = chosenGO.GetComponent<Collider>();
        // Calcular un raio para golpear o collider do lugar de agocho desde o lado oposto a onde
        // Está localizado o obxectivo
        Ray backRay = new Ray(chosenSpot, -chosenDir.normalized);
        RaycastHit info;
        float distance = 250.0f;
        // Realizar un raycast para atopar o punto próximo ao obxecto
        hideCol.Raycast(backRay, out info, distance);

        // Ir e colocarse na parte de atrás do obxecto no punto de impacto do raio
        Seek(info.point + chosenDir.normalized);

    }

    //=============================================================================
    // Actualiza o comportamento do axente cada frame, buscando onde agocharse do obxectivo
    //=============================================================================
    void Update()
    {
        CleverHide();
    }
}
