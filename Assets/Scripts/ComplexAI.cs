using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//=============================================================================
// Controla o comportamento dun axente autónomo que se move polo NavMesh
// No Update: vaga se o obxectivo está fóra de rango; se se ven mutuamente,
// agocha detrás dun obxecto (CleverHide) durante 5 segundos; doutro xeito, persegue (Pursue)
//=============================================================================
public class ComplexAI : MonoBehaviour
{
    NavMeshAgent agent; // Compoñente NavMeshAgent para movemento polo NavMesh
    public GameObject target; // Obxectivo ao que o axente reacciona
    PlayerController playerController; // Referencia ao script PlayerController do obxectivo
    public int targetInRange = 10; // Distancia para considerar o obxectivo unha ameaza
    public float coolDownTime = 5.0f; // Tempo que o axente permanece nun comportamento antes de reavaliar

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
    // Envía o axente na dirección oposta a unha localización no NavMesh
    //=============================================================================
    void Flee(Vector3 location)
    {
        // Calcular o vector na dirección contraria á localización
        // Este é 180 graos ao vector cara á localización
        Vector3 fleeVector = location - this.transform.position;

        // Restar este vector da posición do axente e 
        // Establecer isto como a nova localización no NavMesh
        agent.SetDestination(this.transform.position - fleeVector);
    }

    //=============================================================================
    // Persegue o obxectivo predicindo onde estará no futuro
    //=============================================================================
    void Pursue()
    {
        // O vector desde o axente ata o obxectivo
        Vector3 targetDir = target.transform.position - this.transform.position;

        // O ángulo entre a dirección frontal do axente e a dirección frontal do obxectivo
        float relativeHeading = Vector3.Angle(this.transform.forward, this.transform.TransformVector(target.transform.forward));

        // O ángulo entre a dirección frontal do axente e a posición do obxectivo
        float toTarget = Vector3.Angle(this.transform.forward, this.transform.TransformVector(targetDir));

        // Se o axente está detrás e indo na mesma dirección ou o obxectivo se detivo entón só buscar
        if ((toTarget > 90 && relativeHeading < 20) || playerController.currentSpeed < 0.01f)
        {
            Seek(target.transform.position);
            return;
        }

        // Calcular canto mirar cara adiante e engadilo á localización de busca
        float lookAhead = targetDir.magnitude / (agent.speed + playerController.currentSpeed);
        Seek(target.transform.position + target.transform.forward * lookAhead);
    }


    //=============================================================================
    // Predí a localización futura do obxectivo e despois afástase dela
    //=============================================================================
    void Evade()
    {
        Vector3 targetDir = target.transform.position - this.transform.position;
        float lookAhead = targetDir.magnitude / (agent.speed + playerController.currentSpeed);

        // Igual que Pursue pero en lugar de Seek estamos fuxindo
        Flee(target.transform.position + target.transform.forward * lookAhead);
    }


    //=============================================================================
    // Vaga polo mapa de forma aleatoria
    //=============================================================================
    Vector3 wanderTarget = Vector3.zero; // Obxectivo de vagabundeo, almacenado entre frames
    void Wander()
    {
        float wanderRadius = 10; // Radio do círculo de vagabundeo
        float wanderDistance = 10; // Distancia do círculo á fronte do axente
        float wanderJitter = 1; // Cantidade de aleatoriedade aplicada cada frame

        // Determinar unha localización nun círculo 
        wanderTarget += new Vector3(Random.Range(-1.0f, 1.0f) * wanderJitter,
                                        0,
                                        Random.Range(-1.0f, 1.0f) * wanderJitter);
        wanderTarget.Normalize();
        // Proxectar o punto ao radio do círculo
        wanderTarget *= wanderRadius;

        // Mover o círculo cara adiante do axente á distancia de vagabundeo
        Vector3 targetLocal = wanderTarget + new Vector3(0, 0, wanderDistance);
        // Calcular a localización mundial do punto no círculo
        Vector3 targetWorld = this.gameObject.transform.InverseTransformVector(targetLocal);

        Seek(targetWorld);
    }

