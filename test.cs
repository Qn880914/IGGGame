using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    [SerializeField] private float aa = 1.0000f;

    private static System.Func<int, Player> analyzeCustomDepend;

    Dictionary<string, Player> m_Players = new Dictionary<string, Player>();
    // Start is called before the first frame update
    void Start()
    {
        m_Players.TryGetValue("1", out Player player);
        int k = 10;
        player = new Player { name = "mm"};
        player = analyzeCustomDepend?.Invoke(2);
        int i = 0;


        m_Players.Add("0", new Player());
        m_Players.Add("1", new Player());
        m_Players.Add("2", new Player());
        m_Players.Add("3", new Player());
        m_Players.Add("4", new Player());
    }

    // Update is called once per frame
    void Update()
    {
        //UnityEngine.Debug.Log(aa.ToString("N2"));


        m_Players.TryGetValue("1", out Player player);
    }

    class Player
    {
        public string name;
        public int age;
    }
}
