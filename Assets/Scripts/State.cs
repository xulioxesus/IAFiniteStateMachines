using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Clase base que representa un estado nunha máquina de estados finitos.
// Implementa o patrón State para controlar o comportamento dun NPC.
public class State {

    // Enumeración dos posibles estados do NPC
    public enum STATE {

        IDLE,       // Inactivo
        PATROL,     // Patrullando
        PURSUE,     // Perseguindo
        ATTACK,     // Atacando
        SLEEP,      // Durmindo
        RUNAWAY     // Fuxindo
    };

    // Enumeración dos eventos do ciclo de vida dun estado
    public enum EVENT {

        ENTER,      // Entrada ao estado
        UPDATE,     // Actualización do estado
        EXIT        // Saída do estado
    };

    public STATE name;                      // Nome do estado actual
    protected EVENT stage;                  // Fase actual do estado
    protected GameObject npc;               // Referencia ao GameObject do NPC
    protected Animator animator;                // Controlador de animacións do NPC
    protected Transform player;             // Referencia á transformada do xogador
    protected State nextState;              // Próximo estado ao que transicionar
    protected NavMeshAgent agent;           // Axente de navegación do NPC

    float visionDistance = 10.0f;                  // Distancia de visión do NPC
    float visionAngle = 30.0f;                 // Ángulo de visión do NPC
    float shootDistance = 7.0f;                 // Distancia de disparo do NPC

    //=========================================================================
    // Constructor que inicializa as referencias básicas do estado
    //=========================================================================
    public State(GameObject _npc, NavMeshAgent _agent, Animator _animator, Transform _player)
    {
        npc = _npc;                 // Asigna o GameObject do NPC
        agent = _agent;             // Asigna o axente de navegación
        animator = _animator;       // Asigna o controlador de animacións
        player = _player;           // Asigna a referencia ao xogador
        stage = EVENT.ENTER;        // Inicializa na fase de entrada
    }
    //=========================================================================
    // Métodos virtuais que representan o ciclo de vida do estado
    //=========================================================================
    public virtual void Enter() { stage = EVENT.UPDATE; }
    public virtual void Update() { stage = EVENT.UPDATE; }
    public virtual void Exit() { stage = EVENT.EXIT; }

    //=========================================================================
    // Procesa o estado actual segundo a súa fase
    // Retorna o estado actual ou o seguinte estado se se produce unha transición
    //=========================================================================
    public State Process()
    {
        if (stage == EVENT.ENTER) Enter();      // Executa o método de entrada se estamos na fase ENTER
        if (stage == EVENT.UPDATE) Update();    // Executa o método de actualización se estamos na fase UPDATE
        if (stage == EVENT.EXIT)                // Se estamos na fase EXIT...
        {
            Exit();                             // Executa o método de saída
            return nextState;                   // Retorna o seguinte estado para a transición
        }

        return this;                            // Retorna o estado actual se non hai transición
    }

    //=========================================================================
    // Comproba se o NPC pode ver ao xogador
    // Retorna true se o xogador está dentro da distancia e ángulo de visión
    //=========================================================================
    public bool CanSeePlayer()
    {
        // Calcula o vector dirección desde o NPC cara ao xogador
        Vector3 direction = player.position - npc.transform.position;
        // Calcula o ángulo entre a dirección ao xogador e a dirección frontal do NPC
        float angle = Vector3.Angle(direction, npc.transform.forward);

        // Comproba se o xogador está dentro da distancia E dentro do ángulo de visión
        if (direction.magnitude < visionDistance && angle < visionAngle)
        {
            return true;    // O NPC pode ver ao xogador
        }

        return false;       // O NPC non pode ver ao xogador
    }

    //=========================================================================
    // Comproba se o xogador está detrás do NPC
    // Retorna true se o xogador está prόximo e no ángulo traseiro
    //=========================================================================
    public bool IsPlayerBehind()
    {
        // Calcula o vector dirección desde o xogador cara ao NPC (inverso)
        Vector3 direction = npc.transform.position - player.position;
        // Calcula o ángulo entre esta dirección e a dirección frontal do NPC
        float angle = Vector3.Angle(direction, npc.transform.forward);
        // Comproba se o xogador está moi próximo E no cono traseiro do NPC
        if (direction.magnitude < 2.0f && angle < 30.0f) return true;
        return false;
    }

    //=========================================================================
    // Comproba se o NPC pode atacar ao xogador
    // Retorna true se o xogador está dentro da distancia de disparo
    //=========================================================================
    public bool CanAttackPlayer()
    {
        // Calcula o vector dirección desde o NPC cara ao xogador
        Vector3 direction = player.position - npc.transform.position;
        // Comproba se a distancia é menor que a distancia de disparo
        if (direction.magnitude < shootDistance)
        {
            return true;    // O NPC pode atacar ao xogador
        }

        return false;       // O xogador está fóra do alcance de ataque
    }
}

