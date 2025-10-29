using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//=============================================================================
// Controla o comportamento dun axente autónomo que se move polo NavMesh
// Implementa varios comportamentos de steering: Seek, Flee, Pursue, Evade, Wander e Hide
//=============================================================================
public class Bot : MonoBehaviour
{
    NavMeshAgent agent; //compoñente NavMeshAgent para movemento polo NavMesh
    public GameObject target; //obxectivo ao que o axente reacciona
    PlayerController playerController; //referencia ao script PlayerController do obxectivo

    //=============================================================================
    // inicializa as referencias aos compoñentes necesarios
    //=============================================================================
    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        playerController = target.GetComponent<PlayerController>();
    }

    //=============================================================================
    // envía o axente a unha localización no NavMesh
    //=============================================================================
    void Seek(Vector3 location)
    {
        agent.SetDestination(location);
    }

    //=============================================================================
    // envía o axente na dirección oposta a unha localización no NavMesh
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
    // persegue o obxectivo predicindo onde estará no futuro
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
    // predí a localización futura do obxectivo e despois afástase dela
    //=============================================================================
    void Evade()
    {
        Vector3 targetDir = target.transform.position - this.transform.position;
        float lookAhead = targetDir.magnitude / (agent.speed + playerController.currentSpeed);

        //igual que Pursue pero en lugar de Seek estamos fuxindo
        Flee(target.transform.position + target.transform.forward * lookAhead);
    }


    //=============================================================================
    // vaga polo mapa de forma aleatoria
    //=============================================================================
    Vector3 wanderTarget = Vector3.zero; //obxectivo de vagabundeo, almacenado entre frames
    void Wander()
    {
        float wanderRadius = 10; //radio do círculo de vagabundeo
        float wanderDistance = 10; //distancia do círculo á fronte do axente
        float wanderJitter = 1; //cantidade de aleatoriedade aplicada cada frame

        //determinar unha localización nun círculo 
        wanderTarget += new Vector3(Random.Range(-1.0f, 1.0f) * wanderJitter,
                                        0,
                                        Random.Range(-1.0f, 1.0f) * wanderJitter);
        wanderTarget.Normalize();
        //proxectar o punto ao radio do círculo
        wanderTarget *= wanderRadius;

        //mover o círculo cara adiante do axente á distancia de vagabundeo
        Vector3 targetLocal = wanderTarget + new Vector3(0, 0, wanderDistance);
        //calcular a localización mundial do punto no círculo
        Vector3 targetWorld = this.gameObject.transform.InverseTransformVector(targetLocal);

        Seek(targetWorld);
    }

    //=============================================================================
    // atopa un obxecto detrás do cal agocharse
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
    // busca un lugar de agocho pero determina onde debe estar o axente
    // baseándose no límite do obxecto determinado por un box collider
    //=============================================================================
    void CleverHide()
    {
        float dist = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;
        Vector3 chosenDir = Vector3.zero;
        GameObject chosenGO = World.Instance.GetHidingSpots()[0];

        //mesma lóxica que Hide() para atopar o lugar de agocho máis próximo
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

        //obter o collider do lugar de agocho elixido
        Collider hideCol = chosenGO.GetComponent<Collider>();
        //calcular un raio para golpear o collider do lugar de agocho desde o lado oposto a onde
        //está localizado o obxectivo
        Ray backRay = new Ray(chosenSpot, -chosenDir.normalized);
        RaycastHit info;
        float distance = 250.0f;
        //realizar un raycast para atopar o punto prox ao obxecto
        hideCol.Raycast(backRay, out info, distance);

        //ir e colocarse na parte de atrás do obxecto no punto de impacto do raio
        Seek(info.point + chosenDir.normalized);

    }

    //=============================================================================
    // pode o axente ver o obxectivo desde onde está
    // baseándose noutros obxectos do xogo no mundo
    //=============================================================================
    bool CanSeeTarget()
    {
        RaycastHit raycastInfo;
        //calcular un raio ao obxectivo desde o axente
        Vector3 rayToTarget = target.transform.position - this.transform.position;
        //realizar un raycast para determinar se hai algo entre o axente e o obxectivo
        if (Physics.Raycast(this.transform.position, rayToTarget, out raycastInfo))
        {
            //o raio golpeará o obxectivo se non hai outros colliders no medio
            if (raycastInfo.transform.gameObject.tag == "cop")
                return true;
        }
        return false;
    }

    //=============================================================================
    // pode o obxectivo potencialmente ver o axente
    //=============================================================================
    bool TargetCanSeeMe()
    {
        //calcular unha dirección frontal para o obxectivo e
        //o ángulo entre iso e a dirección ao axente
        Vector3 toAgent = this.transform.position - target.transform.position;
        float lookingAngle = Vector3.Angle(target.transform.forward, toAgent);

        //se o obxectivo está mirando ao axente dentro dun rango de 60 graos
        //asumimos que o obxectivo pode ver o axente
        if (lookingAngle < 60)
            return true;
        return false;
    }

    //=============================================================================
    // proporciona un booleano de tempo de espera para permitir aos axentes tempo para chegar a unha
    // localización do NavMesh antes de que se calcule potencialmente outra localización
    //=============================================================================
    bool coolDown = false; //indica se o axente está en período de espera
    void BehaviourCoolDown()
    {
        coolDown = false;
    }

    //=============================================================================
    // determina o lonxe que está o obxectivo do axente
    //=============================================================================
    bool TargetInRange()
    {
        //se o obxectivo está a menos de 10 unidades do axente entón considérao dentro do rango para
        //afectar o comportamento do axente
        if (Vector3.Distance(this.transform.position, target.transform.position) < 10)
            return true;
        return false;
    }

    //=============================================================================
    // actualiza o comportamento do axente cada frame baseándose na posición e visibilidade do obxectivo
    //=============================================================================
    void Update()
    {
        //se non está agardando a que remate un tempo de espera
        if (!coolDown)
        {
            //se o obxectivo está considerado fóra de rango - é dicir, non é unha ameaza
            if (!TargetInRange())
            {
                Wander();
            }
            else if (CanSeeTarget() && TargetCanSeeMe()) //se non hai nada entre o axente e o obxectivo
            {                                              //e o obxectivo está mirando ao axente
                CleverHide();                               //ir agocharse detrás de algo
                coolDown = true;
                Invoke("BehaviourCoolDown", 5);             //continuar agochado durante 5 segundos
            }
            else
                Pursue();   //doutro xeito perseguir o obxectivo
        }
    }
}
