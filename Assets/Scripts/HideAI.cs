using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//=============================================================================
// Controla o comportamento dun axente autónomo que se move polo NavMesh
// Implementa o comportamento de steering Hide (agocharse detrás dun obxecto para evitar o obxectivo)
//=============================================================================
public class HideAI : MonoBehaviour
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
    // Atopa o mellor lugar para agocharse, buscando o máis próximo ao axente
    // no lado oposto do obxecto de agocho respecto ao obxectivo
    //=============================================================================
    void Hide()
    {
        //inicializar variables para lembrar o lugar de agocho máis próximo ao axente
        float dist = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;

        //revisar todos os lugares de agocho potenciais
        for (int i = 0; i < World.Instance.GetHidingSpots().Length; i++)
        {
            //determinar a dirección do lugar de agocho desde o obxectivo
            Vector3 hideDir = World.Instance.GetHidingSpots()[i].transform.position - target.transform.position;

            //engadir esta dirección á posición do lugar de agocho para atopar unha localización no
            //lado oposto do lugar de agocho a onde está o obxectivo
            Vector3 hidePos = World.Instance.GetHidingSpots()[i].transform.position + hideDir.normalized * 10;

            //se este lugar de agocho está máis próximo ao axente que a distancia ao último
            if (Vector3.Distance(this.transform.position, hidePos) < dist)
            {
                //lembralo
                chosenSpot = hidePos;
                dist = Vector3.Distance(this.transform.position, hidePos);
            }
        }

        //ir á localización de agocho
        Seek(chosenSpot);

    }

    //=============================================================================
    // Actualiza o comportamento do axente cada frame, buscando onde agocharse do obxectivo
    //=============================================================================
    void Update()
    {
        Hide();
    }
}
