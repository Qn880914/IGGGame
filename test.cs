using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    [SerializeField] private float aa = 1.0000f;

    private static System.Func<int, Player> analyzeCustomDepend;

    [SerializeField] private ParticleSystem m_ParticleSystem;

    Dictionary<string, Player> m_Players = new Dictionary<string, Player>();
    // Start is called before the first frame update
    void Start()
    {
        /*m_Players.TryGetValue("1", out Player player);
        int k = 10;
        player = new Player { name = "mm"};
        player = analyzeCustomDepend?.Invoke(2);
        int i = 0;


        m_Players.Add("0", new Player());
        m_Players.Add("1", new Player());
        m_Players.Add("2", new Player());
        m_Players.Add("3", new Player());
        m_Players.Add("4", new Player());*/
        int k = 0;


        //m_EffectGameObject = (GameObject)Resources.Load("e_con3005_fx");
        //m_EffectGameObject = (GameObject)Resources.Load("ParticleSystem"); 
    }

    private GameObject m_EffectGameObject;
    private List<GameObject> m_EffectList = new List<GameObject>();

    // Update is called once per frame
    void Update()
    {
        //UnityEngine.Debug.Log(aa.ToString("N2"));


        m_Players.TryGetValue("1", out Player player);


        /*for(int i = 0; i < 100; ++ i)
        {
            GameObject go = Resources.Load<GameObject>("e_con3005_fx");
        }*/

        if(Input.GetKey(KeyCode.G))
        {
            for(int i = 0; i < 100; ++ i)
            {
                m_EffectList.Add(Instantiate(m_EffectGameObject));
            }
        }
        else if(Input.GetKey(KeyCode.C))
        {
            for(int i = 0; i < m_EffectList.Count; ++ i)
            {
                Destroy(m_EffectList[i]);
            }

            m_EffectList.Clear();
        }

        if(Input.GetKey(KeyCode.L))
        {
            //m_EffectGameObject = (GameObject)Resources.Load("e_con3005_fx");
            object[] objs = Resources.LoadAll("");
        }
    }

    class Player
    {
        public string name;
        public int age;
    }
}