    //=============================================================================
    // Atopa un obxecto detrás do cal agocharse
    //=============================================================================
    void Hide()
    {
        // Inicializar variables para lembrar o lugar de agocho máis próximo ao axente
        float dist = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;

        // Revisar todos os lugares de agocho potenciais
        for (int i = 0; i < World.Instance.GetHidingSpots().Length; i++)
        {
            // Determinar a dirección do lugar de agocho desde o obxectivo
            Vector3 hideDir = World.Instance.GetHidingSpots()[i].transform.position - target.transform.position;

            // Engadir esta dirección á posición do lugar de agocho para atopar unha localización no
            // Lado oposto do lugar de agocho a onde está o obxectivo
            Vector3 hidePos = World.Instance.GetHidingSpots()[i].transform.position + hideDir.normalized * 10;

            // Se este lugar de agocho está máis próximo ao axente que a distancia ao último
            if (Vector3.Distance(this.transform.position, hidePos) < dist)
            {
                // Lembralo
                chosenSpot = hidePos;
                dist = Vector3.Distance(this.transform.position, hidePos);
            }
        }

        // Ir á localización de agocho
        Seek(chosenSpot);

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
    // Pode o axente ver o obxectivo desde onde está
    // Baseándose noutros obxectos do xogo no mundo
    //=============================================================================
    bool CanSeeTarget()
    {
        RaycastHit raycastInfo;
        // Calcular un raio ao obxectivo desde o axente
        Vector3 rayToTarget = target.transform.position - this.transform.position;
        // Realizar un raycast para determinar se hai algo entre o axente e o obxectivo
        if (Physics.Raycast(this.transform.position, rayToTarget, out raycastInfo))
        {
            // O raio golpeará o obxectivo se non hai outros colliders no medio
            if (raycastInfo.transform.gameObject.tag == "cop")
                return true;
        }
        return false;
    }

    //=============================================================================
    // Pode o obxectivo potencialmente ver o axente
    //=============================================================================
    bool TargetCanSeeMe()
    {
        // Calcular unha dirección frontal para o obxectivo e
        // O ángulo entre iso e a dirección ao axente
        Vector3 toAgent = this.transform.position - target.transform.position;
        float lookingAngle = Vector3.Angle(target.transform.forward, toAgent);

        // Se o obxectivo está mirando ao axente dentro dun rango de 60 graos
        // Asumimos que o obxectivo pode ver o axente
        if (lookingAngle < 60)
            return true;
        return false;
    }

    //=============================================================================
    // Proporciona un booleano de tempo de espera para permitir aos axentes tempo para chegar a unha
    // Localización do NavMesh antes de que se calcule potencialmente outra localización
    //=============================================================================
    bool coolDown = false; // Indica se o axente está en período de espera
    void BehaviourCoolDown()
    {
        coolDown = false;
    }

    //=============================================================================
    // Determina o lonxe que está o obxectivo do axente
    //=============================================================================
    bool TargetInRange()
    {
        // Se o obxectivo está a menos da distancia definida en targetInRange entón considérao dentro do rango para
        // Afectar o comportamento do axente
        if (Vector3.Distance(this.transform.position, target.transform.position) < targetInRange)
            return true;
        return false;
    }

    //=============================================================================
    // Actualiza o comportamento do axente cada frame baseándose na posición e visibilidade do obxectivo
    //=============================================================================
    void Update()
    {
        // Se non está agardando a que remate un tempo de espera
        if (!coolDown)
        {
            // Se o obxectivo está considerado fóra de rango - é dicir, non é unha ameaza
            if (!TargetInRange())
            {
                Wander();
            }
            else if (CanSeeTarget() && TargetCanSeeMe()) // Se non hai nada entre o axente e o obxectivo
            {                                              // E o obxectivo está mirando ao axente
                CleverHide();                               // Ir agocharse detrás de algo
                coolDown = true;
                Invoke("BehaviourCoolDown", coolDownTime);             // Continuar agochado durante coolDownTime segundos
            }
            else
                Pursue();   // Doutro xeito perseguir o obxectivo
        }
    }
}