//=========================================================================
// Estado IDLE (Inactivo)
// O NPC permanece inactivo e pode transicionar a Patrol ou Pursue
//=========================================================================
public class Idle : State
{
    public Idle(GameObject _npc, NavMeshAgent _agent, Animator _animator, Transform _player)
        : base(_npc, _agent, _animator, _player)
    {
        name = STATE.IDLE;
    }

    //=========================================================================
    // Ao entrar no estado, activa a animación de inactividade
    //=========================================================================
    public override void Enter()
    {
        animator.SetTrigger("isIdle");  // Activa o trigger da animación de inactividade
        base.Enter();               // Chama ao método Enter da clase base
    }

    //=========================================================================
    // Comproba se debe perseguir ao xogador ou comezar a patrullar
    //=========================================================================
    public override void Update()
    {
        if (CanSeePlayer())         // Se o NPC ve ao xogador...
        {
            nextState = new Pursue(npc, agent, animator, player);  // Crea un estado de persecución
            stage = EVENT.EXIT;                                 // Marca para saír do estado actual
        }
        else if (Random.Range(0, 100) < 10)     // 10% de probabilidade de comezar a patrullar
        {
            nextState = new Patrol(npc, agent, animator, player);  // Crea un estado de patrulla
            stage = EVENT.EXIT;                                 // Marca para saír do estado actual
        }
    }

    //=========================================================================
    // Ao saír do estado, resetea o trigger da animación
    //=========================================================================
    public override void Exit()
    {
        animator.ResetTrigger("isIdle");    // Limpa o trigger da animación
        base.Exit();                    // Chama ao método Exit da clase base
    }
}

//=========================================================================
// Estado PATROL (Patrulla)
// O NPC móvese entre os checkpoints do entorno
//=========================================================================
public class Patrol : State
{
    int currentIndex = -1;      // Índice do checkpoint actual

    public Patrol(GameObject _npc, NavMeshAgent _agent, Animator _animator, Transform _player)
        : base(_npc, _agent, _animator, _player)
    {
        name = STATE.PATROL;
        agent.speed = 2.0f;         // Velocidade de patrulla
        agent.isStopped = false;

    }

    //=========================================================================
    // Ao entrar no estado, busca o checkpoint máis próximo e inicia a patrulla
    //=========================================================================
    public override void Enter()
    {
        float lastDistance = Mathf.Infinity;    // Inicializa coa distancia máxima posible

        // Itera sobre todos os checkpoints para atopar o máis próximo
        for (int i = 0; i < GameEnvironment.Singleton.Checkpoints.Count; ++i)
        {
            GameObject thisWP = GameEnvironment.Singleton.Checkpoints[i];       // Obtén o checkpoint actual
            float distance = Vector3.Distance(npc.transform.position, thisWP.transform.position);  // Calcula a distancia
            if (distance < lastDistance)        // Se é o máis próximo ata agora...
            {
                currentIndex = i - 1;           // Establece o índice (anterior para que o primeiro sexa este)
                lastDistance = distance;        // Actualiza a distancia mínima
            }
        }

        animator.SetTrigger("isWalking");   // Activa a animación de camiñar
        base.Enter();                   // Chama ao método Enter da clase base
    }

    //=========================================================================
    // Move o NPC entre os checkpoints e comproba se debe perseguir ou fuxir
    //=========================================================================
    public override void Update()
    {
        // Se chegou ao checkpoint actual (a menos de 1 unidade)...
        if (agent.remainingDistance < 1)
        {
            // Se chegou ao último checkpoint, volve ao primeiro
            if (currentIndex >= GameEnvironment.Singleton.Checkpoints.Count - 1)
            {
                currentIndex = 0;
            }
            else    // Se non, avanza ao seguinte checkpoint
            {
                currentIndex++;
            }

            // Establece o destino ao novo checkpoint
            agent.SetDestination(GameEnvironment.Singleton.Checkpoints[currentIndex].transform.position);
        }

        // Se ve ao xogador, cambia a modo persecución
        if (CanSeePlayer())
        {
            nextState = new Pursue(npc, agent, animator, player);
            stage = EVENT.EXIT;
        }
        // Se o xogador está detrás, foxe
        else if (IsPlayerBehind())
        {
            nextState = new RunAway(npc, agent, animator, player);
            stage = EVENT.EXIT;
        }
    }

    //=========================================================================
    // Ao saír do estado, resetea o trigger da animación
    //=========================================================================
    public override void Exit()
    {
        animator.ResetTrigger("isWalking");     // Limpa o trigger da animación de camiñar
        base.Exit();                        // Chama ao método Exit da clase base
    }
}

//=========================================================================
// Estado PURSUE (Perseguir)
// O NPC persegue ao xogador a maior velocidade
//=========================================================================
public class Pursue : State
{
    public Pursue(GameObject _npc, NavMeshAgent _agent, Animator _animator, Transform _player)
        : base(_npc, _agent, _animator, _player)
    {
        name = STATE.PURSUE;
        agent.speed = 5.0f;         // Velocidade de persecución
        agent.isStopped = false;
    }

    //=========================================================================
    // Ao entrar no estado, activa a animación de correr
    //=========================================================================
    public override void Enter()
    {
        animator.SetTrigger("isRunning");   // Activa o trigger da animación de correr
        base.Enter();                   // Chama ao método Enter da clase base
    }

