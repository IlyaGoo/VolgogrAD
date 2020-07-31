using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataReader : MonoBehaviour {

    private string docPath = "";
    private int headNum = 0;
    private int bodyNum = 0;
    private int legsNum = 0;

    // Use this for initialization
    void Stdart () {
        docPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/VolgogrAD";

        if (File.Exists(docPath + "/player.txt"))
        {
            using (StreamReader sw = new StreamReader(docPath + "/player.txt", System.Text.Encoding.Default))
            {
                var bodyNums = sw.ReadLine().Split(',');
                headNum = int.Parse(bodyNums[0]);
                bodyNum = int.Parse(bodyNums[1]);
                legsNum = int.Parse(bodyNums[2]);

            }
        }
        else
        {
            var a = File.Create(docPath + "/player.txt");
            a.Close();

        }
    }
	
}
