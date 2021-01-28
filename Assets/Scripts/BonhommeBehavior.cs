using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BonhommeBehavior : MonoBehaviour
{
    public GameObject home_sweet_home;
    public GameObject my_taff;
    public GameObject home_sweet_home2;
    public GameObject my_taff2;
    private bool to_taff = false;
    public Vector3 init_position; 
    public VoronoiDemo Vorono;
    NavMeshAgent agent;
    public int incr = 0;
    private bool to_home = false;
    Rigidbody rb;
    private bool to_rest = false;
    private bool arrived = false;
    private GameObject my_rest;
    private CapsuleCollider mainCollider;
    public LayerMask m_LayerMaskH;
    public LayerMask m_LayerMaskR;
    private bool Imhuman = false;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        mainCollider = this.GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        //GetComponent<NavMeshAgent>().Warp(transform.position);
/*        if (incr < 3 || incr == 4)
        {
            transform.position = init_position;
            incr += 1;
            Debug.Log("Coucou");
            return;
        }*/
        //Debug.Log(transform.position);
        if (Vorono.to_taff && !to_taff)
        {
            to_taff = true;
            to_home = false;
            agent.isStopped = false;
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(0).gameObject.SetActive(true);
            GetComponent<Collider>().enabled = true;
            //transform.position = home_sweet_home.transform.position + new Vector3(0f,0.03f, 0f);
            //transform.position = new Vector3(transform.position.x, -50, transform.position.z);
            agent.SetDestination(my_taff.transform.position);
            if (incr == 0)
            {
                incr++;
            }
            else {
                my_taff2.transform.GetChild(0).GetComponent<Light>().intensity -= 0.02f;
            }
        }
        if (!Vorono.to_taff && !to_home && !to_rest)
        {
            if (Random.Range(0, 100) <= 10)
            {
                to_rest = true;
                to_taff = false;
                agent.isStopped = false;
                transform.GetChild(1).gameObject.SetActive(true);
                transform.GetChild(0).gameObject.SetActive(false);
                GetComponent<Collider>().enabled = true;
                home_sweet_home2.transform.GetChild(0).GetComponent<Light>().intensity -= 0.02f;
                //transform.position -= new Vector3(0f, 5f, 0f);
                int rand = Random.Range(0, Vorono.list_restau2.Count);
                agent.SetDestination(Vorono.list_restau[rand].transform.position);
                my_rest = Vorono.list_restau[rand];
            }
            else
            {
                to_home = true;
                to_taff = false;
                agent.isStopped = false;
                transform.GetChild(1).gameObject.SetActive(true);
                transform.GetChild(0).gameObject.SetActive(false);
                GetComponent<Collider>().enabled = true;
                my_taff2.transform.GetChild(0).GetComponent<Light>().intensity -= 0.02f;
                //transform.position -= new Vector3(0f, 5f, 0f);
                agent.SetDestination(home_sweet_home.transform.position);
            }

        }
        if (to_taff && (my_taff.transform.position - transform.position).magnitude < 0.1) {
            //rb.constraints = RigidbodyConstraints.None;
            agent.isStopped = true;
            agent.ResetPath();
            //rb.constraints = RigidbodyConstraints.FreezePositionY| RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(0).gameObject.SetActive(false);
            GetComponent<Collider>().enabled = false;
            my_taff2.transform.GetChild(0).GetComponent<Light>().intensity += 0.02f;
        }
        if (to_home && (home_sweet_home.transform.position - transform.position).magnitude < 0.07)
        {
            //rb.constraints = RigidbodyConstraints.None;
            agent.isStopped = true;
            agent.ResetPath();
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(0).gameObject.SetActive(false);
            GetComponent<Collider>().enabled = false;
            home_sweet_home2.transform.GetChild(0).GetComponent<Light>().intensity += 0.02f;
            //rb.constraints = RigidbodyConstraints.FreezePositionY;
        }
        if (arrived) {
            if (Random.Range(0, 10000) < 2) {
                arrived = false;
                to_home = true;
                to_taff = false;
                agent.isStopped = false;
                transform.GetChild(1).gameObject.SetActive(true);
                transform.GetChild(0).gameObject.SetActive(false);
                GetComponent<Collider>().enabled = true;
                //home_sweet_home2.transform.GetChild(0).GetComponent<Light>().intensity -= 0.5f;
                //transform.position -= new Vector3(0f, 5f, 0f);
                agent.SetDestination(home_sweet_home.transform.position);
            }
        }
        if (to_rest && (my_rest.transform.position - transform.position).magnitude < 0.07)
        {
            //rb.constraints = RigidbodyConstraints.None;
            agent.isStopped = true;
            agent.ResetPath();
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(0).gameObject.SetActive(false);
            GetComponent<Collider>().enabled = false;
            arrived = true;
            //home_sweet_home2.transform.GetChild(0).GetComponent<Light>().intensity += 0.5f;
            //rb.constraints = RigidbodyConstraints.FreezePositionY;
        }
        Collider[] hitCollidersH = Physics.OverlapBox(gameObject.transform.position, new Vector3(mainCollider.radius / 2, mainCollider.radius / 2, mainCollider.radius / 2), Quaternion.identity, m_LayerMaskH);
        Collider[] hitCollidersR = Physics.OverlapBox(gameObject.transform.position, new Vector3(mainCollider.radius / 2, mainCollider.radius / 2, mainCollider.radius / 2), Quaternion.identity, m_LayerMaskR);
        if ((hitCollidersH.Length > 1 && hitCollidersR.Length > 1 )) {
            return;
        }
        if (Imhuman && hitCollidersH.Length>1) {
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(0).gameObject.SetActive(true);
        }
        if (!Imhuman && hitCollidersR.Length > 1) {
            transform.GetChild(1).gameObject.SetActive(true);
            transform.GetChild(0).gameObject.SetActive(false);
        }
/*        if (to_home) 
        {
            Vector3 to_pos = home_sweet_home.transform.position - transform.position;
            to_pos.Normalize();
            to_pos = to_pos * maxSpeed;
            rb.AddForce(to_pos);    
        }
        if (to_taff) 
        {
            Vector3 to_pos = my_taff.transform.position - transform.position;
            to_pos.Normalize();
            to_pos = to_pos * maxSpeed;
            rb.AddForce(to_pos);
        }
        if (rb.velocity.magnitude > maxSpeed) { 
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }*/
    }
}