    //=========================================================================
    // Persegue ao xogador e comproba se pode atacar ou perdeu de vista ao xogador
    //=========================================================================
    public override void Update()
    {
        // Actualiza constantemente o destino á posición do xogador
        agent.SetDestination(player.position);

        if (agent.hasPath)      // Se existe un camiño válido...
        {
            // Se o xogador está ao alcance de ataque, cambia a modo ataque
            if (CanAttackPlayer())
            {
                nextState = new Attack(npc, agent, animator, player);
                stage = EVENT.EXIT;
            }
            // Se perdeu de vista ao xogador, volve a patrullar
            else if (!CanSeePlayer())
            {
                nextState = new Patrol(npc, agent, animator, player);
                stage = EVENT.EXIT;
            }
        }
    }

    //=========================================================================
    // Ao saír do estado, resetea o trigger da animación
    //=========================================================================
    public override void Exit()
    {
        animator.ResetTrigger("isRunning"); // Limpa o trigger da animación de correr
        base.Exit();                    // Chama ao método Exit da clase base
    }
}

//=========================================================================
// Estado ATTACK (Atacar)
// O NPC detense e ataca ao xogador, rotando para apuntarlle
//=========================================================================
public class Attack : State
{
    float rotationSpeed = 2.0f;    // Velocidade de rotación cara ao xogador
    AudioSource shoot;              // Fonte de audio para o disparo

    public Attack(GameObject _npc, NavMeshAgent _agent, Animator _animator, Transform _player)
        : base(_npc, _agent, _animator, _player)
    {
        name = STATE.ATTACK;
        shoot = _npc.GetComponent<AudioSource>();
    }

    //=========================================================================
    // Ao entrar no estado, detén o NPC e comeza a disparar
    //=========================================================================
    public override void Enter()
    {
        animator.SetTrigger("isShooting");  // Activa a animación de disparar
        agent.isStopped = true;         // Detén o movemento do NPC
        shoot.Play();                   // Reproduce o son de disparo
        base.Enter();                   // Chama ao método Enter da clase base
    }

    //=========================================================================
    // Rota cara ao xogador e detén o ataque se xa non está ao alcance
    //=========================================================================
    public override void Update()
    {
        // Calcula o vector dirección cara ao xogador
        Vector3 direction = player.position - npc.transform.position;
        float angle = Vector3.Angle(direction, npc.transform.forward);
        direction.y = 0.0f;     // Elimina o compoñente Y para evitar inclinacións

        // Rota suavemente o NPC cara ao xogador usando interpolación esférica
        npc.transform.rotation =
            Quaternion.Slerp(npc.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotationSpeed);

        // Se o xogador saíu do alcance de ataque...
        if (!CanAttackPlayer())
        {
            nextState = new Idle(npc, agent, animator, player);     // Volve ao estado inactivo
            shoot.Stop();                                       // Detén o son de disparo
            stage = EVENT.EXIT;                                 // Marca para saír do estado
        }
    }

    //=========================================================================
    // Ao saír do estado, resetea o trigger da animación
    //=========================================================================
    public override void Exit()
    {
        animator.ResetTrigger("isShooting");    // Limpa o trigger da animación de disparar
        base.Exit();                        // Chama ao método Exit da clase base
    }
}

//=========================================================================
// Estado RUNAWAY (Fuxir)
// O NPC foxe cara a unha localización segura cando o xogador está detrás del
//=========================================================================
public class RunAway : State
{

    GameObject safeLocation;        // Localización segura á que fuxir

    public RunAway(GameObject _npc, NavMeshAgent _agent, Animator _animator, Transform _player)
        : base(_npc, _agent, _animator, _player)
    {
        name = STATE.RUNAWAY;
        safeLocation = GameObject.FindGameObjectWithTag("Safe");
    }

    //=========================================================================
    // Ao entrar no estado, establece o destino á localización segura
    //=========================================================================
    public override void Enter()
    {
        animator.SetTrigger("isRunning");                           // Activa a animación de correr
        agent.isStopped = false;                                // Reactiva o movemento do NPC
        agent.speed = 6;                                        // Establece velocidade rápida de fuxida
        agent.SetDestination(safeLocation.transform.position);  // Establece a localización segura como destino
        base.Enter();                                           // Chama ao método Enter da clase base
    }

    //=========================================================================
    // Comproba se chegou á localización segura e transiciona a Idle
    //=========================================================================
    public override void Update()
    {
        // Se chegou á localización segura (a menos de 1 unidade)...
        if (agent.remainingDistance < 1.0f)
        {
            nextState = new Idle(npc, agent, animator, player);     // Cambia ao estado inactivo
            stage = EVENT.EXIT;                                 // Marca para saír do estado
        }
    }

    //=========================================================================
    // Ao saír do estado, resetea o trigger da animación
    //=========================================================================
    public override void Exit()
    {
        animator.ResetTrigger("isRunning");     // Limpa o trigger da animación de correr
        base.Exit();                        // Chama ao método Exit da clase base
    }
}